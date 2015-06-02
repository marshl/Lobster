using Oracle.DataAccess.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Lobster
{
    public class LobsterModel
    {
        public DatabaseConfig dbConfig;
        public OracleConnection oracleCon;

        public List<ClobType> clobTypes;

        public void LoadDatabaseConfig()
        {
            this.dbConfig = new DatabaseConfig();
            XmlSerializer xmls = new XmlSerializer( typeof( DatabaseConfig ) );
            StreamReader streamReader = new StreamReader( Program.SETTINGS_DIR + "/" + Program.DB_CONFIG_FILE );
            XmlReader xmlReader = XmlReader.Create( streamReader );
            this.dbConfig = (DatabaseConfig)xmls.Deserialize( xmlReader );
        }

        public void OpenConnection()
        {
            this.oracleCon = new OracleConnection();
            //con.ConnectionString = "host=icondevdb;database=icondb01;uid=XVIEWMGR;pwd=password12";
            //con.ConnectionString = "User Id=XVIEWMGR;Password=password12;Data Source=ORAC";
            this.oracleCon.ConnectionString = "User Id=" + this.dbConfig.username
                + ";Password=" + this.dbConfig.password
                + ";Data Source=" + this.dbConfig.dataSource;
            this.oracleCon.Open();
        }

        public void LoadClobTypes()
        {
            this.clobTypes = new List<ClobType>();
            DirectoryInfo clobTypeDir = Directory.CreateDirectory( Program.SETTINGS_DIR + "/" + Program.CLOB_TYPE_DIR );
            foreach ( FileInfo file in clobTypeDir.GetFiles() )
            {
                ClobType clobType = new ClobType();

                XmlSerializer xmls = new XmlSerializer( typeof( ClobType ) );
                StreamReader streamReader = new StreamReader( file.FullName );
                XmlReader xmlReader = XmlReader.Create( streamReader );
                clobType = (ClobType)xmls.Deserialize( xmlReader );

                clobType.LoadFiles( this.dbConfig.codeSource );

                this.clobTypes.Add( clobType );
                clobType.parentModel = this;
            }
        }
    }
}
