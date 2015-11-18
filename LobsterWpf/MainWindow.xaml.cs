using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LobsterModel;
using LobsterWpf.Properties;

namespace LobsterWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public sealed partial class MainWindow : Window, IModelEventListener
    {
        private ConnectionView connectionView;

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
                this.ConnectionContainer.DataContext = new ConnectionView(this.Model.CurrentConnection);
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
                this.Model.SendUpdateClobMessage(this.connectionView.SelectedFileNode.FullName);
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
                MessageBox.Show(ex.Message);
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
                this.Model.SendInsertClobMessage(this.connectionView.SelectedFileNode.FullName);
            }
        }

        private void localFileTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            //FileNodeView fnv = (FileNodeView)e.NewValue;
            this.connectionView.SelectedFileNode = (FileNodeView)e.NewValue;
            //MessageBox.Show(fnv.FullName);
        }

        void IModelEventListener.OnFileChange(string filename)
        {
            MessageBox.Show("File Change");
        }

        void IModelEventListener.OnAutoUpdateComplete(string filename)
        {
            throw new NotImplementedException();
        }

        LobsterModel.Table IModelEventListener.PromptForTable(string fullpath, LobsterModel.Table[] tables)
        {
            throw new NotImplementedException();
        }

        string IModelEventListener.PromptForMimeType(string fullpath, string[] mimeTypes)
        {
            throw new NotImplementedException();
        }
    }
}
