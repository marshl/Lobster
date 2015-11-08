using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;
using LobsterModel;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace LobsterWpf
{
    /// <summary>
    /// Interaction logic for ConnectionListWindow.xaml
    /// </summary>
    public partial class ConnectionListWindow : Window
    {
        private Model model;

        public string ConnectionDirectory { get; set; }

        public ObservableCollection<DatabaseConfig> databaseConfigList { get; set; }

        //public DatabaseConfig SelectedConnection { get; private set; }

        public ConnectionListWindow(Model lobsterModel)
        {
            InitializeComponent();

            this.model = lobsterModel;

            this.ConnectionDirectory = this.model.ConnectionDirectory;
            this.databaseConfigList = new ObservableCollection<DatabaseConfig>(this.model.GetConfigList());

            this.DataContext = this;
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void changeDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new CommonOpenFileDialog();
            dlg.IsFolderPicker = true;
            CommonFileDialogResult result = dlg.ShowDialog();
            if ( result == CommonFileDialogResult.Ok )
            {
                this.model.ChnageConnectionDirectory(dlg.FileName);
                this.databaseConfigList = new ObservableCollection<DatabaseConfig>(this.model.GetConfigList());
                this.ConnectionDirectory = this.model.ConnectionDirectory;
            }
            
        }

        private void connectButton_Click(object sender, RoutedEventArgs e)
        {
            if ( this.connectionListBox.SelectedIndex == -1 )
            {
                return;
            }

            DatabaseConfig config = this.databaseConfigList[this.connectionListBox.SelectedIndex];
            if ( this.model.SetDatabaseConnection(config) )
            {
                this.DialogResult = true;
                this.Close();
            }
        }
    }
}
