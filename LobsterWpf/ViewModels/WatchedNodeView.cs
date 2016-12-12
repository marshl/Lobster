using System.ComponentModel;
using System.IO;
using System.Windows.Media;
using LobsterModel;

namespace LobsterWpf.ViewModels
{
    public abstract class WatchedNodeView : INotifyPropertyChanged
    {
        public WatchedNode BaseNode { get; }

        public WatchedNodeView(WatchedNode node)
        {
            this.BaseNode = node;
        }

        public abstract void CheckFileSynchronisation(ConnectionView connectionView, DirectoryWatcherView watcherView);

        public string FileName
        {
            get
            {
                return Path.GetFileName(this.BaseNode.FilePath);
            }
        }

        /// <summary>
        /// Gets the colour to use for the Name of this file.
        /// </summary>
        public abstract string ForegroundColour { get; }

        /// <summary>
        /// Gets the image tha is used to represent this file.
        /// </summary>
        public abstract ImageSource ImageUrl { get; }


        /// <summary>
        /// The event to be raised when a property is changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;


        public abstract bool CanBeInserted { get; }

        public abstract bool CanBeUpdated { get; }

        public abstract bool CanBeDownloaded { get; }

        public abstract bool CanBeCompared { get; }

        public abstract bool CanBeDeleted { get; }

        public abstract bool CanBeExploredTo { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyName"></param>
        protected void NotifyPropertyChanged(string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
