using System.IO;
using System.Windows.Media;
using LobsterModel;

namespace LobsterWpf.ViewModels
{
    public abstract class WatchedNodeView
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
    }
}
