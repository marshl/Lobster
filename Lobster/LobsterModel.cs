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
namespace Lobster
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using System.Xml.Schema;
    using System.Windows.Forms;
    using Oracle.ManagedDataAccess.Client;
    using Oracle.ManagedDataAccess.Types;
    using Properties;
    using System.Runtime.Serialization;

    /// <summary>
    /// The database connection model 
    /// </summary>
    public class LobsterModel
    {
        /// <summary>
        /// 
        /// </summary>
        public List<DatabaseConnection> ConnectionList { get; private set; }
        public DatabaseConnection CurrentConnection { get; set; }

        public List<FileInfo> TempFileList { get; private set; } = new List<FileInfo>();

        private MimeTypeList MimeList { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LobsterModel"/> class.
        /// </summary>
        public LobsterModel()
        {
            this.MimeList = Common.DeserialiseXmlFileUsingSchema<MimeTypeList>("LobsterSettings/MimeTypes.xml", null);

            this.LoadDatabaseConnections();
        }

        /// <summary>
        /// Loads all database connections found in the 
        /// </summary>
        private void LoadDatabaseConnections()
        {
            if (Settings.Default.ConnectionDir == null || !Directory.Exists(Settings.Default.ConnectionDir))
            {
                string connectionDir = Common.PromptForDirectory("Please select your DatabaseConnections folder", null);

                if (connectionDir == null)
                {
                    throw new ConnectionDirNotFoundException();
                }

                Settings.Default.ConnectionDir = connectionDir;
                Settings.Default.Save();
            }

            this.ConnectionList = new List<DatabaseConnection>();
            foreach (string filename in Directory.GetFiles(Settings.Default.ConnectionDir))
            {
                DatabaseConnection dbConfig = this.LoadDatabaseConnection(filename);
                if (dbConfig != null)
                {
                    this.ConnectionList.Add(dbConfig);
                }
            }
        }

        private DatabaseConnection LoadDatabaseConnection(string _fullpath)
        {
            MessageLog.LogInfo("Loading Database Config File " + _fullpath);
            DatabaseConnection dbConnection;
            try
            {
                dbConnection = Common.DeserialiseXmlFileUsingSchema<DatabaseConnection>(_fullpath, "LobsterSettings/DatabaseConfig.xsd");
                dbConnection.ParentModel = this;
            }
            catch (Exception _e)
            {
                if (_e is FileNotFoundException || _e is InvalidOperationException || _e is XmlException || _e is XmlSchemaValidationException)
                {
                    MessageBox.Show("The DBConfig file " + _fullpath + " failed to load. Check the log for more information.", "ClobType Load Failed",
                           MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                    MessageLog.LogError("An error occurred when loading the ClobType " + _fullpath + ": " + _e);
                    return null;
                }
                throw;
            }

            dbConnection.FileLocation = _fullpath;

            // If the CodeSource folder cannot be found, prompt the user for it
            if (dbConnection.CodeSource == null || !Directory.Exists(dbConnection.CodeSource))
            {
                string codeSourceDir = Common.PromptForDirectory("Please select your CodeSource directory for " + dbConnection.Name, null);
                if (codeSourceDir != null)
                {
                    dbConnection.CodeSource = codeSourceDir;
                    DatabaseConnection.SerialiseToFile(_fullpath, dbConnection);
                }
                else // Ignore config files that don't have a valid CodeSource folder
                {
                    return null;
                }
            }


            dbConnection.LoadClobTypes();
            return dbConnection;
        }

        private static OracleConnection OpenConnection(DatabaseConnection _config)
        {
            OracleConnection con = new OracleConnection();
            con.ConnectionString = "User Id=" + _config.Username
                + ";Password=" + _config.Password
                + ";Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)("
                + "HOST=" + _config.Host + ")"
                + "(PORT=" + _config.Port + ")))(CONNECT_DATA="
                + "(SID=" + _config.SID + ")(SERVER=DEDICATED)))"
                + ";Pooling=" + (_config.UsePooling ? "true" : "false");
            try
            {
                MessageLog.LogInfo("Connecting to database " + _config.Name + " using connection string " + con.ConnectionString);
                con.Open();
            }
            catch (Exception _e)
            {
                if (_e is InvalidOperationException || _e is OracleException || _e is FormatException)
                {
                    Common.ShowErrorMessage("Database Connection Failure", "Cannot open connection to database: " + _e.Message);
                    MessageLog.LogError("Connection to Oracle failed: " + _e.Message);
                    return null;
                }
                throw;
            }
            return con;
        }

        /// <summary>
        /// 
        /// </summary>
        private void GetDatabaseFileLists()
        {
            OracleConnection con = OpenConnection(this.CurrentConnection);
            if (con == null)
            {
                MessageLog.LogError("Connection failed, cannot diff files with database.");
                return;
            }

            try
            {
                foreach (KeyValuePair<ClobType, ClobDirectory> pair in this.CurrentConnection.ClobTypeToDirectoryMap)
                {
                    this.GetDatabaseFileListForDirectory(pair.Value, con);
                }
            }
            finally
            {
                con.Dispose();
            }
        }

        public void SendUpdateClobMessage(ClobFile _clobFile)
        {
            OracleConnection con = OpenConnection(this.CurrentConnection);
            bool result;
            if (con == null)
            {
                result = false;
            }
            else
            {
                result = this.UpdateDatabaseClob(_clobFile, con);
                con.Dispose();
            }
            LobsterMain.Instance.OnFileUpdateComplete(_clobFile, result);
        }

        public bool SendInsertClobMessage(ClobFile _clobFile, Table _table, string _mimeType)
        {
            OracleConnection con = OpenConnection(this.CurrentConnection);
            if (con == null)
            {
                return false;
            }
            bool result = this.InsertDatabaseClob(_clobFile, _table, _mimeType, con);
            con.Dispose();
            return result;
        }

        private bool UpdateDatabaseClob(ClobFile _clobFile, OracleConnection _con)
        {
            Debug.Assert(_clobFile.IsSynced);

            OracleTransaction trans = _con.BeginTransaction();
            OracleCommand command = _con.CreateCommand();
            Table table = _clobFile.DatabaseFile.ParentTable;

            try
            {
                command.CommandText = table.BuildUpdateStatement(_clobFile);
            }
            catch (ClobColumnNotFoundException _e)
            {
                Common.ShowErrorMessage("Clob Data Fetch Error", _e.Message);
                MessageLog.LogError(_e.Message);
                return false;
            }
            try
            {
                this.AddFileDataParameter(command, _clobFile, _clobFile.DatabaseFile.ParentTable, _clobFile.DatabaseFile.MimeType);
            }
            catch (IOException _e)
            {
                trans.Rollback();
                Common.ShowErrorMessage("Clob Update Failed", "An IO Exception occurred when updating the database: " + _e.Message);
                MessageLog.LogError("Clob update failed with message \"" + _e.Message + "\" for command \"" + command.CommandText + "\"");
                return false;
            }

            int rowsAffected;
            try
            {
                MessageLog.LogInfo("Executing Update query: " + command.CommandText);
                rowsAffected = command.ExecuteNonQuery();
            }
            catch (Exception _e)
            {
                if (_e is OracleException || _e is InvalidOperationException)
                {
                    trans.Rollback();
                    Common.ShowErrorMessage("Clob Update Failed", "An invalid operation occurred when updating the database: " + _e.Message);
                    MessageLog.LogError("Clob update failed: " + _e.Message + " for command: " + command.CommandText);
                    return false;
                }
                throw;
            }

            if (rowsAffected != 1)
            {
                trans.Rollback();
                Common.ShowErrorMessage("Clob Update Failed", rowsAffected + " rows were affected during the update (expected only 1). The transaction has been rolled back.");
                MessageLog.LogError("In invalid number of rows (" + rowsAffected + ") were updated for command: " + command.CommandText);
                return false;
            }

            trans.Commit();
            command.Dispose();
            MessageLog.LogInfo("Clob file update successful: " + _clobFile.LocalFile.FileInfo.Name);
            return true;
        }

        public FileInfo SendDownloadClobDataToFileMessage(ClobFile _clobFile)
        {
            OracleConnection con = OpenConnection(this.CurrentConnection);
            if (con == null)
            {
                return null;
            }
            FileInfo result = this.DownloadClobDataToFile(_clobFile, con);
            con.Dispose();
            return result;
        }

        /// <summary>
        /// Adds the contents of the given file into the given command under the alias ":data"
        /// </summary>
        /// <param name="command">The command to add the data to.</param>
        /// <param name="clobFile">The local file which will have its data bound to the query.</param>
        /// <param name="table">The table that the file will be added to.</param>
        /// <param name="mimeType">The mime type the file will be added as, if any.</param>
        private void AddFileDataParameter(OracleCommand command, ClobFile clobFile, Table table, string mimeType)
        {
            Debug.Assert(clobFile.LocalFile != null);
            // Wait for the file to unlock
            using (FileStream fs = Common.WaitForFile(clobFile.LocalFile.FileInfo.FullName,
                FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                Column column = table.columns.Find(
                    x => x.ColumnPurpose == Column.Purpose.CLOB_DATA
                        && (mimeType == null || x.MimeTypeList.Contains(mimeType))
                );

                // Binary mode
                if (column.DataType == Column.Datatype.BLOB)
                {
                    byte[] fileData = new byte[fs.Length];
                    fs.Read(fileData, 0, Convert.ToInt32(fs.Length));
                    OracleParameter param = command.Parameters.Add("data", OracleDbType.Blob);
                    param.Value = fileData;
                }
                else // Text mode
                {
                    StreamReader sr = new StreamReader(fs);
                    string contents = sr.ReadToEnd();
                    contents += this.GetClobFooterMessage(mimeType);
                    OracleDbType insertType = column.DataType == Column.Datatype.CLOB ? OracleDbType.Clob : OracleDbType.XmlType;
                    command.Parameters.Add("data", insertType, contents, ParameterDirection.Input);
                }
            }
        }

        private bool InsertDatabaseClob(ClobFile _clobFile, Table _table, string _mimeType, OracleConnection _con)
        {
            Debug.Assert(_clobFile.IsLocalOnly);

            OracleCommand command = _con.CreateCommand();
            OracleTransaction trans = _con.BeginTransaction();
            string mnemonic = this.ConvertFilenameToMnemonic(_clobFile, _table, _mimeType);

            if (_table.parentTable != null)
            {
                try
                {
                    command.CommandText = _table.BuildInsertParentStatement(mnemonic);
                    MessageLog.LogInfo("Executing Insert query on parent table: " + command.CommandText);
                    command.ExecuteNonQuery();
                    command.Dispose();
                }
                catch (Exception _e)
                {
                    if (_e is InvalidOperationException || _e is OracleException)
                    {
                        Common.ShowErrorMessage("Clob Insert Error",
                        "An exception occurred when inserting into the parent table of " + _clobFile.LocalFile.FileInfo.Name + ": " + _e.Message);
                        MessageLog.LogError("Error creating new clob: " + _e.Message + " when executing command: " + command.CommandText);
                        return false;
                    }
                    throw;
                }
            }

            command = _con.CreateCommand();
            command.CommandText = _table.BuildInsertChildStatement(mnemonic, _mimeType);

            this.AddFileDataParameter(command, _clobFile, _table, _mimeType);

            try
            {
                MessageLog.LogInfo("Executing Insert query: " + command.CommandText);
                command.ExecuteNonQuery();
            }
            catch (Exception _e)
            {
                if (_e is InvalidOperationException || _e is OracleException)
                {
                    // Discard the insert amde into the parent table
                    trans.Rollback();
                    Common.ShowErrorMessage("Clob Insert Error",
                            "An invalid operation occurred when inserting " + _clobFile.LocalFile.FileInfo.Name + ": " + _e.Message);
                    MessageLog.LogError("Error creating new clob: " + _e.Message + " when executing command: " + command.CommandText);
                    return false;
                }
                throw;
            }
            command.Dispose();
            trans.Commit();

            _clobFile.DatabaseFile = new DBClobFile()
            {
                Mnemonic = mnemonic,
                Filename = _clobFile.LocalFile.FileInfo.Name,
                ParentTable = _table,
                MimeType = _mimeType
            };

            MessageLog.LogInfo("Clob file creation successful: " + _clobFile.LocalFile.FileInfo.Name);
            return true;
        }

        /// <summary>
        /// Sets the current connection to the given connection, if able.
        /// </summary>
        /// <param name="connection">The connection to open.</param>
        /// <returns>Whether making the connection was successful or not.</returns>
        public bool SetDatabaseConnection(DatabaseConnection connection)
        {
            MessageLog.LogInfo("Changing connection to " + connection.Name);
            using (OracleConnection con = OpenConnection(connection))
            {
                if (con == null)
                {
                    MessageLog.LogError("Could not change connection to " + connection.Name);
                    return false;
                }
            }

            if (connection.ClobTypeDir == null || !Directory.Exists(connection.ClobTypeDir))
            {
                connection.ClobTypeDir = Common.PromptForDirectory("Please select your Clob Type directory for " + connection.Name, connection.CodeSource);
                if (connection.ClobTypeDir != null)
                {
                    DatabaseConnection.SerialiseToFile(connection.FileLocation, connection);
                }
                else // Ignore config files that don't have a valid CodeSource folder
                {
                    MessageLog.LogWarning("User cancelled change to ClobTypeDir, aborting connection change");
                    return false;
                }
            }

            this.CurrentConnection = connection;
            this.CurrentConnection.PopulateClobDirectories();
            this.RequeryDatabase();

            MessageLog.LogInfo("Connection change successful");
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        public void RequeryDatabase()
        {
            Debug.Assert(this.CurrentConnection != null, "Cannot requery the database without being connected to one first");

            this.GetDatabaseFileLists();

            foreach (KeyValuePair<ClobType, ClobDirectory> pair in this.CurrentConnection.ClobTypeToDirectoryMap)
            {
                pair.Value.GetLocalFiles();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clobFile"></param>
        /// <param name="con"></param>
        /// <returns>The <see cref="FileInfo"/> of the temporary file, if it exists.</returns>
        private FileInfo DownloadClobDataToFile(ClobFile clobFile, OracleConnection con)
        {
            Debug.Assert(clobFile.DatabaseFile != null, "The ClobFile must be on the database for it to be downloaded");
            OracleCommand command = con.CreateCommand();

            Table table = clobFile.DatabaseFile.ParentTable;
            Column column;
            try
            {
                column = clobFile.DatabaseFile.GetColumn();
                command.CommandText = table.BuildGetDataCommand(clobFile);
            }
            catch (ClobColumnNotFoundException _e)
            {
                Common.ShowErrorMessage("Clob Data Fetch Error", _e.Message);
                MessageLog.LogError(_e.Message);
                return null;
            }

            command.Parameters.Add(new OracleParameter("mnemonic", OracleDbType.Varchar2, clobFile.DatabaseFile.Mnemonic, ParameterDirection.Input));
            try
            {
                OracleDataReader reader = command.ExecuteReader();
                string tempPath = Path.GetTempFileName();
                new FileInfo(tempPath).Delete();
                string tempName = tempPath.Replace(".tmp", ".") + clobFile.DatabaseFile.Filename;
                FileInfo tempFile = new FileInfo(tempName);

                if (reader.Read())
                {
                    if (column.DataType == Column.Datatype.BLOB)
                    {
                        OracleBlob blob = reader.GetOracleBlob(0);
                        File.WriteAllBytes(tempName, blob.Value);
                    }
                    else
                    {
                        string result;
                        Column clobColumn = clobFile.DatabaseFile.ParentTable.columns.Find(x => x.ColumnPurpose == Column.Purpose.CLOB_DATA);
                        if (clobColumn.DataType == Column.Datatype.CLOB)
                        {
                            OracleClob clob = reader.GetOracleClob(0);
                            result = clob.Value;
                        }
                        else
                        {
                            OracleXmlType xml = reader.GetOracleXmlType(0);
                            result = xml.Value;
                        }

                        StreamWriter streamWriter = File.AppendText(tempName);
                        streamWriter.Write(result);
                        streamWriter.Close();
                    }

                    if (!reader.Read())
                    {
                        reader.Close();
                        return tempFile;
                    }
                    else
                    {
                        reader.Close();
                        Common.ShowErrorMessage(
                            "Clob Data Fetch Error",
                            "Too many rows were found for " + clobFile.DatabaseFile.Mnemonic);

                        MessageLog.LogError("Too many rows found on clob retrieval of " + clobFile.DatabaseFile.Mnemonic);
                        return null;
                    }
                }
            }
            catch (Exception e)
            {
                if (e is InvalidOperationException || e is OracleNullValueException)
                {
                    Common.ShowErrorMessage("Clob Data Fetch Error",
                            "An invalid operation occurred when retreiving the data of " + clobFile.DatabaseFile.Mnemonic + ": " + e.Message);
                    MessageLog.LogError("Error retrieving data: " + e.Message + " when executing command " + command.CommandText);
                    return null;
                }
                throw;
            }
            Common.ShowErrorMessage("Clob Data Fetch Error",
                        "No data was found for " + clobFile.DatabaseFile.Mnemonic);
            MessageLog.LogError("No data found on clob retrieval of " + clobFile.DatabaseFile.Mnemonic + " when executing command: " + command.CommandText);
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clobDir"></param>
        /// <param name="con"></param>
        private void GetDatabaseFileListForDirectory(ClobDirectory clobDir, OracleConnection con)
        {
            clobDir.DatabaseFileList = new List<DBClobFile>();

            ClobType ct = clobDir.ClobType;

            foreach (Table table in ct.Tables)
            {
                OracleCommand command = con.CreateCommand();
                command.CommandText = table.GetFileListCommand();
                OracleDataReader reader;
                try
                {
                    reader = command.ExecuteReader();
                }
                catch (InvalidOperationException _e)
                {
                    command.Dispose();
                    Common.ShowErrorMessage("Directory Comparison Error",
                            "An invalid operation occurred when retriving the file list for  " + ct.Name + ". Check the logs for more information.");
                    MessageLog.LogError("Error comparing to database: " + _e.Message + " when executing command " + command.CommandText);
                    return;
                }

                while (reader.Read())
                {
                    DBClobFile dbClobFile = new DBClobFile();
                    dbClobFile.Mnemonic = reader.GetString(0);
                    dbClobFile.MimeType = table.columns.Find(x => x.ColumnPurpose == Column.Purpose.MIME_TYPE) != null ? reader.GetString(1) : null;
                    dbClobFile.Filename = this.ConvertMnemonicToFilename(dbClobFile.Mnemonic, table, dbClobFile.MimeType);
                    dbClobFile.ParentTable = table;
                    clobDir.DatabaseFileList.Add(dbClobFile);
                }
                reader.Close();
                command.Dispose();
            }
        }

        private string GetClobFooterMessage(string _mimeType)
        {
            return (_mimeType == "text/javascript" ? "/*" : "<!--")
                + " Last clobbed by user " + Environment.UserName
                + " on machine " + Environment.MachineName
                + " at " + DateTime.Now
                + " (Lobster build " + Common.RetrieveLinkerTimestamp().ToShortDateString() + ")"
                + (_mimeType == "text/javascript" ? "*/" : "-->");
        }

        public string ConvertFilenameToMnemonic(ClobFile _clobFile, Table _table, string _mimeType)
        {
            Debug.Assert(_clobFile.LocalFile != null);
            string mnemonic = Path.GetFileNameWithoutExtension(_clobFile.LocalFile.FileInfo.Name);
            if (_table.columns.Find(x => x.ColumnPurpose == Column.Purpose.MIME_TYPE) != null)
            {
                MimeTypeList.MimeType mt = this.MimeList.mimeTypes.Find(x => x.name == _mimeType);
                if (mt == null)
                {
                    throw new ArgumentException("Unknown mime-to-prefix key " + _mimeType);
                }

                if (mt.prefix.Length > 0)
                {
                    mnemonic = mt.prefix + '/' + mnemonic;
                }
            }

            return mnemonic;
        }

        public string ConvertMnemonicToFilename(string mnemonic, Table _table, string _mimeType)
        {
            string filename = mnemonic;

            string prefix = null;
            if (mnemonic.Contains('/'))
            {
                prefix = mnemonic.Substring(0, mnemonic.LastIndexOf('/'));
                filename = mnemonic.Substring(mnemonic.LastIndexOf('/') + 1);
            }

            // Assume xml data types for tables without a datatype column
            if (_table.columns.Find(x => x.ColumnPurpose == Column.Purpose.MIME_TYPE) == null || prefix == null)
            {
                filename += _table.DefaultExtension ?? ".xml";
            }
            else
            {
                MimeTypeList.MimeType mt = this.MimeList.mimeTypes.Find(x => x.name == _mimeType);

                if (mt == null)
                {
                    throw new ArgumentException("Unkown mime-to-extension key " + _mimeType);
                }
                filename += mt.extension;
            }
            return filename;
        }

        [Serializable]
        private class ConnectionDirNotFoundException : Exception
        {
            public ConnectionDirNotFoundException()
            {
            }

            public ConnectionDirNotFoundException(string message) : base(message)
            {
            }

            public ConnectionDirNotFoundException(string message, Exception innerException) : base(message, innerException)
            {
            }

            protected ConnectionDirNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
            {
            }
        }
    }
}
