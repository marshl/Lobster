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
//
//      They were on ponies, and each pony was slung about with all kinds of baggages, packages,
//      parcels, and paraphernalia.
//          [ _The Hobbit_, II "Roast Mutton" ] 
//
// </copyright>
//-----------------------------------------------------------------------
namespace Lobster
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Xml.Serialization;

    /// <summary>
    /// A ClobType defines a particular usage of the database. In its most common form, A ClobType contains a single table that maps to a single directory on the file system.
    /// ClobTypes are stored as Xml files which are deserialized into this structure.
    /// </summary>
    [XmlType("clobtype")]
    public class ClobType : ICloneable
    {
        /// <summary>
        /// The display name for this ClobType. This value has no functional impact, 
        /// and is used for display purposes only.
        /// </summary>
        [DisplayName("Name")]
        [Description("The display name")]
        [XmlElement("name")]
        public string Name { get; set; }

        /// <summary>
        /// The name of the directory in CodeSource to be used for this ClobType. Directory separators can be used.
        /// </summary>
        [DisplayName("Directory")]
        [Description("The name of the directory in CodeSource to be used for this ClobType. Directory separators can be used.")]
        [XmlElement("directory")]
        public string Directory { get; set; }

        /// <summary>
        /// Whether or not all subdirectories under the specified folder should also be used.
        /// </summary>
        [DisplayName("Include Subdirectories")]
        [Description("Whether or not all subdirectories under the specified folder should also be used.")]
        [XmlElement("includeSubDirectories")]
        public bool IncludeSubDirectories { get; set; }

        /// <summary>
        /// Most ClobTypes use only a single table, but if there is more than one, then the user will be asked which to use when inserting a new file.
        /// </summary>
        [DisplayName("Table List")]
        [Description("The tables used by this ClobType")]
        [XmlArray("tables")]
        public List<Table> Tables { get; set; }

        /// <summary>
        /// The file that stored this ClobType.
        /// </summary>
        [XmlIgnore]
        [Browsable(false)]
        public string FilePath { get; set; }

        /// <summary>
        /// The <see cref="DatabaseConnection"/> object that stores this ClobType.
        /// </summary>
        [XmlIgnore]
        [Browsable(false)]
        public DatabaseConnection ParentConnection { get; set; }

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
        /// <param name="parentConnection">The DatabaseConnection to set as this ClobType's parent.</param>
        public void Initialise(DatabaseConnection parentConnection)
        {
            this.Tables.ForEach(x => x.LinkColumns());
            this.ParentConnection = parentConnection;
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
            copy.ParentConnection = this.ParentConnection;
            copy.FilePath = this.FilePath;

            copy.Tables = new List<Table>();
            if (this.Tables != null)
            {
                foreach (Table table in this.Tables)
                {
                    copy.Tables.Add((Table)table.Clone());
                }
            }

            copy.Initialise(this.ParentConnection);
            return copy;
        }
    }
}
