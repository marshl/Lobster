//-----------------------------------------------------------------------
// <copyright file="FileUpdateException.cs" company="marshl">
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
//      "Well yes - and no. Now it comes
//      to it, I don't like parting with it at all, I may say. And I don't really see
//      why I should.Why do you want me to?"
//          -- Bilbo
//
//      [ _The Lord of the Rings_, I/i: "A Long Expected Party"]
//
//-----------------------------------------------------------------------
namespace LobsterModel
{
    using System;

    /// <summary>
    /// The exception for when an error occurs when updating a file to the database.
    /// </summary>
    public class FileUpdateException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileUpdateException"/> class.
        /// </summary>
        public FileUpdateException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileUpdateException"/> class 
        /// with an error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public FileUpdateException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileUpdateException"/> class with an error 
        /// message and th inner exception tha tcaused this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of this exception..</param>
        public FileUpdateException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}