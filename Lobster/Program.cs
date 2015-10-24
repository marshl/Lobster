//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="marshl">
// Copyright 2015, Liam Marshall, marshl.
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
//      '"It's a dangerous business, Frodo, going out of your door," he used to say. 
//      "You step into the Road, and if you don't keep your feet, there is no knowing 
//      where you might be swept off to."'
//          -- Frodo
//
//      [ _The Lord of the Rings_, I/iii: "Three is Company"]
//
//-----------------------------------------------------------------------
namespace Lobster
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Windows.Forms;
    using Properties;

    /// <summary>
    /// The main application class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The application entry point.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            SplashScreen splash = new SplashScreen();
            splash.Show();

            MessageLog log;
            try
            {
                log = new MessageLog();
            }
            catch (Exception e)
            {
                Common.ShowErrorMessage("Error", "An error occurred when attempting to create the log file: " + e);
                return;
            }

            if (!Directory.Exists(Settings.Default.SettingsDirectoryName))
            {
                Common.ShowErrorMessage("Directory Not Found", "The settings directory " + Settings.Default.SettingsDirectoryName + " could not be found.");
                return;
            }

            try
            {
                GitHubUpdater.RunUpdateCheck("marshl", "lobster");
                LobsterMain lobsterMain = new LobsterMain();

                splash.Close();

                Application.ThreadException += Program.Application_ThreadException;
                Application.Run(lobsterMain);
            }
            catch (LobsterModel.ConnectionDirNotFoundException)
            {
                Common.ShowErrorMessage("Error", "The database connection directory could not be found.");
            }
#if !DEBUG
            catch (Exception e)
            {
                MessageLog.LogError("UNHANDLED EXCEPTION: " + e.ToString());
                string msg = $@"An unhandled exception was thrown. Check {Settings.Default.LogFilename}
                    for more information, and please create an error issue at https://github.com/marshl/lobster";
                Common.ShowErrorMessage("Error", msg);
            }
#endif
            finally
            {
                log.Close();
            }
        }

        /// <summary>
        /// The exception handler for uncaught application exceptions.
        /// </summary>
        /// <param name="sender">The sender of the message.</param>
        /// <param name="e">The event arguments.</param>
        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            MessageLog.LogError("UNHANDLED EXCEPTION: " + e.Exception.ToString());
            string msg = $@"An unhandled exception was thrown. Check {Settings.Default.LogFilename}
                for more information, and please create an error issue at https://github.com/marshl/lobster";
            Common.ShowErrorMessage("Error", msg);
        }
    }
}
