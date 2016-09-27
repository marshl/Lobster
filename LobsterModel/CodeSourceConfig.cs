//-----------------------------------------------------------------------
// <copyright file="CodeSourceConfig.cs" company="marshl">
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
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;
    using Properties;

    /// <summary>
    /// Used to store information about a database connection, loaded directly from an XML file.
    /// </summary>
    public class CodeSourceConfig : SerializableObject
    {
        /// <summary>
        /// Gets or sets the name of the connection. This is for display purposes only.
        /// </summary>
        public string Name { get; set; }

        public List<ConnectionConfig> ConnectionConfigList { get; set; }

        /// <summary>
        /// Gets or sets the file from which this DatabaseConfig was loaded.
        /// </summary>
        [XmlIgnore]
        public string FileLocation { get; set; }

        [XmlIgnore]
        public string CodeSourceDirectory
        {
            get
            {
                return Directory.GetParent(this.FileLocation).FullName;
            }
        }

        /// <summary>
        /// Gets the location of the ClobType directory for this connection.
        /// </summary>
        public string ClobTypeDirectory
        {
            get
            {
                return Path.Combine(this.CodeSourceDirectory, Settings.Default.ClobTypeDirectoryName);
            }
        }

        /// <summary>
        /// Writes a <see cref="DatabaseConfig"/> out to file.
        /// </summary>
        /// <param name="filename">The file to write to.</param>
        /// <param name="config">The <see cref="DatabaseConfig"/> to serialise.</param>
        public static void SerialiseToFile(string filename, CodeSourceConfig config)
        {
            XmlSerializer xmls = new XmlSerializer(typeof(CodeSourceConfig));
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
        public static CodeSourceConfig LoadCodeSourceConfig(string codeSourceDirectory)
        {
            MessageLog.LogInfo($"Loading Lobster CodeSource Directory {codeSourceDirectory}");

            FileInfo configFile = new FileInfo(Path.Combine(codeSourceDirectory, Settings.Default.DatabaseConfigFileName));

            if(!configFile.Exists)
            {
                MessageLog.LogError($"{configFile.FullName} could not be found.");
                return null;
            }

            CodeSourceConfig config;
            string schema = Settings.Default.DatabaseConfigSchemaFilename;
            try
            {
                bool result = Utils.DeserialiseXmlFileUsingSchema(configFile.FullName, schema, out config);
                if (!result)
                {
                    config.Name = Path.GetDirectoryName(codeSourceDirectory);
                }
            }
            catch (Exception e) when (e is FileNotFoundException || e is InvalidOperationException || e is XmlException || e is XmlSchemaValidationException || e is IOException)
            {
                MessageLog.LogError($"An error occurred when loading the ClobType {configFile.FullName}: {e}");
                return null;
            }

            config.FileLocation = configFile.FullName;
            return config;
        }

        /// <summary>
        /// Loads each of the  <see cref="DatabaseConfig"/> files in the connection directory, and returns the list.
        /// </summary>
        /// <returns>All valid config files in the connection directory.</returns>
        public static List<CodeSourceConfig> GetConfigList()
        {
            List<CodeSourceConfig> configList = new List<CodeSourceConfig>();

            if (Settings.Default.CodeSourceDirectories == null || Settings.Default.CodeSourceDirectories.Count == 0)
            {
                return configList;
            }

            List<string> invalidDirectories = new List<string>();
            foreach (string directoryName in Settings.Default.CodeSourceDirectories)
            {
                string configFile = Path.Combine(directoryName, Settings.Default.DatabaseConfigFileName);
                if (!File.Exists(configFile))
                {
                    invalidDirectories.Add(directoryName);
                    continue;
                }

                CodeSourceConfig connection = CodeSourceConfig.LoadCodeSourceConfig(configFile);
                if (connection != null)
                {
                    configList.Add(connection);
                }
            }

            foreach (string str in invalidDirectories)
            {
                Settings.Default.CodeSourceDirectories.Remove(str);
            }

            Settings.Default.Save();

            return configList;
        }

        /// <summary>
        /// INitialises the given directory with the necessary configuration files and folders.
        /// </summary>
        /// <param name="directory">The directory to initialise.</param>
        /// <param name="newConfig">The database configuration file that is created.</param>
        /// <returns>True if the directory was set up  correctly, false if there was an issue setting it up, or if the directory already has them.</returns>
        public static bool InitialiseCodeSourceDirectory(string directory, ref CodeSourceConfig newConfig)
        {
            try
            {
                Directory.CreateDirectory(Path.Combine(directory, Settings.Default.ClobTypeDirectoryName));
                newConfig = new CodeSourceConfig();
                string configFile = Path.Combine(directory, Settings.Default.DatabaseConfigFileName);
                CodeSourceConfig.SerialiseToFile(configFile, newConfig);
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
    }
}
