using System;
using System.IO;
using System.Windows.Forms;

namespace Lobster
{
    /// <summary>
    /// 'Now things begin to look more hopeful. This news alters them much for-the
    ///  better. So far we have had no clear idea what to do.'
    ///     -- Thorin
    /// [ _The Hobbits_, i: "An Unexpected Party"]
    /// </summary>
    public class EditClobType : EditCompositeObjectForm<ClobType>
    {
        private DatabaseConnection parentConnection;

        public EditClobType( ClobType _original, bool _newClobType, DatabaseConnection parentConnection ) : base( _original, _newClobType )
        {
            this.parentConnection = parentConnection;
        }

        protected override void InitializeComponent()
        {
            base.InitializeComponent();
            this.editObjectTabPage.Text = "ClobType";
            this.subItemTabPage.Text = "Clob Types";
            this.applyButton.Enabled = !this.isNewObject;
        }

        protected override void PopulateSubItemList()
        {
            this.subItemListView.Clear();
        }

        protected override bool ValidateChanges()
        {
            if ( this.isNewObject )
            {
                SaveFileDialog sfd = new SaveFileDialog();
                if ( this.parentConnection != null )
                {
                    sfd.InitialDirectory = this.parentConnection.ClobTypeDir;
                }
                sfd.AddExtension = true;
                sfd.Filter = "eXtensible Markup Language File (*.xml)|*.xml";
                sfd.FileName = "NewClobType.xml";
                DialogResult result = sfd.ShowDialog();
                if ( result != DialogResult.OK )
                {
                    return false;
                }
                this.workingObject.File = new FileInfo( sfd.FileName );
            }
            return true;
        }

        protected override bool ApplyChanges()
        {
            try
            {
                ClobType.Serialise( this.workingObject.File.FullName, this.workingObject );
            }
            catch ( UnauthorizedAccessException )
            {
                MessageBox.Show( "Cannot save ClobType. " + this.workingObject.File + " is currently in use by another program." );
                return false;
            }
            this.originalObject = this.workingObject;
            this.workingObject = (ClobType)this.originalObject.Clone();
            this.isNewObject = false;

            return true;
        }

        protected override void addSubItemButton_click( object sender, EventArgs e )
        {

        }

        protected override void removeSubItemButton_Click( object sender, EventArgs e )
        {

        }

        protected override void editSubItemButton_Click( object sender, EventArgs e )
        {

        }
    }
}
