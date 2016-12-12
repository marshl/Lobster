using System;
using System.Collections.Generic;
using System.Windows.Media;
using LobsterModel;

namespace LobsterWpf.ViewModels
{
    public class WatchedDirectoryView : WatchedNodeView
    {
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

        WatchedDirectory BaseDirectory { get; }

        public List<WatchedNodeView> ChildNodes { get; }


        public override void CheckFileSynchronisation(ConnectionView connectionView, DirectoryWatcherView watcherView)
        {
            foreach (WatchedNodeView child in this.ChildNodes)
            {
                child.CheckFileSynchronisation(connectionView, watcherView);
            }
        }

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


        public override bool CanBeInserted
        {
            get
            {
                return false;
            }
        }

        public override bool CanBeUpdated
        {
            get
            {
                return false;
            }
        }

        public override bool CanBeDownloaded
        {
            get
            {
                return false;
            }
        }

        public override bool CanBeCompared
        {
            get
            {
                return false;
            }
        }

        public override bool CanBeDeleted
        {
            get
            {
                return false;
            }
        }

        public override bool CanBeExploredTo
        {
            get
            {
                return true;
            }
        }
    }
}
