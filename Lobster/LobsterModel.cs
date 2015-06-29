using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace Lobster
{
    public class LobsterModel
    {
        public DatabaseConfig dbConfig;
        public OracleConnection connection;

        public List<ClobDirectory> clobDirectories;
        public List<FileInfo> tempFileList = new List<FileInfo>();

        public LobsterModel()
        {
            this.LoadDatabaseConfig();
            this.OpenConnection();
            this.LoadClobTypes();
        }

        private void LoadDatabaseConfig()
        {
            this.dbConfig = new DatabaseConfig();
            XmlSerializer xmls = new XmlSerializer( typeof( DatabaseConfig ) );
            StreamReader streamReader = new StreamReader( Program.SETTINGS_DIR + "/" + Program.DB_CONFIG_FILE );
            XmlReader xmlReader = XmlReader.Create( streamReader );
            this.dbConfig = (DatabaseConfig)xmls.Deserialize( xmlReader );
        }

        private bool OpenConnection()
        {
            this.connection = new OracleConnection();
            this.connection.ConnectionString = "User Id=" + this.dbConfig.username
                + ";Password=" + this.dbConfig.password
                + ";Data Source=" + this.dbConfig.dataSource;
            try
            {
                this.connection.Open();
            }
            catch ( Exception _e )
            {
                Console.WriteLine( "Connection error: " + _e.Message );
                return false;
            }
            Console.WriteLine( "Connection successful" );
            return true;
        }

        private void CompareToDatabase()
        {
            foreach ( ClobDirectory clobDir in this.clobDirectories )
            {
                clobDir.CompareToDatabase();
            }
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

                ClobDirectory clobDirectory = new ClobDirectory();
                clobDirectory.clobType = clobType;
                clobDirectory.parentModel = this;
                this.PopulateTreeView( clobDirectory );
                clobDirectory.CreateFileWatchers();
                clobDirectory.CompareToDatabase();
                this.clobDirectories.Add( clobDirectory );
            }
        }

        private bool PopulateTreeView( ClobDirectory _clobDirectory )
        {
            DirectoryInfo info = new DirectoryInfo( this.dbConfig.codeSource + "/" + _clobDirectory.clobType.directory );
            if ( !info.Exists )
            {
                Console.WriteLine( "CodeSource folder \"{0}\" could not be found", this.dbConfig.codeSource );
                return false;
            }

            _clobDirectory.rootClobNode = new ClobNode( info );
            GetDirectories( _clobDirectory.rootClobNode, _clobDirectory );
            return true;
        }

        private void GetDirectories( ClobNode _clobNode, ClobDirectory _clobDirectory )
        {
            DirectoryInfo[] subDirs = _clobNode.dirInfo.GetDirectories();
            foreach ( DirectoryInfo subDir in subDirs )
            {
                ClobNode childNode = new ClobNode( subDir );
                GetDirectories( childNode, _clobDirectory);
                _clobNode.subDirs.Add( childNode );
            }

            foreach ( FileInfo fileInfo in _clobNode.dirInfo.GetFiles() )
            {
                ClobFile clobFile = new ClobFile( fileInfo, _clobNode, _clobDirectory );
                _clobDirectory.fullpathClobMap.Add( fileInfo.FullName, clobFile );
                string key = Path.GetFileNameWithoutExtension( fileInfo.Name );
                try
                {
                    _clobDirectory.filenameClobMap.Add( key, clobFile );
                }
                catch ( Exception _e )
                {
                    Console.WriteLine( "A file with the name '" + key + "' already exists" + _e.Message );
                    //TODO: Something clever here
                }
                _clobNode.clobFiles.Add( clobFile );
            }
        }

        public bool UpdateDatabaseClob( ClobFile _clobFile )
        {
            //string mnemonic = Path.GetFileNameWithoutExtension( _clobFile.fileInfo.Name );
            OracleCommand command = this.connection.CreateCommand();
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

            this.AddFileDataToCommand( command, _clobFile, ct, useBlobColumn );

            try
            {
                int rowsAffected = command.ExecuteNonQuery();
                if ( rowsAffected != 1 )
                {
                    Console.WriteLine( "No rows were affected in the update." );
                    return false;
                }
            }
            catch ( Exception _e )
            {
                Console.WriteLine( "Error updating database: " + _e.Message );
                return false;
            }
            command.Dispose();
            return true;
        }

        private void AddFileDataToCommand( OracleCommand command, ClobFile _clobFile, ClobType _ct, bool _useBlobColumn )
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
                OracleDbType insertType = _ct.clobDataType == "clob" ? OracleDbType.Clob : OracleDbType.XmlType;
                command.Parameters.Add( "data", insertType, contents, ParameterDirection.Input );
            }
        }

        public bool InsertDatabaseClob( ClobFile _clobFile, string _dataType )
        {
            string mnemonic = Path.GetFileNameWithoutExtension( _clobFile.fileInfo.Name );
            OracleCommand command = this.connection.CreateCommand();
            OracleTransaction trans = this.connection.BeginTransaction();
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
                catch ( Exception _e )
                {
                    Console.WriteLine( "Error creating new clob: " + _e.Message );
                    return false;
                }
                command.Dispose();

                // Child table
                command = this.connection.CreateCommand();
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

            this.AddFileDataToCommand( command, _clobFile, ct, useBlobColumn );

            try
            {
                command.ExecuteNonQuery();
            }
            catch ( Exception _e )
            {
                // Discard the insert amde into the parent table
                trans.Rollback();
                Console.WriteLine( "Error creating new clob: " + _e.Message );
                return false;
            }
            command.Dispose();
            trans.Commit();
            _clobFile.status = ClobFile.STATUS.SYNCHRONISED;
            return true;
        }

        public string GetDatabaseClobData( ClobFile _clobFile )
        {
            //string mnemonic = Path.GetFileNameWithoutExtension( _clobFile.fileInfo.Name );
            OracleCommand command = this.connection.CreateCommand();

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
            catch ( Exception _e )
            {
                Console.WriteLine( "Error retrieving data: " + _e.Message );
                return null;
            }
            Console.WriteLine( "No data found" );
            return null;
        }
    }
}
