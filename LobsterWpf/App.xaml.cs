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
        private MessageLog log;

        public App()
        {
            this.log = new MessageLog();
#if !DEBUG
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(GlobalExceptionHandler);
#endif
            }

        private void GlobalExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            MessageLog.LogError("Unhandled Exception " + e);
            MessageLog.Flush();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            MainWindow mainWindow = new MainWindow();
            Model model = new Model(mainWindow);
            mainWindow.Model = model;
            mainWindow.Show();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            MessageLog.Close();
        }
    }
}
