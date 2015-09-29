//-----------------------------------------------------------------------
// <copyright file="LocalClobFile.cs" company="marshl">
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
//      'My advice is: stay there! And don't get mixed up with these outlandish folk.'
//          -- Farmer Maggot, to Frodo
// 
//      [ _The Lord of the Rings_, I/iv: "A Shortcut to Mushrooms"]
//
//-----------------------------------------------------------------------
namespace Lobster
{
    using System.IO;

    /// <summary>
    /// Used to store information about the local version of a ClobFile.
    /// </summary>
    public class LocalClobFile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LocalClobFile"/> class.
        /// </summary>
        /// <param name="fileInfo">The file reference attached to this file.</param>
        public LocalClobFile(FileInfo fileInfo)
        {
            this.Info = fileInfo;
        }

        /// <summary>
        /// The info of the file this local file is for.
        /// </summary>
        public FileInfo Info { get; private set; }
    }
}
