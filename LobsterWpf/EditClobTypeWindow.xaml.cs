namespace LobsterWpf
{
    using System.Windows;
    using LobsterModel;

    /// <summary>
    /// Interaction logic for EditClobTypeWindow.xaml
    /// </summary>
    public partial class EditClobTypeWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EditClobTypeWindow"/> class.
        /// </summary>
        /// <param name="clobType">The clob type to use as the view of this window.</param>
        public EditClobTypeWindow(ClobType clobType)
        {
            InitializeComponent();

            this.ClobTypeView = new ClobTypeView(clobType);

            this.DataContext = this.ClobTypeView;
        }

        public ClobTypeView ClobTypeView { get; }

        /// <summary>
        /// The event that is called when the Ok button is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            bool result = this.ClobTypeView.ApplyChanges();

            if (result)
            {
                this.DialogResult = true;
                this.Close();
            }
        }

        /// <summary>
        /// The event that is called when the accept button is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            bool result = this.ClobTypeView.ApplyChanges();
        }

        /// <summary>
        /// The event that is called when the cancel button is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
