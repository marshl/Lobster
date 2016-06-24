//-----------------------------------------------------------------------
// <copyright file="FileChangeEventArgs.cs" company="marshl">
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
    using System;

    /// <summary>
    /// The event arguments for <see cref="StartChangeProcessingEvent"/>.
    /// </summary>
    public class FileChangeEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileChangeEventArgs"/> class.
        /// </summary>
        public FileChangeEventArgs()
        {
            // Blank
        }
    }
}
