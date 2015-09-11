using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;
using System.Windows.Forms.Design;
using System.Drawing.Design;

namespace Lobster
{
    public class DatabaseConfig
    {
        [DisplayName( "Name" )]
        [Description( "The name of the connection. This is for display purposes only." )]
        public string name { get; set; }

        [DisplayName( "Host" )]
        [Description( "The host of the database." )]
        [Category( "Database" )]
        public string host { get; set; }

        [DisplayName( "Port" )]
        [Description( "The port the database is listening on. Usually 1521 for Oracle." )]
        [Category( "Database" )]
        public string port { get; set; }

        [DisplayName( "SID" )]
        [Description( "The Oracle System ID of the database." )]
        [Category( "Database" )]
        public string sid { get; set; }

        [DisplayName( "User Name" )]
        [Description( "The name of the user/schema to connect as. It is important to connect as a user with the privileges to access every table that could be modified by Lobster." )]
        [Category( "Database" )]
        public string username { get; set; }

        [DisplayName( "Password" )]
        [Description( "The password to connect with." )]
        [Category( "Database" )]
        public string password { get; set; }

        [DisplayName( "CodeSource Directory" )]
        [Description( "This is the location of the CodeSource directory that is used for this database. If it is invalid, Lobster will prompt you as it starts up." )]
        [Editor( typeof( FileNameEditor ), typeof( UITypeEditor ) )]
        [Category( "Directories" )]
        public string codeSource { get; set; }

        [DisplayName( "Pooling" )]
        [Description( "If pooling is enabled, when Lobster connects to the Oracle database Oracle will remember the connection for a time, and reuse it if the same computer connects using the same connection string." )]
        public bool usePooling { get; set; }

        [DisplayName( "ClobType Directory" )]
        [Description( "ClobTypes are Lobster specific Xml files for describing the different tables located on the database and the rules that govern them." )]
        [Editor( typeof( FileNameEditor ), typeof( UITypeEditor ) )]
        [Category( "Directories" )]
        public string clobTypeDir { get; set; }

        [XmlIgnore]
        public string fileLocation;

        public DatabaseConfig()
        {

        }

        public DatabaseConfig( DatabaseConfig _other )
        {
            this.name = _other.name;
            this.host = _other.host;
            this.sid = _other.sid;
            this.port = _other.port;
            this.username = _other.username;
            this.password = _other.password;
            this.codeSource = _other.codeSource;
            this.usePooling = _other.usePooling;
            this.clobTypeDir = _other.clobTypeDir;
            this.fileLocation = _other.fileLocation;
        }

        public static void Serialise( string _fullpath, DatabaseConfig _config )
        {
            XmlSerializer xmls = new XmlSerializer( typeof( DatabaseConfig ) );
            using ( StreamWriter streamWriter = new StreamWriter( _fullpath ) )
            {
                xmls.Serialize( streamWriter, _config );
            }
        }
    }
}
