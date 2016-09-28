//-----------------------------------------------------------------------
// <copyright file="ColumnListWindow.xaml.cs" company="marshl">
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
//      It was lit by deep windows in the wide aisles at either side, beyond the rows of
//      tallpillars that upheld the roof.
//
//      [ _The Lord of the Rings_, V/i: "Chapter"]
//
//-----------------------------------------------------------------------
namespace LobsterWpf
{
    using System;
    using System.Collections.ObjectModel;
    using System.Windows;
    using LobsterModel;
    using ViewModels;

    /// <summary>
    /// Interaction logic for ColumnListWindow.xaml
    /// </summary>
    [Obsolete]
    public partial class ColumnListWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnListWindow"/> class.
        /// </summary>
        /// <param name="table">The table whose columns are being edited.</param>
        public ColumnListWindow(Table table)
        {
            this.BaseTable = table;
            this.RefreshColumnList();

            this.InitializeComponent();

            this.DataContext = this;
        }

        /// <summary>
        /// Gets the base table for which the columns are for.
        /// </summary>
        public Table BaseTable { get; }

        /// <summary>
        /// Gets the tables views from the ClobType.
        /// </summary>
        public ObservableCollection<ColumnView> ColumnList { get; private set; }

        /// <summary>
        /// Refreshes the list of tables with the tables in the clob type.
        /// </summary>
        private void RefreshColumnList()
        {
            this.ColumnList = new ObservableCollection<ColumnView>();
            foreach (Column column in this.BaseTable.Columns)
            {
                ColumnView tableView = new ColumnView(column);
                this.ColumnList.Add(tableView);
            }
        }

        /// <summary>
        /// The event called when the New button is clicked, creating a new Table and opening and edit window for it.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void NewButton_Click(object sender, RoutedEventArgs e)
        {
            Column column = new Column();
            EditColumnWindow window = new EditColumnWindow(column);
            window.Owner = this;
            bool? result = window.ShowDialog();

            this.BaseTable.Columns.Add(column);
            this.RefreshColumnList();
        }

        /// <summary>
        /// The event called when the Edit button is clicked, opening a new edit window for the selected Table.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            ColumnView columnView = (ColumnView)this.columnListBox.SelectedItem;

            if (columnView == null)
            {
                return;
            }

            EditColumnWindow window = new EditColumnWindow(columnView.BaseColumn);
            window.Owner = this;
            bool? result = window.ShowDialog();
        }

        /// <summary>
        /// The event called when the Delete button is clicked, 
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            ColumnView columnView = (ColumnView)this.columnListBox.SelectedItem;

            if (columnView == null)
            {
                return;
            }

            this.BaseTable.Columns.Remove(columnView.BaseColumn);
            this.RefreshColumnList();
        }

        /// <summary>
        /// The event called when the Close button is clicked, closing the window.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
