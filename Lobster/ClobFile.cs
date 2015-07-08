using System.IO;
using System.Threading;

namespace Lobster
{
    public class ClobFile
    {
        public ClobFile( FileInfo _fileInfo, ClobNode _parentClobNode, ClobDirectory _parentClobDirectory )
        {
            this.fileInfo = _fileInfo;
            this.parentClobNode = _parentClobNode;
            this.parentClobDirectory = _parentClobDirectory;
        }
        public FileInfo fileInfo;
        public ClobNode parentClobNode;

        public ClobDirectory parentClobDirectory;

       

        public STATUS? status;
        public string databaseMnemonic;
        public string componentType;
        public bool awaitingUpdate = false;

        public enum STATUS
        {
            SYNCHRONISED,
            LOCAL_ONLY,
        }

        
    }
}
