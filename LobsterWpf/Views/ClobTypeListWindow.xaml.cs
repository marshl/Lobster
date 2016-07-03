﻿//-----------------------------------------------------------------------
// <copyright file="ClobTypeListWindow.xaml.cs" company="marshl">
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
//      It was the middle of the afternoon
//      before they noticed that great patches of flowers had begun to spring up, all
//      the same kinds growing together as if they had been planted.
//
//      [ _The Hobbit_, VII: "Queer Lodgings"]
//
//-----------------------------------------------------------------------
namespace LobsterWpf
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Windows;
    using LobsterModel;

    /// <summary>
    /// Interaction logic for ClobTypeListWindow.xaml
    /// </summary>
    public partial class ClobTypeListWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClobTypeListWindow"/> class.
        /// </summary>
        /// <param name="directory">The directory that the clob types are stored in.</param>
        public ClobTypeListWindow(string directory)
        {
            this.InitializeComponent();
            this.ClobTypeDirectory = directory;
            this.RefreshClobTypeList();
            this.DataContext = this;
        }

        /// <summary>
        /// Gets the directory that the clob types for this window are stored in;
        /// </summary>
        public string ClobTypeDirectory { get; }

        /// <summary>
        /// Gets the ClobType views used for display.
        /// </summary>
        public ObservableCollection<ClobTypeView> ClobTypeList { get; private set; }

        /// <summary>
        /// Cleans the clob type list and refreshes it with new data.
        /// </summary>
        private void RefreshClobTypeList()
        {
            List<ClobType> clobTypes = ClobType.GetClobTypeList(this.ClobTypeDirectory);
            this.ClobTypeList = new ObservableCollection<ClobTypeView>();
            foreach (ClobType type in clobTypes)
            {
                ClobTypeView view = new ClobTypeView(type);
                this.ClobTypeList.Add(view);
            }
        }

        /// <summary>
        /// The event that is called when the New button is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void NewButton_Click(object sender, RoutedEventArgs e)
        {
            ClobType clobType = new ClobType();
            EditClobTypeWindow window = new EditClobTypeWindow(clobType, this.ClobTypeDirectory);
            window.Owner = this;
            bool? result = window.ShowDialog();
            this.RefreshClobTypeList();
        }

        /// <summary>
        /// The event taht is called whhen the edit button is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.clobTypeListBox.SelectedIndex == -1)
            {
                return;
            }

            ClobTypeView clobType = this.ClobTypeList[this.clobTypeListBox.SelectedIndex];
            EditClobTypeWindow window = new EditClobTypeWindow(clobType.ClobTypeObject, this.ClobTypeDirectory);
            window.Owner = this;
            bool? result = window.ShowDialog();
            this.RefreshClobTypeList();
        }

        /// <summary>
        /// The event that is called when the Delete button is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.clobTypeListBox.SelectedIndex == -1)
            {
                return;
            }

            ClobTypeView clobType = this.ClobTypeList[this.clobTypeListBox.SelectedIndex];
            try
            {
                File.Delete(clobType.FilePath);
                this.RefreshClobTypeList();
            }
            catch (IOException ex)
            {
                MessageLog.LogError($"An IO exception occurred when deleting the ClobType file {clobType.FilePath}: {ex}");
                MessageBox.Show($"A error occurred when deleting the file. Check the logs for more information.");
            }
        }

        /// <summary>
        /// The event that is called when the Cancel button is clicked, closing the window.
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