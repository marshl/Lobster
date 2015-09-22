//-----------------------------------------------------------------------
// <copyright file="ClobFile.cs" company="marshl">
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
//      'Men multiply and the Firstborn decrease, and the two kindreds are estranged.'
//          -- Elrond
//
//      [ _The Lord of the Rings_, II/ii: "The Council of Elrond"]
//
//-----------------------------------------------------------------------
namespace Lobster
{
    /// <summary>
    /// A ClobFile is the connection between a <see cref="DBClobFile"/> and a <see cref="LocalClobFile"/>.
    /// A ClobFile is synchronised if both files exist, but it can be local-only or database-only.
    /// </summary>
    public class ClobFile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClobFile"/> class.
        /// </summary>
        /// <param name="parentClobNode">The node (file system directory) to set as this file's parent.</param>
        public ClobFile(ClobNode parentClobNode)
        {
            this.ParentClobNode = parentClobNode;
        }

        /// <summary>
        /// The ClobNode (file system directory) that contains this file.
        /// </summary>
        public ClobNode ParentClobNode { get; private set; }

        /// <summary>
        /// The file on the database linked to this file (if it exists).
        /// </summary>
        public DBClobFile DatabaseFile { get; set; }

        /// <summary>
        /// The local file that is linked to this file (if it exists).
        /// </summary>
        public LocalClobFile LocalFile { get; set; }

        /// <summary>
        /// Gets whether this file is only located on the users computer or not.
        /// </summary>
        public bool IsLocalOnly
        {
            get
            {
                return this.DatabaseFile == null && this.LocalFile != null;
            }
        }

        /// <summary>
        /// Gets whether this file is only found on the database or not.
        /// </summary>
        public bool IsDbOnly
        {
            get
            {
                return this.DatabaseFile != null && this.LocalFile == null;
            }
        }

        /// <summary>
        /// Gets whether this file is both on the database and on the users computer or not.
        /// </summary>
        public bool IsSynced
        {
            get
            {
                return this.DatabaseFile != null && this.LocalFile != null;
            }
        }

        /// <summary>
        /// Gets whether the local file is editable or not.
        /// </summary>
        public bool IsEditable
        {
            get
            {
                return !this.LocalFile.FileInfo.IsReadOnly;
            }
        }
    }
}
