//-----------------------------------------------------------------------
// <copyright file="MimeTypeRequestEventArgs.cs" company="marshl">
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
    /// The event arguments for the RequestMimeTypeEvent event
    /// </summary>
    public class MimeTypeRequestEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MimeTypeRequestEventArgs"/> class.
        /// </summary>
        /// <param name="fullpath">The full path of the file to get the mime type for..</param>
        /// <param name="mimeTypes">The mime types that the user can select from.</param>
        public MimeTypeRequestEventArgs(string fullpath, string[] mimeTypes)
        {
            this.FullPath = fullpath;
            this.MimeTypes = mimeTypes;
        }

        /// <summary>
        /// Gets the full path of the file that he mime type is being request for.
        /// </summary>
        public string FullPath { get; }

        /// <summary>
        /// Gets the mime types that the user can select from.
        /// </summary>
        public string[] MimeTypes { get; }

        /// <summary>
        /// Gets or sets the mime type that the user selected.
        /// </summary>
        public string SelectedMimeType { get; set; } = null;
    }
}
