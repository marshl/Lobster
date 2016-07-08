//-----------------------------------------------------------------------
// <copyright file="FileListRetrievalException.cs" company="marshl">
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
//      'Not far from this tunnel there is, or was for a long time, the 
//      beginning of quite a broad path leading to the Bonfire Glade, 
//      and then on more or less in our direction, east and a little 
//      north.That is the path I am going to try and find.'
//          - Merry
//
//          [ _The Lord of the Rings, I/vi "The Old Forest" ] 
//
//-----------------------------------------------------------------------
namespace LobsterModel
{
    using System;

    /// <summary>
    /// The exception for when an error occurs when attempting to get the list of files for a clob type.
    /// </summary>
    public class FileListRetrievalException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileListRetrievalException"/> class.
        /// </summary>
        public FileListRetrievalException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileListRetrievalException"/> class 
        /// with an error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public FileListRetrievalException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileListRetrievalException"/> class with an error 
        /// message and th inner exception tha tcaused this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of this exception..</param>
        public FileListRetrievalException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}