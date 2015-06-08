using System;
using System.Collections.Generic;
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

        public MessageLog()
        {
            MessageLog.instance = this;
        }

        private void InternalLog( string _message )
        {
            _message = "\r\n" + this.GetDivider() + "\r\n" + System.DateTime.Now + "\r\n" + _message;
            this.textBox.AppendText( _message );
        }

        public static void Log( string _message )
        {
            MessageLog.instance.InternalLog( _message );
        }

        public string GetDivider()
        {
            return "=".PadRight( 80, '=' );
        }
    }
}
