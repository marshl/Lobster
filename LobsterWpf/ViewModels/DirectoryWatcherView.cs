using LobsterModel;

namespace LobsterWpf.ViewModels
{
    public class DirectoryWatcherView
    {
        public DirectoryWatcherView(DirectoryWatcher watcher)
        {
            this.Watcher = watcher;
        }

        public DirectoryWatcher Watcher { get; }
        
        public string Name
        {
            get
            {
                return this.Watcher.Descriptor.Name;
            }
        }

        /// <summary>
        /// Gets the directory name of the clob type.
        /// </summary>
        public string Directory
        {
            get
            {
                return this.Watcher.Descriptor.DirectoryName;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the directory of this clob directory exists on the files system or not.
        /// </summary>
        public bool DirectoryExists
        {
            get
            {
                return System.IO.Directory.Exists(this.Watcher.DirectoryPath);
            }
        }
    }
}
