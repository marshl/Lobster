//-----------------------------------------------------------------------
// <copyright file="MessageLog.cs" company="marshl">
// Copyright 2015, Liam Marshall, marshl.
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
    public class MessageLog : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageLog"/> class.
        /// </summary>
        public MessageLog()
        {
            MessageLog.Instance = this;

            this.MessageList = new List<Message>();

            this.OutStream = new StreamWriter(Settings.Default.LogFilename, true);
            this.OutStream.WriteLine();

            MessageLog.LogInfo("Starting Lobster (build " + Utils.RetrieveLinkerTimestamp() + ")");
        }

        /// <summary>
        /// All messages that have been created by this program since startup.
        /// </summary>
        public List<Message> MessageList { get; private set; }

        /// <summary>
        /// The instance of this logger.
        /// </summary>
        private static MessageLog Instance { get; set; }

        /// <summary>
        /// The file stream this logger writes to.
        /// </summary>
        private StreamWriter OutStream { get; set; }

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
        /// Closes his message log
        /// </summary>
        public void Close()
        {
            MessageLog.LogInfo("Lobster Stopped");
            this.OutStream.Close();
            MessageLog.Instance = null;
        }

        /// <summary>
        /// The dispose override.
        /// </summary>
        public void Dispose()
        {
            this.OutStream.Close();
        }

        /// <summary>
        /// Internal method to log a message of the given type.
        /// </summary>
        /// <param name="messageType">The type of message to create.</param>
        /// <param name="message">The text of the message.</param>
        private void InternalLog(Message.TYPE messageType, string message)
        {
            Message msg = new Message(message, messageType, DateTime.Now);
            this.MessageList.Add(msg);
            lock(this.OutStream)
            {
                this.OutStream.WriteLine(msg.ToString());
                //this.OutStream.Flush();
            }
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
            }

            /// <summary>
            /// The text of this message.
            /// </summary>
            public string Text { get; private set; }

            /// <summary>
            /// The date that this message was created.
            /// </summary>
            public DateTime DateCreated { get; private set; }

            /// <summary>
            /// The type of this message.
            /// </summary>
            public TYPE MessageType { get; private set; }

            /// <summary>
            /// Override forthe ToString() method, returning the message formatted as {Date} [{Type}]: {Text}
            /// </summary>
            /// <returns>The string representation of this <see cref="Message"/>.</returns>
            public override string ToString()
            {
                return this.DateCreated.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture) + " [" + this.MessageType + "]: " + this.Text;
            }
        }
    }
}
