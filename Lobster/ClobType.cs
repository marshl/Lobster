using Oracle.DataAccess.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace Lobster
{
    [Serializable]
    public class ClobType
    {
        public string name;
        public string directory;
        public string schema;
        public string table;
        public string mnemonicColumn;
        public string clobColumn;
        public string dateColumnName; //Optional

        // All optional
        public bool hasParentTable;
        public string parentTable;
        public string parentIDColumn;
        public string parentMnemonicColumn;

    }
}
