//-----------------------------------------------------------------------
// <copyright file="IModelEventListener.cs" company="marshl">
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
        /// Raised when the automatic update of a file has been completed.
        /// This is not raised when a file is manually updated through the UI.
        /// </summary>
        /// <param name="filename">The name of the file that was updated.</param>
        /// <param name="updateSuccess">Whether the update was a success or not.</param>
        void OnAutoUpdateComplete(string filename, bool updateSuccess);

        /// <summary>
        /// Raised when a file being inserted into the database requires a table to be specified for it.
        /// </summary>
        /// <param name="fullpath">The name of the file that is being inserted.</param>
        /// <param name="tables">The list of possible tables the file could be inserted into.</param>
        /// <param name="table">The table that the file should be inserted into, or null if the insert is cancelled.</param>
        /// <returns>A vale indicating whether the user chose a table or not.</returns>
        bool PromptForTable(string fullpath, Table[] tables, ref Table table);

        /// <summary>
        /// Raised when the user is reuiqred to pick the mime type that a new file should be inserted as.
        /// </summary>
        /// <param name="fullpath">The file that will be inserted into the database.</param>
        /// <param name="mimeTypes">The list of mime types that can be selected from.</param>
        /// <param name="mimeType">The mime type the file should be inserted with, or null if the insert is cancelled.</param>
        /// <returns>A value indicating whether ther user chose a mime type or not.</returns>
        bool PromptForMimeType(string fullpath, string[] mimeTypes, ref string mimeType);

        /// <summary>
        /// The event raised when the first file change event in a processing group is received.
        /// </summary>
        void OnEventProcessingStart();

        /// <summary>
        /// The event raised when the first last file change event of a processing group is completed.
        /// </summary>
        /// <param name="fileTreeChanged">Whether or not there was a change to the file tree during event processing (a file create/delete/rename).</param>
        void OnFileProcessingFinished(bool fileTreeChanged);
    }
}
