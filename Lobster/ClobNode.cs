using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lobster
{
    public class ClobNode
    {
        public ClobNode( DirectoryInfo _dirInfo )
        {
            this.dirInfo = _dirInfo;
        }

        public DirectoryInfo dirInfo;

        public List<ClobNode> subDirs = new List<ClobNode>();
        public List<ClobFile> clobFiles = new List<ClobFile>();
    }
}
