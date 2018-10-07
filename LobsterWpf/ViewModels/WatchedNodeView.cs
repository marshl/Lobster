//-----------------------------------------------------------------------
// <copyright file="WatchedNodeView.cs" company="marshl">
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
namespace LobsterWpf.ViewModels
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Windows.Media;
    using LobsterModel;

    /// <summary>
    /// An abstract class for directories or nodes in the database.
    /// </summary>
    public abstract class WatchedNodeView : INotifyPropertyChanged
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WatchedNodeView"/> class.
        /// </summary>
        /// <param name="node">The model object.</param>
        public WatchedNodeView(WatchedNode node)
        {
            this.BaseNode = node;
        }

        /// <summary>
        /// The event to be raised when a property is changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets the base model for this object
        /// </summary>
        public WatchedNode BaseNode { get; }

        /// <summary>
        /// Gets the path of the base node.
        /// </summary>
        public string FilePath
        {
            get
            {
                return this.BaseNode.FilePath;
            }
        }

        /// <summary>
        /// Gets the name of the base node.
        /// </summary>
        public string FileName
        {
            get
            {
                return Path.GetFileName(this.BaseNode.FilePath);
            }
        }

        /// <summary>
        /// Gets the file size of the file (if possible)
        /// </summary>
        public string FileSize
        {
            get
            {
                FileInfo fileInfo = new FileInfo(this.FilePath);
                if (fileInfo.Exists)
                {
                    return Utils.BytesToString(fileInfo.Length);
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// Gets the last write time of the watched node (if possible)
        /// </summary>
        public abstract DateTime? LastWriteTime { get; }

        /// <summary>
        /// Gets the colour to use for the Name of this file.
        /// </summary>
        public abstract string ForegroundColour { get; }

        /// <summary>
        /// Gets the image tha is used to represent this file.
        /// </summary>
        public abstract ImageSource ImageUrl { get; }

        /// <summary>
        /// Gets a value indicating whether this node can be inserted.
        /// </summary>
        public abstract bool CanBeInserted { get; }

        /// <summary>
        /// Gets a value indicating whether this node can be updated
        /// </summary>
        public abstract bool CanBeUpdated { get; }

        /// <summary>
        /// Gets a value indicating whether this node can be odnwloaded.
        /// </summary>
        public abstract bool CanBeDownloaded { get; }

        /// <summary>
        /// Gets a value indicating whether this node can be compared.
        /// </summary>
        public abstract bool CanBeCompared { get; }

        /// <summary>
        /// Gets a value indicating whether this node can be compared.
        /// </summary>
        public abstract bool CanBeDeleted { get; }

        /// <summary>
        /// Gets a value indicating whether this node can be explored to.
        /// </summary>
        public abstract bool CanBeExploredTo { get; }

        /// <summary>
        /// Checks to see if this node is synchronised with the database
        /// </summary>
        /// <param name="connectionView">The parent connection for this node.</param>
        /// <param name="watcherView">The parent directory for this node.</param>
        public abstract void CheckFileSynchronisation(ConnectionView connectionView, DirectoryWatcherView watcherView);

        /// <summary>
        /// Notifies that a property has changed
        /// </summary>
        /// <param name="propertyName">The name of the property that has changed.</param>
        protected void NotifyPropertyChanged(string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
