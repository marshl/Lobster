//-----------------------------------------------------------------------
// <copyright file="DatatypePicker.cs" company="marshl">
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
//      "Who are you and what do you want?" he asked gruffly, standing in front of them and towering
//      tall above Gandalf.
//          --Beorn
//
//      [ _The Hobbit_, VII: "Queer Lodgings"]
//
//-----------------------------------------------------------------------
namespace Lobster
{
    using System;
    using System.Diagnostics;
    using System.Windows.Forms;

    /// <summary>
    /// This form is used to select the mime type that the new file will use when inserted into the database.
    /// </summary>
    public partial class DatatypePicker : Form
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatatypePicker"/> class.
        /// </summary>
        /// <param name="table">The table the data types will be chosen from.</param>
        public DatatypePicker(Table table)
        {
            this.InitializeComponent();
            foreach (Column column in table.Columns)
            {
                this.datatypeComboBox.Items.AddRange(column.MimeTypeList.ToArray());
            }

            Debug.Assert(this.datatypeComboBox.Items.Count > 0, "The table must have at least one mime type.");
            this.datatypeComboBox.SelectedIndex = 0;
        }

        /// <summary>
        /// The event handler for when the form accept button is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void datatypeAccept_Click_1(object sender, EventArgs e)
        {
            if (datatypeComboBox.Text != string.Empty)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }
}
