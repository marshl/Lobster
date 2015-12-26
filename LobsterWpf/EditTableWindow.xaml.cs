//-----------------------------------------------------------------------
// <copyright file="EditTableWindow.xaml.cs" company="marshl">
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
    using System.Windows;
    using LobsterModel;

    /// <summary>
    /// Interaction logic for EditTableWindow.xaml
    /// </summary>
    public partial class EditTableWindow : Window
    {
        public EditTableWindow(Table table)
        {
            this.Table = new TableView(table);
            this.DataContext = this.Table;

            this.InitializeComponent();
        }

        public TableView Table { get; }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void EditParentTable_Click(object sender, RoutedEventArgs e)
        {
            EditTableWindow window = new EditTableWindow(this.Table.ParentTable);
            window.Owner = this;
            bool? result = window.ShowDialog();
        }
    }
}
