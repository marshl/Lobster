//-----------------------------------------------------------------------
// <copyright file="EditClobTypeWindow.xaml.cs" company="marshl">
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
    /// Interaction logic for EditClobTypeWindow.xaml
    /// </summary>
    public partial class EditClobTypeWindow : Window
    {
        /// <summary>
        /// The directory that this clob type should be stored in (to be used when prompting for a new filename).
        /// </summary>
        private string initialDirectory;

        /// <summary>
        /// Initializes a new instance of the <see cref="EditClobTypeWindow"/> class.
        /// </summary>
        /// <param name="clobType">The clob type to use as the view of this window.</param>
        /// <param name="initialDirectory">The initial directory to save to if this is for a new ClobType.</param>
        public EditClobTypeWindow(ClobType clobType, string initialDirectory)
        {
            this.InitializeComponent();

            this.initialDirectory = initialDirectory;
            this.ClobTypeView = new ClobTypeView(clobType);
            this.DataContext = this.ClobTypeView;
        }

        /// <summary>
        /// Gets the ClobTypeView that is used to store data for this window, creatd at construction time.
        /// </summary>
        public ClobTypeView ClobTypeView { get; }

        /// <summary>
        /// The event that is called when the Ok button is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            bool result = this.ClobTypeView.ApplyChanges(this.initialDirectory);

            if (result)
            {
                this.DialogResult = true;
                this.Close();
            }
        }

        /// <summary>
        /// The event that is called when the accept button is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            bool result = this.ClobTypeView.ApplyChanges(this.initialDirectory);
        }

        /// <summary>
        /// The event that is called when the cancel button is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        /// <summary>
        /// The event called when the Show Tables button is clicked, opening a table list window with the tables for this clob type.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void ShowTablesButton_Click(object sender, RoutedEventArgs e)
        {
            TableListWindow window = new TableListWindow(this.ClobTypeView.ClobTypeObject);
            window.Owner = this;
            bool? result = window.ShowDialog();
        }
    }
}
