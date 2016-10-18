//-----------------------------------------------------------------------
// <copyright file="MimeTypeList.cs" company="marshl">
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
//      'What does the writing say?' asked Frodo, who was trying to decipher the inscription 
//      on the arch. 'I thought I knew the elf-letters but I cannot read these.'
// 
//      [ _The Lord of the Rings_, II/iv: "A Journey in the Dark"]
//
//-----------------------------------------------------------------------
namespace LobsterModel
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;

    /// <summary>
    /// Deserialised from a file to store metedata about mime types.
    /// </summary>
    [Obsolete]
    public class MimeTypeList : SerializableObject
    {
        /// <summary>
        /// Gets or sets the list of different mime types.
        /// </summary>
        [XmlArray]
        public List<MimeType> MimeTypes { get; set; }

        /// <summary>
        /// Creates a comment to place at the bottom of a non-binary file of the given mime type.
        /// </summary>
        /// <param name="mimeType">The mime type of the file the footer is for.</param>
        /// <returns>The footer string.</returns>
        public static string GetClobFooterMessage(string mimeType)
        {
            string openingComment = "<!--";
            string closingComment = "-->";
            if (mimeType != null && (mimeType.Equals("text/javascript", StringComparison.OrdinalIgnoreCase) || mimeType.Equals("text/css", StringComparison.OrdinalIgnoreCase)))
            {
                openingComment = "/*";
                closingComment = "*/";
            }

            return $"{openingComment}"
                + $" Last clobbed by user {Environment.UserName}"
                + $" on machine {Environment.MachineName}"
                + $" at {DateTime.Now}"
                + $" (Lobster build {Utils.RetrieveLinkerTimestamp().ToShortDateString()})"
                + $"{closingComment}";
        }

        /// <summary>
        /// A single mime type.
        /// </summary>
        public class MimeType
        {
            /// <summary>
            /// Gets or sets the name of the mime type (e.g. text/js)
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the prefix as the database uses the mime type (e.g. img/)
            /// </summary>
            public string Prefix { get; set; }

            /// <summary>
            /// Gets or sets the file extension that would be used by files of this mime type (e.g. .png).
            /// </summary>
            public string Extension { get; set; }
        }
    }
}
