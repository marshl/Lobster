//-----------------------------------------------------------------------
// <copyright file="EditDatabaseConnection.cs" company="marshl">
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
//      'So!' said the Messenger. 'Then thou art the spokesman, old greybeard?'
// 
//      [ _The Lord of the Rings_, V/x: "The Black Gate Opens"]
//
//-----------------------------------------------------------------------
namespace Lobster
{
    using System;
    using System.IO;
    using System.Windows.Forms;
    using Properties;

    /// <summary>
    /// A form used to edit an old or new DatabaseConnection and its underlying ClobTypes.
    /// </summary>
    public class EditDatabaseConnection : EditCompositeObjectForm<DatabaseConfig>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EditDatabaseConnection"/> class.
        /// </summary>
        /// <param name="originalConnection">The connection to edit.</param>
        /// <param name="isNewConnection">Whether the DatabaseConnection is new or old.</param>
        public EditDatabaseConnection(DatabaseConfig originalConnection, bool isNewConnection)
            : base(originalConnection, isNewConnection)
        {
        }

        /// <summary>
        /// Override for the base InitializeComponent method, with changes specific for editing DatabaseConnections.
        /// </summary>
        protected override void InitializeComponent()
        {
            base.InitializeComponent();
            this.Text = "Edit Database Connection";
            this.editObjectTabPage.Text = "Database Connection";
            this.subItemTabPage.Text = "Clob Types";

            this.applyButton.Enabled = !this.IsNewObject;
        }

        /// <summary>
        /// Override for the bas PopulateSubItemList. Populates the sub item list with ClobTypes 
        /// from the working DatabaseConnection
        /// </summary>
        protected override void PopulateSubItemList()
        {
            this.subItemListView.Clear();

            DirectoryInfo dirInfo = new DirectoryInfo(this.WorkingObject.ClobTypeDir);

            if (!dirInfo.Exists)
            {
                return;
            }

            foreach ( FileInfo file in dirInfo.GetFiles() )
            {
                this.subItemListView.Items.Add(file.Name);
            }
        }

        /// <summary>
        /// Validates that the changes made to the ClobType are valid.
        /// </summary>
        /// <returns>True if the ClobType is valid.</returns>
        protected override bool ValidateChanges()
        {
            if (this.WorkingObject.CodeSource == null
             || this.WorkingObject.ClobTypeDir == null
             || this.WorkingObject.Host == null
             || this.WorkingObject.Name == null
             || this.WorkingObject.Password == null
             || this.WorkingObject.Port == null
             || this.WorkingObject.SID == null
             || this.WorkingObject.Username == null)
            {
                MessageBox.Show("All fields must be completed before saving.", "Validation Errors");
                return false;
            }

            if (this.IsNewObject)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.InitialDirectory = Path.Combine(Directory.GetCurrentDirectory(), Settings.Default.ConnectionDir);
                sfd.AddExtension = true;
                sfd.Filter = "eXtensible Markup Language File (*.xml)|*.xml";
                sfd.FileName = "NewConnection.xml";
                DialogResult result = sfd.ShowDialog();
                if (result != DialogResult.OK)
                {
                    return false;
                }

                //this.WorkingObject.FileLocation = sfd.FileName;
            }

            return true;
        }

        /// <summary>
        /// Saves changes to the ClobType out to file.
        /// </summary>
        /// <returns>True if the changes were succcessfully applied, otherwise false.</returns>
        protected override bool ApplyChanges()
        {
            try
            {
                DatabaseConfig.SerialiseToFile(this.WorkingObject.FileLocation, this.WorkingObject);
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Cannot save DatabaseConnection. " + this.WorkingObject.FileLocation + " is locked.");
                return false;
            }

            this.OriginalObject = this.WorkingObject;
            this.WorkingObject = (DatabaseConfig)this.OriginalObject.Clone();

            this.IsNewObject = false;
            return true;
        }

        /// <summary>
        /// The override handler for the add sub item button click event.
        /// OPens an instance of the EditClobType form with a new ClobType as the context.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        protected override void addSubItemButton_click(object sender, EventArgs e)
        {
            ClobType clobType = new ClobType();
            EditClobType editForm = new EditClobType(clobType, true);
            DialogResult result = editForm.ShowDialog();

            if (result == DialogResult.OK)
            {
                //this.WorkingObject.ClobTypeList.Add(editForm.OriginalObject);
                this.PopulateSubItemList();
            }
        }

        /// <summary>
        /// The override event handler for the remove sub item button click.
        /// Removes the selected ClobType and refreshes the list.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        protected override void removeSubItemButton_Click(object sender, EventArgs e)
        {
            if (this.subItemListView.SelectedItems.Count == 0)
            {
                MessageBox.Show("Select a Database Connection first.");
                return;
            }

            int clobTypeIndex = (int)this.subItemListView.SelectedItems[0].Tag;
            //ClobType clobType = this.WorkingObject.ClobTypeList[clobTypeIndex];
            //this.WorkingObject.ClobTypeList.Remove(clobType);
            this.PopulateSubItemList();
        }

        /// <summary>
        /// The override for the edit sub item button event handler.
        /// Creates a EditClobType form with the selected CLobType as the context.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        protected override void editSubItemButton_Click(object sender, EventArgs e)
        {
            if (this.subItemListView.SelectedItems.Count == 0)
            {
                MessageBox.Show("Select a Database Connection first.");
                return;
            }

            int clobTypeIndex = (int)this.subItemListView.SelectedItems[0].Tag;
            //ClobType clobType = this.WorkingObject.ClobTypeList[clobTypeIndex];

            //EditClobType editForm = new EditClobType(clobType, false);
            //DialogResult result = editForm.ShowDialog();
            //this.WorkingObject.ClobTypeList[clobTypeIndex] = editForm.OriginalObject;
            this.PopulateSubItemList();
        }
    }
}
