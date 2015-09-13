using System;

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
                throw new ClobColumnNotFoundException( this );
            }

            return col;
        }
    }

    public class ClobColumnNotFoundException : Exception
    {
        public ClobColumnNotFoundException( DBClobFile _dbClobFile )
            : base ( "The clob column for file " + _dbClobFile.filename + " of mimetype " + _dbClobFile.mimeType + " could not be found the table " + _dbClobFile.table.FullName ) { }
    }
}
