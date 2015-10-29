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
namespace Lobster
{
    using System;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.IO;
    using System.Windows.Forms.Design;
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
        /// The name of the connection. This is for display purposes only.
        /// </summary>
        [DisplayName("Name")]
        [Description("The name of the connection. This is for display purposes only.")]
        [XmlElement("name")]
        public string Name { get; set; }

        /// <summary>
        /// The host of the database.
        /// </summary>
        [DisplayName("Host")]
        [Description("The host of the database.")]
        [Category("Database")]
        [XmlElement("host")]
        public string Host { get; set; }

        /// <summary>
        /// The port the database is listening on. Usually 1521 for Oracle.
        /// </summary>
        [DisplayName("Port")]
        [Description("The port the database is listening on. Usually 1521 for Oracle.")]
        [Category("Database")]
        [XmlElement("port")]
        public string Port { get; set; }

        /// <summary>
        /// The Oracle System ID of the database.
        /// </summary>
        [DisplayName("SID")]
        [Description("The Oracle System ID of the database.")]
        [Category("Database")]
        [XmlElement("sid")]
        public string SID { get; set; }

        /// <summary>
        /// The name of the user/schema to connect as.
        /// </summary>
        [DisplayName("User Name")]
        [Description("The name of the user/schema to connect as. It is important to connect as a user with the privileges to access every table that could be modified by Lobster.")]
        [Category("Database")]
        [XmlElement("username")]
        public string Username { get; set; }

        /// <summary>
        /// The password to connect with.
        /// </summary>
        [DisplayName("Password")]
        [Description("The password to connect with.")]
        [Category("Database")]
        [XmlElement("password")]
        public string Password { get; set; }

        /// <summary>
        /// This is the location of the CodeSource directory that is used for this database.
        /// </summary>
        [DisplayName("CodeSource Directory")]
        [Description("This is the location of the CodeSource directory that is used for this database. If it is invalid, Lobster will prompt you as it starts up.")]
        [Editor(typeof(FolderNameEditor), typeof(UITypeEditor))]
        [Category("Directories")]
        [XmlElement("codeSource")]
        public string CodeSource { get; set; }

        /// <summary>
        /// If pooling is enabled, when Lobster connects to the Oracle database Oracle will remember the connection for a time, and reuse it if the same computer connects using the same connection string.
        /// </summary>
        [DisplayName("Pooling")]
        [Description("If pooling is enabled, when Lobster connects to the Oracle database Oracle will remember the connection for a time, and reuse it if the same computer connects using the same connection string.")]
        [XmlElement("usePooling")]
        public bool UsePooling { get; set; }

        /// <summary>
        /// The directory name where ClobTypes are stored.
        /// </summary>
        [DisplayName("ClobType Directory")]
        [Description("ClobTypes are Lobster specific Xml files for describing the different tables located on the database and the rules that govern them.")]
        [Editor(typeof(FolderNameEditor), typeof(UITypeEditor))]
        [Category("Directories")]
        [XmlElement("clobTypeDir")]
        public string ClobTypeDir { get; set; }

        /// <summary>
        /// The file from which this DatabaseConfig was loaded.
        /// </summary>
        [Browsable(false)]
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
                config = Common.DeserialiseXmlFileUsingSchema<DatabaseConfig>(fullpath, Settings.Default.DatabaseConfigSchemaFilename);
            }
            catch (Exception e)
            {
                if (e is FileNotFoundException || e is InvalidOperationException || e is XmlException || e is XmlSchemaValidationException)
                {
                    Common.ShowErrorMessage(
                        "ClobType Load Failed",
                        "The DBConfig file " + fullpath + " failed to load. Check the log for more information.");
                    MessageLog.LogError("An error occurred when loading the ClobType " + fullpath + ": " + e);
                    return null;
                }

                throw;
            }

            config.FileLocation = fullpath;

            // If the CodeSource folder cannot be found, prompt the user for it
            if (config.CodeSource == null || !Directory.Exists(config.CodeSource))
            {
                string codeSourceDir = Common.PromptForDirectory("Please select your CodeSource directory for " + config.Name, null);
                if (codeSourceDir != null)
                {
                    config.CodeSource = codeSourceDir;
                    DatabaseConfig.SerialiseToFile(fullpath, config);
                }
                else
                {
                    // Ignore config files that don't have a valid CodeSource folder
                    return null;
                }
            }

            return config;
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
