//-----------------------------------------------------------------------
// <copyright file="EditColumnWindow.xaml.cs" company="marshl">
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
    using System;
    using System.Linq;
    using System.Windows;
    using System.Windows.Media;
    using LobsterModel;

    /// <summary>
    /// Interaction logic for EditColumnWindow.xaml
    /// </summary>
    public partial class EditColumnWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EditColumnWindow"/> class.
        /// </summary>
        /// <param name="column">The column to edit.</param>
        public EditColumnWindow(Column column)
        {
            this.BaseColumn = new ColumnView(column);
            this.DataContext = this.BaseColumn;

            this.InitializeComponent();

            this.purposeComboBox.ItemsSource = Enum.GetValues(typeof(Column.Purpose)).Cast<Column.Purpose>();
            this.datatypeComboBox.ItemsSource = Enum.GetValues(typeof(Column.Datatype)).Cast<Column.Datatype>();
        }

        /// <summary>
        /// Gets the column being edited by this window.
        /// </summary>
        public ColumnView BaseColumn { get; }

        /// <summary>
        /// The event that is called when the Ok button is clicked, closing the window.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
