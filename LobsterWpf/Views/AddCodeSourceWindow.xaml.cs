using System;
using System.Collections.Generic;
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

namespace LobsterWpf.Views
{
    /// <summary>
    /// Interaction logic for AddCodeSourceWindow.xaml
    /// </summary>
    public partial class AddCodeSourceWindow : Window
    {
        public enum Selection
        {
            AddPreparedCodeSource,
            PrepareNewCodeSource
        }

        public Selection? UserSelection { get; private set; }

        public AddCodeSourceWindow()
        {
            InitializeComponent();
        }

        private void addPreparedCodeSourceButton_Click(object sender, RoutedEventArgs e)
        {
            this.UserSelection = Selection.AddPreparedCodeSource;
            this.DialogResult = true;
            this.Close();
        }

        private void prepareNewCodeSourceButton_Click(object sender, RoutedEventArgs e)
        {
            this.UserSelection = Selection.PrepareNewCodeSource;
            this.DialogResult = true;
            this.Close();
        }
    }
}
