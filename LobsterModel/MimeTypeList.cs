//-----------------------------------------------------------------------
// <copyright file="MimeTypeList.cs" company="marshl">
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
//      'What does the writing say?' asked Frodo, who was trying to decipher the inscription 
//      on the arch. 'I thought I knew the elf-letters but I cannot read these.'
// 
//      [ _The Lord of the Rings_, II/iv: "A Journey in the Dark"]
//
//-----------------------------------------------------------------------
namespace LobsterModel
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    /// <summary>
    /// Deserialised from a file to store metedata about mime types.
    /// </summary>
    public class MimeTypeList
    {
        /// <summary>
        /// The list of different mime types.
        /// </summary>
        [XmlArray("mimeTypes")]
        public List<MimeType> MimeTypes { get; set; }

        /// <summary>
        /// A single mime type.
        /// </summary>
        public class MimeType
        {
            /// <summary>
            /// The name of the mime type (e.g. text/js)
            /// </summary>
            [XmlElement("name")]
            public string Name { get; set; }

            /// <summary>
            /// The prefix as the database uses the mime type (e.g. img/)
            /// </summary>
            [XmlElement("prefix")]
            public string Prefix { get; set; }

            /// <summary>
            /// The file extension that would be used by files of this mime type (e.g. .png).
            /// </summary>
            [XmlElement("extension")]
            public string Extension { get; set; }
        }
    }
}
