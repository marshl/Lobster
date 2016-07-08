//-----------------------------------------------------------------------
// <copyright file="FileInsertException.cs" company="marshl">
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
//      Then he ordered the dwarves each to be put in a separate cell and to be given
//      food and drink, but not to be allowed to pass the doors of their little prisons
//
//      [ _The Hobbit_, IX: "Barrels out of Bond"]
//
//-----------------------------------------------------------------------
namespace LobsterModel
{
    using System;

    /// <summary>
    /// The exception for when an error occurs when inserting a new file into the database.
    /// </summary>
    public class FileInsertException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileInsertException"/> class.
        /// </summary>
        public FileInsertException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileInsertException"/> class 
        /// with an error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public FileInsertException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileInsertException"/> class with an error 
        /// message and th inner exception tha tcaused this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of this exception..</param>
        public FileInsertException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}