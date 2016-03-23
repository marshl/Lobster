//-----------------------------------------------------------------------
// <copyright file="MessageView.cs" company="marshl">
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
//      'I am a herald and ambassador, and may not be assailed!' 
//      [The Messenger of Mordor] cried.
//
//      [ _The Lord of the Rings_, V/x: "The Black Gate Opens"]
//
//-----------------------------------------------------------------------
namespace LobsterWpf
{
    using System;
    using System.Windows.Media;
    using LobsterModel;

    /// <summary>
    /// The view of a particular MessageLog.Message.
    /// </summary>
    public class MessageView
    {
        /// <summary>
        /// The underlying MessageLog.Message for this view.
        /// </summary>
        private MessageLog.Message baseMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageView"/> class.
        /// </summary>
        /// <param name="message">The message to use as the basis for this view.</param>
        public MessageView(MessageLog.Message message)
        {
            this.baseMessage = message;
        }

        /// <summary>
        /// Gets the text of this message.
        /// </summary>
        public string Text
        {
            get
            {
                return this.baseMessage.Text;
            }
        }

        /// <summary>
        /// Gets the date that this message was created.
        /// </summary>
        public DateTime DateCreated
        {
            get
            {
                return this.baseMessage.DateCreated;
            }
        }

        /// <summary>
        /// Gets the ImageSource to use for the icon of this message, depending on the type of message.
        /// </summary>
        public ImageSource ImageUrl
        {
            get
            {
                string resourceName = null;
                switch (this.baseMessage.MessageType)
                {
                    case MessageLog.Message.TYPE.SENSITIVE:
                    case MessageLog.Message.TYPE.INFO:
                        resourceName = "InfoImageSource";
                        break;
                    case MessageLog.Message.TYPE.ERROR:
                        resourceName = "PriorityImageSource";
                        break;
                    case MessageLog.Message.TYPE.WARNING:
                        resourceName = "WarningImageSource";
                        break;
                }

                return (ImageSource)App.Current.FindResource(resourceName);
            }
        }

        /// <summary>
        /// Gets the type of this message.
        /// </summary>
        public MessageLog.Message.TYPE MessageType
        {
            get
            {
                return this.baseMessage.MessageType;
            }
        }
    }
}
