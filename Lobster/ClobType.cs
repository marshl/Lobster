using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Design;
using System.Globalization;
using System.IO;
using System.Windows.Forms.Design;
using System.Xml.Serialization;

namespace Lobster
{
    [XmlType( TypeName = "clobtype" )]
    public class ClobType : ICloneable
    {
        [DisplayName( "Name" )]
        [Description( "The display name" )]
        public string name { get; set; }

        [DisplayName( "Directory" )]
        [Description( "The name of the directory in CodeSource to be used for this ClobType. Directory separators can be used." )]
        public string directory { get; set; }

        [DisplayName( "Include Subdirectories" )]
        [Description( "Whether or not all subdirectories under the specified folder should also be used." )]
        public bool includeSubDirectories { get; set; }

        [XmlIgnore]
        public string fileLocation;

        [DisplayName( "Table List" )]
        [Description( "The tables used by this ClobType" )]
        public List<Table> tables { get; set; }

        public void Initialise()
        {
            this.tables.ForEach( x => x.LinkColumns() );
        }

        [DisplayName( "Table" )]
        [XmlType( TypeName = "table" )]
        public class Table : ICloneable
        {
            [DisplayName( "Schema/Owner Name" )]
            [Description( "The schema/owner of this table" )]
            public string schema { get; set; }

            [DisplayName( "Name" )]
            [Description( "The name of this table" )]
            public string name { get; set; }

            [DisplayName( "Column List" )]
            [Description( "The columns in this table" )]
            public List<Column> columns { get; set; }

            [DisplayName( "Parent Table" )]
            [Description( "The parent table if this table is in a parent/child relationship." )]
            public Table parentTable { get; set; }

            public string FullName { get { return this.schema + "." + this.name; } }

            public override string ToString()
            {
                return this.FullName;
            }

            public void LinkColumns()
            {
                if ( this.columns != null )
                {
                    foreach ( Column c in this.columns )
                    {
                        c.parent = this;
                    }
                }

                if ( this.parentTable != null )
                {
                    this.parentTable.LinkColumns();
                }
            }

            public string BuildUpdateStatement( ClobFile _clobFile )
            {
                Column clobCol = _clobFile.dbClobFile.GetColumn();
                Debug.Assert( clobCol != null );
                
                string command =
                       "UPDATE " + this.FullName
                     + " SET " + clobCol.FullName + " = :data";

                Column dateCol = this.columns.Find( x => x.purpose == Column.Purpose.DATETIME );
                if ( dateCol != null )
                {
                    command += ", " + dateCol.FullName + " = SYSDATE";
                }

                if ( this.parentTable != null )
                {
                    Table pt = this.parentTable;
                    Column fkCol = this.columns.Find( x => x.purpose == Column.Purpose.FOREIGN_KEY );
                    Column parentIDCol = pt.columns.Find( x => x.purpose == Column.Purpose.ID );
                    Column parentMnemCol = pt.columns.Find( x => x.purpose == Column.Purpose.MNEMONIC );

                    command += " WHERE " + fkCol.FullName + " = ("
                            + " SELECT " + parentIDCol.FullName
                            + " FROM " + pt.FullName
                            + " WHERE " + parentMnemCol.FullName + " = '" + _clobFile.dbClobFile.mnemonic + "')";
                }
                else
                {
                    Column mnemCol = this.columns.Find( x => x.purpose == Column.Purpose.MNEMONIC );
                    command += " WHERE " + mnemCol.FullName + " = '" + _clobFile.dbClobFile.mnemonic + "'";
                }
                return command;
            }

            public string BuildInsertParentStatement( string _mnemonic )
            {
                Debug.Assert( this.parentTable != null );
                Table pt = this.parentTable;
                Column idCol = pt.columns.Find( x => x.purpose == Column.Purpose.ID );
                Column mnemCol = pt.columns.Find( x => x.purpose == Column.Purpose.MNEMONIC );
                Debug.Assert( idCol != null );
                Debug.Assert( mnemCol != null );

                string command = "INSERT INTO " + pt.FullName
                    + " (" + idCol.FullName + ", " + mnemCol.FullName + " )"
                    + " VALUES( " + idCol.NextID + "), :mnemonic )";

                return command;
            }

            public string BuildInsertChildStatement( string _mnemonic, string _mimeType )
            {
                Column mnemCol = this.columns.Find( x => x.purpose == Column.Purpose.MNEMONIC );
                Debug.Assert( mnemCol != null );
                Column dateCol = this.columns.Find( x => x.purpose == Column.Purpose.DATETIME );
                Column idCol = this.columns.Find( x => x.purpose == Column.Purpose.ID );
                Column dataCol = this.columns.Find( x => x.purpose == Column.Purpose.CLOB_DATA
                    && ( _mimeType == null || x.mimeTypes.Contains( _mimeType ) ) );
                Column fullNameCol = this.columns.Find( x => x.purpose == Column.Purpose.FULL_NAME );

                // Make a string for the column names...
                string insertCommand = "INSERT INTO " + this.FullName + " ( "
                    + mnemCol.FullName;

                // ..and another for the values, which will concatenated together
                string valueCommand = " VALUES ( '" + _mnemonic + "' ";

                if ( this.parentTable != null )
                {
                    Table pt = this.parentTable;
                    Column fkCol = this.columns.Find( x => x.purpose == Column.Purpose.FOREIGN_KEY );
                    Column parentIDCol = pt.columns.Find( x => x.purpose == Column.Purpose.ID );
                    Column parentMnemCol = pt.columns.Find( x => x.purpose == Column.Purpose.MNEMONIC );

                    insertCommand += ", " + fkCol.FullName;

                    valueCommand += parentIDCol.NextID + ", "
                            + "( SELECT " + parentIDCol.FullName + " FROM " + pt.FullName
                            + " WHERE " + parentMnemCol.FullName + " = '" + _mnemonic + "' )";
                }
                else // No parent table
                {
                    if ( idCol != null )
                    {
                        insertCommand += ", " + idCol.FullName;
                        valueCommand += ", " + idCol.NextID;
                    }

                    if ( _mimeType != null )
                    {
                        Column mimeCol = this.columns.Find( x => x.purpose == Column.Purpose.MIME_TYPE );
                        Debug.Assert( mimeCol != null );
                        insertCommand += ", " + mimeCol.FullName;
                        valueCommand += ", " + _mimeType;
                    }
                }

                if ( dateCol != null )
                {
                    insertCommand += ", " + dateCol.FullName;
                    valueCommand += ", SYSDATE ";
                }

                if ( fullNameCol != null )
                {
                    TextInfo textInfo = new CultureInfo( "en-US", false ).TextInfo;
                    string fullName = textInfo.ToTitleCase( _mnemonic.ToLower() );
                    insertCommand += ", " + fullNameCol.FullName;
                    valueCommand += ", '" + fullName + "'";
                }

                insertCommand += ", " + dataCol.FullName + " )";
                valueCommand += ", :data ) ";
                return insertCommand + valueCommand;
            }

            public string BuildGetDataCommand( ClobFile _clobFile )
            {
                Column clobCol = _clobFile.dbClobFile.GetColumn();
                if ( this.parentTable != null )
                {
                    Table pt = this.parentTable;
                    Column fkCol = this.columns.Find( x => x.purpose == Column.Purpose.FOREIGN_KEY );
                    Column parentIDCol = pt.columns.Find( x => x.purpose == Column.Purpose.ID );
                    Column parentMnemCol = pt.columns.Find( x => x.purpose == Column.Purpose.MNEMONIC );
                    return
                        "SELECT " + clobCol.FullName
                        + " FROM " + pt.FullName
                        + " JOIN " + this.FullName
                        + " ON " + fkCol.FullName + " = " + parentIDCol.FullName
                        + " WHERE " + parentMnemCol.FullName + " = :mnemonic";
                }
                else
                {
                    Column mnemCol = this.columns.Find( x => x.purpose == Column.Purpose.MNEMONIC );
                    return
                        "SELECT " + clobCol.FullName
                        + " FROM " + this.FullName
                        + " WHERE " + mnemCol.FullName + " = :mnemonic";
                }
            }

            public string GetFileListCommand()
            {
                Column mimeCol = this.columns.Find( x => x.purpose == Column.Purpose.MIME_TYPE );

                if ( this.parentTable != null )
                {
                    Table pt = this.parentTable;
                    Column fkCol = this.columns.Find( x => x.purpose == Column.Purpose.FOREIGN_KEY );
                    Column parentIDCol = pt.columns.Find( x => x.purpose == Column.Purpose.ID );
                    Column parentMnemCol = pt.columns.Find( x => x.purpose == Column.Purpose.MNEMONIC );

                    return "SELECT " + parentMnemCol.FullName
                        + ( mimeCol != null ? ", " + mimeCol.FullName : null )
                        + " FROM " + pt.FullName
                        + " JOIN " + this.FullName
                        + " ON " + fkCol.FullName + " = " + parentIDCol.FullName;
                }
                else
                {
                    Column mnemCol = this.columns.Find( x => x.purpose == Column.Purpose.MNEMONIC );
                    return "SELECT " + mnemCol.FullName
                        + ( mimeCol != null ? ", " + mimeCol.FullName : null )
                        + " FROM " + this.FullName;
                }
            }

            public object Clone()
            {
                Table copy = new Table();
                copy.schema = this.schema;
                copy.name = this.name;

                if ( this.parentTable != null )
                {
                    copy.parentTable = (Table)this.parentTable.Clone();
                }

                if ( this.columns != null )
                {
                    copy.columns = new List<Column>();
                    foreach ( Column column in this.columns )
                    {
                        copy.columns.Add( (Column)column.Clone() );
                    }
                }
                return copy;
            }
        }

        public static void Serialise( string _fullpath, ClobType _clobType )
        {
            XmlSerializer xmls = new XmlSerializer( typeof( ClobType ) );
            using ( StreamWriter streamWriter = new StreamWriter( _fullpath ) )
            {
                xmls.Serialize( streamWriter, _clobType );
            }
        }

        public object Clone()
        {
            ClobType copy = new ClobType();
            copy.name = this.name;
            copy.directory = this.directory;
            copy.includeSubDirectories = this.includeSubDirectories;

            copy.tables = new List<Table>();
            if ( this.tables != null )
            {
                foreach ( Table table in this.tables )
                {
                    copy.tables.Add( (Table)table.Clone() );
                }
            }
            copy.tables.ForEach( x => x.LinkColumns() );
            copy.fileLocation = this.fileLocation;

            return copy;
        }

        [XmlType( TypeName ="column")]
        public class Column : ICloneable
        {
            [DisplayName( "Name" )]
            [Description( "The name of this column" )]
            //[XmlElement( ElementName = "name" )]
            public string name { get; set; }

            [DisplayName( "Sequence" )]
            [Description( "The name of the sequence for this column, if it exists." )]
            //[XmlElement( ElementName = "sequence" )]
            public string sequence { get; set; }

            [DisplayName( "Purpose" )]
            [Description( "How Lobster will use this column." )]
            //[XmlElement( ElementName = "purpose" )]
            public Purpose purpose { get; set; }

            [DisplayName( "Data Type" )]
            [Description( "The data type of this column if it has the Clob_Data purpose." )]
            //[XmlElement( ElementName = "dataType" )]
            public Datatype? dataType { get; set; }

            [DisplayName( "Mime Types" )]
            [Description( "The mime types that will be put into this column if it is a Clob_Data column" )]
            [Editor( @"System.Windows.Forms.Design.StringCollectionEditor," +
                "System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
               typeof( System.Drawing.Design.UITypeEditor ) )]
            public List<string> mimeTypes { get; set; }

            [XmlIgnore]
            public Table parent;

            public override string ToString()
            {
                return this.FullName;
            }

            public object Clone()
            {
                Column copy = new Column();
                copy.name = this.name;
                copy.sequence = this.sequence;
                copy.purpose = this.purpose;
                copy.dataType = this.dataType;
                copy.mimeTypes = new List<string>( this.mimeTypes );
                return copy;
            }

            public string FullName { get { return parent.FullName + "." + this.name; } }

            [Browsable( false )]
            public string NextID
            {
                get
                {
                    Debug.Assert( this.purpose == Purpose.ID );
                    return this.sequence != null ? this.parent.schema + "." + this.sequence + ".NEXTVAL"
                      : "( SELECT NVL( MAX( " + this.FullName + " ), 0 ) + 1 FROM " + this.parent.FullName + " )";
                }
            }

            /// <summary>
            /// Used to prevent <dataType xsi:nil="true" /> from appearing in the XML
            /// http://stackoverflow.com/questions/1296468/suppress-null-value-types-from-being-emitted-by-xmlserializer
            /// </summary>
            /// <returns></returns>
            public bool ShouldSerializedataType()
            {
                return this.dataType != null;
            }

            public bool ShouldSerializemimeTypes()
            {
                return this.mimeTypes != null && this.mimeTypes.Count > 0;
            }

            public enum Datatype
            {
                CLOB,
                BLOB,
                XMLTYPE,
            }
            
            public enum Purpose
            {
                ID,
                CLOB_DATA,
                MNEMONIC,
                DATETIME,
                FOREIGN_KEY,
                MIME_TYPE,
                /// <summary>
                /// A special case for Work Request Types, Full Name uses the InitCapped mnemonic on insert
                /// </summary>
                FULL_NAME,
            }
        }
    }
}
