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
        public const string SETTINGS_DIR = "LobsterSettings";
        public const string LOG_FILE = "lobster.log";

        public const int BALLOON_TOOLTIP_DURATION_MS = 2000;

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
                DialogResult result = MessageBox.Show( "An unhandled " + _e.GetType().ToString() + " occurred when attempting to create the log file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1 );
                return;
            }

            GitHubUpdater.RunUpdateCheck( "marshl", "lobster" );

            if ( !Directory.Exists( SETTINGS_DIR ) )
            {
                DialogResult result = MessageBox.Show( "The settings directory " + SETTINGS_DIR + " could not be found.", "Directory Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1 );
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (Settings.Default.ConnectionDir == null || !Directory.Exists(Settings.Default.ConnectionDir) )
            {
                string connectionDir = LobsterModel.PromptForDirectory("Please select your DatabaseConnections folder", null);
                
                if (connectionDir == null)
                {
                    return;
                }

                Settings.Default.ConnectionDir = connectionDir;
                Settings.Default.Save();
            }

#if !DEBUG
            try
#endif
            {
                LobsterMain lobsterMain = new LobsterMain();
                Application.Run( lobsterMain );
            }
#if !DEBUG
            catch ( Exception _e )
            {
                MessageLog.LogError( "UNHANDLED EXCEPTION: " + _e.ToString() );
                DialogResult result = MessageBox.Show( "An unhandled " + _e.GetType().ToString() + " was thrown. Check " + LOG_FILE + " for more information, and please create an error issue at https://github.com/marshl/lobster.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1 );
            }
            finally
#endif
            {
                log.Close();
            }
        }
    }
}
