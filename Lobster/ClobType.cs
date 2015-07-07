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
        public string dateColumnName; //Optional TODO: Set to SYSDATE on insert/update

        public string clobDataType;

        // All optional
        public bool hasParentTable;
        public string parentTable;
        public string parentIDColumn;
        public string parentMnemonicColumn;

        public string dataTypeColumnName;
        public List<string> dataTypes;

        public string blobColumnName;
        public List<string> blobColumnTypes;
    }
}
