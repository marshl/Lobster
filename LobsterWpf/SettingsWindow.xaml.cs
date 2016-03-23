//-----------------------------------------------------------------------
// <copyright file="SettingsWindow.xaml.cs" company="marshl">
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
//      "I have not brought you hither to be
//      instructed by you, but to give you a choice."
//          -- Saruman
//
//      [ _The Lord of the Rings_, II/ii: "THe Council of Elrond"]
//
//-----------------------------------------------------------------------
namespace LobsterWpf
{
    using System.Text.RegularExpressions;
    using System.Windows;
    using System.Windows.Input;

    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsWindow"/> class.
        /// </summary>
        public SettingsWindow()
        {
            this.InitializeComponent();

            this.Settings = new SettingsView();
            this.DataContext = this.Settings;
        }

        /// <summary>
        /// Gets or sets the settings to be viewed.
        /// </summary>
        public SettingsView Settings { get; set; }

        /// <summary>
        /// The event called when the Close button is clicked, closing the window.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">Teh event arguments.</param>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        /// <summary>
        /// The event called when the contents of a numeric field is changed, preventing the entering of non-numeric characters.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = !regex.IsMatch(e.Text);
        }
    }
}
