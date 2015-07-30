using System;
using System.Collections.Generic;

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

        // Optional Columns
        public string dateColumnName;
        public string idColumnName;

        public string clobDataType;

        // Optional Parent Table Information
        public bool hasParentTable;
        public string parentTable;
        public string parentIDColumn;
        public string parentMnemonicColumn;

        public string componentTypeColumn;
        public List<string> componentTypes;

        public string blobColumnName;
        public List<string> blobColumnTypes;
    }
}
