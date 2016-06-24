//-----------------------------------------------------------------------
// <copyright file="TableRequestEventArgs.cs" company="marshl">
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
namespace LobsterModel
{
    using System;

    /// <summary>
    /// The event arguments for when the user needs to supply a table to insert into,
    /// </summary>
    public class TableRequestEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TableRequestEventArgs"/> class.
        /// </summary>
        /// <param name="fullpath">The path of the file that needs a table specified.</param>
        /// <param name="tables">The tables that the user can select from.</param>
        public TableRequestEventArgs(string fullpath, Table[] tables)
        {
            this.FullPath = fullpath;
            this.Tables = tables;
        }

        /// <summary>
        /// Gets the path of the file that the needs a tabl specified.
        /// </summary>
        public string FullPath { get; }

        /// <summary>
        /// Gets the tables that the user can selecte from.
        /// </summary>
        public Table[] Tables { get; }

        /// <summary>
        /// Gets or sets the table that the user selected.
        /// </summary>
        public Table SelectedTable { get; set; } = null;
    }
}
