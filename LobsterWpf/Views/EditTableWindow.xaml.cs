//-----------------------------------------------------------------------
// <copyright file="EditTableWindow.xaml.cs" company="marshl">
// Copyright 2016, Liam Marshall, marshl.
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
//
//      Against the opposite wall was a long bench laden with wide earthenware
//      basins, and beside it stood brown ewers filled with water, some cold, some
//      steaming hot.
//
//      [ _The Lord of the Rings_, I/vii: "In the House of Tom Bombadil"]
//
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
        /// <param name="showParentTableControls">Whether this view should allow the table to have a parent table added.</param>
        public EditTableWindow(Table table, bool showParentTableControls)
        {
            this.Table = new TableView(table);
            this.Table.CanHaveParentTable = showParentTableControls;
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
            EditTableWindow window = new EditTableWindow(this.Table.ParentTable, false);
            window.Owner = this;
            bool? result = window.ShowDialog();
        }

        /// <summary>
        /// The event that is called when the Show Columns button is clicked, opening a window with the list of columns.
        /// </summary>
        /// <param name="sender">The sender of th event,</param>
        /// <param name="e">The event arguments.</param>
        private void ShowColumnsButton_Click(object sender, RoutedEventArgs e)
        {
            ColumnListWindow window = new ColumnListWindow(this.Table.BaseTable);
            window.Owner = this;
            bool? result = window.ShowDialog();
        }
    }
}
