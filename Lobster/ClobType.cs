using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Design;
using System.Globalization;
using System.Windows.Forms.Design;
using System.Xml.Serialization;

namespace Lobster
{
    [XmlType( TypeName = "clobtype" )]
    public class ClobType
    {
        [DisplayName( "Name" )]
        [Description( "The display name" )]
        [XmlElement( ElementName = "name" )]
        public string name { get; set; }

        [DisplayName( "Directory" )]
        [Description( "The name of the directory in CodeSource to be used for this ClobType. Directory separators can be used." )]
        [XmlElement( ElementName = "directory" )]
        public string directory { get; set; }

        [DisplayName( "Include Subdirectories" )]
        [Description( "Whether or not all subdirectories under the specified folder should also be used." )]
        [XmlElement( ElementName = "includeSubDirectories")]
        public bool includeSubDirectories { get; set; }

        [XmlElement( ElementName = "enabled" )]
        public bool enabled = true;

        public List<Table> tables;

        public ClobType()
        {

        }

        public ClobType( ClobType _other )
        {
            this.name = _other.name;
            this.directory = _other.directory;
            this.includeSubDirectories = _other.includeSubDirectories;
            this.enabled = _other.enabled;

            this.tables = new List<Table>();
            foreach ( Table table in _other.tables )
            {
                this.tables.Add( new Table( table ) );
            }
            this.tables.ForEach( x => x.LinkColumns() );
        }

        public void Initialise()
        {
            this.tables.ForEach( x => x.LinkColumns() );
        }

        [XmlType( TypeName = "table" )]
        public class Table
        {
            public string schema;
            public string name;
            public List<Column> columns;

            public Table parentTable;

            public Table()
            {

            }

            public Table( Table _other )
            {
                this.schema = _other.schema;
                this.name = _other.name;

                if ( _other.parentTable != null )
                {
                    this.parentTable = new Table( _other.parentTable );
                }

                this.columns = new List<Column>();
                foreach ( Column column in _other.columns )
                {
                    this.columns.Add( new Column( column ) );
                }
            }

            public string FullName { get { return this.schema + "." + this.name; } }

            public void LinkColumns()
            {
                foreach ( Column c in this.columns )
                {
                    c.parent = this;
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
        }

        [XmlType( TypeName ="column")]
        public class Column
        {
            [XmlElement( ElementName = "name" )]
            public string name;
            [XmlElement( ElementName = "sequence" )]
            public string sequence;
            [XmlElement( ElementName = "purpose" )]
            public Purpose purpose;
            [XmlElement( ElementName = "dataType" )]
            public Datatype? dataType = null;
            
            public List<string> mimeTypes;

            [XmlIgnore]
            public Table parent;

            public Column()
            {

            }

            public Column( Column _other )
            {
                this.name = _other.name;
                this.sequence = _other.sequence;
                this.purpose = _other.purpose;
                this.dataType = _other.dataType;
                this.mimeTypes = new List<string>( _other.mimeTypes );
            }

            public string FullName { get { return parent.FullName + "." + this.name; } }

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
                return this.mimeTypes != null;
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
