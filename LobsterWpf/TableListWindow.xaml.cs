//-----------------------------------------------------------------------
// <copyright file="TableListWindow.xaml.cs" company="marshl">
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
    using System.Windows;
    using LobsterModel;

    /// <summary>
    /// Interaction logic for TableListWindow.xaml
    /// </summary>
    public partial class TableListWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TableListWindow"/> class.
        /// </summary>
        /// <param name="clobType">The ClobType whose tables are being modified.</param>
        public TableListWindow(ClobType clobType)
        {
            this.ClobTypeObject = clobType;
            this.RefreshTableList();

            this.InitializeComponent();

            this.DataContext = this;
        }

        /// <summary>
        /// Gets the underlying ClobType governing this table list.
        /// </summary>
        public ClobType ClobTypeObject { get; private set; }

        /// <summary>
        /// Gets the tables views from the ClobType.
        /// </summary>
        public ObservableCollection<TableView> TableList { get; private set; }

        /// <summary>
        /// Refreshes the list of tables with the tables in the clob type.
        /// </summary>
        private void RefreshTableList()
        {
            this.TableList = new ObservableCollection<TableView>();
            foreach (Table table in this.ClobTypeObject.Tables)
            {
                TableView tableView = new TableView(table);
                this.TableList.Add(tableView);
            }
        }

        /// <summary>
        /// The event called when the New button is clicked, creating a new Table and opening and edit window for it.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void NewButton_Click(object sender, RoutedEventArgs e)
        {
            Table table = new Table();
            EditTableWindow window = new EditTableWindow(table, true);
            window.Owner = this;
            bool? result = window.ShowDialog();

            this.ClobTypeObject.Tables.Add(table);
            this.RefreshTableList();
        }

        /// <summary>
        /// The event called when the Edit button is clicked, opening a new edit window for the selected Table.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            TableView tableView = (TableView)this.tableListBox.SelectedItem;

            if (tableView == null)
            {
                return;
            }

            EditTableWindow window = new EditTableWindow(tableView.TableObject, true);
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
            TableView tableView = (TableView)this.tableListBox.SelectedItem;

            if (tableView == null)
            {
                return;
            }

            this.ClobTypeObject.Tables.Remove(tableView.TableObject);
            this.RefreshTableList();
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
