//-----------------------------------------------------------------------
// <copyright file="FileUpdateCompleteEventArgs.cs" company="marshl">
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
    /// The event arguments for an update complete event.
    /// </summary>
    public class FileUpdateCompleteEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileUpdateCompleteEventArgs"/> class.
        /// </summary>
        /// <param name="fullpath">The file that was updated.</param>
        /// <param name="success">Whether the file update succeeded or not.</param>
        /// <param name="ex">The Exception thrown if a failure occurred</param>
        public FileUpdateCompleteEventArgs(string fullpath, bool success, Exception ex)
        {
            this.Fullpath = fullpath;
            this.Success = success;
            this.ExceptionThrown = ex;
        }

        /// <summary>
        /// Gets the full path of the file that was updated.
        /// </summary>
        public string Fullpath { get; }

        /// <summary>
        /// Gets a value indicating whether the file updated succeeded or not.
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// Gets the Exception that was thrown if the update failed
        /// </summary>
        public Exception ExceptionThrown { get; }
    }
}
