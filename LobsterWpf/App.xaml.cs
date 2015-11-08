using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using LobsterModel;

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

            MessageLog messageLog = new MessageLog();

            Console.WriteLine("foobar");
            TempListener tmp = new TempListener();
            LobsterModel.Model model = new LobsterModel.Model(tmp);
            MainWindow mainWindow = new MainWindow(model);
            mainWindow.Show();
        }
    }

    class TempListener : LobsterModel.IModelEventListener
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
