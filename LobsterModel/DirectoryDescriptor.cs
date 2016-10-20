//-----------------------------------------------------------------------
// <copyright file="DirectoryDescriptor.cs" company="marshl">
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
//-----------------------------------------------------------------------
//
//      They were on ponies, and each pony was slung about with all kinds 
//      of baggages, packages, parcels, and paraphernalia.
//
//          [ _The Hobbit_, II "Roast Mutton" ] 
//
// </copyright>
//-----------------------------------------------------------------------
namespace LobsterModel
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml;
    using System.Xml.Serialization;
    using Properties;
    using System.Runtime.Serialization;

    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class DirectoryDescriptor
    {
        /// <summary>
        /// Gets or sets the display name for this Descriptor. This value has no functional impact, 
        /// and is used for display purposes only.
        /// </summary>
        [DataMember(IsRequired = true)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the name of the directory in CodeSource to be used for this Descriptor. Directory separators can be used.
        /// </summary>
        [DataMember(IsRequired = true)]
        public string DirectoryName { get; set; }

        /// <summary>
        /// Gets or sets the rules governing what files should available to this directory.
        /// </summary>
        [DataMember]
        public List<SearchRule> SearchRules { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not a file modified in this directory will trigger an update.
        /// </summary>
        [DataMember]
        public bool PushAutomatically { get; set; } = true;

        /// <summary>
        /// Gets or sets the data type that will be used if the <see cref="FileDataTypeStatement"/> does not exist (if it does, it will instead be used to get the file data type)
        /// </summary>
        [DataMember]
        public string DefaultDataType { get; set; }

        /// <summary>
        /// Gets or sets the SQL statement used to insert a new record into the database.
        /// </summary>
        [DataMember(IsRequired = true)]
        public string InsertStatement { get; set; }

        /// <summary>
        /// Gets or sets the SQL statement used to update an existing record in the database.
        /// </summary>
        [DataMember(IsRequired = true)]
        public string UpdateStatement { get; set; }

        /// <summary>
        /// Gets or sets the statement used to delete the database record.
        /// </summary>
        [DataMember]
        public string DeleteStatement { get; set; }

        /// <summary>
        /// Gets or sets the statement used to determine whether the file exists in the database.
        /// </summary>
        [DataMember(IsRequired = true)]
        public string DatabaseFileExistsStatement { get; set; }

        /// <summary>
        /// Gets or sets the statement used to fetch the file data from the database.
        /// </summary>
        [DataMember(IsRequired = true)]
        public string FetchStatement { get; set; }

        /// <summary>
        /// Gets or sets the statement used to fetch the binary file data from the database.
        /// </summary>
        [DataMember]
        public string FetchBinaryStatement { get; set; }

        /// <summary>
        /// Gets or sets the statement used to determine the type of data in the database (either CLOB or BLOB).
        /// </summary>
        [DataMember]
        public string FileDataTypeStatement { get; set; }

        public string FilePath { get; set; }

        /// <summary>
        /// Loads each of the  <see cref="DatabaseConfig"/> files in the connection directory, and returns the list.
        /// </summary>
        /// <param name="clobTypeDirectory">The directory to load the clob types from.</param>
        /// <returns>All valid config files in the connection directory.</returns>
        public static List<DirectoryDescriptor> GetDirectoryDescriptorList(string directoryDescriptorFolder)
        {
            List<DirectoryDescriptor> dirDescList = new List<DirectoryDescriptor>();

            DirectoryInfo dir = new DirectoryInfo(directoryDescriptorFolder);

            if (!dir.Exists)
            {
                return dirDescList;
            }

            foreach (FileInfo filename in dir.GetFiles("*.xml"))
            {
                var dirDesc = DirectoryDescriptor.LoadDirectoryDescriptor(filename.FullName);
                if (dirDesc != null)
                {
                    dirDescList.Add(dirDesc);
                }
            }

            return dirDescList;
        }

        /// <summary>
        /// Loads the clob type with the given filepath and returns it.
        /// </summary>
        /// <param name="fullpath">The name of the file to load.</param>
        /// <returns>The ClobType, if loaded successfully, otherwise null.</returns>
        public static DirectoryDescriptor LoadDirectoryDescriptor(string fullpath)
        {
            MessageLog.LogInfo($"Loading ClobType {fullpath}");
            DirectoryDescriptor dirDesc;
            try
            {
                string schema = Settings.Default.ClobTypeSchemaFilename;
                XmlSerializer xmls = new XmlSerializer(typeof(DirectoryDescriptor));
                FileStream stream = new FileStream(fullpath, FileMode.Open);
                dirDesc = (DirectoryDescriptor)xmls.Deserialize(stream);
            }
            catch (Exception e) when (e is FileNotFoundException || e is InvalidOperationException || e is XmlException || e is IOException)
            {
                MessageLog.LogError($"An error occurred when loading the ClobType {fullpath}: {e}");
                return null;
            }

            dirDesc.FilePath = fullpath;
            return dirDesc;
        }

        public void Save()
        {
            using (FileStream output = new FileStream(this.FilePath, FileMode.OpenOrCreate))
            {
                XmlSerializer xmls = new XmlSerializer(typeof(DirectoryDescriptor));
                xmls.Serialize(output, this);
            }
        }
    }
}
