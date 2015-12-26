using System.Collections.ObjectModel;
using System.Windows;
using LobsterModel;

namespace LobsterWpf
{
    /// <summary>
    /// Interaction logic for ColumnListWindow.xaml
    /// </summary>
    public partial class ColumnListWindow : Window
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="TableListWindow"/> class.
        /// </summary>
        /// <param name="clobType">The ClobType whose tables are being modified.</param>
        public ColumnListWindow(Table table)
        {
            this.BaseTable = table;
            this.RefreshColumnList();

            this.InitializeComponent();

            this.DataContext = this;
        }

        public Table BaseTable { get; }

        /// <summary>
        /// Gets the tables views from the ClobType.
        /// </summary>
        public ObservableCollection<ColumnView> ColumnList { get; private set; }

        /// <summary>
        /// Refreshes the list of tables with the tables in the clob type.
        /// </summary>
        private void RefreshColumnList()
        {
            this.ColumnList = new ObservableCollection<ColumnView>();
            foreach (Column column in this.BaseTable.Columns)
            {
                ColumnView tableView = new ColumnView(column);
                this.ColumnList.Add(tableView);
            }
        }

        /// <summary>
        /// The event called when the New button is clicked, creating a new Table and opening and edit window for it.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void NewButton_Click(object sender, RoutedEventArgs e)
        {
            Column column = new Column();
            EditColumnWindow window = new EditColumnWindow(column);
            window.Owner = this;
            bool? result = window.ShowDialog();

            this.BaseTable.Columns.Add(column);
            this.RefreshColumnList();
        }

        /// <summary>
        /// The event called when the Edit button is clicked, opening a new edit window for the selected Table.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            ColumnView columnView = (ColumnView)this.columnListBox.SelectedItem;

            if (columnView == null)
            {
                return;
            }

            EditColumnWindow window = new EditColumnWindow(columnView.BaseColumn);
            window.Owner = this;
            bool? result = window.ShowDialog();
        }

        /// <summary>
        /// The event called when the Delete button is clicked, 
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            ColumnView columnView = (ColumnView)this.columnListBox.SelectedItem;

            if (columnView == null)
            {
                return;
            }

            this.BaseTable.Columns.Remove(columnView.BaseColumn);
            this.RefreshColumnList();
        }

        /// <summary>
        /// The event called when the Close button is clicked, closing the window.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
