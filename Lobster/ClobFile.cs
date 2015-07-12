using System.IO;
using System.Threading;

namespace Lobster
{
    public class ClobFile
    {
        public ClobFile( ClobDirectory _parentClobDirectory )
        {
            this.parentClobDirectory = _parentClobDirectory;
        }
       
        public ClobDirectory parentClobDirectory;

        public DBClobFile dbClobFile;
        public LocalClobFile localClobFile;

        public bool awaitingUpdate = false;
    }
}
