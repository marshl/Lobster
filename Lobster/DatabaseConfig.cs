using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace Lobster
{
    public class DatabaseConfig
    {
        public string name;
        public string host;
        public string port;
        public string sid;
        public string username;
        public string password;
        public string codeSource;
        public bool usePooling;
        public string clobTypeDir;

        [XmlIgnore]
        public string fileLocation;

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
