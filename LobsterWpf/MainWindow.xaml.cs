﻿//-----------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="marshl">
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
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Media;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Forms;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Navigation;
    using System.Windows.Shapes;
    using LobsterModel;
    using LobsterWpf.Properties;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public sealed partial class MainWindow : Window, IModelEventListener
    {
        /// <summary>
        /// 
        /// </summary>
        private ConnectionView connectionView;

        /// <summary>
        /// Gets or sets the model 
        /// </summary>
        public Model Model { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenConnectionDialog()
        {
            ConnectionListWindow window = new ConnectionListWindow(this.Model);
            window.Owner = this;
            bool? result = window.ShowDialog();
            if (result.HasValue && result.Value)
            {
                this.ConnectionContainer.IsEnabled = this.Model.CurrentConnection != null;
                this.ConnectionContainer.DataContext = this.connectionView = new ConnectionView(this.Model.CurrentConnection);
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.OpenConnectionDialog();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.OpenConnectionDialog();
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void clobTypeListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.RepopulateFileListView();
        }

        private void RepopulateFileListView()
        {
            if (this.clobTypeListBox.SelectedIndex == -1)
            {
                return;
            }

            ClobType clobType = this.Model.CurrentConnection.ClobDirectoryList[this.clobTypeListBox.SelectedIndex].ClobType;
            this.connectionView = this.ConnectionContainer.DataContext as ConnectionView;
            if (connectionView != null)
            {
                connectionView.PopulateFileTreeForClobType(clobType);
                this.localFileTreeView.ItemsSource = connectionView.RootFile.Children;
            }
        }

        private void hideReadonlyCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            this.RepopulateFileListView();
        }

        private void hideReadonlyCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            this.RepopulateFileListView();
        }

        private void pushButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.connectionView.SelectedFileNode != null)
            {
                string filename = this.connectionView.SelectedFileNode.FullName;

                try
                {
                    this.Model.SendUpdateClobMessage(filename);
                }
                catch (FileUpdateException)
                {
                    this.DisplayUpdateNotification(filename, false);
                    return;
                }
                this.DisplayUpdateNotification(filename, true);
            }
        }

        private void diffButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.connectionView.SelectedFileNode == null)
            {
                return;
            }

            string filename = this.connectionView.SelectedFileNode.FullName;


            try
            {
                string downloadedFile = this.Model.SendDownloadClobDataToFileMessage(filename);
                string args = string.Format(
                    Settings.Default.DiffProgramArguments,
                    downloadedFile,
                    filename);

                Process.Start(Settings.Default.DiffProgramName, args);
            }
            catch (FileDownloadException ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
                return;
            }
        }

        private void exploreButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.connectionView.SelectedFileNode != null)
            {
                // Windows Explorer command line arguments: https://support.microsoft.com/en-us/kb/152457
                Process.Start("explorer", "/select," + this.connectionView.SelectedFileNode.FullName);
            }
        }

        private void insertButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.connectionView.SelectedFileNode != null)
            {
                try
                {
                    string filename = this.connectionView.SelectedFileNode.FullName;
                    bool result = this.Model.SendInsertClobMessage(filename);

                    if (result)
                    {
                        this.DisplayUpdateNotification(filename, true);
                    }
                }
                catch (FileInsertException ex)
                {
                    System.Windows.MessageBox.Show(ex.Message);
                }
            }
        }

        private void localFileTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            this.connectionView.SelectedFileNode = (FileNodeView)e.NewValue;
        }

        void IModelEventListener.OnAutoUpdateComplete(string filename, bool updateSuccess)
        {
            this.Dispatcher.Invoke(new FileOperationDelegate(DisplayUpdateNotification), filename, updateSuccess);
        }

        private delegate void FileOperationDelegate(string filename, bool success);

        private void DisplayUpdateNotification(string filename, bool success)
        {
            string message = $"File update {(success ? "succeeded" : "failed")} for {(System.IO.Path.GetFileName(filename))}.";
            NotificationWindow nw = new NotificationWindow(message, success);
            nw.Show();
            SystemSounds.Question.Play();
        }

        bool IModelEventListener.PromptForTable(string fullpath, LobsterModel.Table[] tables, ref LobsterModel.Table table)
        {
            TableSelectorWindow tsw = new TableSelectorWindow(fullpath, tables);
            bool? result = tsw.ShowDialog();
            if (result ?? false)
            {
                table = tsw.SelectedTable;
            }

            return result ?? false;
        }

        bool IModelEventListener.PromptForMimeType(string fullpath, string[] mimeTypes, ref string mimeType)
        {
            MimeTypeSelectorWindow msw = new MimeTypeSelectorWindow(fullpath, mimeTypes);
            bool? result = msw.ShowDialog();
            if (result ?? false)
            {
                mimeType = msw.SelectedMimeType;
            }

            return result ?? false;
        }

        void IModelEventListener.OnEventProcessingStart()
        {
            this.Dispatcher.Invoke((MethodInvoker)delegate
            {
                this.connectionView.IsEnabled = false;
            });
        }

        void IModelEventListener.OnFileProcessingFinished()
        {
            this.Dispatcher.Invoke((MethodInvoker)delegate
            {
                this.connectionView.IsEnabled = true;

                this.RepopulateFileListView();
            });
        }
    }
}
