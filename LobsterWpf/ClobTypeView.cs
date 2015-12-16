//-----------------------------------------------------------------------
// <copyright file="ClobTypeView.cs" company="marshl">
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
//      ... but that very week orders began to pour out of Bag End for 
//      every kind of provision, commodity, or luxury that could be 
//      obtained in Hobbiton or Bywater or anywhere in the neighbourhood.
//
//      [ _The Lord of the Rings_, I/i: "A Long-expected Party"]
//
//-----------------------------------------------------------------------
namespace LobsterWpf
{
    using LobsterModel;

    /// <summary>
    /// A view for a model clob type.
    /// </summary>
    public class ClobTypeView
    {
        /// <summary>
        /// The base clob type for this view.
        /// </summary>
        public ClobType clobType { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClobTypeView"/> class.
        /// </summary>
        /// <param name="ct">The ClobType to use as the model.</param>
        public ClobTypeView(ClobType ct)
        {
            this.clobType = ct;
        }

        /// <summary>
        /// Gets the name of the clob type.
        /// </summary>
        public string Name
        {
            get
            {
                return this.clobType.Name;
            }           
        }

        /// <summary>
        /// Gets the directory name of the clob type.
        /// </summary>
        public string Directory
        {
            get
            {
                return this.clobType.Directory;
            }
        }
    }
}
