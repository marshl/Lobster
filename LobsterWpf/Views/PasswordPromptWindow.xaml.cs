﻿//-----------------------------------------------------------------------
// <copyright file="PasswordPromptWindow.xaml.cs" company="marshl">
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
//      'That is plain enough,' said Gimli. `If you are a friend, speak the
//      password, and the doors will open, and you can enter.'
//
//      [ _The Lord of the Rings_, V/i: "Chapter"]
//
//-----------------------------------------------------------------------
namespace LobsterWpf.Views
{
    using System.Windows;

    /// <summary>
    /// Interaction logic for TextPromptWindow.xaml
    /// </summary>
    public partial class PasswordPromptWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordPromptWindow"/> class.
        /// </summary>
        /// <param name="databaseName">The name of the database the password is for.</param>
        /// <param name="username">The name of the user the password is for.</param>
        public PasswordPromptWindow(string databaseName, string username)
        {
            this.InitializeComponent();

            this.textField.Focus();
            this.passwordLabel.Content = $"Please enter the password for {username}:";
            this.Title = $"Connect to {databaseName}";
        }

        /// <summary>
        /// The event called when the ok button is clicked, closing the window.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        /// <summary>
        /// The event called when the cancel button is clicked, closing the window.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        /// <summary>
        /// The event called when the text field is changed.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void TextField_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Return)
            {
                this.DialogResult = true;
                this.Close();
            }
        }
    }
}
