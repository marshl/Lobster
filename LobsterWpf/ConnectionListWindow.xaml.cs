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

namespace LobsterWpf
{
    /// <summary>
    /// Interaction logic for ConnectionListWindow.xaml
    /// </summary>
    public partial class ConnectionListWindow : Window
    {
        private Lobster.LobsterModel model;

        public string ConnectionDirectory { get; set; }

        public ObservableCollection<Lobster.DatabaseConfig> databaseConfigList { get; set; }

        public ConnectionListWindow(Lobster.LobsterModel lobsterModel)
        {
            InitializeComponent();

            this.model = lobsterModel;

            this.ConnectionDirectory = 
            this.databaseConfigList = new ObservableCollection<Lobster.DatabaseConfig>(this.model.GetConfigList());

            this.DataContext = this;
            //this.connectionListBox.DataContext = lobsterModel.GetConfigList();
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
