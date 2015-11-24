using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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
    /// Interaction logic for MimeTypeSelectorWindow.xaml
    /// </summary>
    public partial class MimeTypeSelectorWindow : Window
    {
        public string MessageLabel { get; set; }

        public ObservableCollection<string> MimeTypeList { get; set; }

        public string SelectedMimeType
        {
            get
            {
                return (string)this.mimeTypeListBox.SelectedItem;
            }
        }

        public MimeTypeSelectorWindow(string filename, string[] mimeTypes)
        {
            this.MessageLabel = $"Please select a mime type for {System.IO.Path.GetFileName(filename)}:";
            this.MimeTypeList = new ObservableCollection<string>(mimeTypes);
            this.DataContext = this;

            InitializeComponent();
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void acceptButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.SelectedMimeType != null)
            {
                this.DialogResult = true;
                this.Close();
            }
        }
    }
}
