﻿//-----------------------------------------------------------------------
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
    using System.Diagnostics;
    using System.Windows;

    /// <summary>
    /// The window that displays messages logged by LobsterModel.MessageLog
    /// </summary>
    public partial class MessageListWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageListWindow"/> class.
        /// </summary>
        public MessageListWindow()
        {
            Debug.Assert(MessageListWindow.Instance == null, "There can only be one instance of the MessageListWindow class active at once.");
            MessageListWindow.Instance = this;
            this.InitializeComponent();

            this.LogView = new MessageLogView();
            this.DataContext = this.LogView;
        }

        /// <summary>
        /// Gets the instance of this window. There can only be one MessageListWindow active at a time.
        /// </summary>
        public static MessageListWindow Instance { get; private set; }

        /// <summary>
        /// Gets the data context MessageLogView of this window.
        /// </summary>
        public MessageLogView LogView { get; }

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