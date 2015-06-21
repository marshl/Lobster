using Oracle.DataAccess.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace Lobster
{
    public class LobsterModel
    {
        public DatabaseConfig dbConfig;
        public OracleConnection oracleCon;

        public List<ClobDirectory> clobDirectories;
        
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
            this.oracleCon = new OracleConnection();
            this.oracleCon.ConnectionString = "User Id=" + this.dbConfig.username
                + ";Password=" + this.dbConfig.password
                + ";Data Source=" + this.dbConfig.dataSource;
            try
            {
                this.oracleCon.Open();
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
                _clobDirectory.filenameClobMap.Add( fileInfo.Name, clobFile );
                _clobNode.clobFiles.Add( clobFile );
            }
        }
    }
}
