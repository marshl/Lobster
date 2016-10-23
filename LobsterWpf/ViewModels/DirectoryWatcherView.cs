using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
}
