using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace LobsterWpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {

        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Lobster.MessageLog messageLog = new Lobster.MessageLog();

            Console.WriteLine("foobar");
            TempListener tmp = new TempListener();
            Lobster.LobsterModel model = new Lobster.LobsterModel(tmp);
            MainWindow mainWindow = new MainWindow(model);
            mainWindow.Show();
        }
    }

    class TempListener : Lobster.IModelEventListener
    {
        public void OnFileChange()
        {
            throw new NotImplementedException();
        }

        public void OnUpdateComplete()
        {
            throw new NotImplementedException();
        }
    }
}
