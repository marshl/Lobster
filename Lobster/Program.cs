using Lobster.Properties;
using System;
using System.IO;
using System.Windows.Forms;

namespace Lobster
{
    /// <summary>
    /// '"It's a dangerous business, Frodo, going out of your door," he used to say. 
    /// "You step into the Road, and if you don't keep your feet, there is no knowing 
    ///  where you might be swept off to."'
    ///     -- Frodo
    /// [ _The Lord of the Rings_, I/iii: "Three is Company"]
    /// </summary>
    static class Program
    {
        [STAThread]
        static void Main()
        {
            MessageLog log;
            try
            {
                log = new MessageLog();
            }
            catch ( Exception _e )
            {
                Common.ShowErrorMessage("Error", "An unhandled " + _e.GetType().ToString() + " occurred when attempting to create the log file.");
                return;
            }

            GitHubUpdater.RunUpdateCheck( "marshl", "lobster" );

            if ( !Directory.Exists( Settings.Default.SettingsDirectoryName ) )
            {
                Common.ShowErrorMessage("Directory Not Found", "The settings directory " + Settings.Default.SettingsDirectoryName + " could not be found.");
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

#if !DEBUG
            try
#endif
            {
                Application.ThreadException += Application_ThreadException;
                LobsterMain lobsterMain = new LobsterMain();
                Application.Run( lobsterMain );
            }
#if !DEBUG
            catch ( Exception _e )
            {
                MessageLog.LogError( "UNHANDLED EXCEPTION: " + _e.ToString() );
                Common.ShowErrorMessage(
                    "Error",
                    "An unhandled " + _e.GetType().ToString() + " was thrown. Check " + Settings.Default.LogFilename
                    + " for more information, and please create an error issue at https://github.com/marshl/lobster");
            }
            finally
#endif
            {
                log.Close();
            }
        }

        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            MessageLog.LogError("UNHANDLED EXCEPTION: " + e.ToString());
            Common.ShowErrorMessage(
                "Error",
                "An unhandled " + e.GetType().ToString() + " was thrown. Check " + Settings.Default.LogFilename
                + " for more information, and please create an error issue at https://github.com/marshl/lobster");

        }
    }
}
