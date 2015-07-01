﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using Oracle.DataAccess.Client;
using System.IO;
using System.Xml;
using System.Runtime.InteropServices;

namespace Lobster
{
    static class Program
    {
        public static string SETTINGS_DIR = "LobsterSettings";
        public static string DB_CONFIG_FILE = "DatabaseConfig.xml";
        public static string CLOB_TYPE_DIR = "ClobTypes";

        public static int BALLOON_TOOLTIP_DURATION = 3000;

        [DllImport( "kernel32.dll" )]
        static extern bool AttachConsole( uint dwProcessId );

        [DllImport( "kernel32.dll", SetLastError = true )]
        static extern bool AllocConsole();
        private static uint ATTACH_PARENT_PROCESS = 0x0ffffffff;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            AttachConsole( 0x0ffffffff );
            AllocConsole();

            LobsterModel lobsterModel = new LobsterModel();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault( false );
            Application.Run( new LobsterMain( lobsterModel ) );

            foreach ( FileInfo tempFile in lobsterModel.tempFileList )
            {
                File.Delete( tempFile.FullName );
            }
        }
    }
}
