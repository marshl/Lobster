﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace Lobster
{
    public class MessageLog : IDisposable
    {
        public class Message
        {
            public string text;
            public DateTime date;
            public TYPE type;
            public enum TYPE
            {
                INFO,
                WARNING,
                ERROR,
            }

            public override string ToString()
            {
                return this.date + " [" + this.type + "]: " + this.text;
            }
        }
        public List<Message> messageList;

        private static MessageLog instance;
        public TextBox textBox;

        private StreamWriter stream;

        public MessageLog()
        {
            this.messageList = new List<Message>();
            MessageLog.instance = this;
            this.stream = new StreamWriter( Program.LOG_FILE, true );
            this.stream.Write( "\n\n" );
            LogInfo( "Starting Lobster (build " + Common.RetrieveLinkerTimestamp()  + ")" );
        }

        public void Close()
        {
            LogInfo( "Lobster Stopped" );
            this.stream.Close();
        }

        private void InternalLog( Message.TYPE _type, string _message )
        {
            Message msg = new Message() { text = _message, type = _type, date = DateTime.Now };
            this.messageList.Add( msg );
            this.stream.WriteLine( msg.ToString() );
            this.stream.Flush();
        }

        public static void LogWarning( string _message )
        {
            MessageLog.instance.InternalLog( Message.TYPE.WARNING, _message );
        }

        public static void LogError( string _message )
        {
            MessageLog.instance.InternalLog( Message.TYPE.ERROR, _message );
        }

        public static void LogInfo( string _message )
        {
            MessageLog.instance.InternalLog( Message.TYPE.INFO, _message );
        }

        public void Dispose()
        {
            this.stream.Close();
        }
    }
}
