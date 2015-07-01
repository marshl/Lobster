using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;
using System;
using System.Text;
using System.Linq;
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
        public List<ClobDirectory> clobDirectories;
        public List<FileInfo> tempFileList = new List<FileInfo>();

        public LobsterModel()
        {
            this.LoadDatabaseConfig();
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

        private OracleConnection OpenConnection()
        {
            OracleConnection con = new OracleConnection();
            con.ConnectionString = "User Id=" + this.dbConfig.username
                + ";Password=" + this.dbConfig.password
                + ";Data Source=" + this.dbConfig.dataSource;
            try
            {
                con.Open();
            }
            catch ( Exception _e )
            {
                Console.WriteLine( "Connection error: " + _e.Message );
                return null;
            }
            Console.WriteLine( "Connection successful" );
            return con;
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
                this.CompareDirectoryToDatabase( clobDir );
                this.clobDirectories.Add( clobDir );
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
                _clobNode.subDirs.Add( childNode );
            }

            foreach ( FileInfo fileInfo in _clobNode.dirInfo.GetFiles() )
            {
                ClobFile clobFile = new ClobFile( fileInfo, _clobNode, _clobDirectory );
                _clobDirectory.fullpathClobMap.Add( fileInfo.FullName, clobFile );
                string key = Path.GetFileNameWithoutExtension( fileInfo.Name );
                try
                {
                    _clobNode.clobFileMap.Add( key, clobFile );
                }
                catch ( Exception _e )
                {
                    Console.WriteLine( "A file with the name '" + key + "' already exists" + _e.Message );
                    //TODO: Something clever here
                }
            }
        }

        public bool UpdateDatabaseClob( ClobFile _clobFile )
        {
            //string mnemonic = Path.GetFileNameWithoutExtension( _clobFile.fileInfo.Name );
            OracleConnection con = this.OpenConnection();
            OracleCommand command = con.CreateCommand();
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
                    con.Close();
                    return false;
                }
            }
            catch ( Exception _e )
            {
                con.Close();
                Console.WriteLine( "Error updating database: " + _e.Message );
                return false;
            }
            command.Dispose();
            con.Close();
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
            OracleConnection con = this.OpenConnection();
            OracleCommand command = con.CreateCommand();
            OracleTransaction trans = con.BeginTransaction();
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
                    con.Close();
                    return false;
                }
                command.Dispose();

                // Child table
                command = con.CreateCommand();
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
                con.Close();
                return false;
            }
            command.Dispose();
            trans.Commit();
            con.Close();
            _clobFile.status = ClobFile.STATUS.SYNCHRONISED;
            return true;
        }

        public string GetDatabaseClobData( ClobFile _clobFile )
        {
            OracleConnection con = this.OpenConnection();
            OracleCommand command = con.CreateCommand();

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
                con.Close();
                return null;
            }
            Console.WriteLine( "No data found" );
            con.Close();
            return null;
        }

        public void CompareDirectoryToDatabase( ClobDirectory _clobDir )
        {
            OracleConnection con = this.OpenConnection();
            OracleCommand command = con.CreateCommand();

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
            catch ( Exception _e )
            {
                Console.WriteLine( "Error comparing to database: " + _e.Message );
                con.Close();
                return;
            }

            // Assume the file is local only
            foreach ( KeyValuePair<string, ClobFile> pair in _clobDir.fullpathClobMap )
            {
                pair.Value.status = ClobFile.STATUS.LOCAL_ONLY;
            }

            while ( reader.Read() )
            {
                string mnemonic = reader.GetString( 0 );

                string internalMnemonic = null;
                if ( mnemonic.Contains( '/' ) )
                {
                    internalMnemonic = mnemonic.Substring( mnemonic.LastIndexOf( '/' ) + 1 );
                }

                // Any files found on the database are "synchronised" 
                ClobFile clobFile = _clobDir.rootClobNode.FindFileWithName( internalMnemonic ?? mnemonic );
                if ( clobFile != null )
                {
                    clobFile.status = ClobFile.STATUS.SYNCHRONISED;
                    clobFile.databaseMnemonic = mnemonic;
                    clobFile.databaseType = reader.FieldCount > 1 ? reader.GetString( 1 ) : null;
                }
                else
                {
                    Console.WriteLine( "No file found for " + internalMnemonic + " / " + mnemonic );
                }
            }
            command.Dispose();
            con.Close();
        }
    }
}
