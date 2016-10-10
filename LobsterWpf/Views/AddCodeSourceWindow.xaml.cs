//-----------------------------------------------------------------------
// <copyright file="AddCodeSourceWindow.xaml.cs" company="marshl">
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
//      "I have not brought you hither to be instructed by you, but to give you a choice."
//          - Saruman the Wise
//
//      [ _The Lord of the Rings_, II/ii: "The Council of Elrond"]
//-----------------------------------------------------------------------
namespace LobsterWpf.Views
{
    using System.Windows;

    /// <summary>
    /// Interaction logic for AddCodeSourceWindow.xaml
    /// </summary>
    public partial class AddCodeSourceWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddCodeSourceWindow"/> class.
        /// </summary>
        public AddCodeSourceWindow()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// The enumeration for how the user wants to add a new CodeSource.
        /// </summary>
        public enum Selection
        {
            /// <summary>
            /// The choice to add a CodeSource directory that has already been prepared.
            /// </summary>
            AddPreparedCodeSource,

            /// <summary>
            /// The choice to prepare a new CodeSource directory and then add it.
            /// </summary>
            PrepareNewCodeSource
        }

        /// <summary>
        /// Gets what option the user selected.
        /// </summary>
        public Selection? UserSelection { get; private set; }

        /// <summary>
        /// The event handler for when the user clicks the Add Prepared CodeSOurce button.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void AddPreparedCodeSourceButton_Click(object sender, RoutedEventArgs e)
        {
            this.UserSelection = Selection.AddPreparedCodeSource;
            this.DialogResult = true;
            this.Close();
        }

        /// <summary>
        /// The event handler for when the user clicks the Prepare New CodeSource button.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void PrepareNewCodeSourceButton_Click(object sender, RoutedEventArgs e)
        {
            this.UserSelection = Selection.PrepareNewCodeSource;
            this.DialogResult = true;
            this.Close();
        }
    }
}
