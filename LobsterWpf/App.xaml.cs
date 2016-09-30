//-----------------------------------------------------------------------
// <copyright file="App.xaml.cs" company="marshl">
// Copyright 2016, Liam Marshall, marshl.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//-----------------------------------------------------------------------
//
//      'The beginnings lie back in the Black Years, which only the 
//      lore-masters now remember.'
//          - Gandalf
//
//      [ _The Lord of the Rings_, I/ii: "The Shadow of the Past"]
//
//-----------------------------------------------------------------------
namespace LobsterWpf
{
    using System;
    using System.Windows;
    using LobsterModel;
    using Properties;
    using Views;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// The message log for the duration of this app.
        /// </summary>
        private MessageLog log;

        /// <summary>
        /// Initializes a new instance of the <see cref="App"/> class.
        /// </summary>
        public App()
        {
            this.log = MessageLog.Initialise();
            if (Settings.Default.DeleteBackupFiles)
            {
                BackupLog.DeleteOldBackupFiles(Settings.Default.BackupFileLifetimeDays);
            }

#if !DEBUG
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(this.GlobalExceptionHandler);
#endif
        }

        /// <summary>
        /// The event called when the application starts.
        /// </summary>
        /// <param name="e">The startup event arguments.</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }

        /// <summary>
        /// The handler for the global uncaught exception event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="args">The exception arguments.</param>
        private void GlobalExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            string message = @"An unhandled exception has occurred. 
Please add an issue to the Lobster GitHub repo, with a copy of the log file attached. 
Ensure that there are no passwords in the file before uploading it.";

            try
            {
                MessageLog.LogError($"An unhandled exception occurred:\n {e}");
                MessageLog.Close();
                MessageBox.Show(message);
            }
            catch (Exception ex)
            {
                MessageBox.Show($@"An unhandled exception occurred, but an error occurred when attempting to write the message to the log file. 
The original exception was: {e}
The exception that occurred when logging the exception was: {ex}");
            }
            finally
            {
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// The event called when the application exits.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The exit event arguments.</param>
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            MessageLog.Close();
        }
    }
}
