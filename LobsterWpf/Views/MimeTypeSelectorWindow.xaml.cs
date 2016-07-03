//-----------------------------------------------------------------------
// <copyright file="MimeTypeSelectorWindow.xaml.cs" company="marshl">
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
//      'Will you aid me or thwart me? Choose swiftly!'
//          -- Aragorn
//
//      [ _The Lord of the Rings_, III/ii: "The Riders of Rohan"]
//
//-----------------------------------------------------------------------
namespace LobsterWpf
{
    using System.Collections.ObjectModel;
    using System.Windows;

    /// <summary>
    /// The window for the user to select a mime type to insert a file as.
    /// </summary>
    public partial class MimeTypeSelectorWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MimeTypeSelectorWindow"/> class.
        /// </summary>
        /// <param name="filename">The name of the file that the mime type is being selected for.</param>
        /// <param name="mimeTypes">The mime type the user can select from.</param>
        public MimeTypeSelectorWindow(string filename, string[] mimeTypes)
        {
            this.MessageLabel = $"Please select a mime type for {System.IO.Path.GetFileName(filename)}:";
            this.MimeTypeList = new ObservableCollection<string>(mimeTypes);
            this.DataContext = this;

            this.InitializeComponent();
        }

        /// <summary>
        /// Gets the label to display to the user.
        /// </summary>
        public string MessageLabel { get; }

        /// <summary>
        /// Gets the mime types that the user can select from.
        /// </summary>
        public ObservableCollection<string> MimeTypeList { get; }

        /// <summary>
        /// Gets the mime type that the user has selected.
        /// </summary>
        public string SelectedMimeType
        {
            get
            {
                return this.mimeTypeListBox.SelectedItem as string;
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

        /// <summary>
        /// The event for when the accept button is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.SelectedMimeType != null)
            {
                this.DialogResult = true;
                this.Close();
            }
        }
    }
}
