//-----------------------------------------------------------------------
// <copyright file="ConnectionView.cs" company="marshl">
// Copyright 2015, Liam Marshall, marshl.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//-----------------------------------------------------------------------
namespace LobsterWpf
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IO;
    using LobsterModel;

    /// <summary>
    /// The ViewModel for a model DatabaseConnection object.
    /// </summary>
    class ConnectionView : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets the connection model for this view
        /// </summary>
        public DatabaseConnection connection
        {
            get;
        }

        /// <summary>
        /// 
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        public FileNodeView RootFile { get; set; }

        private FileNodeView _selectedFileNode;

        /// <summary>
        /// Gets or sets the currenclty selected file in this connection.
        /// </summary>
        public FileNodeView SelectedFileNode
        {
            get
            {
                return this._selectedFileNode;
            }

            set
            {
                this._selectedFileNode = value;
                this.NotifyPropertyChanged("SelectedFileNode");
            }
        }

        public bool ShowReadOnlyFiles { get; set; } = true;

        public ObservableCollection<ClobTypeView> ClobTypes { get; set; }

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
        private void NotifyPropertyChanged(string propertyName = "")
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
