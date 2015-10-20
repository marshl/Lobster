//-----------------------------------------------------------------------
// <copyright file="DatabaseConnection.cs" company="marshl">
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
//      'What did the Men of old use them for?' asked Pippin, ...
//      'To see far off, and to converse in thought with one another,' said Gandalf
//
//      [ _The Lord of the Rings_, III/xi: "The Palantir"]
// 
//-----------------------------------------------------------------------
namespace Lobster
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.IO;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;
    using Properties;

    /// <summary>
    /// Used to define how a database should be connected to, and where the ClobTypes are stored for it.
    /// </summary>
    [XmlType("DatabaseConfig")]
    public class DatabaseConnection : ICloneable
    {
        public DatabaseConnection(DatabaseConfig config)
        {
            this.Config = config;
        }

        public DatabaseConfig Config { get; set; }

        /// <summary>
        /// The Lobster model that is the parent of this connection.
        /// </summary>
        [XmlIgnore]
        [Browsable(false)]
        public LobsterModel ParentModel { get; set; }

        /// <summary>
        /// The name of the file where this was loaded from.
        /// </summary>
        [XmlIgnore]
        [Browsable(false)]
        public string FileLocation { get; set; }

        /// <summary>
        /// The list of Clob Types that are loaded from the files location in the directory defined by ClobTypeDir.
        /// </summary>
        [XmlIgnore]
        [Browsable(false)]
        public List<ClobType> ClobTypeList { get; set; }

        /// <summary>
        /// A mapping of the ClobTypes to the ClobDirectories that are created when the database is connected to.
        /// </summary>
        [XmlIgnore]
        [Browsable(false)]
        public Dictionary<ClobType, ClobDirectory> ClobTypeToDirectoryMap { get; set; }

        /// <summary>
        /// Loads each of the xml files in the ClobTypeDir (if they are valid).
        /// </summary>
        public void LoadClobTypes()
        {
            this.ClobTypeList = new List<ClobType>();
            DirectoryInfo dirInfo = new DirectoryInfo(this.Config.ClobTypeDir);
            if (!dirInfo.Exists)
            {
                MessageBox.Show(this.Config.ClobTypeDir + " could not be found.", "ClobType Load Failed", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                MessageLog.LogWarning("The directory " + dirInfo + " could not be found when loading connection " + this.Config.Name);
                return;
            }

            foreach (FileInfo file in dirInfo.GetFiles("*.xml"))
            {
                try
                {
                    MessageLog.LogInfo("Loading ClobType file " + file.FullName);
                    ClobType clobType = Common.DeserialiseXmlFileUsingSchema<ClobType>(file.FullName, Settings.Default.ClobTypeSchemaFilename);

                    clobType.Initialise(this);
                    clobType.FilePath = file.FullName;
                    this.ClobTypeList.Add(clobType);
                }
                catch (Exception e)
                {
                    if (e is InvalidOperationException || e is XmlException || e is XmlSchemaValidationException || e is IOException)
                    {
                        MessageBox.Show(
                            "The ClobType " + file.FullName + " failed to load. Check the log for more information.", 
                            "ClobType Load Failed",
                            MessageBoxButtons.OK, 
                            MessageBoxIcon.Error, 
                            MessageBoxDefaultButton.Button1);
                        MessageLog.LogError("An error occurred when loading the ClobType " + file.Name + " " + e);
                        continue;
                    }

                    throw;
                }
            }
        }

        /// <summary>
        /// Creates the ClobDirectories for each of the ClobTypes in this connection.
        /// </summary>
        public void PopulateClobDirectories()
        {
            this.ClobTypeToDirectoryMap = new Dictionary<ClobType, ClobDirectory>();
            foreach (ClobType clobType in this.ClobTypeList)
            {
                ClobDirectory clobDir = new ClobDirectory(clobType);
                bool success = clobDir.BuildDirectoryTree();
                if (success)
                {
                    this.ClobTypeToDirectoryMap.Add(clobType, clobDir);
                }
            }
        }

        /// <summary>
        /// Finds all currently unlocked files in all clob types and populates he input list with them.
        /// </summary>
        /// <param name="workingFileList">The file list to populate.</param>
        public void GetWorkingFiles(ref List<ClobFile> workingFileList)
        {
            if (this.ClobTypeToDirectoryMap == null)
            {
                return;
            }

            foreach (KeyValuePair<ClobType, ClobDirectory> pair in this.ClobTypeToDirectoryMap)
            {
                pair.Value.GetWorkingFiles(ref workingFileList);
            }
        }

        /// <summary>
        /// Creates a deep copy of this DatabaseConnection and returns it.
        /// </summary>
        /// <returns>The new clone.</returns>
        public object Clone()
        {
            DatabaseConnection other = new DatabaseConnection((DatabaseConfig)this.Config.Clone());

            other.FileLocation = this.FileLocation;
            other.ParentModel = this.ParentModel;

            other.ClobTypeList = new List<ClobType>();
            if (this.ClobTypeList == null)
            {
                this.ClobTypeList = new List<ClobType>();
            }

            foreach (ClobType clobType in this.ClobTypeList)
            {
                other.ClobTypeList.Add((ClobType)clobType.Clone());
            }

            return other;
        }
    }
}
