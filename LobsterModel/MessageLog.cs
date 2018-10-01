//-----------------------------------------------------------------------
// <copyright file="MessageLog.cs" company="marshl">
// Copyright 2016, Liam Marshall, marshl.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//-----------------------------------------------------------------------
//
//      'Mercy!' cried Gandalf. 'If the giving of information is to be the cure
//          of your inquisitiveness, I shall spend all the rest of my days in answering you.
//          What more do you want to know?'
//      'The names of all the stars, and of all living things, and the whole
//          history of Middle-earth and Over-heaven and of the Sundering Seas ' laughed Pippin
// 
//      [ _The Lord of the Rings_, III/xi: "The Palantir"] 
//
//-----------------------------------------------------------------------
namespace LobsterModel
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using Properties;

    /// <summary>
    /// Used to log detailed information about the 
    /// </summary>
    public sealed class MessageLog : IDisposable
    {
        /// <summary>
        /// The file stream this logger writes to.
        /// </summary>
        private FileStream outStream;

        /// <summary>
        /// The writer of the stream the messages are written to.
        /// </summary>
        private StreamWriter streamWriter;

        /// <summary>
        /// The object to lock the filestream on.
        /// </summary>
        private object fileLock;

        /// <summary>
        /// Prevents a default instance of the <see cref="MessageLog"/> class from being created.
        /// </summary>
        private MessageLog()
        {
            MessageLog.Instance = this;

            this.MessageList = new List<Message>();
            this.outStream = new FileStream(Settings.Default.LogFilename, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            this.streamWriter = new StreamWriter(this.outStream);
            this.streamWriter.WriteLine();
            this.fileLock = new object();

            MessageLog.LogInfo($"Starting Lobster (build {Utils.RetrieveLinkerTimestamp()})");
        }

        /// <summary>
        /// Gets the instance of this logger.
        /// </summary>
        public static MessageLog Instance { get; private set; }

        /// <summary>
        /// Gets the messages that have been created by this program since startup.
        /// </summary>
        public List<Message> MessageList { get; private set; }

        /// <summary>
        /// Gets or sets the listener of events this log raises (such as when a new message is received).
        /// </summary>
        public IMessageLogEventListener EventListener { get; set; }

        /// <summary>
        /// Initalizes a new instance of the <see cref="MessageLog"/> class.
        /// </summary>
        /// <returns>The old instance of the log if it exists. Otherwise the new instance.</returns>
        public static MessageLog Initialise()
        {
            if (Instance == null)
            {
                Instance = new MessageLog();
            }

            return Instance;
        }

        /// <summary>
        /// Flushes the stream buffer.
        /// </summary>
        public static void Flush()
        {
            MessageLog.Instance.outStream.Flush();
        }

        /// <summary>
        /// Logs a warning type message.
        /// </summary>
        /// <param name="message">The text of the message to create.</param>
        public static void LogWarning(string message)
        {
            MessageLog.Instance.InternalLog(Message.TYPE.WARNING, message);
        }

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="message">The text of the message to create.</param>
        public static void LogError(string message)
        {
            MessageLog.Instance.InternalLog(Message.TYPE.ERROR, message);
        }

        /// <summary>
        /// Logs an information message.
        /// </summary>
        /// <param name="message">The text of the message to create.</param>
        public static void LogInfo(string message)
        {
            MessageLog.Instance.InternalLog(Message.TYPE.INFO, message);
        }

        /// <summary>
        /// Logs a sensitive message.
        /// </summary>
        /// <param name="message">The text of the message to create.</param>
        public static void LogSensitive(string message)
        {
            MessageLog.Instance.InternalLog(Message.TYPE.SENSITIVE, message);
        }

        /// <summary>
        /// Executes the log file
        /// </summary>
        public static void OpenLogFile()
        {
            System.Diagnostics.Process.Start(Settings.Default.LogFilename);
        }

        /// <summary>
        /// Closes his message log
        /// </summary>
        public static void Close()
        {
            MessageLog.LogInfo("Lobster Stopped");
            Instance.Dispose();
            Instance = null;
        }

        /// <summary>
        /// Disposes this object.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Internal method to log a message of the given type.
        /// </summary>
        /// <param name="messageType">The type of message to create.</param>
        /// <param name="message">The text of the message.</param>
        private void InternalLog(Message.TYPE messageType, string message)
        {
            Message msg = new Message(message, messageType, DateTime.Now);

            lock (this.fileLock)
            {
                this.MessageList.Add(msg);
                if (messageType != Message.TYPE.SENSITIVE || Settings.Default.LogSensitiveMessages)
                {
                    this.streamWriter.WriteLine(msg.ToString());
                    this.streamWriter.Flush();
                }
            }

            if (this.EventListener != null)
            {
                this.EventListener.OnNewMessage(msg);
            }
        }

        /// <summary>
        /// Optionally disposes this object.
        /// </summary>
        /// <param name="disposing">Whther this object should be disposed or not.</param>
        private void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            if (this.streamWriter != null)
            {
                this.streamWriter.Dispose();
                this.streamWriter = null;
            }

            if (this.outStream != null)
            {
                this.outStream.Dispose();
                this.outStream = null;
            }

            MessageLog.Instance = null;
        }

        /// <summary>
        /// A single message used by MessageLog.
        /// </summary>
        public class Message
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Message"/> class.
            /// </summary>
            /// <param name="text">The text of the message.</param>
            /// <param name="type">The message type.</param>
            /// <param name="dateCreated">The date the message was created.</param>
            public Message(string text, TYPE type, DateTime dateCreated)
            {
                this.Text = text;
                this.MessageType = type;
                this.DateCreated = dateCreated;
            }

            /// <summary>
            /// The possible types a message can be.
            /// </summary>
            public enum TYPE
            {
                /// <summary>
                /// Informational message type: Used to describe events during program flow.
                /// </summary>
                INFO,

                /// <summary>
                /// Warning message type: Used to describe unexepected problems that do not interrupt program flow.
                /// </summary>
                WARNING,

                /// <summary>
                /// Error message type: Used to describe unexpected problems that prevented the program from operating.
                /// </summary>
                ERROR,

                /// <summary>
                /// THe message type for messages that should not be included in a log file that is unsecurely transmitted.
                /// </summary>
                SENSITIVE,
            }

            /// <summary>
            /// Gets the text of this message.
            /// </summary>
            public string Text { get; }

            /// <summary>
            /// Gets the date that this message was created.
            /// </summary>
            public DateTime DateCreated { get; }

            /// <summary>
            /// Gets the type of this message.
            /// </summary>
            public TYPE MessageType { get; }

            /// <summary>
            /// Override forthe ToString() method, returning the message formatted as {Date} [{Type}]: {Text}
            /// </summary>
            /// <returns>The string representation of this <see cref="Message"/>.</returns>
            public override string ToString()
            {
                return $"{this.DateCreated:yyyy-MM-dd HH:mm:ss.fff} [{this.MessageType }]: {this.Text}";
            }
        }
    }
}
