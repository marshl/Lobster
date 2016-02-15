using System.Windows;

namespace LobsterWpf
{
    /// <summary>
    /// Interaction logic for MessageListWindow.xaml
    /// </summary>
    public partial class MessageListWindow : Window
    {
        public static MessageListWindow Instance { get; private set; }

        public MessageLogView LogView { get; set; }

        public MessageListWindow()
        {
            MessageListWindow.Instance = this;
            InitializeComponent();

            this.LogView = new MessageLogView();

            this.DataContext = this.LogView;
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            MessageListWindow.Instance = null;
        }
    }
}
