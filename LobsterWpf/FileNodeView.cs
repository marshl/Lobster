using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LobsterWpf
{
    class FileNodeView
    {
        private ConnectionView parentConnectionView;
        public FileNodeView(ConnectionView connectionView, string path)
        {
            this.FullName = path;
            this.parentConnectionView = connectionView;

            DirectoryInfo dirInfo = new DirectoryInfo(path);
            this.IsDirectory = dirInfo.Exists;

            if (this.IsDirectory)
            {
                this.Children = new ObservableCollection<FileNodeView>();

                foreach (DirectoryInfo subDir in dirInfo.GetDirectories())
                {
                    FileNodeView node = new FileNodeView(connectionView, subDir.FullName);
                    this.Children.Add(node);
                }

                foreach (FileInfo file in dirInfo.GetFiles())
                {
                    if (!this.parentConnectionView.ShowReadOnlyFiles
                        || (File.GetAttributes(this.FullName) & FileAttributes.ReadOnly) != 0)
                    {
                        continue;
                    }

                    FileNodeView node = new FileNodeView(connectionView, file.FullName);
                    this.Children.Add(node);
                }
            }
        }

        public string FullName { get; private set; }

        public string Name
        {
            get
            {
                return Path.GetFileName(this.FullName);
            }
        }

        public bool IsDirectory { get; private set; }

        public ObservableCollection<FileNodeView> Children { get; set; }

       /* public string ImageURL
        {
            get
            {

            }
        }*/

        public bool IsVisible
        {
            get
            {
                if (this.IsDirectory)
                {
                    return true;
                }
                try
                {
                    return this.parentConnectionView.ShowReadOnlyFiles
                        || (File.GetAttributes(this.FullName) & FileAttributes.ReadOnly) == 0;
                }
                catch (FileNotFoundException)
                {
                    return false;
                }
            }
        }
    }
}
