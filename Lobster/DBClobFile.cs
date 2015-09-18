using System;

namespace Lobster
{
    /// <summary>
    /// But most of the folk of the old Shire regarded the Bucklanders as peculiar,
    /// half foreigners as it were.
    /// 
    /// [ _The Lord of the Rings_, I/v: "A Conspiracy Unmasked"]
    /// </summary>
    public class DBClobFile
    {
        public string mnemonic;
        public string mimeType;
        public string filename;

        public Table table;

        public Column GetColumn()
        {
            // Find the column that is used for storing the clob data that can store the mime type of this file
            Column col = this.table.columns.Find(
                x => x.ColumnPurpose == Column.Purpose.CLOB_DATA
                    && ( this.mimeType == null || x.MimeTypeList.Contains( this.mimeType ) ) );

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
