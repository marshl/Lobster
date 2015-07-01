using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lobster
{
    public class MessageLog
    {
        private static MessageLog instance;
        public TextBox textBox;

        private StreamWriter stream;

        public MessageLog()
        {
            MessageLog.instance = this;
            this.stream = new StreamWriter( Program.LOG_FILE, true );
            this.stream.Write( "\n\n" );
            Log( "Lobster Started" );
        }

        public void Close()
        {
            Log( "Lobster Terminated Normally" );
            this.stream.Close();
        }

        private void InternalLog( string _message )
        {
            _message = DateTime.Now + ": " + _message;
            this.stream.WriteLine( _message );
#if DEBUG
            this.stream.Flush();
#endif
        }

        public static void Log( string _message )
        {
            MessageLog.instance.InternalLog( _message );
        }
    }
}
