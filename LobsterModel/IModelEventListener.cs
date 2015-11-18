//-----------------------------------------------------------------------
// <copyright file="IModelEventListener.cs" company="marshl">
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
//
//      'On, Shadowfax! We must hasten. Time is short. See! 
//       The beacons of Gondor are alight, calling for aid.'
//          -- Gandalf
//      [ _The Lord of the Rings_, V/i: "Minas Tirith"] 
//
//-----------------------------------------------------------------------
namespace LobsterModel
{
    /// <summary>
    /// The interface for the Model class, with events raised whenever a 
    /// change to a file causes a FileSystemEvent to trigger.
    /// </summary>
    public interface IModelEventListener
    {
        /// <summary>
        /// Raised whenver any file is changed.
        /// </summary>
        /// <param name="filename">The name of the file that was changed.</param>
        void OnFileChange(string filename);

        /// <summary>
        /// Raised when the automatic update of a file has been completed.
        /// This is not raised when a file is manually updated through the UI.
        /// </summary>
        /// <param name="filename">The name of the file that was updated.</param>
        void OnAutoUpdateComplete(string filename);

        /// <summary>
        /// Raised when a file being inserted into the database requires a table to be specified for it.
        /// </summary>
        /// <param name="fullpath">The name of the file that is being inserted.</param>
        /// <param name="tables">The list of possible tables the file could be inserted into.</param>
        /// <returns>The table that the file should be inserted into, or null if the insert is cancelled.</returns>
        Table PromptForTable(string fullpath, Table[] tables);

        /// <summary>
        /// Raised when the user is reuiqred to pick the mime type that a new file should be inserted as.
        /// </summary>
        /// <param name="fullpath">The file that will be inserted into the database.</param>
        /// <param name="mimeTypes">The list of mime types that can be selected from.</param>
        /// <returns>The mime type the file should be inserted with, or null if the insert is cancelled.</returns>
        string PromptForMimeType(string fullpath, string[] mimeTypes);
    }
}
