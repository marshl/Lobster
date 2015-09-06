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
using System.Xml.Schema;

namespace Lobster
{
    public class LobsterModel
    {
        public List<DatabaseConfig> dbConfigList;
        public DatabaseConnection currentConnection;

        public List<FileInfo> tempFileList = new List<FileInfo>();

        private MimeTypeList mimeTypeList;

        public LobsterModel()
        {
            this.mimeTypeList = Common.DeserialiseXmlFileUsingSchema<MimeTypeList>( "LobsterSettings/MimeTypes.xml", null );
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
            MessageLog.LogInfo( "Loading Database Config File {0}", _fullpath );
            DatabaseConfig dbConfig;
            try
            {
                dbConfig = Common.DeserialiseXmlFileUsingSchema<DatabaseConfig>( _fullpath, "LobsterSettings/DatabaseConfig.xsd" );
            }
            catch ( Exception _e )
            {
                if ( _e is FileNotFoundException || _e is InvalidOperationException || _e is XmlException || _e is XmlSchemaValidationException )
                {
                    MessageBox.Show( "The DBConfig file {0} failed to load. Check the log for more information.", "ClobType Load Failed",
                           MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1 );
                    MessageLog.LogError( "An error occurred when loading the ClobType {0}: {1}", _fullpath, _e.Message );
                    return null;
                }
                throw;
            }

            dbConfig.fileLocation = _fullpath;

            // If the CodeSource folder cannot be found, prompt the user for it
            if ( dbConfig.codeSource == null || !Directory.Exists( dbConfig.codeSource ) )
            {
                string codeSourceDir = PromptForDirectory( String.Format( "Please select your CodeSource directory for {0}", dbConfig.name ), null );
                if ( codeSourceDir != null )
                {
                    dbConfig.codeSource = codeSourceDir;
                    DatabaseConfig.Serialise( _fullpath, dbConfig );
                }
                else // Ignore config files that don't have a valid CodeSource folder
                {
                    return null;
                }
            }
            return dbConfig;
        }

        private static string PromptForDirectory( string _description, string _startingPath )
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = _description;
            if ( _startingPath != null )
            {
                fbd.SelectedPath = _startingPath;
            }
            DialogResult result = fbd.ShowDialog();
            if ( result != DialogResult.OK )
            {
                return null;
            }
            return fbd.SelectedPath;
        }

        private static OracleConnection OpenConnection( DatabaseConfig _config )
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
                MessageLog.LogInfo( "Connecting to database {0} using connection string {1}", _config.name, con.ConnectionString );
                con.Open();
            }
            catch ( Exception _e )
            {
                if ( _e is InvalidOperationException || _e is OracleException )
                {
                    LobsterMain.OnErrorMessage( "Database Connection Failure", "Cannot open connection to database: " + _e.Message );
                    MessageLog.LogError( "Connection to Oracle failed: {0}", _e.Message );
                    return null;
                }
                throw;
            }
            return con;
        }

        public void GetDatabaseFileLists()
        { 
            OracleConnection con = OpenConnection( this.currentConnection.dbConfig );
            if ( con == null )
            {
                MessageLog.LogError( "Connection failed, cannot diff files with database." );
                return;
            }
            
            foreach ( KeyValuePair<ClobType, ClobDirectory> pair in this.currentConnection.clobTypeToDirectoryMap )
            {
                this.GetDatabaseFileListForDirectory( pair.Value, con );
            }
            con.Dispose();
        }

        public void SendUpdateClobMessage( ClobFile _clobFile )
        {
            OracleConnection con = OpenConnection( this.currentConnection.dbConfig );
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

        public bool SendInsertClobMessage( ClobFile _clobFile, ClobType.Table _table, string _mimeType )
        {
            OracleConnection con = OpenConnection( this.currentConnection.dbConfig );
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
            Debug.Assert( _clobFile.IsSynced );

            OracleTransaction trans = _con.BeginTransaction();
            OracleCommand command = _con.CreateCommand();
            ClobType.Table table = _clobFile.dbClobFile.table;

            try
            {
                command.CommandText = table.BuildUpdateStatement( _clobFile );
            }
            catch ( ClobColumnNotFoundException _e )
            {
                LobsterMain.OnErrorMessage( "Clob Data Fetch Error", _e.Message );
                MessageLog.LogError( _e.Message );
                return false;
            }
            try
            {
                this.AddFileDataParameter( command, _clobFile, _clobFile.dbClobFile.table, _clobFile.dbClobFile.mimeType );
            }
            catch ( IOException _e )
            {
                trans.Rollback();
                LobsterMain.OnErrorMessage( "Clob Update Failed", "An IO Exception occurred when updating the database: " + _e.Message );
                MessageLog.LogError( "Clob update failed with message \"" + _e.Message + "\" for command \"" + command.CommandText + "\"" );
                return false;
            }

            int rowsAffected;
            try
            {
                MessageLog.LogInfo( "Executing Update query: {0}", command.CommandText );
                rowsAffected = command.ExecuteNonQuery();
            }
            catch ( Exception _e )
            {
                if ( _e is OracleException || _e is InvalidOperationException )
                {
                    trans.Rollback();
                    LobsterMain.OnErrorMessage( "Clob Update Failed", "An invalid operation occurred when updating the database: " + _e.Message );
                    MessageLog.LogError( "Clob update failed: " + _e.Message + " for command: " + command.CommandText );
                    return false;
                }
                throw;
            }

            if ( rowsAffected != 1 )
            {
                trans.Rollback();
                LobsterMain.OnErrorMessage( "Clob Update Failed", rowsAffected + " rows were affected during the update (expected only 1). The transaction has been rolled back." );
                MessageLog.LogError( "In invalid number of rows ({0}) were updated for command: {1}", rowsAffected, command.CommandText );
                return false;
            }

            trans.Commit();
            command.Dispose();
            MessageLog.LogInfo( "Clob file update successful: " + _clobFile.localClobFile.fileInfo.Name );
            return true;
        }

        public FileInfo SendDownloadClobDataToFileMessage( ClobFile _clobFile )
        {
            OracleConnection con = OpenConnection( this.currentConnection.dbConfig );
            if ( con == null )
            {
                return null;
            }
            FileInfo result = this.DownloadClobDataToFile( _clobFile, con );
            con.Dispose();
            return result;
        }

        private void AddFileDataParameter( OracleCommand _command, ClobFile _clobFile, ClobType.Table _table, string _mimeType )
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
                    OracleParameter param = _command.Parameters.Add( "data", OracleDbType.Blob );
                    param.Value = fileData;
                }
                else // Text mode
                {
                    StreamReader sr = new StreamReader( fs );
                    string contents = sr.ReadToEnd();
                    contents += this.GetClobFooterMessage( _mimeType );
                    OracleDbType insertType = column.dataType == ClobType.Column.Datatype.CLOB ? OracleDbType.Clob : OracleDbType.XmlType;
                    _command.Parameters.Add( "data", insertType, contents, ParameterDirection.Input );
                }
            }
        }

        private bool InsertDatabaseClob( ClobFile _clobFile, ClobType.Table _table, string _mimeType, OracleConnection _con )
        {
            Debug.Assert( _clobFile.IsLocalOnly );

            OracleCommand command = _con.CreateCommand();
            OracleTransaction trans = _con.BeginTransaction();
            string mnemonic = this.ConvertFilenameToMnemonic( _clobFile, _table, _mimeType );

            if ( _table.parentTable != null )
            {
                // Parent Table
                command.CommandText = _table.BuildInsertParentStatement( mnemonic );

                try
                {
                    MessageLog.LogInfo( "Executing Insert query on parent table: {0}", command.CommandText );
                    command.ExecuteNonQuery();
                }
                catch ( Exception _e )
                {
                    if ( _e is InvalidOperationException || _e is OracleException )
                    {
                        LobsterMain.OnErrorMessage( "Clob Insert Error",
                        "An invalid operation occurred when inserting into the parent table of " + _clobFile.localClobFile.fileInfo.Name + ": " + _e.Message );
                        MessageLog.LogError( "Error creating new clob: " + _e.Message + " when executing command: " + command.CommandText );
                        return false;
                    }
                    throw;
                }
                command.Dispose();
            }

            command = _con.CreateCommand();
            command.CommandText = _table.BuildInsertChildStatement( mnemonic, _mimeType );

            this.AddFileDataParameter( command, _clobFile, _table, _mimeType );

            try
            {
                MessageLog.LogInfo( "Executing Insert query: {0}", command.CommandText );
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
                    MessageLog.LogError( "Error creating new clob: " + _e.Message + " when executing command: " + command.CommandText );
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
            _clobFile.dbClobFile.mimeType = _mimeType;

            MessageLog.LogInfo( "Clob file creation successful: " + _clobFile.localClobFile.fileInfo.Name );
            return true;
        }

        public bool SetDatabaseConnection( DatabaseConfig _config )
        {
            OracleConnection con = OpenConnection( _config );
            if ( con == null )
            {
                return false;
            }
            con.Close();

            if ( _config.clobTypeDir == null || !Directory.Exists( _config.clobTypeDir ) )
            {
                _config.clobTypeDir = PromptForDirectory( String.Format( "Please select your Clob Type directory for {0}.", _config.name ), _config.codeSource );
                if ( _config.clobTypeDir != null )
                {
                    DatabaseConfig.Serialise( _config.fileLocation, _config );
                }
                else // Ignore config files that don't have a valid CodeSource folder
                {
                    return false;
                }
            }

            this.currentConnection = new DatabaseConnection( _config );
            this.currentConnection.LoadClobTypes();
            this.currentConnection.PopulateClobDirectories( this );
            this.RequeryDatabase();

            return true;
        }

        public void RequeryDatabase()
        {
            Debug.Assert( this.currentConnection != null );
            this.GetDatabaseFileLists();

            foreach ( KeyValuePair<ClobType, ClobDirectory> pair in this.currentConnection.clobTypeToDirectoryMap )
            {
                pair.Value.RefreshFileLists();
            }
        }

        private FileInfo DownloadClobDataToFile( ClobFile _clobFile, OracleConnection _con )
        {
            Debug.Assert( _clobFile.dbClobFile != null );
            OracleCommand command = _con.CreateCommand();

            ClobType.Table table = _clobFile.dbClobFile.table;
            ClobType.Column column;
            try
            {
                column = _clobFile.dbClobFile.GetColumn();
                command.CommandText = table.BuildGetDataCommand( _clobFile );
            }
            catch ( ClobColumnNotFoundException _e )
            {
                LobsterMain.OnErrorMessage( "Clob Data Fetch Error", _e.Message );
                MessageLog.LogError( _e.Message );
                return null;
            }
            
            command.Parameters.Add( new OracleParameter( "mnemonic", OracleDbType.Varchar2, _clobFile.dbClobFile.mnemonic, ParameterDirection.Input ) );
            try
            {
                OracleDataReader reader = command.ExecuteReader();
                string tempPath = Path.GetTempFileName();
                new FileInfo( tempPath ).Delete();
                string tempName = tempPath.Replace( ".tmp", "." ) + _clobFile.dbClobFile.filename;
                FileInfo tempFile = new FileInfo( tempName );
                
                if ( reader.Read() )
                {
                    if ( column.dataType == ClobType.Column.Datatype.BLOB )
                    {
                        OracleBlob blob = reader.GetOracleBlob( 0 );
                        File.WriteAllBytes( tempName, blob.Value );
                    }
                    else
                    {
                        string result;
                        ClobType.Column clobColumn =_clobFile.dbClobFile.table.columns.Find( x => x.purpose == ClobType.Column.Purpose.CLOB_DATA );
                        if ( clobColumn.dataType == ClobType.Column.Datatype.CLOB )
                        {
                            OracleClob clob = reader.GetOracleClob( 0 );
                            result = clob.Value;
                        }
                        else
                        {
                            OracleXmlType xml = reader.GetOracleXmlType( 0 );
                            result = xml.Value;
                        }
                        StreamWriter streamWriter = File.AppendText( tempName );
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
                       "Too many rows were found for " + _clobFile.dbClobFile.mnemonic );
                        MessageLog.LogError( "Too many rows found on clob retrieval of {0}", _clobFile.dbClobFile.mnemonic );
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
                    MessageLog.LogError( "Error retrieving data: " + _e.Message + " when executing command " + command.CommandText );
                    return null;
                }
                throw;
            }
            LobsterMain.OnErrorMessage( "Clob Data Fetch Error",
                        "No data was found for " + _clobFile.dbClobFile.mnemonic );
            MessageLog.LogError( "No data found on clob retrieval of " + _clobFile.dbClobFile.mnemonic + " when executing command: " + command.CommandText );
            return null;
        }

        private void GetDatabaseFileListForDirectory( ClobDirectory _clobDir, OracleConnection _con )
        {
            _clobDir.dbClobFileList = new List<DBClobFile>();
            
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
                    MessageLog.LogError( "Error comparing to database: " + _e.Message + " when executing command " + command.CommandText );
                    return;
                }

                while ( reader.Read() )
                {
                    DBClobFile dbClobFile = new DBClobFile();
                    dbClobFile.mnemonic = reader.GetString( 0 );
                    dbClobFile.mimeType = table.columns.Find( x => x.purpose == ClobType.Column.Purpose.MIME_TYPE ) != null ? reader.GetString( 1 ) : null;
                    dbClobFile.filename = this.ConvertMnemonicToFilename( dbClobFile.mnemonic, table, dbClobFile.mimeType );
                    dbClobFile.table = table;
                    _clobDir.dbClobFileList.Add( dbClobFile );
                }
                reader.Close();
                command.Dispose();
            }
        }

        private string GetClobFooterMessage( string _mimeType )
        {
            return String.Format( "{0} Last clobbed by user {1} on machine {2} at {3} (Lobster build {4}) {5}",
                _mimeType == "text/javascript" ? "/*" : "<!--",
                Environment.UserName,
                Environment.MachineName,
                DateTime.Now,
                Common.RetrieveLinkerTimestamp().ToShortDateString(),
                _mimeType == "text/javascript" ? "*/" : "-->");
        }

        public string ConvertFilenameToMnemonic( ClobFile _clobFile, ClobType.Table _table, string _mimeType )
        {
            Debug.Assert( _clobFile.localClobFile != null );
            string mnemonic = Path.GetFileNameWithoutExtension( _clobFile.localClobFile.fileInfo.Name );
            if ( _table.columns.Find( x => x.purpose == ClobType.Column.Purpose.MIME_TYPE ) != null )
            {
                MimeTypeList.MimeType mt = this.mimeTypeList.mimeTypes.Find( x => x.name == _mimeType );
                if ( mt == null )
                {
                    throw new ArgumentException( "Unknown mime-to-prefix key " + _mimeType );
                }
                
                if ( mt.prefix.Length > 0 )
                {
                    mnemonic = mt.prefix + '/' + mnemonic;
                }
            }

            return mnemonic;
        }

        public string ConvertMnemonicToFilename( string _mnemonic, ClobType.Table _table, string _mimeType )
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
                MimeTypeList.MimeType mt = this.mimeTypeList.mimeTypes.Find( x => x.name == _mimeType );

                if ( mt == null )
                {
                    throw new ArgumentException( "Unkown mime-to-extension key " + _mimeType );
                }
                filename += mt.extension;
            }
            return filename;
        }
    }
}
