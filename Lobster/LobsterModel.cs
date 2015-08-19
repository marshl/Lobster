﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Diagnostics;
using System.Windows.Forms;

namespace Lobster
{
    public class LobsterModel
    {
        public List<DatabaseConfig> dbConfigList;
        public DatabaseConfig currentConfig;

        public List<ClobType> clobTypeList;
        public Dictionary<ClobType, ClobDirectory> clobTypeToDirectoryMap;
        public List<FileInfo> tempFileList = new List<FileInfo>();

        public Dictionary<string, string> mimeToPrefixMap;
        public Dictionary<string, string> mimeToExtensionMap;

        public LobsterModel()
        {
            LoadFileIntoMap( @"LobsterSettings\mime_to_prefix.ini", out this.mimeToPrefixMap );
            LoadFileIntoMap( @"LobsterSettings\mime_to_extension.ini", out this.mimeToExtensionMap );
        }

        public void LoadDatabaseConfig()
        {
            this.dbConfigList = new List<DatabaseConfig>();
            foreach ( string filename in Directory.GetFiles( Program.DB_CONFIG_DIR ) )
            {
                DatabaseConfig dbConfig = LoadDatabaseConfigFile( filename );
                if ( dbConfig != null )
                {
                    this.dbConfigList.Add( dbConfig );
                }
            }
        }

        private static DatabaseConfig LoadDatabaseConfigFile( string _fullpath )
        {
            DatabaseConfig dbConfig = new DatabaseConfig();
            XmlSerializer xmls = new XmlSerializer( typeof( DatabaseConfig ) );
            StreamReader streamReader = new StreamReader( _fullpath );
            XmlReader xmlReader = XmlReader.Create( streamReader );
            dbConfig = (DatabaseConfig)xmls.Deserialize( xmlReader );
            xmlReader.Close();
            streamReader.Close();

            // If the CodeSource folder cannot be found, prompt the user for it
            if ( dbConfig.codeSource == null || !Directory.Exists( dbConfig.codeSource ) )
            {
                string codeSourceDir = PromptForCodeSource();
                if ( codeSourceDir != null )
                {
                    dbConfig.codeSource = codeSourceDir;
                    SerialiseConfig( _fullpath, dbConfig );
                }
                else // Ignore config files that don't have a valid CodeSource folder
                {
                    return null;
                }
            }
            return dbConfig;
        }

        private static string PromptForCodeSource()
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Please select your CodeSource directory.";
            DialogResult result = fbd.ShowDialog();
            if ( result != DialogResult.OK )
            {
                return null;
            }
            return fbd.SelectedPath;
        }

        private static void SerialiseConfig( string _fullpath, DatabaseConfig _config )
        {
            XmlSerializer xmls = new XmlSerializer( typeof( DatabaseConfig ) );
            StreamWriter streamWriter = new StreamWriter( _fullpath );
            xmls.Serialize( streamWriter, _config );
        }

        private OracleConnection OpenConnection( DatabaseConfig _config )
        {
            OracleConnection con = new OracleConnection();
            con.ConnectionString = "User Id=" + _config.username
                + ";Password=" + _config.password
                + ";Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)("
                + "HOST=" + _config.host + ")"
                + "(PORT=" + _config.port + ")))(CONNECT_DATA="
                + "(SID=" + _config.sid + ")(SERVER=DEDICATED)))"
                + ";Pooling=" + ( _config.usePooling ? "true" : "false" );
            try
            {
                con.Open();
            }
            catch ( Exception _e )
            {
                if ( _e is InvalidOperationException || _e is OracleException )
                {
                    LobsterMain.OnErrorMessage( "Database Connection Failure", "Cannot open connection to database: " + _e.Message );
                    MessageLog.Log( "Connection to Oracle failed: " + _e.Message + " using connection string: " + con.ConnectionString );
                    return null;
                }
                throw;
            }
            return con;
        }

        public void GetDatabaseFileLists()
        { 
            OracleConnection con = this.OpenConnection( this.currentConfig );
            if ( con == null )
            {
                MessageLog.Log( "Connection failed, cannot diff files with database." );
                return;
            }
            
            foreach ( KeyValuePair<ClobType, ClobDirectory> pair in this.clobTypeToDirectoryMap )
            {
                this.GetDatabaseFileListForDirectory( pair.Value, con );
            }
            con.Dispose();
        }

        public void LoadClobTypes()
        {
            this.clobTypeList = new List<ClobType>();
            DirectoryInfo clobTypeDir = Directory.CreateDirectory( Program.CLOB_TYPE_DIR );
            foreach ( FileInfo file in clobTypeDir.GetFiles() )
            {
                try
                {
                    ClobType clobType = new ClobType();
                    XmlSerializer xmls = new XmlSerializer( typeof( ClobType ) );
                    StreamReader streamReader = new StreamReader( file.FullName );
                    XmlReader xmlReader = XmlReader.Create( streamReader );
                    clobType = (ClobType)xmls.Deserialize( xmlReader );
                    xmlReader.Close();
                    streamReader.Close();

                    if ( !clobType.enabled )
                    {
                        continue;
                    }

                    foreach ( ClobType.Table t in clobType.tables )
                    {
                        t.LinkColumns();
                    }

                    this.clobTypeList.Add( clobType );
                }
                catch ( InvalidOperationException _e )
                {
                    MessageBox.Show( "The ClobType " + file.Name + " failed to load. Check the log for more information.", "ClobType Load Failed", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1 );
                    MessageLog.Log( String.Format( "An InvalidOperationException was thrown when loading the ClobType {0}: {1}", file.Name, _e.Message ) );
                }
            }
        }

        public void LoadClobDirectories()
        {
            this.clobTypeToDirectoryMap = new Dictionary<ClobType, ClobDirectory>();
            foreach ( ClobType clobType in this.clobTypeList )
            {
                ClobDirectory clobDir = new ClobDirectory();
                clobDir.clobType = clobType;
                clobDir.parentModel = this;
                bool result = this.PopulateClobDirectory( clobDir );
                if ( result )
                {
                    this.clobTypeToDirectoryMap.Add( clobType, clobDir );
                }
            }
        }

        public void SendUpdateClobMessage( ClobFile _clobFile )
        {
            OracleConnection con = this.OpenConnection( this.currentConfig );
            bool result;
            if ( con == null )
            {
                result = false;
            }
            else
            {
                result = this.UpdateDatabaseClob( _clobFile, con );
                con.Dispose();
            }
            LobsterMain.instance.OnFileUpdateComplete( _clobFile, result );
        }

        public void GetWorkingFiles( ref List<ClobFile> _workingFiles )
        {
            if ( this.clobTypeToDirectoryMap == null )
            {
                return;
            }

            foreach ( KeyValuePair<ClobType, ClobDirectory> pair in this.clobTypeToDirectoryMap )
            {
                pair.Value.GetWorkingFiles( ref _workingFiles );
            }
        }

        public bool PopulateClobDirectory( ClobDirectory _clobDirectory )
        {
            DirectoryInfo info = new DirectoryInfo( this.currentConfig.codeSource + "/" + _clobDirectory.clobType.directory );
            if ( !info.Exists )
            {
                MessageLog.Log( "Folder could not be found: " + info.FullName );
                LobsterMain.OnErrorMessage( "Folder not found", "Folder \"" + info.FullName + "\" could not be found for ClobType " + _clobDirectory.clobType.name );
                return false;
            }
            _clobDirectory.rootClobNode = new ClobNode( info, _clobDirectory );
            if ( _clobDirectory.clobType.includeSubDirectories )
            {
                PopulateClobNodeDirectories_r( _clobDirectory.rootClobNode, _clobDirectory );
            }
            return true;
        }

        public void PopulateClobNodeDirectories_r( ClobNode _clobNode, ClobDirectory _clobDirectory )
        {
            DirectoryInfo[] subDirs = _clobNode.dirInfo.GetDirectories();
            foreach ( DirectoryInfo subDir in subDirs )
            {
                ClobNode childNode = new ClobNode( subDir, _clobDirectory );
                PopulateClobNodeDirectories_r( childNode, _clobDirectory );
                _clobNode.childNodes.Add( childNode );
            }
        }

        public bool SendInsertClobMessage( ClobFile _clobFile, ClobType.Table _table, string _mimeType )
        {
            OracleConnection con = this.OpenConnection( this.currentConfig );
            if ( con == null )
            {
                return false;
            }
            bool result = this.InsertDatabaseClob( _clobFile, _table, _mimeType, con );
            con.Dispose();
            return result;
        }

        private bool UpdateDatabaseClob( ClobFile _clobFile, OracleConnection _con )
        {
            Debug.Assert( _clobFile.localClobFile != null && _clobFile.dbClobFile != null );
            Debug.Assert( _clobFile.dbClobFile.mnemonic != null );

            OracleTransaction trans = _con.BeginTransaction();
            OracleCommand command = _con.CreateCommand();
            ClobType.Table table = _clobFile.dbClobFile.table;
            command.CommandText = table.BuildUpdateStatement( _clobFile );
            
            try
            {
                this.AddFileDataParameter( command, _clobFile, _clobFile.dbClobFile.table, _clobFile.dbClobFile.mimeType );
            }
            catch ( IOException _e )
            {
                trans.Rollback();
                LobsterMain.OnErrorMessage( "Clob Update Failed", "An IO Exception occurred when updating the database: " + _e.Message );
                MessageLog.Log( "Clob update failed with message \"" + _e.Message + "\" for command \"" + command.CommandText + "\"" );
                return false;
            }

            int rowsAffected;
            try
            {
                rowsAffected = command.ExecuteNonQuery();
            }
            catch ( Exception _e )
            {
                if ( _e is OracleException || _e is InvalidOperationException )
                {
                    trans.Rollback();
                    LobsterMain.OnErrorMessage( "Clob Update Failed", "An invalid operation occurred when updating the database: " + _e.Message );
                    MessageLog.Log( "Clob update failed: " + _e.Message + " for command: " + command.CommandText );
                    return false;
                }
                throw;
            }

            if ( rowsAffected != 1 )
            {
                LobsterMain.OnErrorMessage( "Clob Update Failed", rowsAffected + " rows were affected during the update (expected only 1). Rolling back..." );
                MessageLog.Log( "In invalid number of rows ( " + rowsAffected + ") were updated for command: " + command.CommandText );
                trans.Rollback();
                return false;
            }

            trans.Commit();
            command.Dispose();
            MessageLog.Log( "Clob file update successful: " + _clobFile.localClobFile.fileInfo.Name );
            return true;
        }

        public FileInfo SendDownloadClobDataToFileMessage( ClobFile _clobFile )
        {
            OracleConnection con = this.OpenConnection( this.currentConfig );
            if ( con == null )
            {
                return null;
            }
            FileInfo result = this.DownloadClobDataToFile( _clobFile, con );
            con.Dispose();
            return result;
        }

        private void AddFileDataParameter( OracleCommand command, ClobFile _clobFile, ClobType.Table _table, string _mimeType )
        {
            Debug.Assert( _clobFile.localClobFile != null );
            // Wait for the file to unlock
            using ( FileStream fs = Common.WaitForFile( _clobFile.localClobFile.fileInfo.FullName,
                FileMode.Open, FileAccess.Read, FileShare.ReadWrite ) )
            {
                ClobType.Column column = _table.columns.Find( x => x.purpose == ClobType.Column.Purpose.CLOB_DATA
                    && ( _mimeType == null || x.mimeTypes.Contains( _mimeType ) ) );
                // Binary mode
                if ( column.dataType == ClobType.Column.Datatype.BLOB )
                {
                    byte[] fileData = new byte[fs.Length];
                    fs.Read( fileData, 0, Convert.ToInt32( fs.Length ) );
                    OracleParameter param = command.Parameters.Add( "data", OracleDbType.Blob );
                    param.Value = fileData;
                }
                else // Text mode
                {
                    StreamReader sr = new StreamReader( fs );
                    string contents = sr.ReadToEnd();
                    contents += this.GetClobFooterMessage();
                    OracleDbType insertType = column.dataType == ClobType.Column.Datatype.CLOB ? OracleDbType.Clob : OracleDbType.XmlType;
                    command.Parameters.Add( "data", insertType, contents, ParameterDirection.Input );
                }
            }
        }

        private bool InsertDatabaseClob( ClobFile _clobFile, ClobType.Table _table, string _mimeType, OracleConnection _con )
        {
            Debug.Assert( _clobFile.localClobFile != null );
            Debug.Assert( _clobFile.dbClobFile == null );

            OracleCommand command = _con.CreateCommand();
            OracleTransaction trans = _con.BeginTransaction();
            string mnemonic = this.ConvertFilenameToMnemonic( _clobFile, _table, _mimeType );

            if ( _table.parentTable != null )
            {
                // Parent Table
                command.CommandText = _table.BuildInsertParentStatement( mnemonic );

                try
                {
                    command.ExecuteNonQuery();
                }
                catch ( Exception _e )
                {
                    LobsterMain.OnErrorMessage( "Clob Insert Error",
                        "An invalid operation occurred when inserting into the parent table of " + _clobFile.localClobFile.fileInfo.Name + ": " + _e.Message );
                    MessageLog.Log( "Error creating new clob: " + _e.Message + " when executing command: " + command.CommandText );
                    return false;
                }
                command.Dispose();
            }

            command = _con.CreateCommand();
            command.CommandText = _table.BuildInsertChildStatement( mnemonic, _mimeType );

            this.AddFileDataParameter( command, _clobFile, _table, _mimeType );
            //command.Parameters.Add( new OracleParameter( "mnemonic", OracleDbType.Varchar2, mnemonic, ParameterDirection.Input ) );
            try
            {
                command.ExecuteNonQuery();
            }
            catch ( Exception _e )
            {
                if ( _e is InvalidOperationException || _e is OracleException )
                {
                    // Discard the insert amde into the parent table
                    trans.Rollback();
                    LobsterMain.OnErrorMessage( "Clob Insert Error",
                            "An invalid operation occurred when inserting " + _clobFile.localClobFile.fileInfo.Name + ": " + _e.Message );
                    MessageLog.Log( "Error creating new clob: " + _e.Message + " when executing command: " + command.CommandText );
                    return false;
                }
                throw;
            }
            command.Dispose();
            trans.Commit();

            _clobFile.dbClobFile = new DBClobFile();
            _clobFile.dbClobFile.mnemonic = mnemonic;
            _clobFile.dbClobFile.filename = _clobFile.localClobFile.fileInfo.Name;
            _clobFile.dbClobFile.table = _table;

            MessageLog.Log( "Clob file creation successful: " + _clobFile.localClobFile.fileInfo.Name );
            return true;
        }

        public bool SetDatabaseConnection( DatabaseConfig _config )
        {
            OracleConnection con = this.OpenConnection( _config );
            if ( con == null )
            {
                return false;
            }
            con.Close();

            this.currentConfig = _config;
            this.LoadClobDirectories();
            this.RequeryDatabase();
            return true;
        }

        public void RequeryDatabase()
        {
            this.GetDatabaseFileLists();

            foreach ( KeyValuePair<ClobType, ClobDirectory> pair in this.clobTypeToDirectoryMap )
            {
                pair.Value.RefreshFileLists();
            }
        }

        private FileInfo DownloadClobDataToFile( ClobFile _clobFile, OracleConnection _con )
        {
            Debug.Assert( _clobFile.dbClobFile != null );
            OracleCommand command = _con.CreateCommand();

            ClobType.Table table = _clobFile.dbClobFile.table;
            ClobType.Column column = _clobFile.dbClobFile.GetColumn();
            command.CommandText = table.BuildGetDataCommand( _clobFile );
            command.Parameters.Add( new OracleParameter( "mnemonic", OracleDbType.Varchar2, _clobFile.dbClobFile.mnemonic, ParameterDirection.Input ) );
            try
            {
                OracleDataReader reader = command.ExecuteReader();
                string tempName = Path.GetTempFileName().Replace( ".tmp", "." ) + _clobFile.dbClobFile.filename;
                FileInfo tempFile = new FileInfo( tempName );

                if ( reader.Read() )
                {
                    string result;
                    if ( column.dataType == ClobType.Column.Datatype.BLOB )
                    {
                        OracleBlob blob = reader.GetOracleBlob( 0 );
                        File.WriteAllBytes( tempName, blob.Value );
                    }
                    else
                    {
                        StreamWriter streamWriter = File.AppendText( tempName );
                        
                        if ( _clobFile.dbClobFile.table.columns.Find( x => x.purpose == ClobType.Column.Purpose.CLOB_DATA ).dataType == ClobType.Column.Datatype.CLOB )
                        {
                            OracleClob clob = reader.GetOracleClob( 0 );
                            result = clob.Value;
                        }
                        else
                        {
                            OracleXmlType xml = reader.GetOracleXmlType( 0 );
                            result = xml.Value;
                        }
                        streamWriter.Write( result );
                        streamWriter.Close();
                    }
                    
                    if ( !reader.Read() )
                    {
                        reader.Close();
                        return tempFile;
                    }
                    else
                    {
                        reader.Close();
                        LobsterMain.OnErrorMessage( "Clob Data Fetch Error",
                       "Too many rows were found for " + _clobFile.localClobFile.fileInfo.Name );
                        MessageLog.Log( "Too many rows found on clob retrieval of " + _clobFile.dbClobFile.mnemonic + " when executing command: " + command.CommandText );
                        return null;
                    }
                }
            }
            catch ( Exception _e )
            {
                if ( _e is InvalidOperationException || _e is OracleNullValueException )
                {
                    LobsterMain.OnErrorMessage( "Clob Data Fetch Error",
                            "An invalid operation occurred when retreiving the data of " + _clobFile.dbClobFile.mnemonic + ": " + _e.Message );
                    MessageLog.Log( "Error retrieving data: " + _e.Message + " when executing command " + command.CommandText );
                    return null;
                }
                throw;
            }
            LobsterMain.OnErrorMessage( "Clob Data Fetch Error",
                        "No data was found for " + _clobFile.dbClobFile.mnemonic );
            MessageLog.Log( "No data found on clob retrieval of " + _clobFile.dbClobFile.mnemonic + " when executing command: " + command.CommandText );
            return null;
        }

        private void GetDatabaseFileListForDirectory( ClobDirectory _clobDir, OracleConnection _con )
        {
            _clobDir.databaseClobMap = new Dictionary<string, DBClobFile>();
            
            ClobType ct = _clobDir.clobType;

            foreach ( ClobType.Table table in ct.tables )
            {
                OracleCommand command = _con.CreateCommand();
                command.CommandText = table.GetFileListCommand();
                OracleDataReader reader;
                try
                {
                    reader = command.ExecuteReader();
                }
                catch ( InvalidOperationException _e )
                {
                    command.Dispose();
                    LobsterMain.OnErrorMessage( "Directory Comparison Error",
                            "An invalid operation occurred when retriving the file list for  " + ct.name + ". Check the logs for more information." );
                    MessageLog.Log( "Error comparing to database: " + _e.Message + " when executing command " + command.CommandText );
                    return;
                }

                while ( reader.Read() )
                {
                    DBClobFile dbClobFile = new DBClobFile();
                    dbClobFile.mnemonic = reader.GetString( 0 );
                    dbClobFile.mimeType = table.columns.Find( x => x.purpose == ClobType.Column.Purpose.MIME_TYPE ) != null ? reader.GetString( 1 ) : null;
                    dbClobFile.filename = this.ConvertMnemonicToFilename( dbClobFile.mnemonic, table, dbClobFile.mimeType );
                    dbClobFile.table = table;
                    _clobDir.databaseClobMap.Add( dbClobFile.mnemonic, dbClobFile );
                }
                reader.Close();
                command.Dispose();
            }
        }

        private string GetClobFooterMessage()
        {
            return String.Format( "<!-- Last clobbed by user {0} on machine {1} at {2} (Lobster build {3}) -->",
                Environment.UserName,
                Environment.MachineName,
                DateTime.Now,
                RetrieveLinkerTimestamp().ToShortDateString() );
        }

        //http://stackoverflow.com/questions/1600962/displaying-the-build-date
        public static DateTime RetrieveLinkerTimestamp()
        {
            string filePath = System.Reflection.Assembly.GetCallingAssembly().Location;
            const int c_PeHeaderOffset = 60;
            const int c_LinkerTimestampOffset = 8;
            byte[] b = new byte[2048];
            System.IO.Stream s = null;

            try
            {
                s = new System.IO.FileStream( filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read );
                s.Read( b, 0, 2048 );
            }
            finally
            {
                if ( s != null )
                {
                    s.Close();
                }
            }

            int i = System.BitConverter.ToInt32( b, c_PeHeaderOffset );
            int secondsSince1970 = System.BitConverter.ToInt32( b, i + c_LinkerTimestampOffset );
            DateTime dt = new DateTime( 1970, 1, 1, 0, 0, 0, DateTimeKind.Utc );
            dt = dt.AddSeconds( secondsSince1970 );
            dt = dt.ToLocalTime();
            return dt;
        }

        public string ConvertFilenameToMnemonic( ClobFile _clobFile, ClobType.Table _table, string _componentType )
        {
            Debug.Assert( _clobFile.localClobFile != null );
            string mnemonic = Path.GetFileNameWithoutExtension( _clobFile.localClobFile.fileInfo.Name );
            if ( _table.columns.Find( x => x.purpose == ClobType.Column.Purpose.MIME_TYPE ) != null )
            {
                if ( !this.mimeToPrefixMap.ContainsKey( _componentType ) )
                {
                    throw new ArgumentException( "Unknown mime-to-prefix key " + _componentType );
                }
                string prefix = this.mimeToPrefixMap[_componentType];

                if ( prefix.Length > 0 )
                {
                    mnemonic = prefix + '/' + mnemonic;
                }
            }

            return mnemonic;
        }

        public string ConvertMnemonicToFilename( string _mnemonic, ClobType.Table _table, string _databaseType )
        {
            string filename = _mnemonic;
            
            string prefix = null;
            if ( _mnemonic.Contains( '/' ) )
            {
                prefix = _mnemonic.Substring( 0, _mnemonic.LastIndexOf( '/' ) );
                filename = _mnemonic.Substring( _mnemonic.LastIndexOf( '/' ) + 1 );
            }

            // Assume xml data types for tables without a datatype column
            if ( _table.columns.Find( x => x.purpose == ClobType.Column.Purpose.MIME_TYPE ) == null || prefix == null )
            {
                filename += ".xml";
            }
            else
            {
                string extension;
                if ( !this.mimeToExtensionMap.TryGetValue( _databaseType, out extension ) )
                {
                    throw new ArgumentException( "Unkown mime-to-extension key " + _databaseType );
                }
                filename += extension;
            }
            return filename;
        }

        public static void LoadFileIntoMap( string _path, out Dictionary<string, string> _map )
        {
            _map = new Dictionary<string, string>();
            StreamReader reader = new StreamReader( _path );
            string line;
            while ( ( line = reader.ReadLine() ) != null )
            {
                line = line.Trim();
                if ( line.Contains( '#' ) )
                {
                    line = line.Substring( line.IndexOf( '#' ) );
                }

                if ( line.Length == 0 || !line.Contains( '=' ) )
                {
                    continue;
                }

                string[] split = line.Split( '=' );
                string extension = split[0];
                string type = split[1];

                _map.Add( extension, type );
            }
            reader.Close();
        }
    }
}
