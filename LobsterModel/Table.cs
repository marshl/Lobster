//-----------------------------------------------------------------------
// <copyright file="Table.cs" company="marshl">
// Copyright 2015, Liam Marshall, marshl.
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
//
//
//      And Mr. Drogo was staying at Brandy Hall with his father-in-law, old Master Gorbadoc, as he
//      often did after his marriage (him being partial to his vittles, and old Gorbadoc keeping a mighty
//      generous table);
//          -- The Old Gaffer
//      [ _The Lord of the Rings_, I/i: "A Long Expected Party"]
//
// </copyright>
//-----------------------------------------------------------------------
namespace LobsterModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Xml.Serialization;

    /// <summary>
    /// Rperesents a single SQL table in the database. Used to build commands for 
    /// inserting, updating and querying values from this table.
    /// </summary>
    [DisplayName("Table")]
    [XmlType("table")]
    public class Table : ICloneable
    {
        /// <summary>
        /// The schema/user that this table belongs to.
        /// </summary>
        [DisplayName("Schema/Owner Name")]
        [Description("The schema/owner of this table")]
        [XmlElement("schema")]
        public string Schema { get; set; }

        /// <summary>
        /// The name of this table in the database.
        /// </summary>
        [DisplayName("Name")]
        [Description("The name of this table")]
        [XmlElement("name")]
        public string Name { get; set; }

        /// <summary>
        /// The extension that will be added to the mnemonic to create the guesstimate local file name. 
        /// If the default extension is not set, then .xml will be used
        /// </summary>
        [DisplayName("Default Extension")]
        [Description("The file extension that will be used to compare mnemonics on the database to local files. The default is '.xml'")]
        [XmlElement("defaultExtension")]
        public string DefaultExtension { get; set; }

        /// <summary>
        /// The columns in this table that Lobster needs.
        /// </summary>
        [DisplayName("Column List")]
        [Description("The columns in this table")]
        [XmlArray("columns")]
        public List<Column> Columns { get; set; }

        /// <summary>
        /// The parent table of this table, if this table is part of a parent-child relationship.
        /// </summary>
        [DisplayName("Parent Table")]
        [Description("The parent table if this table is in a parent/child relationship.")]
        [XmlElement("parentTable")]
        public Table ParentTable { get; set; }

        /// <summary>
        /// The full name of this table, including the schema to ensure queries are not ambiguous.
        /// </summary>
        public string FullName
        {
            get
            {
                return this.Schema + "." + this.Name;
            }
        }

        /// <summary>
        /// Returns the string representation of this table.
        /// This needs to be overriden for display purposes in the property grid editor.
        /// </summary>
        /// <returns>The string representation of this Table.</returns>
        public override string ToString()
        {
            return this.FullName;
        }

        /// <summary>
        /// Connects all the columns in this table to this table.
        /// (This cannot be done as part of the deserialisation, so it must be done explicitly)
        /// </summary>
        public void Initialise()
        {
            if (this.Columns != null)
            {
                this.Columns.ForEach(x => x.ParentTable = this);
            }

            if (this.ParentTable != null)
            {
                this.ParentTable.Initialise();
            }
        }

        /// <summary>
        /// Prevents the DefaultExtension element from being serialised to file if it is null.
        /// </summary>
        /// <returns>Whether the default extension should be serialised.</returns>
        public bool ShouldSerializeDefaultExtension()
        {
            return this.DefaultExtension != null;
        }

        /// <summary>
        /// Constructs an SQL statement to update a row in the database
        /// </summary>
        /// <param name="clobFile">The ClobFile to build an update statement for.</param>
        /// <returns>The update statement.</returns>
        public string BuildUpdateStatement(DBClobFile clobFile)
        {
            Column clobCol = clobFile.GetColumn();
            string command =
                   "UPDATE " + this.FullName
                 + " SET " + clobCol.FullName + " = :data";

            Column dateCol = this.Columns.Find(x => x.ColumnPurpose == Column.Purpose.DATETIME);
            if (dateCol != null)
            {
                command += ", " + dateCol.FullName + " = SYSDATE";
            }

            if (this.ParentTable != null)
            {
                Table pt = this.ParentTable;
                Column foreignKeyCol = this.Columns.Find(x => x.ColumnPurpose == Column.Purpose.FOREIGN_KEY);
                Column parentIDCol = pt.Columns.Find(x => x.ColumnPurpose == Column.Purpose.ID);
                Column parentMnemCol = pt.Columns.Find(x => x.ColumnPurpose == Column.Purpose.MNEMONIC);

                command += " WHERE " + foreignKeyCol.FullName + " = ("
                        + " SELECT " + parentIDCol.FullName
                        + " FROM " + pt.FullName
                        + " WHERE " + parentMnemCol.FullName + " = '" + clobFile.Mnemonic + "')";
            }
            else
            {
                Column mnemCol = this.Columns.Find(x => x.ColumnPurpose == Column.Purpose.MNEMONIC);
                command += " WHERE " + mnemCol.FullName + " = '" + clobFile.Mnemonic + "'";
            }

            return command;
        }

        /// <summary>
        /// Constructs an SQL statement to insert a row into the parent table of this table with the given mnemonic.
        /// </summary>
        /// <param name="mnemonic">The mnemonic for the file to insert.</param>
        /// <returns>The insert statement.</returns>
        public string BuildInsertParentStatement(string mnemonic)
        {
            Debug.Assert(this.ParentTable != null, "Inserting into a parent table requires a parent to be defined");
            Table pt = this.ParentTable;

            Column idCol = pt.Columns.Find(x => x.ColumnPurpose == Column.Purpose.ID);
            Debug.Assert(idCol != null, "Inserting into a parent table requires the parent to have an ID column");

            Column mnemCol = pt.Columns.Find(x => x.ColumnPurpose == Column.Purpose.MNEMONIC);
            Debug.Assert(mnemCol != null, "Inserting into a parent table requires the parent to have a mnemonic column");

            string command = "INSERT INTO " + pt.FullName
                + " (" + idCol.FullName + ", " + mnemCol.FullName + " )"
                + " VALUES( " + idCol.NextID + "), '" + mnemonic + "' )";

            return command;
        }

        /// <summary>
        /// Constructs an SQL statement to insert a file into the database.
        /// If this table has a parent, then the parent information will be applied. 
        /// If not, the wile will be inserted normally.
        /// </summary>
        /// <param name="mnemonic">The mnemonic for the file to add.</param>
        /// <param name="mimeType">The mime type of the file to add, if applicable.</param>
        /// <returns>The insert SQl statement.</returns>
        public string BuildInsertChildStatement(string mnemonic, string mimeType)
        {
            Column mnemCol = this.Columns.Find(x => x.ColumnPurpose == Column.Purpose.MNEMONIC);
            Debug.Assert(mnemCol != null, "The mnemonic column must exist to insert as a child table");

            // Make a string for the column names...
            string insertCommand = "INSERT INTO " + this.FullName + " ( "
                + mnemCol.FullName;

            // ..and another for the values. The insertCommand and valueCommand are joined to form the full statement
            string valueCommand = " VALUES ( '" + mnemonic + "' ";

            if (this.ParentTable != null)
            {
                Table pt = this.ParentTable;
                Column foreignKeyCol = this.Columns.Find(x => x.ColumnPurpose == Column.Purpose.FOREIGN_KEY);
                Column parentIDCol = pt.Columns.Find(x => x.ColumnPurpose == Column.Purpose.ID);
                Column parentMnemCol = pt.Columns.Find(x => x.ColumnPurpose == Column.Purpose.MNEMONIC);

                insertCommand += ", " + foreignKeyCol.FullName;

                valueCommand += parentIDCol.NextID + ", "
                        + "( SELECT " + parentIDCol.FullName + " FROM " + pt.FullName
                        + " WHERE " + parentMnemCol.FullName + " = '" + mnemonic + "' )";
            }
            else
            {
                // No parent table
                Column idCol = this.Columns.Find(x => x.ColumnPurpose == Column.Purpose.ID);
                if (idCol != null)
                {
                    insertCommand += ", " + idCol.FullName;
                    valueCommand += ", " + idCol.NextID;
                }

                if (mimeType != null)
                {
                    Column mimeCol = this.Columns.Find(x => x.ColumnPurpose == Column.Purpose.MIME_TYPE);
                    Debug.Assert(mimeCol != null, "If a mime type is given, the table must have a mime type column");
                    insertCommand += ", " + mimeCol.FullName;
                    valueCommand += ", '" + mimeType + "'";
                }
            }

            // The date column is optional
            Column dateCol = this.Columns.Find(x => x.ColumnPurpose == Column.Purpose.DATETIME);
            if (dateCol != null)
            {
                insertCommand += ", " + dateCol.FullName;
                valueCommand += ", SYSDATE ";
            }

            // The full name column is optional. It is set to the PascalCase of the mnemonic
            Column fullNameCol = this.Columns.Find(x => x.ColumnPurpose == Column.Purpose.FULL_NAME);
            if (fullNameCol != null)
            {
                TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                string fullName = textInfo.ToTitleCase(mnemonic.ToLower());
                insertCommand += ", " + fullNameCol.FullName;
                valueCommand += ", '" + fullName + "'";
            }

            Column dataCol = this.Columns.Find(x => x.ColumnPurpose == Column.Purpose.CLOB_DATA
              && (mimeType == null || x.MimeTypeList.Contains(mimeType)));

            insertCommand += ", " + dataCol.FullName + " )";
            valueCommand += ", :data ) ";
            return insertCommand + valueCommand;
        }

        /// <summary>
        /// Constructs an SQL statement to get the 
        /// </summary>
        /// <param name="clobFile">The ClobFile to build the statement for.</param>
        /// <returns>The SQL command</returns>
        public string BuildGetDataCommand(DBClobFile clobFile)
        {
            Column clobCol = clobFile.GetColumn();
            if (this.ParentTable != null)
            {
                Table pt = this.ParentTable;
                Column foreignKeyCol = this.Columns.Find(x => x.ColumnPurpose == Column.Purpose.FOREIGN_KEY);
                Column parentIDCol = pt.Columns.Find(x => x.ColumnPurpose == Column.Purpose.ID);
                Column parentMnemCol = pt.Columns.Find(x => x.ColumnPurpose == Column.Purpose.MNEMONIC);
                return
                    "SELECT " + clobCol.FullName
                    + " FROM " + pt.FullName
                    + " JOIN " + this.FullName
                    + " ON " + foreignKeyCol.FullName + " = " + parentIDCol.FullName
                    + " WHERE " + parentMnemCol.FullName + " = '" + clobFile.Mnemonic + "'";
            }
            else
            {
                Column mnemCol = this.Columns.Find(x => x.ColumnPurpose == Column.Purpose.MNEMONIC);
                return
                    "SELECT " + clobCol.FullName
                    + " FROM " + this.FullName
                    + " WHERE " + mnemCol.FullName + " = '" + clobFile.Mnemonic + "'";
            }
        }

        /// <summary>
        /// Constructs an SQL query to retrieve the list of files in this table on the database.
        /// If this table has a mime type column, then the value of that column will be included in the query.
        /// </summary>
        /// <returns>The SQL command the retrive the file lists for this table.</returns>
        public string GetFileListCommand()
        {
            Column mimeCol = this.Columns.Find(x => x.ColumnPurpose == Column.Purpose.MIME_TYPE);

            if (this.ParentTable != null)
            {
                Table pt = this.ParentTable;
                Column foreignKeyCol = this.Columns.Find(x => x.ColumnPurpose == Column.Purpose.FOREIGN_KEY);
                Column parentIDCol = pt.Columns.Find(x => x.ColumnPurpose == Column.Purpose.ID);
                Column parentMnemCol = pt.Columns.Find(x => x.ColumnPurpose == Column.Purpose.MNEMONIC);

                return "SELECT " + parentMnemCol.FullName
                    + (mimeCol != null ? ", " + mimeCol.FullName : null)
                    + " FROM " + pt.FullName
                    + " JOIN " + this.FullName
                    + " ON " + foreignKeyCol.FullName + " = " + parentIDCol.FullName;
            }
            else
            {
                Column mnemCol = this.Columns.Find(x => x.ColumnPurpose == Column.Purpose.MNEMONIC);
                return "SELECT " + mnemCol.FullName
                    + (mimeCol != null ? ", " + mimeCol.FullName : null)
                    + " FROM " + this.FullName;
            }
        }

        /// <summary>
        /// Creates a deep copy of this table and all columns beaneath it.
        /// </summary>
        /// <returns>The copy of the table.</returns>
        public object Clone()
        {
            Table copy = new Table();
            copy.Schema = this.Schema;
            copy.Name = this.Name;

            if (this.ParentTable != null)
            {
                copy.ParentTable = (Table)this.ParentTable.Clone();
            }

            if (this.Columns != null)
            {
                copy.Columns = new List<Column>();
                this.Columns.ForEach(x => copy.Columns.Add((Column)x.Clone()));
            }

            return copy;
        }
    }
}
