//-----------------------------------------------------------------------
// <copyright file="SqlBuilder.cs" company="marshl">
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
//      'I am naked in the dark. Sam, and there is no veil between me and
//      the wheel of fire.'
//          -- Frodo
// 
//      [ _The Lord of the Rings_, VI/iii: "Mount Doom"]
//
//-----------------------------------------------------------------------
namespace LobsterModel
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using Oracle.ManagedDataAccess.Client;
    using Properties;

    /// <summary>
    /// Used to construct SQL statements from ClobType metadata to then insert, update and query database file information
    /// </summary>
    public static class SqlBuilder
    {
        /// <summary>
        /// Constructs an SQL statement to update a row in the database
        /// </summary>
        /// <param name="clobFile">The ClobFile to build an update statement for.</param>
        /// <returns>The update statement.</returns>
        public static string BuildUpdateStatement(Table table, DBClobFile clobFile)
        {
            Column clobCol = clobFile.GetDataColumn();
            string command = $"UPDATE {table.FullName} c SET c.{clobCol.Name} = :clob";

            Column dateCol;
            if (table.TryGetColumnWithPurpose(Column.Purpose.DATETIME, out dateCol))
            {
                command += $", {dateCol.Name} = SYSDATE";
            }

            if (table.ParentTable != null)
            {
                Table pt = table.ParentTable;
                Column foreignKeyCol = table.GetColumnWithPurpose(Column.Purpose.FOREIGN_KEY);
                Column parentIDCol = pt.GetColumnWithPurpose(Column.Purpose.ID);
                Column parentMnemCol = pt.GetColumnWithPurpose(Column.Purpose.MNEMONIC);

                command += $" WHERE c.{foreignKeyCol.Name} = ("
                        + $" SELECT p.{parentIDCol.Name}"
                        + $" FROM {pt.FullName} p"
                        + $" WHERE p.{parentMnemCol.Name} = '{clobFile.Mnemonic}')";
            }
            else
            {
                Column mnemCol = table.GetColumnWithPurpose(Column.Purpose.MNEMONIC);
                command += $" WHERE c.{mnemCol.Name} = '{clobFile.Mnemonic}'";
            }

            return command;
        }

        /// <summary>
        /// Constructs an SQL statement to insert a row into the parent table of this table with the given mnemonic.
        /// </summary>
        /// <param name="mnemonic">The mnemonic for the file to insert.</param>
        /// <returns>The insert statement.</returns>
        public static string BuildInsertParentStatement(Table table, string mnemonic)
        {
            Debug.Assert(table.ParentTable != null, "Inserting into a parent table requires a parent to be defined");
            Table pt = table.ParentTable;

            Column idCol = pt.GetColumnWithPurpose(Column.Purpose.ID);
            Column mnemCol = pt.GetColumnWithPurpose(Column.Purpose.MNEMONIC);

            string command = $"INSERT INTO {pt.FullName} p"
                + $" (p.{idCol.Name}, p.{mnemCol.Name} )"
                + $" VALUES( {idCol.NextID}, '{mnemonic}' )";

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
        public static string BuildInsertChildStatement(Table table, string mnemonic, string mimeType)
        {
            // Make a string for the column names...
            string insertCommand = "INSERT INTO " + table.FullName + " t ( ";

            // ..and another for the values. The insertCommand and valueCommand are joined to form the full statement
            string valueCommand = " VALUES ( ";

            if (table.ParentTable != null)
            {
                Table pt = table.ParentTable;
                Column foreignKeyCol = table.GetColumnWithPurpose(Column.Purpose.FOREIGN_KEY);
                Column parentIDCol = pt.GetColumnWithPurpose(Column.Purpose.ID);
                Column parentMnemCol = pt.GetColumnWithPurpose(Column.Purpose.MNEMONIC);

                insertCommand += $"t.{foreignKeyCol.Name}";

                valueCommand += $"( SELECT p.{parentIDCol.Name} FROM {pt.FullName} p WHERE p.{parentMnemCol.Name} = '{mnemonic}' )";
            }
            else
            {
                Column mnemCol = table.GetColumnWithPurpose(Column.Purpose.MNEMONIC);
                insertCommand += $" t.{mnemCol.Name} ";
                valueCommand += $" '{mnemonic}' ";

                if (mimeType != null)
                {
                    Column mimeCol = table.GetColumnWithPurpose(Column.Purpose.MIME_TYPE);
                    insertCommand += $", t.{mimeCol.Name} ";
                    valueCommand += $", '{mimeType}' ";
                }
            }

            // No parent table
            Column idCol;
            if (table.TryGetColumnWithPurpose(Column.Purpose.ID, out idCol))
            {
                insertCommand += $", t.{idCol.Name} ";
                valueCommand += $", {idCol.NextID} ";
            }

            // The date column is optional
            Column dateCol;
            if (table.TryGetColumnWithPurpose(Column.Purpose.DATETIME, out dateCol))
            {
                insertCommand += $", t.{dateCol.Name} ";
                valueCommand += ", SYSDATE ";
            }

            // The full name column is optional. It is set to the PascalCase of the mnemonic
            Column fullNameCol;
            if (table.TryGetColumnWithPurpose(Column.Purpose.FULL_NAME, out fullNameCol))
            {
                TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                string fullName = textInfo.ToTitleCase(mnemonic.ToLower());
                insertCommand += $", t.{fullNameCol.Name} ";
                valueCommand += $", '{fullName}' ";
            }

            Column dataCol = table.Columns.Find(x => x.ColumnPurpose == Column.Purpose.CLOB_DATA
                                                && (mimeType == null || x.MimeTypeList.Contains(mimeType)));

            if (dataCol == null)
            {
                throw new ColumnNotFoundException(table, Column.Purpose.DATETIME, mimeType);
            }

            insertCommand += $", t.{dataCol.Name} )";
            valueCommand += ", :clob ) ";
            return insertCommand + valueCommand;
        }

        /// <summary>
        /// Constructs an SQL query to retrieve the list of files in this table on the database.
        /// If this table has a mime type column, then the value of that column will be included in the query.
        /// </summary>
        /// <returns>The SQL command the retrive the file lists for this table.</returns>
        public static string GetFileListCommand(Table table)
        {
            Column mimeCol;
            table.TryGetColumnWithPurpose(Column.Purpose.MIME_TYPE, out mimeCol);

            if (table.ParentTable != null)
            {
                Table pt = table.ParentTable;
                Column foreignKeyCol = table.GetColumnWithPurpose(Column.Purpose.FOREIGN_KEY);
                Column parentIDCol = pt.GetColumnWithPurpose(Column.Purpose.ID);
                Column parentMnemCol = pt.GetColumnWithPurpose(Column.Purpose.MNEMONIC);

                return $"SELECT p.{parentMnemCol.Name}"
                    + (mimeCol != null ? $", c.{mimeCol.Name}" : null)
                    + $" FROM {pt.FullName} p"
                    + $" JOIN {table.FullName} c"
                    + $" ON c.{foreignKeyCol.Name}= p.{parentIDCol.Name}";
            }
            else
            {
                Column mnemCol = table.GetColumnWithPurpose(Column.Purpose.MNEMONIC);
                return $"SELECT t.{mnemCol.Name}"
                    + (mimeCol != null ? $", t.{mimeCol.Name}" : null)
                    + $" FROM {table.FullName} t";
            }
        }


        /// <summary>
        /// Constructs an SQL statement to get the 
        /// </summary>
        /// <param name="clobFile">The ClobFile to build the statement for.</param>
        /// <returns>The SQL command</returns>
        public static string BuildGetDataCommand(Table table, DBClobFile clobFile)
        {
            Column clobCol = clobFile.GetDataColumn();
            if (table.ParentTable != null)
            {
                Table pt = table.ParentTable;
                Column foreignKeyCol = table.GetColumnWithPurpose(Column.Purpose.FOREIGN_KEY);
                Column parentIDCol = pt.GetColumnWithPurpose(Column.Purpose.ID);
                Column parentMnemCol = pt.GetColumnWithPurpose(Column.Purpose.MNEMONIC);
                return
                    $"SELECT c.{clobCol.Name}"
                    + $" FROM {pt.FullName} p"
                    + $" JOIN {table.FullName} c"
                    + $" ON c.{foreignKeyCol.Name} = p.{parentIDCol.Name}"
                    + $" WHERE p.{parentMnemCol.Name} = :mnemonic";
            }
            else
            {
                Column mnemCol = table.GetColumnWithPurpose(Column.Purpose.MNEMONIC);
                return
                    $"SELECT t.{clobCol.Name}"
                    + $" FROM {table.FullName} t"
                    + $" WHERE t.{mnemCol.Name} = :mnemonic";
            }
        }

        /// <summary>
        /// Adds the contents of the given file into the given command under the alias ":clob"
        /// </summary>
        /// <param name="fullpath">The file which will have its data bound to the query.</param>
        /// <param name="table">The table that the file will be added to.</param>
        /// <param name="mimeType">The mime type the file will be added as, if any.</param>
        /// <returns>The parameter that was created.</returns>
        public static OracleParameter CreateFileDataParameter(string fullpath, Table table, string mimeType)
        {
            OracleParameter param = new OracleParameter();
            param.ParameterName = "clob";

            Column column = table.Columns.Find(
                    x => x.ColumnPurpose == Column.Purpose.CLOB_DATA
                        && (mimeType == null || x.MimeTypeList.Contains(mimeType)));

            // Wait for the file to unlock
            using (FileStream fs = Utils.WaitForFile(
                fullpath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite))
            {
                // Binary mode
                if (column.DataType == Column.Datatype.BLOB)
                {
                    byte[] fileData = new byte[fs.Length];
                    fs.Read(fileData, 0, Convert.ToInt32(fs.Length));

                    param.Value = fileData;
                    param.OracleDbType = OracleDbType.Blob;
                }
                else
                {
                    // Text mode
                    string contents = File.ReadAllText(fullpath);

                    if (Settings.Default.AppendFooterToDatabaseFiles)
                    {
                        contents += MimeTypeList.GetClobFooterMessage(mimeType);
                    }

                    param.Value = contents;
                    param.OracleDbType = column.DataType == Column.Datatype.XMLTYPE ? OracleDbType.XmlType : OracleDbType.Clob;
                }
            }

            return param;
        }
    }
}
