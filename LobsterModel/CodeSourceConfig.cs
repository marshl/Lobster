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
//      "But great though his lore may be, it must have a source."
//        - Gandalf the Grey
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
    using System.Runtime.Serialization;
    using Properties;

    /// <summary>
    /// Used to store information about a database connection, loaded directly from an XML file.
    /// </summary>
    [DataContract(IsReference = true)]
    [KnownType(typeof(ConnectionConfig))]
    public class CodeSourceConfig
    {
        /// <summary>
        /// Gets or sets the name of the connection. This is for display purposes only.
        /// </summary>
        [DataMember(Order = 1)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the databases that this CodeSource cna be connected to.
        /// </summary>
        [DataMember(Order = 2)]
        public List<ConnectionConfig> ConnectionConfigList { get; set; } = new List<ConnectionConfig>();

        /// <summary>
        /// Gets the path of the file from which this DatabaseConfig was loaded.
        /// </summary>
        public string FileLocation { get; set; }

        /// <summary>
        /// Gets the path of the directory for this CodeSource.
        /// </summary>
        public string CodeSourceDirectory
        {
            get
            {
                return Directory.GetParent(this.FileLocation).FullName;
            }
        }

        public string DirectoryDescriptorFolder
        {
            get
            {
                return Path.Combine(this.CodeSourceDirectory, Settings.Default.DirectoryDescriptorFolderName);
            }
        }

        /// <summary>
        /// Writes a <see cref="DatabaseConfig"/> out to file.
        /// </summary>
        /// <param name="filename">The file to write to.</param>
        /// <param name="config">The <see cref="DatabaseConfig"/> to serialise.</param>
        public void SerialiseToFile()
        {
            DataContractSerializer xmls = new DataContractSerializer(typeof(CodeSourceConfig));
            XmlWriterSettings writerSettings = new XmlWriterSettings();
            writerSettings.Indent = true;
            writerSettings.IndentChars = "  ";
            using (StreamWriter streamWriter = new StreamWriter(this.FileLocation))
            using (XmlWriter xmlWriter = XmlWriter.Create(streamWriter, writerSettings))
            {
                xmls.WriteObject(xmlWriter, this);
            }
        }
    }
}
