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
    public abstract class WatchedNode
    {
        public WatchedNode(string filePath, WatchedDirectory parent)
        {
            this.FilePath = filePath;
            this.ParentDirectory = parent;
        }

        public string FilePath { get; }

        public WatchedDirectory ParentDirectory { get; }

        public bool HasParent
        {
            get
            {
                return this.ParentDirectory != null;
            }
        }
    }
}
