//-----------------------------------------------------------------------
// <copyright file="SetConnectionException.cs" company="marshl">
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
//      'I can't believe that Gollum was connected with
//       hobbits, however distantly,' said Frodo with some heat.
//          -- Frodo
//
//      [ _The Lord of the Rings_, I/ii: "Shadow of the Past"]
//
//-----------------------------------------------------------------------
namespace LobsterModel
{
    using System;

    /// <summary>
    /// The exception for when an error occurs when attemtping to open a connection to the database.
    /// </summary>
    public class SetConnectionException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetConnectionException"/> class.
        /// </summary>
        public SetConnectionException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SetConnectionException"/> class 
        /// with an error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public SetConnectionException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SetConnectionException"/> class with an error 
        /// message and th inner exception tha tcaused this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of this exception..</param>
        public SetConnectionException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}