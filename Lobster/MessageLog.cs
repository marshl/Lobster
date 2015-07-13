﻿using System;
using System.IO;
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
            Log( "Starting Lobster" );
        }

        public void Close()
        {
            Log( "Lobster Stopped" );
            this.stream.Close();
        }

        private void InternalLog( string _message )
        {
            _message = DateTime.Now + ": " + _message;
            this.stream.WriteLine( _message );
        }

        public static void Log( string _message )
        {
            MessageLog.instance.InternalLog( _message );
        }
    }
}
