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

        private FileNodeView _selectedFileNode;
        public FileNodeView SelectedFileNode
        {
            get
            {
                return this._selectedFileNode;
            }

            set
            {
                this._selectedFileNode = value;

                if (this.SelectedFileNode == null)
                {
                    this.UpdateFileButtonEnabled = false;
                    this.InsertFileButtonEnabled = false;
                    this.DiffFileButtonEnabled = false;
                    this.ExploreFileButtonEnabled = false;
                }
                else if (this.SelectedFileNode.IsDirectory)
                {
                    this.UpdateFileButtonEnabled = false;
                    this.InsertFileButtonEnabled = false;
                    this.DiffFileButtonEnabled = false;
                    this.ExploreFileButtonEnabled = true;
                }
                else
                {
                    bool selectedFileIsInDatabase = false;
                    if (this.SelectedFileNode != null )
                    {
                        string fullpath = this.SelectedFileNode.FullName;
                        ClobDirectory clobDir = this.connection.GetClobDirectoryForFile(fullpath);
                        selectedFileIsInDatabase = clobDir.GetDatabaseFileForFullpath(fullpath) != null;
                    }

                    this.UpdateFileButtonEnabled = selectedFileIsInDatabase;
                    this.InsertFileButtonEnabled = !selectedFileIsInDatabase;
                    this.DiffFileButtonEnabled = selectedFileIsInDatabase;
                    this.ExploreFileButtonEnabled = true;
                }

                this.NotifyPropertyChanged("SelectedFileNode");
            }
        }

        public bool ShowReadOnlyFiles { get; set; } = true;

        public ObservableCollection<ClobTypeView> ClobTypes { get; set; }

        private bool _updateFileButtonEnabled = false;

        public bool UpdateFileButtonEnabled
        {
            get
            {
                return this._updateFileButtonEnabled;
            }
            set
            {
                this._updateFileButtonEnabled = value;
                this.NotifyPropertyChanged("UpdateFileButtonEnabled");
            }
        }

        private bool _insertFileButtonEnabled = false;
        public bool InsertFileButtonEnabled
        {
            get
            {
                return this._insertFileButtonEnabled;
            }

            set
            {
                this._insertFileButtonEnabled = value;
                this.NotifyPropertyChanged("InsertFileButtonEnabled");
            }
        }

        private bool _diffFileButtonEnabled = false;

        public bool DiffFileButtonEnabled
        {
            get
            {
                return this._diffFileButtonEnabled;
            }
            set
            {
                this._diffFileButtonEnabled = value;
                this.NotifyPropertyChanged("DiffFileButtonEnabled");
            }
        }

        private bool _exploreFileButtonEnabled = false;

        public bool ExploreFileButtonEnabled
        {
            get
            {
                return this._exploreFileButtonEnabled;
            }

            set
            {
                this._exploreFileButtonEnabled = value;
                this.NotifyPropertyChanged("ExploreFileButtonEnabled");
            }
        }

        private bool _isEnabled = true;
        public bool IsEnabled
        {
            get
            {
                return this._isEnabled;
            }

            set
            {
                this._isEnabled = value;
                this.NotifyPropertyChanged("IsEnabled");
            }
        }
        
        public bool IsAutoClobEnabled
        {
            get
            {
                return this.connection.IsAutoClobEnabled;
            }
            set
            {
                this.connection.IsAutoClobEnabled = value;
                this.NotifyPropertyChanged("IsAutoClobEnabled");
            }
        }

        // This method is called by the Set accessor of each property.
        // The CallerMemberName attribute that is applied to the optional propertyName
        // parameter causes the property name of the caller to be substituted as an argument.
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
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
