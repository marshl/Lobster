using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace LobsterWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Model model;

        //public DatabaseConnection CurrentConnection { get; private set; }

        private ConnectionView connectionView;

        public MainWindow(Model lobsterModel)
        {
            InitializeComponent();

            this.model = lobsterModel;


            this.ConnectionContainer.DataContext = null;
        }

        private void OpenConnectionDialog()
        {
            ConnectionListWindow window = new ConnectionListWindow(this.model);
            bool? result = window.ShowDialog();
            if (result.HasValue && result.Value)
            {
                this.ConnectionContainer.IsEnabled = this.model.CurrentConnection != null;
                this.ConnectionContainer.DataContext = new ConnectionView(this.model.CurrentConnection);
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
            if (this.clobTypeListBox.SelectedIndex == -1)
            {
                return;
            }

            ClobType clobType = this.model.CurrentConnection.ClobDirectoryList[this.clobTypeListBox.SelectedIndex].ClobType;

            this.connectionView = this.ConnectionContainer.DataContext as ConnectionView;
            if (connectionView != null)
            {
                connectionView.PopulateFileTreeForClobType(clobType);
                this.localFileTreeView.ItemsSource = connectionView.RootFile.Children;
            }

            //this.PopulateFileTreeForClobType(clobType);
        }
        /*
        private void PopulateFileTreeForClobType(ClobType clobType)
        {
            //this.localFileTreeView.Items.Clear();

            DirectoryInfo rootDirInfo = new DirectoryInfo(clobType.Fullpath);
            if (!rootDirInfo.Exists)
            {
                return;
            }

            /*foreach (DirectoryInfo childDirInfo in rootDirInfo.GetDirectories())
            {
                this.localFileTreeView.Items.Add(childDirInfo.Name);
            }* /

            FileNodeView node = new FileNodeView(rootDirInfo.FullName);
            this.localFileTreeView.DataContext = node;
        }*/

        private void hideReadonlyCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            //this.clobTypeListBox_SelectionChanged(null, null);
            this.connectionView.NotifyPropertyChanged("ClobTypes");
        }

        private void hideReadonlyCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            //this.clobTypeListBox_SelectionChanged(null, null);
            this.connectionView.NotifyPropertyChanged("ClobTypes");
        }

        /*
        private void PopulateFileTreeNode(DirectoryInfo dirInfo )
        {
        foreach (DirectoryInfo childDirInfo in rootDirInfo.GetDirectories())
        {
        this.localFileTreeView.Items.Add(childDirInfo.Name);
        }

        this.localFileTreeView.DataContext = this.
        }*/
    }
}
