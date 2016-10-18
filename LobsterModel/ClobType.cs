﻿//-----------------------------------------------------------------------
// <copyright file="ClobType.cs" company="marshl">
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

    /// <summary>
    /// A ClobType defines a particular usage of the database. In its most common form, A ClobType contains a single table that maps to a single directory on the file system.
    /// ClobTypes are stored as Xml files which are deserialized into this structure.
    /// </summary>
    [Obsolete]
    public class ClobType : SerializableObject
    {
        /// <summary>
        /// Gets or sets the display name for this ClobType. This value has no functional impact, 
        /// and is used for display purposes only.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the name of the directory in CodeSource to be used for this ClobType. Directory separators can be used.
        /// </summary>
        public string Directory { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not all subdirectories under the specified folder should also be used.
        /// </summary>
        public bool IncludeSubDirectories { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not a file modified in this directory will trigger an update.
        /// </summary>
        public bool AllowAutomaticUpdates { get; set; } = true;

        /// <summary>
        /// Gets or sets the tables that files for this ClobType are stored in.
        /// ClobTypes usually have only a single table, but if there is more than one, then the user will be asked which to use when inserting a new file.
        /// </summary>
        [XmlArray]
        public List<Table> Tables { get; set; }

        /// <summary>
        /// Gets or sets the path of the file from where this ClobType was deserialised.
        /// </summary>
        [XmlIgnore]
        public string FilePath { get; set; }

        /// <summary>
        /// Serializes a ClobType and writes it out to the given filename.
        /// </summary>
        /// <param name="filename">The name of the file to write to.</param>
        /// <param name="clobType">The ClobType to serialize.</param>
        public static void Serialise(string filename, ClobType clobType)
        {
            XmlSerializer xmls = new XmlSerializer(typeof(ClobType));
            using (StreamWriter streamWriter = new StreamWriter(filename))
            {
                xmls.Serialize(streamWriter, clobType);
            }
        }

        /// <summary>
        /// Loads each of the  <see cref="DatabaseConfig"/> files in the connection directory, and returns the list.
        /// </summary>
        /// <param name="clobTypeDirectory">The directory to load the clob types from.</param>
        /// <returns>All valid config files in the connection directory.</returns>
        public static List<ClobType> GetClobTypeList(string clobTypeDirectory)
        {
            List<ClobType> clobTypeList = new List<ClobType>();

            DirectoryInfo dir = new DirectoryInfo(clobTypeDirectory);

            if (!dir.Exists)
            {
                return clobTypeList;
            }

            foreach (FileInfo filename in dir.GetFiles("*.xml"))
            {
                ClobType clobType = ClobType.LoadClobType(filename.FullName);
                if (clobType != null)
                {
                    clobTypeList.Add(clobType);
                }
            }

            return clobTypeList;
        }

        /// <summary>
        /// Loads the clob type with the given filepath and returns it.
        /// </summary>
        /// <param name="fullpath">The name of the file to load.</param>
        /// <returns>The ClobType, if loaded successfully, otherwise null.</returns>
        public static ClobType LoadClobType(string fullpath)
        {
            MessageLog.LogInfo($"Loading ClobType {fullpath}");
            ClobType clobType;
            try
            {
                string schema = Settings.Default.ClobTypeSchemaFilename;
                bool result = Utils.DeserialiseXmlFileUsingSchema(fullpath, schema, out clobType);

                if (!result)
                {
                    clobType.Name = Path.GetFileNameWithoutExtension(Path.GetFileName(fullpath));
                }
            }
            catch (Exception e) when (e is FileNotFoundException || e is InvalidOperationException || e is XmlException || e is IOException)
            {
                MessageLog.LogError($"An error occurred when loading the ClobType {fullpath}: {e}");
                return null;
            }

            clobType.FilePath = fullpath;
            return clobType;
        }

        /// <summary>
        /// Initializes values that cannot be set during deserialisation.
        /// </summary>
        public void Initialise()
        {
            this.Tables?.ForEach(x => x.Initialise());
        }
    }
}
