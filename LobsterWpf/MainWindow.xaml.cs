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

namespace LobsterWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<ClobType> ClobTypes { get; }

        private Lobster.LobsterModel model;

        public class ClobType
        {
            public string Name { get; set; }
        }

        public MainWindow(Lobster.LobsterModel lobsterModel)
        {
            InitializeComponent();

            this.model = lobsterModel;

            this.ClobTypes = new ObservableCollection<ClobType>();
            this.ClobTypes.Add(new ClobType() { Name = "Hello" });
            this.ClobTypes.Add(new ClobType() { Name = "World" });

            this.DataContext = this;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            ConnectionListWindow window = new ConnectionListWindow(this.model);
            window.ShowDialog();
        }
    }
}
