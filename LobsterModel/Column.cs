//-----------------------------------------------------------------------
// <copyright file="Column.cs" company="marshl">
// Copyright 2016, Liam Marshall, marshl.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//-----------------------------------------------------------------------
//
//       ... where the walls were nearest could
//      dimly be seen coats of mail, helms and axes, swords and spears hanging; and there in rows
//      stood great jars and vessels filled with a wealth that could not be guessed.
//      
//      [ _The Hobbit_, XII "Inside Information" ] 
//
//-----------------------------------------------------------------------
namespace LobsterModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Xml.Serialization;

    /// <summary>
    /// A ClobType table contains a number of columns that are required to use the table. Each column has a different purpose describing how it affects the table.
    /// </summary>
    public class Column : ICloneable
    {
        /// <summary>
        /// The purpose defines how the column will be used by Lobster.
        /// </summary>
        public enum Purpose
        {
            /// <summary>
            /// An ID column is a number value column that has a new value inserted in when a new file is added.
            /// If the column has a sequence defined, then that sequence will be incremented.
            /// Otherwise the highest value + 1 of that column will be used.
            /// </summary>
            ID,

            /// <summary>
            /// A clob data column is where the file data is stored. A table usually has only one such column, 
            /// but it can have more, with the MimeTypeList defining which column it should be put into.
            /// </summary>
            CLOB_DATA,

            /// <summary>
            /// The mnemonic column is what matches a filename to the row in the table.
            /// The mnemonic is appended with the mime type extension before being compared with the file system.
            /// </summary>
            MNEMONIC,

            /// <summary>
            /// A datetime column is automatically set to SYSDATE when the file is created or updated.
            /// This ensures new files can be added to tables which have a NOT NULL date column.
            /// </summary>
            DATETIME,

            /// <summary>
            /// A Foreign Key column is used to match a child table to it's parent, with the child.foreign_key equal to the parent.id
            /// </summary>
            FOREIGN_KEY,

            /// <summary>
            /// A MimeType column is used to store the MimeType of the file that is stored in the CLOB_DATA column.
            /// The MimeType is converted to a file extension and added to the MNEMONIC column to compare it against the file system.
            /// </summary>
            MIME_TYPE,

            /// <summary>
            /// A special case for Work Request Types, Full Name uses the InitCapped mnemonic on insert
            /// </summary>
            FULL_NAME,
        }

        /// <summary>
        /// The DataType defines the oracle type that the column is stored as for CLOB_DATA purpose columns.
        /// </summary>
        public enum Datatype
        {
            /// <summary>
            /// Character Large OBject (.css, .js etc.)
            /// </summary>
            CLOB,

            /// <summary>
            /// Binary Large OBject (.png, .tif, .wav, etc.)
            /// </summary>
            BLOB,

            /// <summary>
            /// Xml (.xml)
            /// </summary>
            XMLTYPE,
        }

        /// <summary>
        /// Gets or sets the name of this column as it appears in the database.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the name of the sequence that this columns will call NEXTVAL on if this column has the ID purpose.
        /// Sequence names should not be put on a column that has any other purpose.
        /// </summary>
        public string Sequence { get; set; }

        /// <summary>
        /// Gets or sets the purpose of the column is an enumeration that defines how the column will be used.
        /// When a column with a specific purpose is needed, it will be queried from the table column list.
        /// </summary>
        [XmlElement("Purpose")]
        public Purpose ColumnPurpose { get; set; }

        /// <summary>
        /// Gets or sets the Oracle data type that the column on the database uses. 
        /// The DataType of a column should only be used if it has the CLOB_DATA purpose.
        /// A table can have multiple CLOB_DATA columns, but each must have a differnt DataType.
        /// </summary>
        public Datatype? DataType { get; set; }

        /// <summary>
        /// Gets or sets the list of mime types that this column will accept.
        /// If a table with multiple CLOB_DATA columns has a new file inserted, then the column with the matching data type will be used.
        /// Information on the Editor attribute can be found here: http://stackoverflow.com/questions/6307006/how-can-i-use-a-winforms-propertygrid-to-edit-a-list-of-strings
        /// </summary>
        [XmlArray("MimeTypes")]
        public List<string> MimeTypeList { get; set; }

        /// <summary>
        /// Gets or sets the parent Table of this column.
        /// </summary>
        [XmlIgnore]
        public Table ParentTable { get; set; }

        /// <summary>
        /// Gets the complete name of this column, including the name of the parent table, and the schema the table is in.
        /// </summary>
        [XmlIgnore]
        public string FullName
        {
            get
            {
                return $"{this.ParentTable.FullName}.{this.Name}";
            }
        }

        /// <summary>
        /// Gets an SQL string that returns the next ID for this column.
        /// If this column has a sequence, it will return {sequence}.NEXTVAL
        /// Otherwise it will return a query that will find the highest value of the column + 1
        /// </summary>
        [Browsable(false)]
        public string NextID
        {
            get
            {
                Debug.Assert(this.ColumnPurpose == Purpose.ID, "NextID should only be called on a column with the ID purpose");

                return this.Sequence != null ? $"{this.ParentTable.Schema}.{this.Sequence}.NEXTVAL"
                  : $"( SELECT NVL( MAX( x.{this.Name} ), 0 ) + 1 FROM {this.ParentTable.FullName} x )";
            }
        }

        /// <summary>
        /// Returns the string representation of this Column (used for display in a PropertyGrid).
        /// </summary>
        /// <returns>The string representation of this Column.</returns>
        public override string ToString()
        {
            return this.Name;
        }

        /// <summary>
        /// Creates a deep copy of this Column
        /// </summary>
        /// <returns>A deep copy of this Column.</returns>
        public object Clone()
        {
            Column copy = new Column();
            copy.Name = this.Name;
            copy.Sequence = this.Sequence;
            copy.ColumnPurpose = this.ColumnPurpose;
            copy.DataType = this.DataType;
            copy.MimeTypeList = new List<string>(this.MimeTypeList);
            return copy;
        }

        /// <summary>
        /// Prevents the datatype field from being serialised if it is null.
        /// Source: http://stackoverflow.com/questions/1296468/suppress-null-value-types-from-being-emitted-by-xmlserializer
        /// </summary>
        /// <returns>Whether the DataType field should be serialised.</returns>
        public bool ShouldSerializeDataType()
        {
            return this.DataType != null;
        }

        /// <summary>
        /// Prevents the MimeTypes field from being serialised if it is null.
        /// Source: http://stackoverflow.com/questions/1296468/suppress-null-value-types-from-being-emitted-by-xmlserializer
        /// </summary>
        /// <returns>Whether the MimeType field should be serialised.</returns>
        public bool ShouldSerializeMimeTypeList()
        {
            return this.MimeTypeList != null && this.MimeTypeList.Count > 0;
        }
    }
}
