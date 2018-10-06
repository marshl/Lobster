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
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;

    /// <summary>
    /// A class used for serialising and deserialising the configuration optiosn for a managed directory within a CodeSource directory.
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
        public bool PushOnFileChange { get; set; } = true;

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
        /// Gets or sets the statement used to determine the mime type of the file.
        /// </summary>
        [DataMember]
        public string FileMimeTypeStatement { get; set; }

        /// <summary>
        /// Gets or sets the statement used to determine the type of data in the database (either CLOB or BLOB).
        /// </summary>
        [DataMember]
        public string FileDataTypeStatement { get; set; }

        /// <summary>
        /// Gets or sets the path where this descriptor was loaded from and will be saved to when <see cref="Save"/> is called.
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Saves this DirectoryDescriptor to file with the path <see cref="FilePath"/> .
        /// </summary>
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
