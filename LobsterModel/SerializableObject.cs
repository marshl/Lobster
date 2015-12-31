//-----------------------------------------------------------------------
// <copyright file="SerializableObject.cs" company="marshl">
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
namespace LobsterModel
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    /// <summary>
    /// An object that can be used even when deserialisation raises warnings/errors, that are stored here.
    /// </summary>
    public abstract class SerializableObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SerializableObject"/> class.
        /// </summary>
        public SerializableObject()
        {
            // This space is left intentionally blank
        }

        /// <summary>
        /// Gets a value indicating whether this object has passed validation or not.
        /// </summary>
        [XmlIgnore]
        public bool IsValid
        {
            get
            {
                return this.ErrorList.Count == 0;
            }
        }

        /// <summary>
        /// Gets or sets the errors that have occurred during the deserialisation process.
        /// </summary>
        [XmlIgnore]
        public List<string> ErrorList { get; set; } = new List<string>();
    }
}
