//-----------------------------------------------------------------------
// <copyright file="MessageLogView.cs" company="marshl">
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
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using LobsterModel;

    /// <summary>
    /// The view of the LobsterModel.MessageLog
    /// </summary>
    public class MessageLogView : IMessageLogEventListener, IDisposable, INotifyPropertyChanged
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageLogView"/> class.
        /// </summary>
        public MessageLogView()
        {
            MessageLog.Instance.EventListener = this;
        }

        /// <summary>
        /// The event to be raised when a property is changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets the list of message views of the underlying log.
        /// </summary>
        public List<MessageView> MessageList
        {
            get
            {
                return MessageLog.Instance.MessageList.Select(item => new MessageView(item)).ToList();
            }
        }

        /// <summary>
        /// The event that is raised when the underlying message log changes its state.
        /// </summary>
        /// <param name="msg">The message that caused the log to change state.</param>
        void IMessageLogEventListener.OnNewMessage(MessageLog.Message msg)
        {
            this.NotifyPropertyChanged("MessageList");
        }

        /// <summary>
        /// Overloads the dispose method.
        /// </summary>
        public void Dispose()
        {
            MessageLog.Instance.EventListener = null;
        }

        /// <summary>
        /// Implementation of the INotifyPropertyChange, to tell WPF when a data value has changed
        /// </summary>
        /// <param name="propertyName">The name of the property that has changed.</param>
        /// <remarks>This method is called by the Set accessor of each property.
        /// The CallerMemberName attribute that is applied to the optional propertyName
        /// parameter causes the property name of the caller to be substituted as an argument.</remarks>
        private void NotifyPropertyChanged(string propertyName = "")
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
