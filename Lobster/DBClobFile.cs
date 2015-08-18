using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lobster
{
    public class DBClobFile
    {
        public string mnemonic;
        public string mimeType;
        public string filename;

        public ClobType.Table table;

        public ClobType.Column GetColumn()
        {
            return this.table.columns.Find(
                x => x.purpose == ClobType.Column.Purpose.CLOB_DATA
                    && ( this.mimeType == null || x.mimeTypes.Contains( this.mimeType ) ) );
        }

    }
}
