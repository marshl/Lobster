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
            // Find the column that is used for storing the clob data that can store the mime type of this file
            ClobType.Column col = this.table.columns.Find(
                x => x.purpose == ClobType.Column.Purpose.CLOB_DATA
                    && ( this.mimeType == null || x.mimeTypes.Contains( this.mimeType ) ) );

            if ( col == null )
            {
                throw new MimeTypeNotFoundException( this );
            }

            return col;
        }
    }

    public class MimeTypeNotFoundException : Exception
    {
        public MimeTypeNotFoundException( DBClobFile _dbClobFile )
            : base ( String.Format( "The mime type {0} of file {1} could not be found the table {2}",
                _dbClobFile.filename, _dbClobFile.mimeType, _dbClobFile.table.FullName ) )
        {
            
        }
    }
}
