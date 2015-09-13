using System;
using System.IO;
using System.Runtime.Serialization;
using System.Windows.Forms;

namespace Lobster
{
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
