//-----------------------------------------------------------------------
// <copyright file="ClobTypeListWindow.xaml.cs" company="marshl">
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
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Windows;
    using LobsterModel;

    /// <summary>
    /// Interaction logic for ClobTypeListWindow.xaml
    /// </summary>
    public partial class ClobTypeListWindow : Window
    {
       
        /// <summary>
        /// Initializes a new instance of the <see cref="ClobTypeListWindow"/> class.
        /// </summary>
        /// <param name="directory">The directory that the clob types are stored in.</param>
        public ClobTypeListWindow(string directory)
        {
            this.InitializeComponent();
            this.ClobTypeDirectory = directory;
            this.RefreshClobTypeList();
            this.DataContext = this;
        }

        /// <summary>
        /// The directory that the clob types for this window are stored in;
        /// </summary>
        public string ClobTypeDirectory { get; }

        /// <summary>
        /// The ClobType views used for display.
        /// </summary>
        public ObservableCollection<ClobTypeView> ClobTypeList { get; set; }

        /// <summary>
        /// Cleans the clob type list and refreshes it with new data.
        /// </summary>
        private void RefreshClobTypeList()
        {
            List<ClobType> clobTypes = ClobType.GetClobTypeList(this.ClobTypeDirectory);
            this.ClobTypeList = new ObservableCollection<ClobTypeView>();
            foreach (ClobType type in clobTypes)
            {
                ClobTypeView view = new ClobTypeView(type);
                this.ClobTypeList.Add(view);
            }
        }
    }
}
