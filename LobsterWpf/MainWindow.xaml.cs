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
    }
}
