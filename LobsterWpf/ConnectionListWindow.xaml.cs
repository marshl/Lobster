using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
    public partial class ConnectionListWindow : Window, INotifyPropertyChanged
    {
        private Model model;

        private string _connectionDirectory;

        public event PropertyChangedEventHandler PropertyChanged;

        public string ConnectionDirectory
        {
            get
            {
                return this._connectionDirectory;
            }
            set
            {
                this._connectionDirectory = value;
                this.NotifyPropertyChanged("ConnectionDirectory");
            }
        }

        // This method is called by the Set accessor of each property.
        // The CallerMemberName attribute that is applied to the optional propertyName
        // parameter causes the property name of the caller to be substituted as an argument.
        private void NotifyPropertyChanged(string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private ObservableCollection<DatabaseConfig> _databaseConfigList;
        public ObservableCollection<DatabaseConfig> DatabaseConfigList
        {
            get
            {
                return this._databaseConfigList;
            }

            set
            {
                this._databaseConfigList = value;
                this.NotifyPropertyChanged("DatabaseConfigList");
            }
        }
        
        public ConnectionListWindow(Model lobsterModel)
        {
            InitializeComponent();

            this.model = lobsterModel;

            this.ConnectionDirectory = this.model.ConnectionDirectory;
            this.DatabaseConfigList = new ObservableCollection<DatabaseConfig>(this.model.GetConfigList());

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
            if (result == CommonFileDialogResult.Ok)
            {
                this.model.ChnageConnectionDirectory(dlg.FileName);
                this.DatabaseConfigList = new ObservableCollection<DatabaseConfig>(this.model.GetConfigList());
                this.ConnectionDirectory = this.model.ConnectionDirectory;
            }

        }

        private void connectButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.connectionListBox.SelectedIndex == -1)
            {
                return;
            }

            DatabaseConfig config = this.DatabaseConfigList[this.connectionListBox.SelectedIndex];

            this.TryConnectWithConfig(config);
        }

        protected void HandleDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DatabaseConfig config = ((ListBoxItem)sender).Content as DatabaseConfig;
            this.TryConnectWithConfig(config);
        }

        private void TryConnectWithConfig(DatabaseConfig config)
        {
            try
            {
                this.model.SetDatabaseConnection(config);
                this.DialogResult = true;
                this.Close();
            }
            catch (SetConnectionException ex)
            {
                MessageBox.Show($"An error occurred when attempting to connect to the database: \n{ex}");
            }
        }
    }
}
