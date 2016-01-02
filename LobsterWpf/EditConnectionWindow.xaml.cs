//-----------------------------------------------------------------------
// <copyright file="EditConnectionWindow.xaml.cs" company="marshl">
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
    using System.Windows;

    /// <summary>
    /// Interaction logic for EditConnectionWindow.xaml
    /// </summary>
    public partial class EditConnectionWindow : Window
    {
        /// <summary>
        /// The directory that that the connection will initially be saved in if it is new.
        /// </summary>
        private string initialDirectory;

        /// <summary>
        /// Initializes a new instance of the <see cref="EditConnectionWindow"/> class.
        /// </summary>
        /// <param name="configView">The configuration view this should use as the data source.</param>
        /// <param name="initialDirectory">The initial directory to save to if saving a new DatabaseConfig.</param>
        public EditConnectionWindow(DatabaseConfigView configView, string initialDirectory)
        {
            this.initialDirectory = initialDirectory;
            this.ConfigView = configView;
            this.DataContext = this.ConfigView;
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the config view used as the data source of this window.
        /// </summary>
        public DatabaseConfigView ConfigView { get; set; }

        /// <summary>
        /// The event that is called when the Ok button is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            bool result = this.ConfigView.ApplyChanges(this.initialDirectory);
            this.Focus();
            if (result)
            {
                this.DialogResult = true;
                this.Close();
            }
        }

        /// <summary>
        /// The event that is called when the apply button is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            bool result = this.ConfigView.ApplyChanges(this.initialDirectory);
            this.Focus();
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
        /// The event that is called when the test connection button is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void TestConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            Exception ex = null;
            bool result = this.ConfigView.TestConnection(ref ex);

            string message = result ? "Connection test successful" : "Connection test unsuccessful.\n" + ex;

            MessageBox.Show(message);
        }

        /// <summary>
        /// The event that is called when the codesource button is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void CodeSourceButton_Click(object sender, RoutedEventArgs e)
        {
            this.ConfigView.SelectCodeSourceDirectory();
            this.Focus();
        }

        /// <summary>
        /// The event that is called when the clobtype button is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void ClobTypeButton_Click(object sender, RoutedEventArgs e)
        {
            this.ConfigView.SelectClobTypeDirectory();
            this.Focus();
        }

        /// <summary>
        /// The event that is called when the edit clob type button is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void EditClobTypeButton_Click(object sender, RoutedEventArgs e)
        {
            ClobTypeListWindow window = new ClobTypeListWindow(this.ConfigView.ClobTypeDir);
            window.Owner = this;
            bool? result = window.ShowDialog();
            this.Focus();
        }
    }
}
