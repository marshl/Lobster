using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace Lobster
{
    public class LobsterModel
    {
        public DatabaseConfig dbConfig;
        public List<ClobDirectory> clobDirectories;
        public List<FileInfo> tempFileList = new List<FileInfo>();

        public LobsterModel()
        {
            this.LoadDatabaseConfig();
            this.LoadClobTypes();
            this.CompareFilesToDatabase();
        }

        private void LoadDatabaseConfig()
        {
            this.dbConfig = new DatabaseConfig();
            XmlSerializer xmls = new XmlSerializer( typeof( DatabaseConfig ) );
            StreamReader streamReader = new StreamReader( Program.SETTINGS_DIR + "/" + Program.DB_CONFIG_FILE );
            XmlReader xmlReader = XmlReader.Create( streamReader );
            this.dbConfig = (DatabaseConfig)xmls.Deserialize( xmlReader );
        }

        private OracleConnection OpenConnection()
        {
            OracleConnection con = new OracleConnection();
            con.ConnectionString = "User Id=" + this.dbConfig.username
                + ";Password=" + this.dbConfig.password
                + ";Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)("
                + "HOST=" + this.dbConfig.host + ")(PORT=1521)))(CONNECT_DATA="
                + "(SID=" + this.dbConfig.sid + ")(SERVER=DEDICATED)))";
               // + ";Data Source=" + this.dbConfig.dataSource;
                
            //+ ";Pooling=false";
            try
            {
                con.Open();
            }
            catch ( Exception _e )
            {
                LobsterMain.instance.OnErrorMessage( "Database Connection Failure", "Cannot open connection to database: " + _e.Message );
                MessageLog.Log( "Connection to Oracle failed: " + _e.Message );
                return null;
            }
            return con;
        }

        private void CompareFilesToDatabase()
        {
            OracleConnection con = this.OpenConnection();
            if ( con == null )
            {
                MessageLog.Log( "Connection failed, cannot diff files with database." );
                return;
            }

            foreach ( ClobDirectory clobDir in this.clobDirectories )
            {
                this.CompareDirectoryToDatabase( clobDir, con );
            }
            con.Dispose();
        }

        private void LoadClobTypes()
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
                this.PopulateTreeView( clobDir );
                this.clobDirectories.Add( clobDir );
            }
        }

        public bool SendUpdateClobMessage( ClobFile _clobFile )
        {
            /*Task task = Task.Delay( 200 );
            task.
            */
            OracleConnection con = this.OpenConnection();
            if ( con == null )
            {
                return false;
            }
            bool result = this.UpdateDatabaseClob( _clobFile, con );
            con.Dispose();
            return result;
        }

        public void GetWorkingFiles( ref List<ClobFile> _workingFiles )
        {
            foreach ( ClobDirectory clobDir in this.clobDirectories )
            {
                clobDir.GetWorkingFiles( ref _workingFiles );
            }
        }

        private bool PopulateTreeView( ClobDirectory _clobDirectory )
        {
            DirectoryInfo info = new DirectoryInfo( this.dbConfig.codeSource + "/" + _clobDirectory.clobType.directory );
            if ( !info.Exists )
            {
                MessageLog.Log( "CodeSource folder \"" + this.dbConfig.codeSource + "\" could not be found" );
                LobsterMain.instance.OnErrorMessage( "Folder not found", "CodeSource folder \"" + this.dbConfig.codeSource + "\" could not be found" );
                return false;
            }

            _clobDirectory.rootClobNode = new ClobNode( info, this );
            GetDirectories( _clobDirectory.rootClobNode, _clobDirectory );
            return true;
        }

        private void GetDirectories( ClobNode _clobNode, ClobDirectory _clobDirectory )
        {
            DirectoryInfo[] subDirs = _clobNode.dirInfo.GetDirectories();
            foreach ( DirectoryInfo subDir in subDirs )
            {
                ClobNode childNode = new ClobNode( subDir, this );
                GetDirectories( childNode, _clobDirectory);
                _clobNode.childNodes.Add( childNode );
            }

            foreach ( FileInfo fileInfo in _clobNode.dirInfo.GetFiles() )
            {
                ClobFile clobFile = new ClobFile( fileInfo, _clobNode, _clobDirectory );
                _clobDirectory.fullpathClobMap.Add( fileInfo.FullName.ToLower(), clobFile );
                string key = fileInfo.Name;
                try
                {
                    _clobNode.clobFileMap.Add( key.ToLower(), clobFile );
                }
                catch ( Exception _e )
                {
                    MessageLog.Log( "A file with the name '" + key + "' already exists" + _e.Message );
                }
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
            OracleTransaction trans = _con.BeginTransaction();
            OracleCommand command = _con.CreateCommand();
            ClobType ct = _clobFile.parentClobDirectory.clobType;
            bool useBlobColumn = ct.blobColumnName != null
                && ct.blobColumnTypes.Contains( _clobFile.databaseType );

            if ( ct.hasParentTable )
            {
                command.CommandText =
                    "UPDATE " + ct.schema + "." + ct.table + " child"
                    + " SET " + ( useBlobColumn ? ct.blobColumnName : ct.clobColumn ) + " = :data"
                    + " WHERE " + ct.mnemonicColumn + " = ("
                        + "SELECT parent." + ct.parentIDColumn
                        + " FROM " + ct.schema + "." + ct.parentTable + " parent"
                        + " WHERE parent." + ct.parentMnemonicColumn + " = '" + _clobFile.databaseMnemonic + "')";
            }
            else
            {
                command.CommandText = "UPDATE " + ct.schema + "." + ct.table
                    + " SET " + ( useBlobColumn ? ct.blobColumnName : ct.clobColumn ) + " = :data"
                    + " WHERE " + ct.mnemonicColumn + " = '" + _clobFile.databaseMnemonic + "'";
            }

            try
            {
                this.AddFileDataParameter( command, _clobFile, ct, useBlobColumn );
            }
            catch ( IOException _e )
            {
                trans.Rollback();
                LobsterMain.instance.OnErrorMessage( "Clob Update Failed", "An IO Exception occurred when updating the database: " + _e.Message );
                MessageLog.Log( "Clob update failed: " + _e.Message );
                return false;
            }

            try
            {
                int rowsAffected = command.ExecuteNonQuery();
                if ( rowsAffected != 1 )
                {
                    LobsterMain.instance.OnErrorMessage( "Clob Update Failed", rowsAffected + " rows were affected during the update (expected only 1). Rolling back..." );
                    MessageLog.Log( rowsAffected + " rows were updated" );
                    trans.Rollback();
                    return false;
                }
            }
            catch ( InvalidOperationException _e )
            {
                trans.Rollback();
                LobsterMain.instance.OnErrorMessage( "Clob Update Failed", "An invalid operation occurred when updating the database: " + _e.Message );
                MessageLog.Log( "Clob update failed: " + _e.Message + " for command " + command.CommandText );
                return false;
            }
            trans.Commit();
            command.Dispose();
            MessageLog.Log( "Clob file update successful: " + _clobFile.fileInfo.Name );
            return true;
        }

        public string SendGetClobDataMessage( ClobFile _clobFile )
        {
            OracleConnection con = this.OpenConnection();
            if ( con == null )
            {
                return null;
            }
            string result = this.GetDatabaseClobData( _clobFile, con );
            con.Dispose();
            return result;
        }

        private void AddFileDataParameter( OracleCommand command, ClobFile _clobFile, ClobType _ct, bool _useBlobColumn )
        {
            if ( _useBlobColumn )
            {
                FileStream fileStream = new FileStream( _clobFile.fileInfo.FullName, FileMode.Open, FileAccess.Read );
                byte[] fileData = new byte[fileStream.Length];
                fileStream.Read( fileData, 0, Convert.ToInt32( fileStream.Length ) );
                fileStream.Close();
                OracleParameter param = command.Parameters.Add( "data", OracleDbType.Blob );
                param.Value = fileData;
            }
            else
            {
                string contents = File.ReadAllText( _clobFile.fileInfo.FullName );
                contents += this.GetClobFooterMessage();
                OracleDbType insertType = _ct.clobDataType == "clob" ? OracleDbType.Clob : OracleDbType.XmlType;
                command.Parameters.Add( "data", insertType, contents, ParameterDirection.Input );
            }
        }

        private bool InsertDatabaseClob( ClobFile _clobFile, string _dataType, OracleConnection _con )
        {
            string mnemonic = Path.GetFileNameWithoutExtension( _clobFile.fileInfo.Name );
            OracleCommand command = _con.CreateCommand();
            OracleTransaction trans = _con.BeginTransaction();
            ClobType ct = _clobFile.parentClobDirectory.clobType;

            bool useBlobColumn = ct.blobColumnName != null
                && ct.blobColumnTypes.Contains( _dataType );

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
                catch ( InvalidOperationException _e )
                {
                    LobsterMain.instance.OnErrorMessage( "Clob Insert Error",
                        "An invalid operation occurred when inserting into the parent table of " + _clobFile.fileInfo.Name + ": " + _e.Message );
                    MessageLog.Log( "Error creating new clob: " + _e.Message );
                    return false;
                }
                command.Dispose();

                // Child table
                command = _con.CreateCommand();
                command.CommandText = "INSERT INTO " + ct.schema + "." + ct.table
                    + "( " + ct.mnemonicColumn + ", " + ct.parentIDColumn + ", start_datetime, " + ( useBlobColumn ? ct.blobColumnName : ct.clobColumn );

                if ( _dataType != null )
                {
                    command.CommandText += ", type";
                }
                command.CommandText += " ) VALUES( ( SELECT MAX( " + ct.parentIDColumn + " ) + 1 FROM " + ct.schema + "." + ct.table + " ), "
                    + "( SELECT " + ct.parentIDColumn + " FROM " + ct.schema + "." + ct.parentTable
                        + " WHERE " + ct.parentMnemonicColumn + " = '" + mnemonic + "'),"
                    + " SYSDATE, :data ";
                if ( _dataType != null )
                {
                    command.CommandText += ", '" + _dataType + "'";
                }
                command.CommandText += " )";
            }
            else // No parent table
            {
                command.CommandText = "INSERT INTO " + ct.schema + "." + ct.table
                    + " ( " + ct.mnemonicColumn + ", " + ( useBlobColumn ? ct.blobColumnName : ct.clobColumn );

                if ( _dataType != null )
                {
                    command.CommandText += ", type";
                }
                command.CommandText += ") VALUES ( '" + mnemonic + "', :data";
                if ( _dataType != null )
                {
                    command.CommandText += ", '" + _dataType + "'";
                }
                command.CommandText += ")";
            }

            this.AddFileDataParameter( command, _clobFile, ct, useBlobColumn );

            try
            {
                command.ExecuteNonQuery();
            }
            catch ( InvalidOperationException _e )
            {
                // Discard the insert amde into the parent table
                trans.Rollback();
                LobsterMain.instance.OnErrorMessage( "Clob Insert Error",
                        "An invalid operation occurred when inserting " + _clobFile.fileInfo.Name + ": " + _e.Message );
                MessageLog.Log( "Error creating new clob: " + _e.Message );
                return false;
            }
            command.Dispose();
            trans.Commit();
            _clobFile.status = ClobFile.STATUS.SYNCHRONISED;
            MessageLog.Log( "Clob file creation successful: " + _clobFile.fileInfo.Name );
            return true;
        }

        private string GetDatabaseClobData( ClobFile _clobFile, OracleConnection _con )
        {
            OracleCommand command = _con.CreateCommand();

            ClobType ct = _clobFile.parentClobDirectory.clobType;

            if ( ct.hasParentTable )
            {
                command.CommandText =
                    "SELECT " + ct.clobColumn + " FROM " + ct.schema + "." + ct.parentTable + " parent"
                    + " JOIN " + ct.schema + "." + ct.table + " child"
                    + " ON child." + ct.mnemonicColumn + " = parent." + ct.parentIDColumn
                    + " WHERE parent." + ct.parentMnemonicColumn + " = '" + _clobFile.databaseMnemonic + "'";
            }
            else
            {
                command.CommandText =
                    "SELECT " + ct.clobColumn + " FROM " + ct.schema + "." + ct.table
                    + " WHERE " + ct.mnemonicColumn + " = '" + _clobFile.databaseMnemonic + "'";
            }

            try
            {
                OracleDataReader reader = command.ExecuteReader();
                while ( reader.Read() )
                {
                    if ( ct.clobDataType == "clob" )
                    {
                        OracleClob clob = reader.GetOracleClob( 0 );
                        return clob.Value;
                    }
                    else
                    {
                        OracleXmlType xml = reader.GetOracleXmlType( 0 );
                        return xml.Value;
                    }
                }
            }
            catch ( InvalidOperationException _e )
            {
                LobsterMain.instance.OnErrorMessage( "Clob Data Fetch Error",
                        "An invalid operation occurred when retreiving the data of " + _clobFile.fileInfo.Name + ": " + _e.Message );
                MessageLog.Log( "Error retrieving data: " + _e.Message );
                return null;
            }
            LobsterMain.instance.OnErrorMessage( "Clob Data Fetch Error",
                        "No data was found for " + _clobFile.fileInfo.Name );
            MessageLog.Log( "No data found on clob retrieval" );
            return null;
        }

        private void CompareDirectoryToDatabase( ClobDirectory _clobDir, OracleConnection _con )
        {
            OracleCommand command = _con.CreateCommand();

            ClobType ct = _clobDir.clobType;
            if ( ct.hasParentTable )
            {
                command.CommandText = "SELECT parent." + ct.parentMnemonicColumn
                    + ( ct.dataTypeColumnName != null ? ", child." + ct.dataTypeColumnName : null )
                    + " FROM " + ct.schema + "." + ct.parentTable + " parent"
                    + " JOIN " + ct.schema + "." + ct.table + " child"
                    + " ON child." + ct.mnemonicColumn + " = parent." + ct.parentIDColumn;
            }
            else
            {
                command.CommandText = "SELECT " + ct.mnemonicColumn
                    + ( ct.dataTypeColumnName != null ? ", " + ct.dataTypeColumnName : null )
                    + " FROM " + ct.schema + "." + ct.table;
            }

            OracleDataReader reader;
            try
            {
                reader = command.ExecuteReader();
            }
            catch ( InvalidOperationException _e )
            {
                LobsterMain.instance.OnErrorMessage( "Directory Comparison Error",
                        "An invalid operation occurred when retriving the file list for  " + ct.schema + "." + ct.table + ": " + _e.Message );
                MessageLog.Log( "Error comparing to database: " + _e.Message );
                return;
            }

            // Assume the file is local only
            foreach ( KeyValuePair<string, ClobFile> pair in _clobDir.fullpathClobMap )
            {
                pair.Value.status = ClobFile.STATUS.LOCAL_ONLY;
            }

            while ( reader.Read() )
            {
                string mnemonic, filename;
                mnemonic = filename = reader.GetString( 0 );
                string databaseType = ct.dataTypeColumnName != null ? reader.GetString( 1 ) : null;

                string prefix = null;
                if ( mnemonic.Contains( '/' ) )
                {
                    prefix = mnemonic.Substring( 0, mnemonic.LastIndexOf( '/' ) );
                    filename = mnemonic.Substring( mnemonic.LastIndexOf( '/' ) + 1 );
                }

                // Assume xml data types for tables without a datatype column
                if ( ct.dataTypeColumnName == null || prefix == null )
                {
                    filename += ".xml";
                }
                else
                {
                    filename += this.ConvertPrefixToExtension( prefix, databaseType );
                }

                // Any files found on the database are "synchronised" 
                ClobFile clobFile = _clobDir.rootClobNode.FindFileWithName( filename.ToLower() );
                if ( clobFile != null )
                {
                    clobFile.status = ClobFile.STATUS.SYNCHRONISED;
                    clobFile.databaseMnemonic = mnemonic;
                    clobFile.databaseType = databaseType;
                }
                else
                {
                    MessageLog.Log( "No file found for " + filename + " (" + mnemonic + ")" );
                }
            }
            command.Dispose();
        }

        private string ConvertPrefixToExtension( string _prefix, string _datatype )
        {
            switch ( _prefix )
            {
                case "js":
                    return ".js";
                case "css":
                    return ".css";
                case "Fox":
                    return ".xml";
                case "img":
                {
                    switch ( _datatype )
                    {
                        case "image/png":
                            return ".png";
                        case "image/gif":
                            return ".gif";
                        case "image/jpg":
                            return ".jpg";
                        case "image/x-icon":
                            return ".ico";
                        default:
                            throw new ArgumentException( "Unknown datatype " + _datatype + " for img prefix" );
                    }
                }
                default:
                    throw new ArgumentException( "Unknown prefix " + _prefix );
            }
        }

        private string GetClobFooterMessage()
        {
            return String.Format( "<!-- Last clobbered by {0} ({1}) at {2} using Lobster build {3} -->",
                Environment.UserName,
                Environment.MachineName,
                DateTime.Now,
                RetrieveLinkerTimestamp() );
        }

        //http://stackoverflow.com/questions/1600962/displaying-the-build-date
        private DateTime RetrieveLinkerTimestamp()
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
    }
}
