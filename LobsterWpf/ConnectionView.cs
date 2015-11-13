using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using LobsterModel;


namespace LobsterWpf
{
    class ConnectionView : INotifyPropertyChanged
    {
        private DatabaseConnection connection;

        public event PropertyChangedEventHandler PropertyChanged;

        public FileNodeView RootFile { get; set; }

        public bool ShowReadOnlyFiles { get; set; }

        public ObservableCollection<ClobTypeView> ClobTypes { get; set; }

        // This method is called by the Set accessor of each property.
        // The CallerMemberName attribute that is applied to the optional propertyName
        // parameter causes the property name of the caller to be substituted as an argument.
        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public ConnectionView(DatabaseConnection con)
        {
            this.connection = con;

            this.ClobTypes = new ObservableCollection<ClobTypeView>();
            foreach (ClobDirectory clobDir in con.ClobDirectoryList)
            {
                this.ClobTypes.Add(new ClobTypeView(clobDir.ClobType));
            }
        }

        public void PopulateFileTreeForClobType(ClobType clobType)
        {
            this.RootFile = null;

            DirectoryInfo rootDirInfo = new DirectoryInfo(clobType.Fullpath);
            if (!rootDirInfo.Exists)
            {
                return;
            }

            this.RootFile = new FileNodeView(this, rootDirInfo.FullName);
        }
    }
}
