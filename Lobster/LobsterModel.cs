using Oracle.DataAccess.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Lobster
{
    public class LobsterModel
    {
        public DatabaseConfig dbConfig;
        public OracleConnection oracleCon;

        public List<ClobDirectory> clobDirectories;

        public void LoadDatabaseConfig()
        {
            this.dbConfig = new DatabaseConfig();
            XmlSerializer xmls = new XmlSerializer( typeof( DatabaseConfig ) );
            StreamReader streamReader = new StreamReader( Program.SETTINGS_DIR + "/" + Program.DB_CONFIG_FILE );
            XmlReader xmlReader = XmlReader.Create( streamReader );
            this.dbConfig = (DatabaseConfig)xmls.Deserialize( xmlReader );
        }

        public bool OpenConnection()
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

        public void CompareToDatabase()
        {
            foreach ( ClobDirectory clobDir in this.clobDirectories )
            {
                clobDir.CompareToDatabase();
            }
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

                ClobDirectory clobDirectory = new ClobDirectory();
                clobDirectory.clobType = clobType;
                clobDirectory.parentModel = this;
                clobDirectory.LoadFiles();

                this.clobDirectories.Add( clobDirectory );
            }
        }
    }
}
