using LobsterModel;

namespace LobsterWpf.ViewModels
{
    public class DirectoryWatcherView
    {
        public DirectoryWatcherView(DirectoryWatcher watcher)
        {
            this.BaseWatcher = watcher;
        }

        public DirectoryWatcher BaseWatcher { get; }
        
        public string Name
        {
            get
            {
                return this.BaseWatcher.Descriptor.Name;
            }
        }

        /// <summary>
        /// Gets the directory name of the clob type.
        /// </summary>
        public string Directory
        {
            get
            {
                return this.BaseWatcher.Descriptor.DirectoryName;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the directory of this clob directory exists on the files system or not.
        /// </summary>
        public bool DirectoryExists
        {
            get
            {
                return System.IO.Directory.Exists(this.BaseWatcher.DirectoryPath);
            }
        }
    }
}
