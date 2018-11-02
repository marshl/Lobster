//-----------------------------------------------------------------------
// <copyright file="CodeSourceNameWindow.xaml.cs" company="marshl">
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
//      'Who are they, and what do they say?' asked Merry.
//
//-----------------------------------------------------------------------
namespace LobsterWpf.Views
{
    using System.Windows;

    /// <summary>
    /// Interaction logic for CodeSourceNameWindow.xaml
    /// </summary>
    public partial class CodeSourceNameWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CodeSourceNameWindow"/> class.
        /// </summary>
        public CodeSourceNameWindow()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets the CodeSource name that the user entered.
        /// </summary>
        public string CodeSourceName { get; private set; }

        /// <summary>
        /// Handles the event when the dialog OK button is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.CodeSourceName = this.codeSourceNameField.Text;
            this.DialogResult = true;
            this.Close();
        }

        /// <summary>
        /// Handles the event when the dialog Cancel button is clicked.
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
