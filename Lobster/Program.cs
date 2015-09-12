using System;
using System.IO;
using System.Windows.Forms;

namespace Lobster
{
    static class Program
    {
        public static string SETTINGS_DIR = "LobsterSettings";
        public static string DB_CONFIG_DIR = SETTINGS_DIR + "\\DatabaseConnections";
        public static string LOG_FILE = "lobster.log";

        public static int BALLOON_TOOLTIP_DURATION_MS = 2000;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
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

            if ( !Directory.Exists( DB_CONFIG_DIR ) )
            {
                DialogResult result = MessageBox.Show( "The Database Connections directory " + DB_CONFIG_DIR + " could not be found.", "Directory Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1 );
                return;
            }

#if !DEBUG
            try
#endif
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault( false );
                LobsterMain lobsterMain = new LobsterMain();
                Application.Run( lobsterMain );
            }
#if !DEBUG
            catch ( Exception _e )
            {
                MessageLog.LogError( "UNHANDLED EXCEPTION: {0}", _e.ToString() );
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
