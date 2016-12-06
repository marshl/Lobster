using System;
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
            this.SynchronisationErrors = new List<FileSynchronisationCheckException>();
        }

        public WatchedFile WatchedFile { get; }

        public bool IsSynchronisedWithDatabase()
        {
            return false;
        }

        public override void CheckFileSynchronisation(ConnectionView connectionView, DirectoryWatcherView watcherView)
        {
            try
            {
                this.IsSynchronised = this.WatchedFile.IsSynchonisedWithDatabase(connectionView.BaseConnection, watcherView.BaseWatcher);
            }
            catch (FileSynchronisationCheckException ex)
            {
                this.SynchronisationErrors.Add(ex);
            }
        }

        public bool IsSynchronised { get; private set; } = false;

        /// <summary>
        /// Gets the colour to use for the Name of this file.
        /// </summary>
        public override string ForegroundColour
        {
            get
            {
                if(this.IsSynchronised)
                {
                    return Colors.White.ToString();
                }

                if (this.SynchronisationErrors.Count > 0)
                {
                    return Colors.Red.ToString();
                }

                return Colors.Green.ToString();
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

        public List<FileSynchronisationCheckException> SynchronisationErrors { get; set; }
        
        public enum SynchronisationStatus
        {
            Unknown,
            Synchronised,
            LocalOnly,
        }
    }
}
