
//-----------------------------------------------------------------------
// <copyright file="filename.cs" company="marshl">
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

namespace LobsterWpf
{
    using System;
    using System.Windows.Media;
    using LobsterModel;

    public class MessageView
    {
        private MessageLog.Message baseMessage;

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
