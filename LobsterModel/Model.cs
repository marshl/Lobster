//-----------------------------------------------------------------------
// <copyright file="LobsterModel.cs" company="marshl">
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
// </copyright>
//-----------------------------------------------------------------------
//
//      'Long ago they fell under the dominion of the One, and they became Ringwraiths,
//      shadows under his great Shadow, his most terrible servants.'
//          -- Gandalf
// 
//      [ _The Lord of the Rings_, I/ii: "The Shadow of the Past"]
//
//-----------------------------------------------------------------------
namespace LobsterModel
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using Oracle.ManagedDataAccess.Client;
    using Oracle.ManagedDataAccess.Types;
    using Properties;

    /// <summary>
    /// The database connection model 
    /// </summary>
    public class Model
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Model"/> class.
        /// </summary>
        public Model(IModelEventListener eventListener)
        {
            this.eventListener = eventListener;
            this.MimeList = Utils.DeserialiseXmlFileUsingSchema<MimeTypeList>("LobsterSettings/MimeTypes.xml", null);

            //this.LoadDatabaseConnections();
        }

        private IModelEventListener eventListener;

        /// <summary>
        /// The current connection that is being used by this model.
        /// </summary>
        public DatabaseConnection CurrentConnection { get; set; }

        /// <summary>
        /// The list of temporary files that have been downloaded so far.
        /// These files are deleted when the model is disposed.
        /// </summary>
        public List<string> TempFileList { get; private set; } = new List<string>();

        /// <summary>
        /// The list of mime types that are used to translate from file names to database mnemonics and vice-sersa.
        /// </summary>
        private MimeTypeList MimeList { get; set; }

        /// <summary>
        /// public facing method for updating a database file with its local content.
        /// </summary>
        /// <param name="clobFile">The file to update.</param>
        public void SendUpdateClobMessage(string fullpath)
        {
            DbConnection con = OpenConnection(this.CurrentConnection.Config);
            bool result;
            if (con == null)
            {
                result = false;
            }
            else
            {
                ClobDirectory clobDir = this.CurrentConnection.GetClobDirectoryForFile(fullpath);
                DBClobFile clobFile = clobDir?.GetDatabaseFileForFullpath(fullpath);

                if (Settings.Default.BackupEnabled)
                {
                    DirectoryInfo backupDir = new DirectoryInfo(Settings.Default.BackupDirectory);
                    if (!backupDir.Exists)
                    {
                        backupDir.Create();
                    }

                    string tempName = string.Format("{0:yyyy-MM-dd_HH-mm-ss}", DateTime.Now)
                        + Path.GetFileName(fullpath);
                    string filepath = Path.Combine(Settings.Default.BackupDirectory, tempName);
                    this.DownloadClobDataToFile(clobFile, con, filepath);
                }

                result = this.UpdateDatabaseClob(fullpath, clobFile, con);
                con.Dispose();
            }

            this.eventListener.OnUpdateComplete(fullpath);
            //LobsterMain.Instance.OnFileUpdateComplete(fullpath, result);
        }

        /// <summary>
        /// Public access for inserting files into the database.
        /// </summary>
        /// <param name="clobFile">The file to insert into the database.</param>
        /// <param name="table">The table to insert the file into.</param>
        /// <param name="mimeType">The mimetype to insert the file as (if applicable, otherwise null).</param>
        /// <returns>True if the file was inserted, otherwise false.</returns>
        public bool SendInsertClobMessage(string fullpath)
        {
            ClobDirectory clobDir = this.CurrentConnection.GetClobDirectoryForFile(fullpath);

            Table table;
            if (clobDir.ClobType.Tables.Count > 1)
            {
                table = this.eventListener.PromptForTable(fullpath);
            }
            else
            {
                table = clobDir.ClobType.Tables[0];
            }

            string mimeType = null;
            Column mimeTypeColumn = table.Columns.Find(x => x.ColumnPurpose == Column.Purpose.MIME_TYPE);
            if (mimeTypeColumn != null)
            {
                this.eventListener.PromptForMimeType(fullpath, table);
            }

            DbConnection con = OpenConnection(this.CurrentConnection.Config);
            if (con == null)
            {
                return false;
            }

            bool result = this.InsertDatabaseClob(fullpath, table, mimeType, con);
            con.Dispose();
            return result;

            /*

            Debug.Assert(clobFile.IsLocalOnly, "A ClobFile that is already on the database cannot be inserted again.");
            Table table;
            ClobDirectory clobDir = clobFile.ParentClobNode.BaseClobDirectory;

            // If there is > 1 tables in this ClobType, ask the user for which one to use
            if (clobDir.ClobType.Tables.Count > 1)
            {
                TablePicker tablePicker = new TablePicker(clobDir.ClobType);
                DialogResult dialogResult = tablePicker.ShowDialog();
                if (dialogResult != DialogResult.OK)
                {
                    return;
                }

                table = tablePicker.tableCombo.SelectedItem as Table;
            }
            else
            {
                table = clobDir.ClobType.Tables[0];
            }

            // If the table has a MimeType column, ask the user for the type to use
            string mimeType = null;
            Column mimeTypeColumn = table.Columns.Find(x => x.ColumnPurpose == Column.Purpose.MIME_TYPE);
            if (mimeTypeColumn != null)
            {
                DatatypePicker typePicker = new DatatypePicker(table);
                DialogResult dialogResult = typePicker.ShowDialog();
                if (dialogResult != DialogResult.OK)
                {
                    return;
                }

                mimeType = typePicker.datatypeComboBox.Text;
            }

            bool result = this.model.SendInsertClobMessage(clobFile, table, mimeType);
            if (result)
            {
                this.RefreshClobLists();
            }

            this.OnFileInsertComplete(clobFile, result);
            */
        }

        /// <summary>
        /// Public access for downloading database files.
        /// </summary>
        /// <param name="clobFile">The database file that will be downloaded.</param>
        /// <returns>The path of the resulting file.</returns>
        public string SendDownloadClobDataToFileMessage(DBClobFile clobFile)
        {
            DbConnection con = OpenConnection(this.CurrentConnection.Config);
            if (con == null)
            {
                return null;
            }

            string filepath = Utils.GetTempFilepath(clobFile.Filename);
            bool result = this.DownloadClobDataToFile(clobFile, con, filepath);
            con.Dispose();

            return result ? filepath : null;
        }

        public List<DatabaseConfig> GetConfigList()
        {
            List<DatabaseConfig> configList = new List<DatabaseConfig>();

            if (!this.IsConnectionDirectoryValid)
            {
                return configList;
            }

            foreach (string filename in Directory.GetFiles(Settings.Default.ConnectionDir))
            {
                DatabaseConfig connection = DatabaseConfig.LoadDatabaseConfig(filename);
                if (connection != null)
                {
                    configList.Add(connection);
                }
            }
            return configList;
        }

        /// <summary>
        /// Sets the current connection to the given connection, if able.
        /// </summary>
        /// <param name="config">The connection to open.</param>
        /// <returns>Whether making the connection was successful or not.</returns>
        public bool SetDatabaseConnection(DatabaseConfig config)
        {
            MessageLog.LogInfo("Changing connection to " + config.Name);
            using (DbConnection con = OpenConnection(config))
            {
                if (con == null)
                {
                    MessageLog.LogError("Could not change connection to " + config.Name);
                    return false;
                }
            }

            if (config.ClobTypeDir == null || !Directory.Exists(config.ClobTypeDir))
            {
                config.ClobTypeDir = Utils.PromptForDirectory("Please select your Clob Type directory for " + config.Name, config.CodeSource);
                if (config.ClobTypeDir != null)
                {
                    DatabaseConfig.SerialiseToFile(config.FileLocation, config);
                }
                else
                {
                    // Ignore config files that don't have a valid CodeSource folder
                    MessageLog.LogWarning("User cancelled change to ClobTypeDir, aborting connection change");
                    return false;
                }
            }

            this.CurrentConnection = new DatabaseConnection(this, config);

            this.CurrentConnection.LoadClobTypes();

            //this.CurrentConnection.PopulateClobDirectories();
            //this.RebuildLocalAndDatabaseFileLists();
            this.GetDatabaseFileLists();

            MessageLog.LogInfo("Connection change successful");
            return true;
        }

        /// <summary>
        /// Converts the name of a local file to a suitable database mnemonic.
        /// </summary>
        /// <param name="clobFile">The file to convert.</param>
        /// <param name="table">The table the file would be inserted into.</param>
        /// <param name="mimeType">The mime type the file would be inserted as.</param>
        /// <returns>The database mnemonic representation of the file.</returns>
        public string ConvertFilenameToMnemonic(string fullpath, Table table, string mimeType)
        {
            string mnemonic = Path.GetFileNameWithoutExtension(fullpath);

            // If the table stores mime types, then the mnemonic will also need to have 
            // the prefix representation of the mime type prefixed to it.
            if (table.Columns.Find(x => x.ColumnPurpose == Column.Purpose.MIME_TYPE) != null)
            {
                MimeTypeList.MimeType mt = this.MimeList.MimeTypes.Find(x => x.Name.Equals(mimeType));
                if (mt == null)
                {
                    throw new MimeTypeNotFoundException("Unknown mime-to-prefix key " + mimeType);
                }

                if (mt.Prefix.Length > 0)
                {
                    mnemonic = mt.Prefix + '/' + mnemonic;
                }
            }

            return mnemonic;
        }

        /// <summary>
        /// Converts the mnemonic of a file on the database to the local filename that it would represent.
        /// </summary>
        /// <param name="mnemonic">The database mnemonic.</param>
        /// <param name="table">The table the mnemonic is from.</param>
        /// <param name="mimeType">The mime type of the database file, if applicable.</param>
        /// <returns>The name as converted from the mnemonic.</returns>
        public string ConvertMnemonicToFilename(string mnemonic, Table table, string mimeType)
        {
            string filename = mnemonic;
            string prefix = null;

            if (mnemonic.Contains('/'))
            {
                prefix = mnemonic.Substring(0, mnemonic.LastIndexOf('/'));
                filename = mnemonic.Substring(mnemonic.LastIndexOf('/') + 1);
            }

            // Assume xml data types for tables without a datatype column, or a prefix
            if (table.Columns.Find(x => x.ColumnPurpose == Column.Purpose.MIME_TYPE) == null || prefix == null)
            {
                filename += table.DefaultExtension ?? ".xml";
            }
            else
            {
                MimeTypeList.MimeType mt = this.MimeList.MimeTypes.Find(x => x.Name.Equals(mimeType));

                if (mt == null)
                {
                    throw new MimeTypeNotFoundException("Unkown mime-to-extension key " + mimeType);
                }

                filename += mt.Extension;
            }

            return filename;
        }

        /// <summary>
        /// Opens a new OracleConnection and returns it.
        /// </summary>
        /// <param name="config">The connection configuration settings to use.</param>
        /// <returns>A new connectionif it opened successfully, otherwise null.</returns>
        private static DbConnection OpenConnection(DatabaseConfig config)
        {
            try
            {
                OracleConnection con = new OracleConnection();
                con.ConnectionString = "User Id=" + config.Username
                    + (string.IsNullOrEmpty(config.Password) ? null : ";Password=" + config.Password)
                    + ";Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)("
                    + "HOST=" + config.Host + ")"
                    + "(PORT=" + config.Port + ")))(CONNECT_DATA="
                    + "(SID=" + config.SID + ")(SERVER=DEDICATED)))"
                    + ";Pooling=" + (config.UsePooling ? "true" : "false");

                MessageLog.LogInfo("Connecting to database " + config.Name + " using connection string " + con.ConnectionString);
                con.Open();
                return con;
            }
            catch (Exception e)
            {
                if (e is InvalidOperationException || e is OracleException || e is FormatException || e is ArgumentOutOfRangeException)
                {
                    Utils.ShowErrorMessage("Database Connection Failure", "Cannot open connection to database: " + e.Message);
                    MessageLog.LogError("Connection to Oracle failed: " + e.Message);
                    return null;
                }

                throw;
            }
        }

        /// <summary>
        /// Loads all database connections found in the 
        /// </summary>
        private void LoadDatabaseConnections()
        {
            // TODO: Get rid of this shit
            if (Settings.Default.ConnectionDir == null || !Directory.Exists(Settings.Default.ConnectionDir))
            {
                string connectionDir = Utils.PromptForDirectory("Please select your DatabaseConnections folder", null);

                if (connectionDir == null)
                {
                    throw new ConnectionDirNotFoundException();
                }

                Settings.Default.ConnectionDir = connectionDir;
                Settings.Default.Save();
            }
        }

        public bool IsConnectionDirectoryValid
        {
            get
            {
                return Settings.Default.ConnectionDir != null && Directory.Exists(Settings.Default.ConnectionDir);
            }
        }

        public string ConnectionDirectory
        {
            get
            {
                return Settings.Default.ConnectionDir;
            }
        }

        /// <summary>
        /// Queries the database for all files in all ClobDireectories and stores them in the directory.
        /// </summary>
        private void GetDatabaseFileLists()
        {
            DbConnection con = OpenConnection(this.CurrentConnection.Config);
            if (con == null)
            {
                MessageLog.LogError("Connection failed, cannot diff files with database.");
                return;
            }

            try
            {
                foreach (ClobDirectory clobDir in this.CurrentConnection.ClobDirectoryList)
                {
                    this.GetDatabaseFileListForDirectory(clobDir, con);
                }
            }
            finally
            {
                con.Dispose();
            }
        }

        /// <summary>
        /// Inserts the given file into the database.
        /// If the insert is successful, then the database information about that file will be set.
        /// </summary>
        /// <param name="clobFile">The file to insert into the database.</param>
        /// <param name="con">The Oracle connction to use.</param>
        /// <returns>True if the file was updated successfully, otherwise false.</returns>
        private bool UpdateDatabaseClob(string fullpath, DBClobFile clobFile, DbConnection con)
        {
            DbTransaction trans = con.BeginTransaction();
            DbCommand command = con.CreateCommand();
            Table table = clobFile.ParentTable;

            try
            {
                command.CommandText = table.BuildUpdateStatement(clobFile);
            }
            catch (ClobColumnNotFoundException e)
            {
                Utils.ShowErrorMessage("Clob Data Fetch Error", e.Message);
                MessageLog.LogError(e.Message);
                return false;
            }

            try
            {
                this.AddFileDataParameter(command, fullpath, clobFile.ParentTable, clobFile.MimeType);
            }
            catch (IOException e)
            {
                trans.Rollback();
                Utils.ShowErrorMessage("Clob Update Failed", "An IO Exception occurred when updating the database: " + e.Message);
                MessageLog.LogError("Clob update failed with message \"" + e.Message + "\" for command \"" + command.CommandText + "\"");
                return false;
            }

            int rowsAffected;
            try
            {
                MessageLog.LogInfo("Executing Update query: " + command.CommandText);
                rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected != 1)
                {
                    trans.Rollback();
                    Utils.ShowErrorMessage("Clob Update Failed", rowsAffected + " rows were affected during the update (expected only 1). The transaction has been rolled back.");
                    MessageLog.LogError("In invalid number of rows (" + rowsAffected + ") were updated for command: " + command.CommandText);
                    return false;
                }

                trans.Commit();
                command.Dispose();
                MessageLog.LogInfo("Clob file update successful: " + fullpath);
                return true;
            }
            catch (Exception e)
            {
                if (e is OracleException || e is InvalidOperationException)
                {
                    trans.Rollback();
                    Utils.ShowErrorMessage("Clob Update Failed", "An invalid operation occurred when updating the database: " + e.Message);
                    MessageLog.LogError("Clob update failed: " + e.Message + " for command: " + command.CommandText);
                    return false;
                }

                throw;
            }
        }

        /// <summary>
        /// Adds the contents of the given file into the given command under the alias ":data"
        /// </summary>
        /// <param name="command">The command to add the data to.</param>
        /// <param name="clobFile">The local file which will have its data bound to the query.</param>
        /// <param name="table">The table that the file will be added to.</param>
        /// <param name="mimeType">The mime type the file will be added as, if any.</param>
        private void AddFileDataParameter(DbCommand command, string fullpath, Table table, string mimeType)
        {
            // Wait for the file to unlock
            using (FileStream fs = Utils.WaitForFile(
                fullpath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite))
            {
                Column column = table.Columns.Find(
                    x => x.ColumnPurpose == Column.Purpose.CLOB_DATA
                        && (mimeType == null || x.MimeTypeList.Contains(mimeType)));

                DbParameter param = command.CreateParameter();
                param.ParameterName = "data";

                // Binary mode
                if (column.DataType == Column.Datatype.BLOB)
                {
                    byte[] fileData = new byte[fs.Length];
                    fs.Read(fileData, 0, Convert.ToInt32(fs.Length));

                    param.Value = fileData;
                    OracleParameter op = (OracleParameter)param;
                    op.OracleDbType = OracleDbType.Blob;
                }
                else
                {
                    // Text mode
                    StreamReader sr = new StreamReader(fs);
                    string contents = sr.ReadToEnd();
                    contents += this.GetClobFooterMessage(mimeType);

                    param.Value = contents;
                    OracleParameter op = (OracleParameter)param;
                    op.OracleDbType = column.DataType == Column.Datatype.XMLTYPE ? OracleDbType.XmlType : OracleDbType.Clob;
                }

                command.Parameters.Add(param);
            }
        }

        /// <summary>
        /// Inserts the given local file into the database.
        /// </summary>
        /// <param name="clobFile">The local-only file to insert.</param>
        /// <param name="table">The table to insert the file into.</param>
        /// <param name="mimeType">The mimetype to insert the file as.</param>
        /// <param name="con">The Oracle connection to use.</param>
        /// <returns>True if the file was inserted successfully, otherwise false.</returns>
        private bool InsertDatabaseClob(string fullpath, Table table, string mimeType, DbConnection con)
        {
            DbCommand command = con.CreateCommand();
            DbTransaction trans = con.BeginTransaction();
            string mnemonic = this.ConvertFilenameToMnemonic(fullpath, table, mimeType);

            if (table.ParentTable != null)
            {
                try
                {
                    command.CommandText = table.BuildInsertParentStatement(mnemonic);
                    MessageLog.LogInfo("Executing Insert query on parent table: " + command.CommandText);
                    command.ExecuteNonQuery();
                    command.Dispose();
                }
                catch (Exception e)
                {
                    if (e is InvalidOperationException || e is OracleException)
                    {
                        Utils.ShowErrorMessage(
                            "Clob Insert Error",
                            $"An exception occurred when inserting into the parent table of {fullpath}: {e.Message}");
                        MessageLog.LogError($"Error creating new clob: {e.Message} when executing command: {command.CommandText}");
                        return false;
                    }

                    throw;
                }
            }

            command = con.CreateCommand();
            command.CommandText = table.BuildInsertChildStatement(mnemonic, mimeType);

            this.AddFileDataParameter(command, fullpath, table, mimeType);

            try
            {
                MessageLog.LogInfo("Executing Insert query: " + command.CommandText);
                command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                if (e is InvalidOperationException || e is OracleException)
                {
                    // Discard the insert amde into the parent table
                    trans.Rollback();
                    Utils.ShowErrorMessage(
                        "Clob Insert Error",
                        $"An invalid operation occurred when inserting {fullpath}: {e.Message}");
                    MessageLog.LogError($"Error creating new clob: {e.Message} when executing command: {command.CommandText}");
                    return false;
                }

                throw;
            }

            command.Dispose();
            trans.Commit();

            DBClobFile clobFile = new DBClobFile(table, mnemonic, mimeType, fullpath);

            MessageLog.LogInfo($"Clob file creation successful: {fullpath}");
            return true;
        }

        /// <summary>
        /// Downloads the current content of a file on the database to a local temp file.
        /// </summary>
        /// <param name="clobFile">The file to download.</param>
        /// <param name="con">The connection to use.</param>
        /// <param name="filename">The filepath to download the data into.</param>
        /// <returns>The path of the temporary file, if it exists.</returns>
        private bool DownloadClobDataToFile(DBClobFile clobFile, DbConnection con, string filename)
        {
            DbCommand command = con.CreateCommand();

            Table table = clobFile.ParentTable;
            Column column;
            try
            {
                column = clobFile.GetColumn();
                command.CommandText = table.BuildGetDataCommand(clobFile);
            }
            catch (ClobColumnNotFoundException e)
            {
                Utils.ShowErrorMessage("Clob Data Fetch Error", e.Message);
                MessageLog.LogError(e.Message);
                return false;
            }

            try
            {
                DbDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    if (column.DataType == Column.Datatype.BLOB)
                    {
                        byte[] b = new Byte[(reader.GetBytes(0, 0, null, 0, int.MaxValue))];
                        reader.GetBytes(0, 0, b, 0, b.Length);
                        File.WriteAllBytes(filename, b);
                    }
                    else
                    {
                        string result;
                        Column clobColumn = clobFile.ParentTable.Columns.Find(x => x.ColumnPurpose == Column.Purpose.CLOB_DATA);
                        if (clobColumn.DataType == Column.Datatype.CLOB)
                        {
                            //OracleClob clob = reader.GetOracleClob(0);
                            //reader.GetBytes(0,0,null,0,)
                            result = reader.GetString(0);
                            //result = clob.Value;
                        }
                        else
                        {
                            //OracleXmlType xml = reader.GetOracleXmlType(0);
                            //result = xml.Value;
                            result = reader.GetString(0);
                        }

                        StreamWriter streamWriter = File.AppendText(filename);
                        streamWriter.Write(result);
                        streamWriter.Close();
                    }

                    if (!reader.Read())
                    {
                        reader.Close();
                        return true;
                    }
                    else
                    {
                        reader.Close();
                        Utils.ShowErrorMessage(
                            "Clob Data Fetch Error",
                            "Too many rows were found for " + clobFile.Mnemonic);

                        MessageLog.LogError("Too many rows found on clob retrieval of " + clobFile.Mnemonic);
                        return false;
                    }
                }
            }
            catch (Exception e) when (e is InvalidOperationException || e is OracleNullValueException)
            {
                Utils.ShowErrorMessage(
                    "Clob Data Fetch Error",
                    "An invalid operation occurred when retreiving the data of " + clobFile.Mnemonic + ": " + e.Message);
                MessageLog.LogError("Error retrieving data: " + e.Message + " when executing command " + command.CommandText);
                return false;
            }

            Utils.ShowErrorMessage(
                "Clob Data Fetch Error",
                "No data was found for " + clobFile.Mnemonic);
            MessageLog.LogError("No data found on clob retrieval of " + clobFile.Mnemonic + " when executing command: " + command.CommandText);
            return false;
        }

        internal void QueryDatabaseFiles()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Finds all DBClobFIles from the tables in the given ClobDirectory and populates its lists with them.
        /// </summary>
        /// <param name="clobDir">The directory to get the file lists for.</param>
        /// <param name="con">The Oracle connection to use.</param>
        private void GetDatabaseFileListForDirectory(ClobDirectory clobDir, DbConnection con)
        {
            clobDir.DatabaseFileList = new List<DBClobFile>();
            ClobType ct = clobDir.ClobType;
            foreach (Table table in ct.Tables)
            {
                DbCommand command = con.CreateCommand();
                command.CommandText = table.GetFileListCommand();
                DbDataReader reader;
                try
                {
                    reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        string mnemonic = reader.GetString(0);
                        string mimeType = table.Columns.Find(x => x.ColumnPurpose == Column.Purpose.MIME_TYPE) != null ? reader.GetString(1) : null;
                        DBClobFile databaseFile = new DBClobFile(
                            parentTable: table,
                            mnemonic: mnemonic,
                            mimetype: mimeType,
                            filename: this.ConvertMnemonicToFilename(mnemonic, table, mimeType)
                        );
                        clobDir.DatabaseFileList.Add(databaseFile);
                    }

                    reader.Close();
                }
                catch (InvalidOperationException e)
                {
                    command.Dispose();
                    Utils.ShowErrorMessage(
                        "Directory Comparison Error",
                        "An invalid operation occurred when retriving the file list for  " + ct.Name);
                    MessageLog.LogError("Error comparing to database: " + e.Message + " when executing command " + command.CommandText);
                    return;
                }
                finally
                {
                    command.Dispose();
                }
            }
        }

        /// <summary>
        /// Creates a comment to place at the bottom of a non-binary file of the given mime type.
        /// </summary>
        /// <param name="mimeType">The mime type of the file the footer is for.</param>
        /// <returns>The footer string.</returns>
        private string GetClobFooterMessage(string mimeType)
        {
            return (mimeType == "text/javascript" ? "/*" : "<!--")
                + " Last clobbed by user " + Environment.UserName
                + " on machine " + Environment.MachineName
                + " at " + DateTime.Now
                + " (Lobster build " + Utils.RetrieveLinkerTimestamp().ToShortDateString() + ")"
                + (mimeType.Equals("text/javascript", StringComparison.OrdinalIgnoreCase) ? "*/" : "-- >");
        }

        public void ChnageConnectionDirectory(string fileName)
        {
            Settings.Default.ConnectionDir = fileName;
            Settings.Default.Save();
        }

        /// <summary>
        /// An exception for when the user refuses to select a Connection Directory
        /// </summary>
        /// TODO: Kill this sumbitch
        [Serializable]
        public class ConnectionDirNotFoundException : Exception
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ConnectionDirNotFoundException"/> class.
            /// </summary>
            public ConnectionDirNotFoundException()
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="ConnectionDirNotFoundException"/> class.
            /// </summary>
            /// <param name="message">The exception messaage.</param>
            public ConnectionDirNotFoundException(string message) : base(message)
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="ConnectionDirNotFoundException"/> class.
            /// </summary>
            /// <param name="message">The exception message.</param>
            /// <param name="innerException">The inner exception.</param>
            public ConnectionDirNotFoundException(string message, Exception innerException) : base(message, innerException)
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="ConnectionDirNotFoundException"/> class.
            /// </summary>
            /// <param name="info">The serialisation info.</param>
            /// <param name="context">The streaming context.</param>
            protected ConnectionDirNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
            {
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public enum DatabaseProvider
        {
            /// <summary>
            /// 
            /// </summary>
            ORACLE,

            /// <summary>
            /// 
            /// </summary>
            MYSQL,
        }
    }
}
