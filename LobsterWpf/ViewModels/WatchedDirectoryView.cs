//-----------------------------------------------------------------------
// <copyright file="WatchedDirectoryView.cs" company="marshl">
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
    using System.Collections.Generic;
    using System.Windows.Media;
    using LobsterModel;

    /// <summary>
    /// The view for the <see cref="WatchedDirectory"/> model. 
    /// </summary>
    public class WatchedDirectoryView : WatchedNodeView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WatchedDirectoryView"/> class.
        /// </summary>
        /// <param name="baseDirectory">The base <see cref="WatchedDirectory"/> model.</param>
        public WatchedDirectoryView(WatchedDirectory baseDirectory) : base(baseDirectory)
        {
            this.BaseDirectory = baseDirectory;

            this.ChildNodes = new List<WatchedNodeView>();

            foreach (WatchedNode node in this.BaseDirectory.ChildNodes)
            {
                if (node is WatchedDirectory)
                {
                    this.ChildNodes.Add(new WatchedDirectoryView((WatchedDirectory)node));
                }
                else
                {
                    this.ChildNodes.Add(new WatchedFileView((WatchedFile)node));
                }
            }
        }

        /// <summary>
        /// Gets the model object.
        /// </summary>
        public WatchedDirectory BaseDirectory { get; }

        /// <summary>
        /// Gets the list of child nodes.
        /// </summary>
        public List<WatchedNodeView> ChildNodes { get; }
        
        /// <summary>
        /// Gets the colour to use for the Name of this file.
        /// </summary>
        public override string ForegroundColour
        {
            get
            {
                return Colors.White.ToString();
            }
        }

        /// <summary>
        /// Gets the image tha is used to represent this file.
        /// </summary>
        public override ImageSource ImageUrl
        {
            get
            {
                string resourceName = this.ChildNodes.Count > 0 ? "FullDirectoryImageSource" : "EmptyDirectoryImageSource";
                return (ImageSource)System.Windows.Application.Current.FindResource(resourceName);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this directory can be inserted into the database.
        /// </summary>
        public override bool CanBeInserted
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this directory can be updated.
        /// </summary>
        public override bool CanBeUpdated
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this directory can be downloaded.
        /// </summary>
        public override bool CanBeDownloaded
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this directory can be compared with the database
        /// </summary>
        public override bool CanBeCompared
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this directory can be deleted.
        /// </summary>
        public override bool CanBeDeleted
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this directory can be navigated to.
        /// </summary>
        public override bool CanBeExploredTo
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Returns whether this directory and all of its children are synchronised with the database.
        /// </summary>
        /// <param name="connectionView">The parent connection of this file.</param>
        /// <param name="watcherView">The parent watcher for this file.</param>
        public override void CheckFileSynchronisation(ConnectionView connectionView, DirectoryWatcherView watcherView)
        {
            foreach (WatchedNodeView child in this.ChildNodes)
            {
                child.CheckFileSynchronisation(connectionView, watcherView);
            }
        }
    }
}
