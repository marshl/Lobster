﻿//-----------------------------------------------------------------------
// <copyright file="DBClobFile.cs" company="marshl">
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
//      But most of the folk of the old Shire regarded the Bucklanders as peculiar,
//      half foreigners as it were.
// 
//      [ _The Lord of the Rings_, I/v: "A Conspiracy Unmasked"]
//
//-----------------------------------------------------------------------

namespace LobsterModel
{
    using System;

    /// <summary>
    /// A DBClobFile describes a row in a table on the database used by a ClobType.
    /// </summary>
    [Obsolete]
    public class DBClobFile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DBClobFile"/> class.
        /// </summary>
        /// <param name="parentTable">The table that contains this clob file</param>
        /// <param name="mnemonic">The database mnemonic of this file.</param>
        /// <param name="mimetype">The mime type of this file, if applicable.</param>
        /// <param name="filename">A guesstimated filename, that may not match any local files.</param>
        public DBClobFile(Table parentTable, string mnemonic, string mimetype, string filename)
        {
            this.ParentTable = parentTable;
            this.Mnemonic = mnemonic;
            this.MimeType = mimetype;
            this.Filename = filename;
        }

        /// <summary>
        /// Gets the mnemonic that came from the mnemonic column. 
        /// </summary>
        public string Mnemonic { get; }

        /// <summary>
        /// Gets the mime type stored against this file, if it exists.
        /// </summary>
        public string MimeType { get; }

        /// <summary>
        /// Gets the name of file this DBClobFile should match to.
        /// The mnemonic and mimetype is processed to get this value.
        /// </summary>
        public string Filename { get; }

        /// <summary>
        /// Gets the table that this file was pulled from.
        /// </summary>
        public Table ParentTable { get; }

        /// <summary>
        /// Gets or sets the last time that this file was updated.
        /// </summary>
        public DateTime LastUpdatedTime { get; set; } = DateTime.MinValue;

        /// <summary>
        /// Returns the column that data from this row should be pulled from, depending on the mnemonic.
        /// </summary>
        /// <returns>The column, if it exists</returns>
        /// <exception cref="ColumnNotFoundException">Thrown when a column that matches the mime type isn't found.</exception>
        public Column GetDataColumn()
        {
            // Find the column that is used for storing the clob data that can store the mime type of this file
            Column col = this.ParentTable.Columns.Find(
                x => x.ColumnPurpose == Column.Purpose.CLOB_DATA
                    && (this.MimeType == null || x.MimeTypeList.Contains(this.MimeType)));

            if (col == null)
            {
                throw new ColumnNotFoundException(this.ParentTable, Column.Purpose.CLOB_DATA, this.MimeType, this.Filename);
            }

            return col;
        }
    }
}
