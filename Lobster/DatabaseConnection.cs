//-----------------------------------------------------------------------
// <copyright file="filename.cs" company="marshl">
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
//
//      'What did the Men of old use them for?' asked Pippin, ...
//      'To see far off, and to converse in thought with one another,' said Gandalf
//          [ _The Lord of the Rings_, III/xi: "The Palantir"]
/// 
// </copyright>
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

    /// <summary>

    /// </summary>
    [XmlType("DatabaseConfig")]
    public class DatabaseConnection : ICloneable
    {
        /// <summary>
        /// 
        /// </summary>
        [DisplayName("Name")]
        [Description("The name of the connection. This is for display purposes only.")]
        [XmlElement("name")]
        public string name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("Host")]
        [Description("The host of the database.")]
        [Category("Database")]
        [XmlElement("host")]
        public string host { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("Port")]
        [Description("The port the database is listening on. Usually 1521 for Oracle.")]
        [Category("Database")]
        [XmlElement("port")]
        public string port { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("SID")]
        [Description("The Oracle System ID of the database.")]
        [Category("Database")]
        [XmlElement("sid")]
        public string sid { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("User Name")]
        [Description("The name of the user/schema to connect as. It is important to connect as a user with the privileges to access every table that could be modified by Lobster.")]
        [Category("Database")]
        [XmlElement("username")]
        public string username { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("Password")]
        [Description("The password to connect with.")]
        [Category("Database")]
        [XmlElement("password")]
        public string password { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("CodeSource Directory")]
        [Description("This is the location of the CodeSource directory that is used for this database. If it is invalid, Lobster will prompt you as it starts up.")]
        [Editor(typeof(FolderNameEditor), typeof(UITypeEditor))]
        [Category("Directories")]
        [XmlElement("codeSource")]
        public string codeSource { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("Pooling")]
        [Description("If pooling is enabled, when Lobster connects to the Oracle database Oracle will remember the connection for a time, and reuse it if the same computer connects using the same connection string.")]
        [XmlElement("usePooling")]
        public bool usePooling { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("ClobType Directory")]
        [Description("ClobTypes are Lobster specific Xml files for describing the different tables located on the database and the rules that govern them.")]
        [Editor(typeof(FolderNameEditor), typeof(UITypeEditor))]
        [Category("Directories")]
        [XmlElement("clobTypeDir")]
        public string clobTypeDir { get; set; }

        [XmlIgnore]
        [Browsable(false)]
        public LobsterModel ParentModel { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlIgnore]
        [Browsable(false)]
        public string fileLocation;

        /// <summary>
        /// 
        /// </summary>
        [XmlIgnore]
        [Browsable(false)]
        public List<ClobType> clobTypeList;

        /// <summary>
        /// 
        /// </summary>
        [XmlIgnore]
        [Browsable(false)]
        public Dictionary<ClobType, ClobDirectory> clobTypeToDirectoryMap;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_fullpath"></param>
        /// <param name="_connection"></param>
        public static void Serialise(string _fullpath, DatabaseConnection _connection)
        {
            XmlSerializer xmls = new XmlSerializer(typeof(DatabaseConnection));
            using (StreamWriter streamWriter = new StreamWriter(_fullpath))
            {
                xmls.Serialize(streamWriter, _connection);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void LoadClobTypes()
        {
            this.clobTypeList = new List<ClobType>();
            DirectoryInfo dirInfo = new DirectoryInfo(this.clobTypeDir);
            if (!dirInfo.Exists)
            {
                MessageBox.Show(this.clobTypeDir + " could not be found.", "ClobType Load Failed", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                MessageLog.LogWarning("The directory " + dirInfo + " could not be found when loading " + this.clobTypeDir);
                return;
            }

            foreach (FileInfo file in dirInfo.GetFiles("*.xml"))
            {
                try
                {
                    MessageLog.LogInfo("Loading ClobType file " + file.FullName);
                    ClobType clobType = Common.DeserialiseXmlFileUsingSchema<ClobType>(file.FullName, "LobsterSettings/ClobType.xsd");

                    clobType.Initialise(this);
                    clobType.File = file;
                    this.clobTypeList.Add(clobType);
                }
                catch (Exception _e)
                {
                    if (_e is InvalidOperationException || _e is XmlException || _e is XmlSchemaValidationException || _e is IOException)
                    {
                        MessageBox.Show("The ClobType " + file.FullName + " failed to load. Check the log for more information.", "ClobType Load Failed",
                            MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                        MessageLog.LogError("An error occurred when loading the ClobType " + file.Name + " " + _e);
                        continue;
                    }

                    throw;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_model"></param>
        public void PopulateClobDirectories(LobsterModel _model)
        {
            this.clobTypeToDirectoryMap = new Dictionary<ClobType, ClobDirectory>();
            foreach (ClobType clobType in this.clobTypeList)
            {
                ClobDirectory clobDir = new ClobDirectory(clobType);
                bool success = clobDir.BuildDirectoryTree();
                if (success)
                {
                    this.clobTypeToDirectoryMap.Add(clobType, clobDir);
                }
            }
        }

        /// <summary>
        /// Finds all currently unlocked files in all clob types and populates he input list with them.
        /// </summary>
        /// <param name="workingFileList">The file list to populate.</param>
        public void GetWorkingFiles(ref List<ClobFile> workingFileList)
        {
            if (this.clobTypeToDirectoryMap == null)
            {
                return;
            }

            foreach (KeyValuePair<ClobType, ClobDirectory> pair in this.clobTypeToDirectoryMap)
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
            DatabaseConnection other = new DatabaseConnection();
            other.name = this.name;
            other.host = this.host;
            other.sid = this.sid;
            other.port = this.port;
            other.username = this.username;
            other.password = this.password;
            other.codeSource = this.codeSource;
            other.usePooling = this.usePooling;
            other.clobTypeDir = this.clobTypeDir;
            other.fileLocation = this.fileLocation;
            other.ParentModel = this.ParentModel;

            other.clobTypeList = new List<ClobType>();
            if (this.clobTypeList == null)
            {
                this.clobTypeList = new List<ClobType>();
            }

            foreach (ClobType clobType in this.clobTypeList)
            {
                other.clobTypeList.Add((ClobType)clobType.Clone());
            }

            return other;
        }
    }
}
