﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using LobsterModel;

namespace LobsterWpf.ViewModels
{
    class WatchedFileView : WatchedNodeView
    {
        public WatchedFileView(WatchedFile watchedFile) : base(watchedFile)
        {
            this.WatchedFile = watchedFile;
        }

        public WatchedFile WatchedFile { get; }

        public bool IsSynchronisedWithDatabase()
        {


            return false;
        }

        public override void CheckFileSynchronisation(ConnectionView connectionView, DirectoryWatcherView watcherView)
        {
            connectionView.BaseConnection.IsFileSynchronised(watcherView.Watcher, this.WatchedFile);
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

        public bool IsReadOnly
        {
            get
            {
                return this.WatchedFile.IsReadOnly;
            }
        }

        /// <summary>
        /// Gets the image tha is used to represent this file.
        /// </summary>
        public override ImageSource ImageUrl
        {
            get
            {
                string resourceName = this.IsReadOnly ? "LockedFileImageSource" : "NormalFileImageSource";
                return (ImageSource)System.Windows.Application.Current.FindResource(resourceName);
            }
        }
    }
}