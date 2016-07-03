//-----------------------------------------------------------------------
// <copyright file="DatabaseFileView.cs" company="marshl">
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
//      It was often said (in
//      other families) that long ago one of the Took ancestors must have taken a fairy
//      wife.
//
//      [ _The Hobbit_, I: "An Unexpected Party"]
//
//-----------------------------------------------------------------------
namespace LobsterWpf
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Windows.Media;
    using LobsterModel;
    using Properties;

    /// <summary>
    /// The view for a file that is on the database, and may also be found locally.
    /// </summary>
    public class DatabaseFileView : FileNodeView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseFileView"/> class.
        /// </summary>
        /// <param name="connection">The parent connection of this file.</param>
        /// <param name="databaseFile">The database file </param>
        /// <param name="localFile">The local equivalent of this file, if it exists.</param>
        public DatabaseFileView(ConnectionView connection, DBClobFile databaseFile, string localFile) : base(connection)
        {
            this.DatabaseFile = databaseFile;
            this.FilePath = localFile;
            this.DisplayName = this.DatabaseFile?.Filename;

            this.RefreshFileInformation(false);
        }

        /// <summary>
        /// Gets a value indicating whether this file can be diffed with the local version.
        /// </summary>
        public override bool CanBeDiffed
        {
            get
            {
                if (this.FilePath == null)
                {
                    return false;
                }

                string extension = Path.GetExtension(this.FilePath);
                return Settings.Default.DiffableExtensions.Contains(extension);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the file can be directly shown in explorer.
        /// </summary>
        public override bool CanBeExploredTo
        {
            get
            {
                return this.FilePath != null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this file is new and can be inserted into the database.
        /// </summary>
        public override bool CanBeInserted
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this file can be downloaded or not.
        /// </summary>
        public override bool CanBePulled
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this file can be used to update the database.
        /// </summary>
        public override bool CanBeUpdated
        {
            get
            {
                return this.FilePath != null;
            }
        }

        /// <summary>
        /// Gets the colour that should be used for the text of the Name in the file list.
        /// </summary>
        public override string ForegroundColour
        {
            get
            {
                return (this.FilePath != null ? Colors.White : Colors.DodgerBlue).ToString();
            }
        }

        /// <summary>
        /// Gets the image that should be displayed next to the name in the file list.
        /// </summary>
        public override ImageSource ImageUrl
        {
            get
            {
                string resourceName = this.IsReadOnly ? "LockedFileImageSource" : "NormalFileImageSource";
                return (ImageSource)App.Current.FindResource(resourceName);
            }
        }
    }
}
