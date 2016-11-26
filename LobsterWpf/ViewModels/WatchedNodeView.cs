using System.IO;
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
    }
}
