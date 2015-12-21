//-----------------------------------------------------------------------
// <copyright file="DatabaseConfig.cs" company="marshl">
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
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;
    using Properties;

    /// <summary>
    /// Used to store information about a database connection, loaded directly from an XML file.
    /// </summary>
    public class DatabaseConfig : ICloneable
    {
        /// <summary>
        /// Gets or sets the name of the connection. This is for display purposes only.
        /// </summary>
        [XmlElement("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the host of the database.
        /// </summary>
        [XmlElement("host")]
        public string Host { get; set; }

        /// <summary>
        /// Gets or sets the port the database is listening on. Usually 1521 for Oracle.
        /// </summary>
        [XmlElement("port")]
        public string Port { get; set; }

        /// <summary>
        /// Gets or sets the Oracle System ID of the database.
        /// </summary>
        [XmlElement("sid")]
        public string SID { get; set; }

        /// <summary>
        /// Gets or sets the name of the user/schema to connect as.
        /// </summary>
        [XmlElement("username")]
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the password to connect with.
        /// </summary>
        [XmlElement("password")]
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the location of the CodeSource directory that is used for this database.
        /// </summary>
        [XmlElement("codeSource")]
        public string CodeSource { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether pooling is enabled or not. 
        /// When enabled, Oracle will remember new connections for a time, and reuse it if the same computer connects using the same connection string.
        /// </summary>
        [XmlElement("usePooling")]
        public bool UsePooling { get; set; }

        /// <summary>
        /// Gets or sets the directory name where ClobTypes are stored.
        /// </summary>
        [XmlElement("clobTypeDir")]
        public string ClobTypeDir { get; set; }

        /// <summary>
        /// Gets or sets the file from which this DatabaseConfig was loaded.
        /// </summary>
        [XmlIgnore]
        public string FileLocation { get; set; }

        /// <summary>
        /// Writes a <see cref="DatabaseConfig"/> out to file.
        /// </summary>
        /// <param name="filename">The file to write to.</param>
        /// <param name="config">The <see cref="DatabaseConfig"/> to serialise.</param>
        public static void SerialiseToFile(string filename, DatabaseConfig config)
        {
            XmlSerializer xmls = new XmlSerializer(typeof(DatabaseConfig));
            using (StreamWriter streamWriter = new StreamWriter(filename))
            {
                xmls.Serialize(streamWriter, config);
            }
        }

        /// <summary>
        /// Deserialises the given file into a new <see cref="DatabaseConnection"/>.
        /// </summary>
        /// <param name="fullpath">The location of the file to deserialise.</param>
        /// <returns>The new <see cref="DatabaseConnection"/>.</returns>
        public static DatabaseConfig LoadDatabaseConfig(string fullpath)
        {
            MessageLog.LogInfo("Loading Database Config File " + fullpath);
            DatabaseConfig config;
            try
            {
                config = Utils.DeserialiseXmlFileUsingSchema<DatabaseConfig>(fullpath, Settings.Default.DatabaseConfigSchemaFilename);
            }
            catch (Exception e) when (e is FileNotFoundException || e is InvalidOperationException || e is XmlException || e is XmlSchemaValidationException || e is IOException)
            {
                MessageLog.LogError("An error occurred when loading the ClobType " + fullpath + ": " + e);
                return null;
            }
            
            config.FileLocation = fullpath;
            return config;
        }

        /// <summary>
        /// Loads each of the  <see cref="DatabaseConfig"/> files in the connection directory, and returns the list.
        /// </summary>
        /// <returns>All valid config files in the connection directory.</returns>
        public static List<DatabaseConfig> GetConfigList()
        {
            List<DatabaseConfig> configList = new List<DatabaseConfig>();

            if (!Model.IsConnectionDirectoryValid)
            {
                return configList;
            }

            foreach (string filename in Directory.GetFiles(Model.ConnectionDirectory, "*.xml"))
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
        /// Creates a deep copy of this DatabaseConfig
        /// </summary>
        /// <returns>The newly created copy.</returns>
        public object Clone()
        {
            DatabaseConfig other = new DatabaseConfig();
            other.Name = this.Name;
            other.Host = this.Host;
            other.SID = this.SID;
            other.Port = this.Port;
            other.Username = this.Username;
            other.Password = this.Password;
            other.CodeSource = this.CodeSource;
            other.UsePooling = this.UsePooling;
            other.ClobTypeDir = this.ClobTypeDir;

            other.FileLocation = this.FileLocation;

            return other;
        }
    }
}
