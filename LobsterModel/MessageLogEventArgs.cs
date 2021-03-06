﻿//-----------------------------------------------------------------------
// <copyright file="MessageLogEventArgs.cs" company="marshl">
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
//      "What message?" said poor Mr. Baggins all in a fluster.
//
//      [ _The Hobbit_, II: "Roast Mutton"]
//
//-----------------------------------------------------------------------
namespace LobsterModel
{
    using System;

    /// <summary>
    /// The interface for listeneing to events on the MessageLog
    /// </summary>
    public class MessageLogEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageLogEventArgs"/> class.
        /// </summary>
        /// <param name="msg">The message for this event</param>
        public MessageLogEventArgs(MessageLog.Message msg)
        {
            this.Message = msg;
        }

        /// <summary>
        /// Gets the message for this event
        /// </summary>
        public MessageLog.Message Message { get; }
    }
}
