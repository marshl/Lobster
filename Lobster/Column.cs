using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace Lobster
{
    [XmlType(TypeName = "column")]
    public class Column : ICloneable
    {
        [DisplayName("Name")]
        [Description("The name of this column")]
        public string name { get; set; }

        [DisplayName("Sequence")]
        [Description("The name of the sequence for this column, if it exists.")]
        public string sequence { get; set; }

        [DisplayName("Purpose")]
        [Description("How Lobster will use this column.")]
        public Purpose purpose { get; set; }

        [DisplayName("Data Type")]
        [Description("The data type of this column if it has the Clob_Data purpose.")]
        public Datatype? dataType { get; set; }

        /// <summary>
        /// Found here http://stackoverflow.com/questions/6307006/how-can-i-use-a-winforms-propertygrid-to-edit-a-list-of-strings
        /// </summary>
        [DisplayName("Mime Types")]
        [Description("The mime types that will be put into this column if it is a Clob_Data column")]
        [Editor(@"System.Windows.Forms.Design.StringCollectionEditor," +
            "System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
           typeof(System.Drawing.Design.UITypeEditor))]
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
            copy.mimeTypes = new List<string>(this.mimeTypes);
            return copy;
        }

        public string FullName { get { return parent.FullName + "." + this.name; } }

        [Browsable(false)]
        public string NextID
        {
            get
            {
                Debug.Assert(this.purpose == Purpose.ID);
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
