//-----------------------------------------------------------------------
// <copyright file="TablePicker.cs" company="marshl">
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
//      'Èxactly! And who are they to be? That seems to me what this Council
//      has to decide, and all that it has to decide.'
//          - Bilbo Baggins
// 
//      [ _The Lord of the Rings_, II/ii: "The Council of Elrond"]
//
//-----------------------------------------------------------------------
namespace Lobster
{
    using System;
    using System.Diagnostics;
    using System.Windows.Forms;

    /// <summary>
    /// Used to pick the table that a new file should be inserted into.
    /// </summary>
    public partial class TablePicker : Form
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TablePicker"/> class.
        /// </summary>
        /// <param name="clobType">The ClobType this picker is for.</param>
        public TablePicker(ClobType clobType)
        {
            this.InitializeComponent();
            Debug.Assert(clobType.Tables.Count > 0, "A TablePicker cannot be created for a ClobType that has no tables.");
            foreach (Table table in clobType.Tables)
            {
                this.tableCombo.Items.Add(table);
            }

            this.tableCombo.SelectedIndex = 0;
        }

        /// <summary>
        /// Event for when the accept button is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void tablePickerAccept_Click(object sender, EventArgs e)
        {
            if (this.tableCombo.SelectedIndex != -1)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }
}
