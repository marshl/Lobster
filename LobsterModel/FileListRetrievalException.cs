//-----------------------------------------------------------------------
// <copyright file="FileListRetrievalException.cs" company="marshl">
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
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// The exception for when an error occurs when attempting to get the list of files for a clob type.
    /// </summary>
    [Serializable]
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

        /// <summary>
        /// Initializes a new instance of the <see cref="FileListRetrievalException"/> class with the serialied info.
        /// </summary>
        /// <param name="info">The System.Runtime.Serialization.SerializationInfo that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The System.Runtime.Serialization.StreamingContext that contains contextual information
        /// about the source or destination.</param>
        protected FileListRetrievalException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}