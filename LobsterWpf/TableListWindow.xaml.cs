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
        private ClobType clobType;

        public TableListWindow(ClobType clobType)
        {
            this.clobType = clobType;
            this.TableList = new ObservableCollection<TableView>();
            foreach (Table table in clobType.Tables)
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
            Table table = new Table();
            EditTableWindow window = new EditTableWindow(table);
            window.Owner = this;
            bool? result = window.ShowDialog();

            if (result ?? false)
            {
                this.TableList.Add(window.Table);
                this.clobType.Tables.Add(table);
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            TableView tableView = (TableView)this.tableListBox.SelectedItem;

            if (tableView == null)
            {
                return;
            }

            EditTableWindow window = new EditTableWindow(tableView.TableObject);
            window.Owner = this;
            bool? result = window.ShowDialog();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            TableView tableView = (TableView)this.tableListBox.SelectedItem;

            if (tableView == null)
            {
                return;
            }

            this.TableList.Remove(tableView);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
