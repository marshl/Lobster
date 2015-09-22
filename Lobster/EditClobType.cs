//-----------------------------------------------------------------------
// <copyright file="EditClobType.cs" company="marshl">
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
//      'Now things begin to look more hopeful. This news alters them much for-the
//      better. So far we have had no clear idea what to do.'
//          -- Thorin
//
//      [ _The Hobbits_, i: "An Unexpected Party"]
//
//-----------------------------------------------------------------------
namespace Lobster
{
    using System;
    using System.IO;
    using System.Windows.Forms;

    /// <summary>
    /// The form for editting a ClobType and its underlying tables.
    /// </summary>
    public class EditClobType : EditCompositeObjectForm<ClobType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EditClobType"/> class.
        /// </summary>
        /// <param name="original">The ClobType that is being editted</param>
        /// <param name="isNewClobType">Whether the original is a new ClobType or edits to an old one.</param>
        public EditClobType(ClobType original, bool isNewClobType) : base(original, isNewClobType)
        {
        }

        /// <summary>
        /// Overrides the default InitializeComponent by modifying the form to suit the editting of Clob Types.
        /// </summary>
        protected override void InitializeComponent()
        {
            base.InitializeComponent();

            this.Text = "Edit Clob Type";
            this.editObjectTabPage.Text = "ClobType";
            this.subItemTabPage.Text = "Clob Types";
            this.editObjectTabControl.TabPages.Remove(this.subItemTabPage);
            this.applyButton.Enabled = !this.IsNewObject;
        }

        /// <summary>
        /// Overrides the base <see cref="EditCompositeObjectForm"/>. Does not put anything in the list as the edting of ClobType Tables is handled by .Net.
        /// </summary>
        protected override void PopulateSubItemList()
        {
            this.subItemListView.Clear();
        }

        /// <summary>
        /// Validates that the form has been completed correctly.
        /// </summary>
        /// <returns>True if the form content was validated successfully, otherwise false.</returns>
        protected override bool ValidateChanges()
        {
            if (this.IsNewObject)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                if (this.WorkingObject.ParentConnection != null)
                {
                    sfd.InitialDirectory = this.WorkingObject.ParentConnection.ClobTypeDir;
                }

                sfd.AddExtension = true;
                sfd.Filter = "eXtensible Markup Language File (*.xml)|*.xml";
                sfd.FileName = "NewClobType.xml";
                DialogResult result = sfd.ShowDialog();
                if (result != DialogResult.OK)
                {
                    return false;
                }

                this.WorkingObject.File = new FileInfo(sfd.FileName);
            }

            return true;
        }

        /// <summary>
        /// Saves changes made to the ClobType out to file.
        /// </summary>
        /// <returns>Whether or not the ClobType was saved correctly.</returns>
        protected override bool ApplyChanges()
        {
            try
            {
                ClobType.Serialise(this.WorkingObject.File.FullName, this.WorkingObject);
                MessageLog.LogInfo("ClobType " + this.WorkingObject.Name + " was saved to " + this.WorkingObject.File.FullName);
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Cannot save ClobType. " + this.WorkingObject.File + " is currently in use by another program.");
                return false;
            }

            this.OriginalObject = this.WorkingObject;
            this.WorkingObject = (ClobType)this.OriginalObject.Clone();
            this.IsNewObject = false;

            return true;
        }

        /// <summary>
        /// Override for the addSubItem button click event handler.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The arguments of the event.</param>
        protected override void addSubItemButton_click(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Override for the removeSubItem button click event handler.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The arguments of the event.</param>
        protected override void removeSubItemButton_Click(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Override for the editSubItem button click event handler.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The arguments of the event.</param>
        protected override void editSubItemButton_Click(object sender, EventArgs e)
        {
        }
    }
}
