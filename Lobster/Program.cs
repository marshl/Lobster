using System;
using System.Windows.Forms;

namespace Lobster
{
    static class Program
    {
        public static string SETTINGS_DIR = "LobsterSettings";
        public static string DB_CONFIG_FILE = "DatabaseConfig.xml";
        public static string CLOB_TYPE_DIR = "ClobTypes";
        public static string LOG_FILE = "lobster.log";

        public static int BALLOON_TOOLTIP_DURATION_MS = 2000;
        public static int CLOB_DELAY_DURATION_MS = 200;

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
                DialogResult result = MessageBox.Show( "An unhandled " + _e.GetType().ToString() + " occurred when attempting to craete the log file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1 );
                return;
            }

            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault( false );
                LobsterMain lobsterMain = new LobsterMain();
                Application.Run( lobsterMain );
            }
            catch ( Exception _e )
            {
                MessageLog.Log( _e.ToString() );
                DialogResult result = MessageBox.Show( "An unhandled " + _e.GetType().ToString() + " was thrown. Check " + LOG_FILE + " for more information.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1 );
            }
            finally
            {
                log.Close();
            }
        }
    }
}
