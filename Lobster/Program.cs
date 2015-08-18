using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace Lobster
{
    static class Program
    {
        public static string SETTINGS_DIR = "LobsterSettings";
        public static string DB_CONFIG_DIR = SETTINGS_DIR + "\\DatabaseConnections";
        public static string CLOB_TYPE_DIR = SETTINGS_DIR + "\\ClobTypes";
        public static string LOG_FILE = "lobster.log";

        public static int BALLOON_TOOLTIP_DURATION_MS = 2000;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            /*ClobType ct = new ClobType();
            ct.name = "Business Process Definitions";
            ct.directory = "BusinessProcessDefinitions";
            ct.tables = new List<ClobType.Table>();
            ClobType.Table t = new ClobType.Table();
            ct.tables.Add( t );
            t.schema = "bpmmgr";
            t.name = "business_process_definitions";

            
            t.columns = new List<ClobType.Column>();
            ClobType.Column idc = new ClobType.Column();
            t.columns.Add( idc );
            idc.name = "bp_id";
            idc.purpose = ClobType.Column.Purpose.FOREIGN_KEY;
            idc.parent = t;
            / *
            ClobType.Column mc = new ClobType.Column();
            t.columns.Add( mc );
            mc.name = "document_library_type";
            mc.purpose = ClobType.Column.Purpose.MNEMONIC;
            mc.parent = t;* /
            
            ClobType.Column dc = new ClobType.Column();
            t.columns.Add( dc );
            dc.name = "xml_data";
            dc.purpose = ClobType.Column.Purpose.CLOB_DATA;
            dc.dataType = ClobType.Column.Datatype.XMLTYPE;
            dc.parent = t;

            ClobType.Column tc = new ClobType.Column();
            t.columns.Add( tc );
            tc.name = "start_datetime";
            tc.purpose = ClobType.Column.Purpose.DATETIME;
            tc.parent = t;
            


            ClobType.Table pt = new ClobType.Table();
            pt.name = "business_processes";
            t.parentTable = pt;
            pt.columns = new List<ClobType.Column>();
            pt.schema = "bpmmgr";

            ClobType.Column pc = new ClobType.Column();
            pc.name = "id";
            pc.purpose = ClobType.Column.Purpose.ID;
            pt.columns.Add( pc );
            pc.parent = pt;

            ClobType.Column pmc = new ClobType.Column();
            pmc.name = "short_name";
            pmc.purpose = ClobType.Column.Purpose.MNEMONIC;
            pt.columns.Add( pmc );
            pmc.parent = pt;

            XmlSerializer xsSubmit = new XmlSerializer( typeof( ClobType ) );
            StreamWriter sww = new StreamWriter( "temp.xml" );
            XmlWriterSettings xws = new XmlWriterSettings();
            xws.Indent = true;
            XmlWriter writer = XmlWriter.Create( sww, xws );
            xsSubmit.Serialize( writer, ct );
            writer.Close();
            sww.Close();

            string s = ct.BuildUpdateStatement( 0, ClobType.Column.Datatype.XMLTYPE );
            
            return;
            */

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

            if ( !Directory.Exists( CLOB_TYPE_DIR ) )
            {
                DialogResult result = MessageBox.Show( "The Clob Type directory " + CLOB_TYPE_DIR + " could not be found.", "Directory Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1 );
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
