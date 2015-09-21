//-----------------------------------------------------------------------
// <copyright file="filename.cs" company="marshl">
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
//      'There is a change in him, but just what
//      kind of a change and how deep, I'm not sure yet.' 
//          -- Frodo
// 
//      [ _The Lord of the Rings_, IV/ii: "The Passage of the Marshes"]
//
// </copyright>
//-----------------------------------------------------------------------

namespace Lobster
{
    using System;
    using System.Windows.Forms;
    /// <summary>
    /// 
    /// </summary>
    public abstract partial class EditCompositeObjectForm<T> : Form where T : ICloneable
    {
        public T originalObject;
        public T workingObject;

        protected bool isNewObject;

        public EditCompositeObjectForm( T _original, bool _new )
        {
            this.originalObject = _original;
            this.isNewObject = _new;

            this.workingObject = (T)this.originalObject.Clone();
            this.InitializeComponent();
            this.editObjectPropertyGrid.SelectedObject = this.workingObject;

            this.PopulateSubItemList();
        }

        protected abstract void PopulateSubItemList();

        protected abstract bool ValidateChanges();

        protected abstract bool ApplyChanges();

        private void okButton_Click( object sender, System.EventArgs e )
        {
            if ( !this.ValidateChanges() )
            {
                return;
            }
            if ( !this.ApplyChanges() )
            {
                return;
            }
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void applyButton_Click( object sender, System.EventArgs e )
        {
            if ( !this.ValidateChanges() )
            {
                return;
            }
            this.ApplyChanges();
        }

        private void cancelButton_Click( object sender, System.EventArgs e )
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        protected abstract void addSubItemButton_click( object sender, System.EventArgs e );

        protected abstract void removeSubItemButton_Click( object sender, System.EventArgs e );

        protected abstract void editSubItemButton_Click( object sender, System.EventArgs e );
    }
}
