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
//
//      "Who are you and what do you want?" he asked gruffly, standing in front of them and towering
//      tall above Gandalf.
//          --Beorn
//      [ _The Hobbit_, VII: "Queer Lodgings"]
//
// </copyright>
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
        /// The default constructor for <see cref="DatatypePicker"/>.
        /// </summary>
        /// <param name="column">The mime type column that definees the mime types that can be chosen from.</param>
        public DatatypePicker(Column column)
        {
            this.InitializeComponent();
            Debug.Assert(column.MimeTypeList.Count > 0);
            foreach (string str in column.MimeTypeList)
            {
                this.datatypeComboBox.Items.Add(str);
            }
            this.datatypeComboBox.SelectedIndex = 0;
        }

        /// <summary>
        /// The event handler for when the form accept button is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void datatypeAccept_Click_1(object sender, EventArgs e)
        {
            if (datatypeComboBox.Text != "")
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }
}
