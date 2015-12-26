using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using LobsterModel;

namespace LobsterWpf
{
    /// <summary>
    /// Interaction logic for TableListWindow.xaml
    /// </summary>
    public partial class TableListWindow : Window
    {
        public TableListWindow( List<Table> tables)
        {
            this.TableList = new ObservableCollection<TableView>();
            foreach (Table table in tables)
            {
                TableView tableView = new TableView(table);
                this.TableList.Add(tableView);
            }

            this.InitializeComponent();

            this.DataContext = this;
        }

        public ObservableCollection<TableView> TableList { get; }

        private void NewButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
