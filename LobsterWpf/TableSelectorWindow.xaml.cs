using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Documents;
using LobsterModel;

namespace LobsterWpf
{
    /// <summary>
    /// Interaction logic for TableSelectorWindow.xaml
    /// </summary>
    public partial class TableSelectorWindow : Window
    {
        public string MessageLabel { get; set; }

        public ObservableCollection<LobsterModel.Table> TableList { get; set; }
        public LobsterModel.Table SelectedTable
        {
            get
            {
                return (LobsterModel.Table)this.tableListBox.SelectedItem;
            }
        }

        public TableSelectorWindow(string filepath, LobsterModel.Table[] tables)
        {
            InitializeComponent();

            this.MessageLabel = $"Please select the table to insert {System.IO.Path.GetFileName(filepath)} into:";
            this.TableList = new ObservableCollection<LobsterModel.Table>(tables);
        }

        private void acceptButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.SelectedTable != null)
            {
                this.DialogResult = true;
                this.Close();
            }
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
