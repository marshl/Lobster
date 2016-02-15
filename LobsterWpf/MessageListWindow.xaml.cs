//-----------------------------------------------------------------------
// <copyright file="MessageListWindow.xaml.cs" company="marshl">
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
    using System.Windows;

    /// <summary>
    /// The window that displays messages logged by LobsterModel.MessageLog
    /// </summary>
    public partial class MessageListWindow : Window
    {
        public static MessageListWindow Instance { get; private set; }

        public MessageLogView LogView { get; set; }

        public MessageListWindow()
        {
            MessageListWindow.Instance = this;
            InitializeComponent();

            this.LogView = new MessageLogView();

            this.DataContext = this.LogView;
        }

        /// <summary>
        /// The event that is called when the window is closed.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void Window_Closed(object sender, EventArgs e)
        {
            MessageListWindow.Instance = null;
        }
    }
}
