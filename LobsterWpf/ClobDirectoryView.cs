//-----------------------------------------------------------------------
// <copyright file="ClobDirectoryView.cs" company="marshl">
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
namespace LobsterWpf
{
    using System.IO;
    using LobsterModel;

    /// <summary>
    /// The view for the ClobDirectory model.
    /// </summary>
    public class ClobDirectoryView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClobDirectoryView"/> class.
        /// </summary>
        /// <param name="connection">The parent database connection of this directory.</param>
        /// <param name="clobDirectory">The ClobDirectory to use as the model.</param>
        public ClobDirectoryView(DatabaseConnection connection, ClobDirectory clobDirectory)
        {
            this.BaseClobDirectory = clobDirectory;
            this.DirectoryExists = System.IO.Directory.Exists(clobDirectory.GetFullPath(connection));
        }

        /// <summary>
        /// Gets the base clob directory for this view.
        /// </summary>
        public ClobDirectory BaseClobDirectory { get; }

        /// <summary>
        /// Gets the name of the clob type of the clob directory.
        /// </summary>
        public string Name
        {
            get
            {
                return this.BaseClobDirectory.ClobType.Name;
            }
        }

        /// <summary>
        /// Gets the directory name of the clob type.
        /// </summary>
        public string Directory
        {
            get
            {
                return this.BaseClobDirectory.ClobType.Directory;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the directory of this clob directory exists on the files system or not.
        /// </summary>
        public bool DirectoryExists { get; }
    }
}
