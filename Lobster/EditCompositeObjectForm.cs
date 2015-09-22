//-----------------------------------------------------------------------
// <copyright file="EditCompositeObjectForm.cs" company="marshl">
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
//      'There is a change in him, but just what
//      kind of a change and how deep, I'm not sure yet.' 
//          -- Frodo
// 
//      [ _The Lord of the Rings_, IV/ii: "The Passage of the Marshes"]
//
//-----------------------------------------------------------------------
namespace Lobster
{
    using System;
    using System.Windows.Forms;

    /// <summary>
    /// A generic form to allow the editing of an object that contains
    /// a number of child objects that need to be edited and saved separately
    /// </summary>
    /// <typeparam name="T">The type of the object to be edited.</typeparam>
    public abstract partial class EditCompositeObjectForm<T> : Form where T : ICloneable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EditCompositeObjectForm{T}" /> class.
        /// </summary>
        /// <param name="original">The original object, which will be copied and used as the working object.</param>
        /// <param name="isNewObject">Whether this form should handle the object as a newly created object (that needs 
        /// a save location) or a currently existing one (which will save changes over its old file).</param>
        public EditCompositeObjectForm(T original, bool isNewObject)
        {
            this.OriginalObject = original;
            this.IsNewObject = isNewObject;

            this.WorkingObject = (T)this.OriginalObject.Clone();
            this.InitializeComponent();
            this.editObjectPropertyGrid.SelectedObject = this.WorkingObject;

            this.PopulateSubItemList();
        }

        /// <summary>
        /// The original object without any edits
        /// </summary>
        public T OriginalObject { get; set; }

        /// <summary>
        /// The object that changes are made to before being applied to the original object.
        /// </summary>
        public T WorkingObject { get; set; }

        /// <summary>
        /// A flag that stores whether this is a newly created object, or an old one that is being edited.
        /// </summary>
        protected bool IsNewObject { get; set; }

        /// <summary>
        /// Populates the sub item list.
        /// </summary>
        protected abstract void PopulateSubItemList();

        /// <summary>
        /// Validates that the changes to the working object can be saved.
        /// </summary>
        /// <returns>Whether the contents of the form validated successfully or not.</returns>
        protected abstract bool ValidateChanges();

        /// <summary>
        /// Saves changes to the working object over the original object and updates the file it was loaded from.
        /// </summary>
        /// <returns>Whether the changes were applied successfully or not.</returns>
        protected abstract bool ApplyChanges();

        /// <summary>
        /// The addSubItem button click event handler.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The arguments of the event.</param>
        protected abstract void addSubItemButton_click(object sender, EventArgs e);

        /// <summary>
        /// The removeSubItem button click event handler.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The arguments of the event.</param>
        protected abstract void removeSubItemButton_Click(object sender, EventArgs e);

        /// <summary>
        /// The editSubItem button click event handler.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The arguments of the event.</param>
        protected abstract void editSubItemButton_Click(object sender, EventArgs e);

        /// <summary>
        /// The event handler for when the ok button is pressed.
        /// Validates changes to the object, then saves them out.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments.</param>
        private void okButton_Click(object sender, EventArgs e)
        {
            if (!this.ValidateChanges())
            {
                return;
            }

            if (!this.ApplyChanges())
            {
                return;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        /// <summary>
        /// The event handler for when the apply button is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void applyButton_Click(object sender, System.EventArgs e)
        {
            if (!this.ValidateChanges())
            {
                return;
            }

            this.ApplyChanges();
        }

        /// <summary>
        /// The event handler for pressing the cancel button.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
