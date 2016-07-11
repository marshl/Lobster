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
    using System.IO;
    using Oracle.ManagedDataAccess.Client;
    using Properties;

    /// <summary>
    /// Used to construct SQL statements from ClobType metadata to then insert, update and query database file information
    /// </summary>
    public static class SqlBuilder
    {
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
