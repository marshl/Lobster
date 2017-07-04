//-----------------------------------------------------------------------
// <copyright file="WatchedNode.cs" company="marshl">
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
namespace LobsterModel
{
    /// <summary>
    /// A single file or directory that is monitored for changes
    /// </summary>
    public abstract class WatchedNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WatchedNode"/> class.
        /// </summary>
        /// <param name="filePath">The path of the file or directory.</param>
        /// <param name="parent">The parent directory of this node.</param>
        public WatchedNode(string filePath, WatchedDirectory parent)
        {
            this.FilePath = filePath;
            this.ParentDirectory = parent;
        }

        /// <summary>
        /// Gets the full path of this node.
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// Gets the directory that is the parent of this node
        /// </summary>
        public WatchedDirectory ParentDirectory { get; }

        /// <summary>
        /// Gets a value indicating whether this node has a parent or not.
        /// </summary>
        public bool HasParent
        {
            get
            {
                return this.ParentDirectory != null;
            }
        }
    }
}
