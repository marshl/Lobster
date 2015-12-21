//-----------------------------------------------------------------------
// <copyright file="ClobType.cs" company="marshl">
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
    using System.Xml.Schema;
    using System.Xml.Serialization;
    using Properties;

    /// <summary>
    /// A ClobType defines a particular usage of the database. In its most common form, A ClobType contains a single table that maps to a single directory on the file system.
    /// ClobTypes are stored as Xml files which are deserialized into this structure.
    /// </summary>
    [XmlType("clobtype")]
    public class ClobType : ICloneable
    {
        /// <summary>
        /// Gets or sets the display name for this ClobType. This value has no functional impact, 
        /// and is used for display purposes only.
        /// </summary>
        [XmlElement("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the name of the directory in CodeSource to be used for this ClobType. Directory separators can be used.
        /// </summary>
        [XmlElement("directory")]
        public string Directory { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not all subdirectories under the specified folder should also be used.
        /// </summary>
        [XmlElement("includeSubDirectories")]
        public bool IncludeSubDirectories { get; set; }

        /// <summary>
        /// Gets or sets the SQL statement that is used to update and insert files into the database.
        /// </summary>
        [XmlElement("loaderStatement")]
        public string LoaderStatement { get; set; }

        /// <summary>
        /// Gets or sets the tables that files for this ClobType are stored in.
        /// ClobTypes usually have only a single table, but if there is more than one, then the user will be asked which to use when inserting a new file.
        /// </summary>
        [XmlArray("tables")]
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
        /// Initializes values that cannot be set during deserialisation.
        /// </summary>
        public void Initialise()
        {
            this.Tables.ForEach(x => x.Initialise());
        }

        /// <summary>
        /// Creates and returns a deep copy of this ClobType.
        /// </summary>
        /// <returns>A boxed copy of this ClobType.</returns>
        public object Clone()
        {
            ClobType copy = new ClobType();
            copy.Name = this.Name;
            copy.Directory = this.Directory;
            copy.IncludeSubDirectories = this.IncludeSubDirectories;
            copy.FilePath = this.FilePath;

            copy.Tables = new List<Table>();
            if (this.Tables != null)
            {
                foreach (Table table in this.Tables)
                {
                    copy.Tables.Add((Table)table.Clone());
                }
            }

            copy.Initialise();
            return copy;
        }

        /// <summary>
        /// Loads each of the  <see cref="DatabaseConfig"/> files in the connection directory, and returns the list.
        /// </summary>
        /// <returns>All valid config files in the connection directory.</returns>
        public static List<ClobType> GetClobTypeList(string clobTypeDirectory)
        {
            List<ClobType> clobTypeList = new List<ClobType>();

            if (!Model.IsConnectionDirectoryValid)
            {
                return clobTypeList;
            }

            foreach (string filename in System.IO.Directory.GetFiles(clobTypeDirectory, "*.xml"))
            {
                ClobType clobType = ClobType.LoadClobType(filename);
                if ( clobType != null )
                {
                    clobTypeList.Add(clobType);
                }
            }

            return clobTypeList;
        }

        public static ClobType LoadClobType(string fullpath)
        {
            MessageLog.LogInfo("Loading Database Config File " + fullpath);
            ClobType clobType;
            try
            {
                clobType = Utils.DeserialiseXmlFileUsingSchema<ClobType>(fullpath, Settings.Default.ClobTypeSchemaFilename);
            }
            catch (Exception e) when (e is FileNotFoundException || e is InvalidOperationException || e is XmlException || e is XmlSchemaValidationException || e is IOException)
            {
                MessageLog.LogError("An error occurred when loading the ClobType " + fullpath + ": " + e);
                return null;
            }

            clobType.FilePath = fullpath;
            return clobType;
        }
    }
}
