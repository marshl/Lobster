//-----------------------------------------------------------------------
// <copyright file="Model.cs" company="marshl">
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
    using System.Collections.Generic;
    using System.Data.Common;
    using System.IO;
    using System.Linq;
    using System.Security;
    using Oracle.ManagedDataAccess.Client;
    using Properties;

    /// <summary>
    /// The database connection model 
    /// </summary>
    [Obsolete]
    public static class Model
    {
        /// <summary>
        /// Sets the current connection to the given connection, if able.
        /// </summary>
        /// <param name="config">The connection to open.</param>
        /// <param name="password">The password to connect to the database with.</param>
        /// <returns>The successfully created database connection.</returns>
        public static DatabaseConnection SetDatabaseConnection(DatabaseConfig config, SecureString password)
        {
            // TODO: Move to DatabaseConnection
            MessageLog.LogInfo($"Changing connection to {config.Name}");
            try
            {
                DbConnection con = config.OpenConnection(password);
                con.Close();
            }
            catch (ConnectToDatabaseException ex)
            {
                throw new SetConnectionException($"A connection could not be made to the database: {ex.Message}", ex);
            }

            if (config.ClobTypeDirectory == null || !Directory.Exists(config.ClobTypeDirectory))
            {
                throw new SetConnectionException($"The clob type directory {config.ClobTypeDirectory} could not be found.");
            }

            DatabaseConnection databaseConnection = new DatabaseConnection(config, password);

            List<ClobTypeLoadException> errors = new List<ClobTypeLoadException>();
            List<FileListRetrievalException> fileLoadErrors = new List<FileListRetrievalException>();

            databaseConnection.LoadClobTypes(ref errors);
            databaseConnection.GetDatabaseFileLists(ref fileLoadErrors);

            MessageLog.LogInfo("Connection change successful");

            return databaseConnection;
        }

        /// <summary>
        /// INitialises the given directory with the necessary configuration files and folders.
        /// </summary>
        /// <param name="directory">The directory to initialise.</param>
        /// <param name="newConfig">The database configuration file that is created.</param>
        /// <returns>True if the directory was set up  correctly, false if there was an issue setting it up, or if the directory already has them.</returns>
        public static bool InitialiseCodeSourceDirectory(string directory, ref DatabaseConfig newConfig)
        {
            try
            {
                Directory.CreateDirectory(Path.Combine(directory, Settings.Default.ClobTypeDirectoryName));
                newConfig = new DatabaseConfig();
                string configFile = Path.Combine(directory, Settings.Default.DatabaseConfigFileName);
                DatabaseConfig.SerialiseToFile(configFile, newConfig);
                newConfig.FileLocation = configFile;

                if (!AddCodeSourceDirectory(directory))
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                return false;
            }
        }

        /// <summary>
        /// Adds the given directory to the CodeSource dirctory list.
        /// </summary>
        /// <param name="directoryName">The directory to add.</param>
        /// <returns>True if the directory is new and was added ok, false if the directory was already being used.</returns>
        public static bool AddCodeSourceDirectory(string directoryName)
        {
            if (Settings.Default.CodeSourceDirectories == null)
            {
                Settings.Default.CodeSourceDirectories = new System.Collections.Specialized.StringCollection();
            }
            else if (Settings.Default.CodeSourceDirectories.Contains(directoryName))
            {
                return false;
            }

            Settings.Default.CodeSourceDirectories.Add(directoryName);
            Settings.Default.Save();
            return true;
        }

        /// <summary>
        /// Removes the CodeSource folder with the given name from the known list.
        /// </summary>
        /// <param name="directoryName">The  name of the directory to remove.</param>
        /// <returns>True if the directory already exists and was removed, otherwise false.</returns>
        public static bool RemoveCodeSource(string directoryName)
        {
            if (Settings.Default.CodeSourceDirectories == null || !Settings.Default.CodeSourceDirectories.Contains(directoryName))
            {
                return false;
            }

            Settings.Default.CodeSourceDirectories.Remove(directoryName);
            return true;
        }

        /// <summary>
        /// Validates that the given direcotry is currently a valid CodeSource folder (it has the correct files and folders).
        /// </summary>
        /// <param name="directory">The directory to check.</param>
        /// <param name="errorMessage">The error message to return (if any).</param>
        /// <returns>True if the directory is valid, otherwise false (with the error message initialised).</returns>
        public static bool ValidateCodeSourceLocation(string directory, ref string errorMessage)
        {
            if (Settings.Default.CodeSourceDirectories != null && Settings.Default.CodeSourceDirectories.Contains(directory))
            {
                errorMessage = $"The chosen directory {directory} has already been used.";
                return false;
            }

            DirectoryInfo dirInfo = new DirectoryInfo(directory);
            if (!dirInfo.Exists)
            {
                errorMessage = $"The chosen directory {directory} does not exist, or is not a directory";
                return false;
            }

            DirectoryInfo lobsterTypeFolder = new DirectoryInfo(Path.Combine(directory, Settings.Default.ClobTypeDirectoryName));
            if (!lobsterTypeFolder.Exists)
            {
                errorMessage = $"The chosen folder must contain a directory named {Settings.Default.ClobTypeDirectoryName}";
                return false;
            }

            FileInfo configFile = new FileInfo(Path.Combine(dirInfo.FullName, Settings.Default.DatabaseConfigFileName));
            if (!configFile.Exists)
            {
                errorMessage = $"The chosen folder must contain a file named {Settings.Default.DatabaseConfigFileName}";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates whether the given directory is a vliad place to set as a new CodeSOurce location.
        /// </summary>
        /// <param name="directory">The directory to validate.</param>
        /// <param name="errorMessage">Any error message that occurred, if any.</param>
        /// <returns>True if the location is valid, otherwise false.</returns>
        public static bool ValidateNewCodeSourceLocation(string directory, ref string errorMessage)
        {
            if (Settings.Default.CodeSourceDirectories != null && Settings.Default.CodeSourceDirectories.Contains(directory))
            {
                errorMessage = $"The chosen directory {directory} has already been used.";
                return false;
            }

            DirectoryInfo dirInfo = new DirectoryInfo(directory);
            if (!dirInfo.Exists)
            {
                errorMessage = $"The chosen directory {directory} does not exist, or is not a directory";
                return false;
            }

            FileInfo configFile = new FileInfo(Path.Combine(directory, Settings.Default.DatabaseConfigFileName));
            if (configFile.Exists)
            {
                errorMessage = $"The chosen folder already contains a file named {Settings.Default.DatabaseConfigFileName}";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Converts the mnemonic of a file on the database to the local filename that it would represent.
        /// </summary>
        /// <param name="databaseConnection">The connection to make the file with.</param>
        /// <param name="mnemonic">The database mnemonic.</param>
        /// <param name="table">The table the mnemonic is from (can be null).</param>
        /// <param name="mimeType">The mime type of the database file, if applicable.</param>
        /// <returns>The name as converted from the mnemonic.</returns>
        public static string ConvertMnemonicToFilename(DatabaseConnection databaseConnection, string mnemonic, Table table, string mimeType)
        {
            string filename = mnemonic;
            string prefix = null;

            if (mnemonic.Contains('/'))
            {
                prefix = mnemonic.Substring(0, mnemonic.LastIndexOf('/'));
                filename = mnemonic.Substring(mnemonic.LastIndexOf('/') + 1);
            }

            // Assume xml data types for tables without a datatype column, or a prefix
            Column mimeTypeColumn;
            if (table == null || !table.TryGetColumnWithPurpose(Column.Purpose.MIME_TYPE, out mimeTypeColumn) || prefix == null)
            {
                filename += table?.DefaultExtension ?? ".xml";
            }
            else
            {
                MimeTypeList.MimeType mt = databaseConnection.MimeTypeList.MimeTypes.Find(x => x.Name.Equals(mimeType));

                if (mt == null)
                {
                    throw new MimeTypeNotFoundException($"Unkown mime-to-extension key {mimeType}");
                }

                filename += mt.Extension;
            }

            return filename;
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
                        contents += Model.GetClobFooterMessage(mimeType);
                    }

                    param.Value = contents;
                    param.OracleDbType = column.DataType == Column.Datatype.XMLTYPE ? OracleDbType.XmlType : OracleDbType.Clob;
                }
            }

            return param;
        }

        /// <summary>
        /// Creates a comment to place at the bottom of a non-binary file of the given mime type.
        /// </summary>
        /// <param name="mimeType">The mime type of the file the footer is for.</param>
        /// <returns>The footer string.</returns>
        private static string GetClobFooterMessage(string mimeType)
        {
            string openingComment = "<!--";
            string closingComment = "-->";
            if (mimeType != null && (mimeType.Equals("text/javascript", StringComparison.OrdinalIgnoreCase) || mimeType.Equals("text/css", StringComparison.OrdinalIgnoreCase)))
            {
                openingComment = "/*";
                closingComment = "*/";
            }

            return $"{openingComment}"
                + $" Last clobbed by user {Environment.UserName}"
                + $" on machine {Environment.MachineName}"
                + $" at {DateTime.Now}"
                + $" (Lobster build {Utils.RetrieveLinkerTimestamp().ToShortDateString()})"
                + $"{closingComment}";
        }
    }
}
