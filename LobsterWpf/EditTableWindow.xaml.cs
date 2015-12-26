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
        /// <summary>
        /// Initializes a new instance of the <see cref="EditTableWindow"/> class.
        /// </summary>
        /// <param name="table">The model table to edit.</param>
        public EditTableWindow(Table table)
        {
            this.Table = new TableView(table);
            this.DataContext = this.Table;

            this.InitializeComponent();
        }

        /// <summary>
        /// Getst the table view for the table to edit.
        /// </summary>
        public TableView Table { get; }

        /// <summary>
        /// The event called when the Ok button is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        /// <summary>
        /// The event called when the Edit Parent Table button is clicked. Opens a dialog window where the parent table can be edited.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void EditParentTable_Click(object sender, RoutedEventArgs e)
        {
            EditTableWindow window = new EditTableWindow(this.Table.ParentTable);
            window.Owner = this;
            bool? result = window.ShowDialog();
        }
    }
}
