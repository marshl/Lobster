//-----------------------------------------------------------------------
// <copyright file="TableSelectorWindow.xaml.cs" company="marshl">
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
namespace LobsterWpf
{
    using System.Collections.ObjectModel;
    using System.Windows;
    using LobsterModel;

    /// <summary>
    /// The window for letting the user select a table to insert a new file into.
    /// </summary>
    public partial class TableSelectorWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TableSelectorWindow"/> class.
        /// </summary>
        /// <param name="filename">The full path of the file the table is being selected for.</param>
        /// <param name="tables">The tables the user can select from.</param>
        public TableSelectorWindow(string filename, Table[] tables)
        {
            this.InitializeComponent();

            this.MessageLabel = $"Please select the table to insert {System.IO.Path.GetFileName(filename)} into:";
            this.TableList = new ObservableCollection<LobsterModel.Table>(tables);
        }

        /// <summary>
        /// Gets the text of the messsage to display.
        /// </summary>
        public string MessageLabel { get; }

        /// <summary>
        /// Gets the tables that the user can select from.
        /// </summary>
        public ObservableCollection<Table> TableList { get; }

        /// <summary>
        /// Gets the table currently selected by the user.
        /// </summary>
        public Table SelectedTable
        {
            get
            {
                return this.tableListBox.SelectedItem as Table;
            }
        }

        /// <summary>
        /// The event for when the accept button is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.SelectedTable != null)
            {
                this.DialogResult = true;
                this.Close();
            }
        }

        /// <summary>
        /// The event for when the cancel button is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
