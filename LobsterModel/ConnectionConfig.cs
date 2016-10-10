//-----------------------------------------------------------------------
// <copyright file="ConnectionConfig.cs" company="marshl">
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
//      "I have not brought you hither to be instructed by you, but to give you a choice."
//          - Saruman the Wise
//
//      [ _The Lord of the Rings_, II/ii: "The Council of Elrond"]
//-----------------------------------------------------------------------
namespace LobsterModel
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Sockets;
    using System.Security;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;
    using Oracle.ManagedDataAccess.Client;
    using Properties;

    /// <summary>
    /// Used to store information about a database connection, loaded directly from an XML file.
    /// </summary>
    [Serializable]
    public class ConnectionConfig
    {
        /// <summary>
        /// Gets or sets the name of the connection. This is for display purposes only.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the host of the database.
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// Gets or sets the port the database is listening on. Usually 1521 for Oracle.
        /// </summary>
        public string Port { get; set; }

        /// <summary>
        /// Gets or sets the Oracle System ID of the database.
        /// </summary>
        public string SID { get; set; }

        /// <summary>
        /// Gets or sets the name of the user/schema to connect as.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether pooling is enabled or not. 
        /// When enabled, Oracle will remember new connections for a time, and reuse it if the same computer connects using the same connection string.
        /// </summary>
        public bool UsePooling { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user can enable automatic file updates when a file is modified.
        /// </summary>
        public bool AllowAutomaticClobbing { get; set; } = true;
        
        [XmlIgnore]
        public CodeSourceConfig Parent { get; set; }

        public bool IsRestrictedEnvironment { get; set; }

        /// <summary>
        /// Tests if a connection could be made.
        /// </summary>
        /// <param name="password">The password to connect to the database with.</param>
        /// <param name="e">The exception that was raised, if any.</param>
        /// <returns>True if the connection was successful, otherwise false.</returns>
        public bool TestConnection(SecureString password, ref Exception e)
        {
            try
            {
                OracleConnection con = this.OpenSqlConnection(password);
                con.Close();
                return true;
            }
            catch (ConnectToDatabaseException ex)
            {
                e = ex;
                return false;
            }
        }

        /// <summary>
        /// Opens a new OracleConnection and returns it.
        /// </summary>
        /// <param name="password">The password to connect to the database with.</param>
        /// <returns>A new connectionif it opened successfully, otherwise null.</returns>
        public OracleConnection OpenSqlConnection(SecureString password)
        {
            try
            {
                OracleConnection con = new OracleConnection();
                con.ConnectionString = "User Id=" + this.Username
                    + (password.Length == 0 ? null : ";Password=" + Utils.SecureStringToString(password))
                    + ";Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)("
                    + $"HOST={this.Host})"
                    + $"(PORT={this.Port})))(CONNECT_DATA="
                    + $"(SID={this.SID})(SERVER=DEDICATED)))"
                    + $";Pooling=" + (this.UsePooling ? "true" : "false");

                MessageLog.LogInfo($"Connecting to database {this.Name}");
                con.Open();
                return con;
            }
            catch (Exception e) when (e is InvalidOperationException || e is OracleException || e is ArgumentException || e is SocketException)
            {
                MessageLog.LogError($"Connection to Oracle failed: {e.Message}");
                throw new ConnectToDatabaseException($"Failed to open connection: {e.Message}", e);
            }
        }
    }
}
