//-----------------------------------------------------------------------
// <copyright file="WatchedDirectory.cs" company="marshl">
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
    using System.Collections.Generic;

    /// <summary>
    /// A kind of <see cref="WatchedNode"/> that represents a directory (instead of a file).
    /// </summary>
    public class WatchedDirectory : WatchedNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WatchedDirectory"/> class.
        /// </summary>
        /// <param name="path">The path to the directory.</param>
        /// <param name="parent">The parent directory.</param>
        public WatchedDirectory(string path, WatchedDirectory parent) : base(path, parent)
        {
        }

        /// <summary>
        /// Gets the list of<see cref="WatchedNode"/> that are children of this directory.
        /// </summary>
        public List<WatchedNode> ChildNodes { get; } = new List<WatchedNode>();
    }
}
