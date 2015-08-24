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
    }
}
