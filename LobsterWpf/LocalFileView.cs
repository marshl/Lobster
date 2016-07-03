//-----------------------------------------------------------------------
// <copyright file="LocalFileView.cs" company="marshl">
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
//      Dwalin and Balin had swarmed up a tall
//      slender fir with few branches and were trying to find a place to sit in the
//      greenery of the topmost boughs.
//
//      [ _The Hobbit_, VI: "Out of the Frying Pan and Into the Fire"]
//
//-----------------------------------------------------------------------
namespace LobsterWpf
{
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Windows.Media;
    using LobsterModel;
    using Properties;

    /// <summary>
    /// A view representing a single local file, with a possible database file connection.
    /// </summary>
    public sealed class LocalFileView : FileNodeView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LocalFileView"/> class.
        /// </summary>
        /// <param name="connection">The parent connection of this file view.</param>
        /// <param name="clobDir">The directory from which this file originated.</param>
        /// <param name="filename">The full path of the file this view will represent.</param>
        /// <param name="recurse">Whether to include sub directories or not.</param>
        public LocalFileView(ConnectionView connection, ClobDirectory clobDir, string filename, bool recurse) : base(connection)
        {
            this.FilePath = filename;
            this.DisplayName = Path.GetFileName(this.FilePath);

            DirectoryInfo dirInfo = new DirectoryInfo(filename);
            this.IsDirectory = dirInfo.Exists;

            if (this.IsDirectory)
            {
                this.Children = new ObservableCollection<FileNodeView>();

                if (recurse)
                {
                    foreach (DirectoryInfo subDir in dirInfo.GetDirectories())
                    {
                        FileNodeView node = new LocalFileView(ParentConnectionView, clobDir, subDir.FullName, recurse);
                        this.Children.Add(node);
                    }
                }

                foreach (FileInfo file in dirInfo.GetFiles())
                {
                    if (connection.ShowReadOnlyFiles || !file.IsReadOnly)
                    {
                        FileNodeView node = new LocalFileView(this.ParentConnectionView, clobDir, file.FullName, recurse);
                        this.Children.Add(node);
                    }
                }

                this.IsExpanded = this.ParentConnectionView.ExpandedDirectoryNames.Contains(this.FilePath);
            }
            else 
            {
                // Not a directory
                try
                {
                    this.DatabaseFile = clobDir.GetDatabaseFileForFullpath(this.FilePath);
                }
                catch (ClobFileLookupException)
                {
                    this.DatabaseFile = null;
                }
            }

            this.RefreshFileInformation(false);
        }

        /// <summary>
        /// Gets a value indicating whether this file can be updated or not.
        /// </summary>
        public override sealed bool CanBeUpdated
        {
            get
            {
                return !this.IsDirectory && this.DatabaseFile != null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this file can be diffed or not.
        /// </summary>
        public override sealed bool CanBeDiffed
        {
            get
            {
                if (this.IsDirectory || this.DatabaseFile == null)
                {
                    return false;
                }

                string extension = Path.GetExtension(this.DisplayName);
                return Settings.Default.DiffableExtensions.Contains(extension);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this file can inserted or not.
        /// </summary>
        public override sealed bool CanBeInserted
        {
            get
            {
                return !this.IsDirectory && this.DatabaseFile == null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this file can be downloaded from the database
        /// </summary>
        public override sealed bool CanBePulled
        {
            get
            {
                return !this.IsDirectory && this.DatabaseFile != null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this file can be explored to or not.
        /// </summary>
        public override sealed bool CanBeExploredTo
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets a colour that is used as the foreground for the name.
        /// </summary>
        public override string ForegroundColour
        {
            get
            {
                return (this.IsDirectory || this.DatabaseFile != null ? Colors.White : Colors.LimeGreen).ToString();
            }
        }

        /// <summary>
        /// Gets the image tha is used to represent this file.
        /// </summary>
        public override ImageSource ImageUrl
        {
            get
            {
                string resourceName;

                if (this.IsDirectory)
                {
                    resourceName = this.Children?.Count > 0 ? "FullDirectoryImageSource" : "EmptyDirectoryImageSource";
                }
                else
                {
                    try
                    {
                        resourceName = this.IsReadOnly ? "LockedFileImageSource" : "NormalFileImageSource";
                    }
                    catch (IOException)
                    {
                        // In case the file was not found
                        resourceName = "FileNotFoundImageSource";
                    }
                }

                return (ImageSource)System.Windows.Application.Current.FindResource(resourceName);
            }
        }
    }
}
