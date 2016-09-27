//-----------------------------------------------------------------------
// <copyright file="Table.cs" company="marshl">
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
//      And Mr. Drogo was staying at Brandy Hall with his father-in-law, old Master Gorbadoc, as he
//      often did after his marriage (him being partial to his vittles, and old Gorbadoc keeping a mighty
//      generous table);
//          -- The Old Gaffer
//      [ _The Lord of the Rings_, I/i: "A Long Expected Party"]
//
//-----------------------------------------------------------------------
namespace LobsterModel
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;

    /// <summary>
    /// Rperesents a single SQL table in the database. Used to build commands for 
    /// inserting, updating and querying values from this table.
    /// </summary>
    public class Table
    {
        /// <summary>
        /// Gets or sets the schema/user that this table belongs to.
        /// </summary>
        public string Schema { get; set; }

        /// <summary>
        /// Gets or sets the name of this table in the database.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the extension that will be added to the mnemonic to create the guesstimate local file name. 
        /// If the default extension is not set, then .xml will be used
        /// </summary>
        public string DefaultExtension { get; set; }

        /// <summary>
        /// Gets or sets the columns in this table that Lobster needs.
        /// </summary>
        public List<Column> Columns { get; set; }

        /// <summary>
        /// Gets or sets the custom statements to override automated SQL construction.
        /// </summary>
        public CustomStatementBlock CustomStatements { get; set; }

        /// <summary>
        /// Gets or sets the parent table of this table, if this table is part of a parent-child relationship.
        /// </summary>
        public Table ParentTable { get; set; }

        /// <summary>
        /// Gets the full name of this table, including the schema to ensure queries are not ambiguous.
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
        /// Tries to find a Column with the specified purpose.
        /// </summary>
        /// <param name="purpose">The column purpose that will be searched for.</param>
        /// <param name="result">The <see cref="Column"/> column to be returned, if found.</param>
        /// <returns>True if a coolumn with the specified purpose was found, otherwise false.</returns>
        public bool TryGetColumnWithPurpose(Column.Purpose purpose, out Column result)
        {
            result = this.Columns.Find(x => x.ColumnPurpose == purpose);
            return result != null;
        }

        /// <summary>
        /// Gets the column with the specified purpose, or throws a <see cref="ColumnNotFoundException"/> if it cannot be found.
        /// </summary>
        /// <param name="purpose">The column purpose to search for.</param>
        /// <returns>The column in this column list that has the specified purpose.</returns>
        public Column GetColumnWithPurpose(Column.Purpose purpose)
        {
            Column c;
            if (!this.TryGetColumnWithPurpose(purpose, out c))
            {
                throw new ColumnNotFoundException(this, purpose);
            }

            return c;
        }

        /// <summary>
        /// The custom SQL statements used to override Lobster automated SQL functionality.
        /// </summary>
        public class CustomStatementBlock
        {
            /// <summary>
            /// Gets or sets the SQL statement used to insert or update a single file into the database.
            /// </summary>
            public string UpsertStatement { get; set; }

            /// <summary>
            /// Gets or sets the SQL statement used to get all the files in this table on the database.
            /// </summary>
            public string FileListStatement { get; set; }

            /// <summary>
            /// Gets or sets the SQL statement used to download a single file from the database.
            /// </summary>
            public string DownloadStatement { get; set; }
        }
    }
}
