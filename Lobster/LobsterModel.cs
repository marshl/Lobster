using System;
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
using System.Threading;

namespace Lobster
{
    public class LobsterModel
    {
        public DatabaseConfig dbConfig;
        public List<ClobDirectory> clobDirectories;
        public List<FileInfo> tempFileList = new List<FileInfo>();

        public Dictionary<string, string> mimeToPrefixMap;
        public Dictionary<string, string> mimeToExtensionMap;

        public LobsterModel()
        {
            LoadFileIntoMap( @"LobsterSettings\mime_to_prefix.ini", out this.mimeToPrefixMap );
            LoadFileIntoMap( @"LobsterSettings\mime_to_extension.ini", out this.mimeToExtensionMap );
        }

        public bool LoadDatabaseConfig()
        {
            this.dbConfig = new DatabaseConfig();
            XmlSerializer xmls = new XmlSerializer( typeof( DatabaseConfig ) );
            StreamReader streamReader = new StreamReader( Program.SETTINGS_DIR + "/" + Program.DB_CONFIG_FILE );
            XmlReader xmlReader = XmlReader.Create( streamReader );
            this.dbConfig = (DatabaseConfig)xmls.Deserialize( xmlReader );
            streamReader.Close();

            if ( this.dbConfig.codeSource == null || !Directory.Exists( this.dbConfig.codeSource ) )
            {
                 return this.PromptForCodeSource();
            }
            return true;
        }

        private bool PromptForCodeSource()
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Please select your CodeSource folder.";
            DialogResult result = fbd.ShowDialog();
            if ( result != DialogResult.OK )
            {
                return false;
            }
            this.dbConfig.codeSource = fbd.SelectedPath;

            this.SerialiseConfig();
            return true;
        }

        private void SerialiseConfig()
        {
            XmlSerializer xmls = new XmlSerializer( typeof( DatabaseConfig ) );
            StreamWriter streamWriter = new StreamWriter( Program.SETTINGS_DIR + "/" + Program.DB_CONFIG_FILE );
            xmls.Serialize( streamWriter, this.dbConfig );
        }

        private OracleConnection OpenConnection()
        {
            OracleConnection con = new OracleConnection();
            con.ConnectionString = "User Id=" + this.dbConfig.username
                + ";Password=" + this.dbConfig.password
                + ";Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)("
                + "HOST=" + this.dbConfig.host + ")"
                + "(PORT=" + this.dbConfig.port + ")))(CONNECT_DATA="
                + "(SID=" + this.dbConfig.sid + ")(SERVER=DEDICATED)))"
                + ";Pooling=" + ( this.dbConfig.usePooling ? "true" : "false" );
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

        public void RetrieveDatabaseFiles()
        { 
            OracleConnection con = this.OpenConnection();
            if ( con == null )
            {
                MessageLog.Log( "Connection failed, cannot diff files with database." );
                return;
            }

            foreach ( ClobDirectory clobDir in this.clobDirectories )
            {
                this.RetrieveDatabaseFiles( clobDir, con );
            }
            con.Dispose();
        }

        public void LoadClobTypes()
        {
            this.clobDirectories = new List<ClobDirectory>();
            DirectoryInfo clobTypeDir = Directory.CreateDirectory( Program.SETTINGS_DIR + "/" + Program.CLOB_TYPE_DIR );
            foreach ( FileInfo file in clobTypeDir.GetFiles() )
            {
                ClobType clobType = new ClobType();

                XmlSerializer xmls = new XmlSerializer( typeof( ClobType ) );
                StreamReader streamReader = new StreamReader( file.FullName );
                XmlReader xmlReader = XmlReader.Create( streamReader );
                clobType = (ClobType)xmls.Deserialize( xmlReader );
                streamReader.Close();

                ClobDirectory clobDir = new ClobDirectory();
                clobDir.clobType = clobType;
                clobDir.parentModel = this;
                bool result = this.PopulateClobDirectory( clobDir );
                if ( result )
                {
                    this.clobDirectories.Add( clobDir );
                }
            }
        }

        public void SendUpdateClobMessage( ClobFile _clobFile )
        {
            OracleConnection con = this.OpenConnection();
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
            foreach ( ClobDirectory clobDir in this.clobDirectories )
            {
                clobDir.GetWorkingFiles( ref _workingFiles );
            }
        }

        public bool PopulateClobDirectory( ClobDirectory _clobDirectory )
        {
            DirectoryInfo info = new DirectoryInfo( this.dbConfig.codeSource + "/" + _clobDirectory.clobType.directory );
            if ( !info.Exists )
            {
                MessageLog.Log( "Folder could not be found: " + info.FullName );
                LobsterMain.OnErrorMessage( "Folder not found", "Folder \"" + info.FullName + "\" could not be found for ClobType " + _clobDirectory.clobType.name );
                return false;
            }

            _clobDirectory.rootClobNode = new ClobNode( info, this, _clobDirectory );
            PopulateClobNodeDirectories( _clobDirectory.rootClobNode, _clobDirectory );
            return true;
        }

        public void PopulateClobNodeDirectories( ClobNode _clobNode, ClobDirectory _clobDirectory )
        {
            DirectoryInfo[] subDirs = _clobNode.dirInfo.GetDirectories();
            foreach ( DirectoryInfo subDir in subDirs )
            {
                ClobNode childNode = new ClobNode( subDir, this, _clobDirectory );
                PopulateClobNodeDirectories( childNode, _clobDirectory);
                _clobNode.childNodes.Add( childNode );
            }
        }

        public bool SendInsertClobMessage( ClobFile _clobFile, string _chosenType )
        {
            OracleConnection con = this.OpenConnection();
            if ( con == null )
            {
                return false;
            }
            bool result = this.InsertDatabaseClob( _clobFile, _chosenType, con );
            con.Dispose();
            return result;
        }

        private bool UpdateDatabaseClob( ClobFile _clobFile, OracleConnection _con)
        {
            Debug.Assert( _clobFile.localClobFile != null && _clobFile.dbClobFile != null );
            Debug.Assert( _clobFile.dbClobFile.mnemonic != null );

            OracleTransaction trans = _con.BeginTransaction();
            OracleCommand command = _con.CreateCommand();
            ClobType ct = _clobFile.parentClobDirectory.clobType;
            bool useBlobColumn = ct.blobColumnName != null
                && ct.blobColumnTypes.Contains( _clobFile.dbClobFile.componentType );

            if ( ct.hasParentTable )
            {
                command.CommandText =
                    "UPDATE " + ct.schema + "." + ct.table + " child"
                    + " SET " + ( useBlobColumn ? ct.blobColumnName : ct.clobColumn ) + " = :data"
                    + ( ct.dateColumnName != null ? ", " + ct.dateColumnName + " = SYSDATE " : null )
                    + " WHERE " + ct.mnemonicColumn + " = ("
                        + "SELECT parent." + ct.parentIDColumn
                        + " FROM " + ct.schema + "." + ct.parentTable + " parent"
                        + " WHERE parent." + ct.parentMnemonicColumn + " = '" + _clobFile.dbClobFile.mnemonic + "')";
            }
            else
            {
                command.CommandText = "UPDATE " + ct.schema + "." + ct.table
                    + " SET " + ( useBlobColumn ? ct.blobColumnName : ct.clobColumn ) + " = :data"
                    + ( ct.dateColumnName != null ? ", " + ct.dateColumnName + " = SYSDATE " : null )
                    + " WHERE " + ct.mnemonicColumn + " = '" + _clobFile.dbClobFile.mnemonic + "'";
            }

            try
            {
                this.AddFileDataParameter( command, _clobFile, ct, useBlobColumn );
            }
            catch ( IOException _e )
            {
                trans.Rollback();
                LobsterMain.OnErrorMessage( "Clob Update Failed", "An IO Exception occurred when updating the database: " + _e.Message );
                MessageLog.Log( "Clob update failed with message \"" + _e.Message + "\" for command \"" + command.CommandText + "\"" );
                return false;
            }

            try
            {
                int rowsAffected = command.ExecuteNonQuery();
                if ( rowsAffected != 1 )
                {
                    LobsterMain.OnErrorMessage( "Clob Update Failed", rowsAffected + " rows were affected during the update (expected only 1). Rolling back..." );
                    MessageLog.Log( "In invalid number of rows ( " + rowsAffected + ") were updated for command: " + command.CommandText );
                    trans.Rollback();
                    return false;
                }
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
            trans.Commit();
            command.Dispose();
            MessageLog.Log( "Clob file update successful: " + _clobFile.localClobFile.fileInfo.Name );
            return true;
        }

        public FileInfo SendDownloadClobDataToFileMessage( ClobFile _clobFile )
        {
            OracleConnection con = this.OpenConnection();
            if ( con == null )
            {
                return null;
            }
            FileInfo result = this.DownloadClobDataToFile( _clobFile, con );
            con.Dispose();
            return result;
        }

        private void AddFileDataParameter( OracleCommand command, ClobFile _clobFile, ClobType _ct, bool _useBlobColumn )
        {
            Debug.Assert( _clobFile.localClobFile != null );
            if ( _useBlobColumn )
            {
                FileStream fileStream = new FileStream( _clobFile.localClobFile.fileInfo.FullName, FileMode.Open, FileAccess.Read );
                byte[] fileData = new byte[fileStream.Length];
                fileStream.Read( fileData, 0, Convert.ToInt32( fileStream.Length ) );
                fileStream.Close();
                OracleParameter param = command.Parameters.Add( "data", OracleDbType.Blob );
                param.Value = fileData;
            }
            else
            {
                string contents = File.ReadAllText( _clobFile.localClobFile.fileInfo.FullName );
                contents += this.GetClobFooterMessage();
                OracleDbType insertType = _ct.clobDataType == "clob" ? OracleDbType.Clob : OracleDbType.XmlType;
                command.Parameters.Add( "data", insertType, contents, ParameterDirection.Input );
            }
        }

        private bool InsertDatabaseClob( ClobFile _clobFile, string _componentType, OracleConnection _con )
        {
            Debug.Assert( _clobFile.localClobFile != null );
            Debug.Assert( _clobFile.dbClobFile == null );

            OracleCommand command = _con.CreateCommand();
            OracleTransaction trans = _con.BeginTransaction();
            ClobType ct = _clobFile.parentClobDirectory.clobType;
            string mnemonic = this.ConvertFilenameToMnemonic( _clobFile, ct, _componentType );

            bool useBlobColumn = ct.blobColumnName != null
                && ct.blobColumnTypes.Contains( _componentType );

            if ( ct.hasParentTable )
            {
                // Parent Table
                command.CommandText = "INSERT INTO " + ct.schema + "." + ct.parentTable
                    + " (" + ct.parentIDColumn + ", " + ct.parentMnemonicColumn + " )"
                    + " VALUES( ( SELECT MAX( " + ct.parentIDColumn + " ) + 1 "
                    + " FROM " + ct.schema + "." + ct.parentTable + " ), '" + mnemonic + "' )";

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

                // Child table
                command = _con.CreateCommand();
                command.CommandText = "INSERT INTO " + ct.schema + "." + ct.table
                    + "( " + ct.mnemonicColumn + ", " + ct.parentIDColumn
                        + ( ct.dateColumnName != null ? ", " + ct.dateColumnName : null ) + ", "
                    + ( useBlobColumn ? ct.blobColumnName : ct.clobColumn );

                if ( _componentType != null )
                {
                    command.CommandText += ", " + ct.componentTypeColumn;
                }
                command.CommandText += " ) VALUES( ( SELECT MAX( " + ct.parentIDColumn + " ) + 1 FROM " + ct.schema + "." + ct.table + " ), "
                    + "( SELECT " + ct.parentIDColumn + " FROM " + ct.schema + "." + ct.parentTable
                        + " WHERE " + ct.parentMnemonicColumn + " = '" + mnemonic + "')"
                    + ( ct.dateColumnName != null ? ", SYSDATE " : null ) + ", :data ";
                if ( _componentType != null )
                {
                    command.CommandText += ", '" + _componentType + "'";
                }
                command.CommandText += " )";
            }
            else // No parent table
            {
                command.CommandText = "INSERT INTO " + ct.schema + "." + ct.table
                    + " ( " + ct.mnemonicColumn + ", " + ( useBlobColumn ? ct.blobColumnName : ct.clobColumn )
                    + ( _componentType != null ? ", type" : null )
                    + ") VALUES ( '" + mnemonic + "', :data"
                    + ( _componentType != null ? ", '" + _componentType + "'" : null )
                    + ")";
            }

            this.AddFileDataParameter( command, _clobFile, ct, useBlobColumn );

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

            MessageLog.Log( "Clob file creation successful: " + _clobFile.localClobFile.fileInfo.Name );
            return true;
        }

        public void RequeryDatabase()
        {
            this.RetrieveDatabaseFiles();

            foreach ( ClobDirectory clobDir in this.clobDirectories )
            {
                clobDir.RefreshFileLists();
            }
        }

        private FileInfo DownloadClobDataToFile( ClobFile _clobFile, OracleConnection _con )
        {
            Debug.Assert( _clobFile.dbClobFile != null );
            OracleCommand command = _con.CreateCommand();

            ClobType ct = _clobFile.parentClobDirectory.clobType;
            bool useBlobColumn = ct.blobColumnName != null
                 && ct.blobColumnTypes.Contains( _clobFile.dbClobFile.componentType );

            if ( ct.hasParentTable )
            {
                command.CommandText =
                    "SELECT " + ( useBlobColumn ? ct.blobColumnName : ct.clobColumn )
                    + " FROM " + ct.schema + "." + ct.parentTable + " parent"
                    + " JOIN " + ct.schema + "." + ct.table + " child"
                    + " ON child." + ct.mnemonicColumn + " = parent." + ct.parentIDColumn
                    + " WHERE parent." + ct.parentMnemonicColumn + " = '" + _clobFile.dbClobFile.mnemonic + "'";
            }
            else
            {
                command.CommandText =
                    "SELECT " + ( useBlobColumn ? ct.blobColumnName : ct.clobColumn )
                    + " FROM " + ct.schema + "." + ct.table
                    + " WHERE " + ct.mnemonicColumn + " = '" + _clobFile.dbClobFile.mnemonic + "'";
            }

            try
            {
                OracleDataReader reader = command.ExecuteReader();
                string tempName = Path.GetFileNameWithoutExtension( Path.GetTempFileName() ) + _clobFile.dbClobFile.filename;
                FileInfo tempFile = new FileInfo( tempName );

                if ( reader.Read() )
                {
                    string result;
                    if ( useBlobColumn )
                    {
                        OracleBlob blob = reader.GetOracleBlob( 0 );
                        File.WriteAllBytes( tempName, blob.Value );
                    }
                    else
                    {
                        StreamWriter streamWriter = File.AppendText( tempName );
                        if ( ct.clobDataType == "clob" )
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
                        return tempFile;
                    }
                    else
                    {
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

        private void RetrieveDatabaseFiles( ClobDirectory _clobDir, OracleConnection _con )
        {
            _clobDir.databaseClobMap = new Dictionary<string, DBClobFile>();
            OracleCommand command = _con.CreateCommand();

            ClobType ct = _clobDir.clobType;
            if ( ct.hasParentTable )
            {
                command.CommandText = "SELECT parent." + ct.parentMnemonicColumn
                    + ( ct.componentTypeColumn != null ? ", child." + ct.componentTypeColumn : null )
                    + " FROM " + ct.schema + "." + ct.parentTable + " parent"
                    + " JOIN " + ct.schema + "." + ct.table + " child"
                    + " ON child." + ct.mnemonicColumn + " = parent." + ct.parentIDColumn;
            }
            else
            {
                command.CommandText = "SELECT " + ct.mnemonicColumn
                    + ( ct.componentTypeColumn != null ? ", " + ct.componentTypeColumn : null )
                    + " FROM " + ct.schema + "." + ct.table;
            }

            OracleDataReader reader;
            try
            {
                reader = command.ExecuteReader();
            }
            catch ( InvalidOperationException _e )
            {
                LobsterMain.OnErrorMessage( "Directory Comparison Error",
                        "An invalid operation occurred when retriving the file list for  " + ct.schema + "." + ct.table + ": " + _e.Message );
                MessageLog.Log( "Error comparing to database: " + _e.Message + " when executing command " + command.CommandText );
                return;
            }

            while ( reader.Read() )
            {
                DBClobFile dbClobFile = new DBClobFile();
                dbClobFile.mnemonic = reader.GetString( 0 );
                dbClobFile.componentType = ct.componentTypeColumn != null ? reader.GetString( 1 ) : null;
                dbClobFile.filename = this.ConvertMnemonicToFilename( dbClobFile.mnemonic, ct, dbClobFile.componentType );
                _clobDir.databaseClobMap.Add( dbClobFile.mnemonic, dbClobFile );
            }
            command.Dispose();
        }

        private string GetClobFooterMessage()
        {
            return String.Format( "<!-- Last clobbed by user {0} on machine {1} at {2} using Lobster build {3} -->",
                Environment.UserName,
                Environment.MachineName,
                DateTime.Now,
                RetrieveLinkerTimestamp() );
        }

        //http://stackoverflow.com/questions/1600962/displaying-the-build-date
        private static DateTime RetrieveLinkerTimestamp()
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

        public string ConvertFilenameToMnemonic( ClobFile _clobFile, ClobType _ct, string _componentType )
        {
            Debug.Assert( _clobFile.localClobFile != null );
            string mnemonic = Path.GetFileNameWithoutExtension( _clobFile.localClobFile.fileInfo.Name );
            if ( _ct.componentTypeColumn != null )
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

        public string ConvertMnemonicToFilename( string _mnemonic, ClobType _ct, string _databaseType )
        {
            string filename = _mnemonic;
            
            string prefix = null;
            if ( _mnemonic.Contains( '/' ) )
            {
                prefix = _mnemonic.Substring( 0, _mnemonic.LastIndexOf( '/' ) );
                filename = _mnemonic.Substring( _mnemonic.LastIndexOf( '/' ) + 1 );
            }

            // Assume xml data types for tables without a datatype column
            if ( _ct.componentTypeColumn == null || prefix == null )
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
