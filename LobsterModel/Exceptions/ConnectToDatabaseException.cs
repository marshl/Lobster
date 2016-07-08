//-----------------------------------------------------------------------
// <copyright file="ConnectToDatabaseException.cs" company="marshl">
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
//      After some time he felt for his pipe. It was not broken, and that was something. Then he felt for his pouch, 
//      and there was some tobacco in it, and that was something more. Then he felt for matches and he could not find 
//      any at all, and that shattered his hopes completely
//
//      [ _The Hobbit_, V: "Riddles in the Dark"]
//
//-----------------------------------------------------------------------
namespace LobsterModel
{
    using System;

    /// <summary>
    /// The exception for when an error occurs when connecting to the database.
    /// </summary>
    public class ConnectToDatabaseException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectToDatabaseException"/> class.
        /// </summary>
        public ConnectToDatabaseException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectToDatabaseException"/> class 
        /// with an error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ConnectToDatabaseException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectToDatabaseException"/> class with an error 
        /// message and th inner exception tha tcaused this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of this exception..</param>
        public ConnectToDatabaseException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}