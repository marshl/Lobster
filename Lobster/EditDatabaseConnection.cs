using System;
using System.IO;
using System.Windows.Forms;

namespace Lobster
{

    /// <summary>
    /// 
    /// 'So!' said the Messenger. 'Then thou art the spokesman, old greybeard?'
    /// [ _The Lord of the Rings_, V/x: "The Black Gate Opens"]
    /// 
    /// </summary>
    public class EditDatabaseConnection : EditCompositeObjectForm<DatabaseConnection>
    {
        public EditDatabaseConnection( DatabaseConnection _original, bool _newConnection ) : base( _original, _newConnection )
        {

        }

        protected override void InitializeComponent()
        {
            base.InitializeComponent();
            this.editObjectTabPage.Text = "Database Connection";
            this.subItemTabPage.Text = "Clob Types";

            this.applyButton.Enabled = !this.isNewObject;
        }

        protected override void PopulateSubItemList()
        {
            this.subItemListView.Clear();

            for ( int i = 0; i < this.workingObject.clobTypeList.Count; ++i )
            {
                ClobType clobType = this.workingObject.clobTypeList[i];
                ListViewItem item = new ListViewItem( clobType.name );

                this.subItemListView.Items.Add( item );
                item.Tag = i;
            }
        }

        protected override bool ValidateChanges()
        {
            if ( this.workingObject.codeSource == null
                || this.workingObject.clobTypeDir == null
                || this.workingObject.host == null
                || this.workingObject.name == null
                || this.workingObject.password == null
                || this.workingObject.port == null
                || this.workingObject.sid == null
                || this.workingObject.username == null )
            {
                MessageBox.Show( "All fields must be completed before saving.", "Validation Errors" );
                return false;
            }

            if ( this.isNewObject )
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.InitialDirectory = Path.Combine( Directory.GetCurrentDirectory(), Program.DB_CONFIG_DIR );
                sfd.AddExtension = true;
                sfd.Filter = "eXtensible Markup Language File (*.xml)|*.xml";
                sfd.FileName = "NewConnection.xml";
                DialogResult result = sfd.ShowDialog();
                if ( result != DialogResult.OK )
                {
                    return false;
                }
                this.workingObject.fileLocation = sfd.FileName;
            }
            return true;
        }

        protected override bool ApplyChanges()
        {
            try
            {
                DatabaseConnection.Serialise( this.originalObject.fileLocation, this.originalObject );
            }
            catch ( UnauthorizedAccessException )
            {
                MessageBox.Show( "Cannot save DatabaseConnection. " + this.originalObject.fileLocation + " is locked." );
                return false;
            }

            this.originalObject = this.workingObject;
            this.workingObject = (DatabaseConnection)this.originalObject.Clone();
            
            this.isNewObject = false;
            return true;
        }

        protected override void addSubItemButton_click( object sender, EventArgs e )
        {
            ClobType clobType = new ClobType();
            clobType.fileLocation = this.workingObject.clobTypeDir;
            EditClobType editForm = new EditClobType( clobType, true );
            DialogResult result = editForm.ShowDialog();
           
            if ( result == DialogResult.OK )
            {
                this.workingObject.clobTypeList.Add( editForm.originalObject );
                this.PopulateSubItemList();
            }
        }

        protected override void removeSubItemButton_Click( object sender, EventArgs e )
        {
            if ( this.subItemListView.SelectedItems.Count == 0 )
            {
                MessageBox.Show( "Select a Database Connection first." );
                return;
            }
            int clobTypeIndex = (int)this.subItemListView.SelectedItems[0].Tag;
            ClobType clobType = this.workingObject.clobTypeList[clobTypeIndex];
            this.workingObject.clobTypeList.Remove( clobType );
            this.PopulateSubItemList();
        }

        protected override void editSubItemButton_Click( object sender, EventArgs e )
        {
            if ( this.subItemListView.SelectedItems.Count == 0 )
            {
                MessageBox.Show( "Select a Database Connection first." );
                return;
            }
            int clobTypeIndex = (int)this.subItemListView.SelectedItems[0].Tag;
            ClobType clobType = this.workingObject.clobTypeList[clobTypeIndex];

            EditClobType editForm = new EditClobType( clobType, false );
            DialogResult ctResult = editForm.ShowDialog();
            this.workingObject.clobTypeList[clobTypeIndex] = editForm.originalObject;
            this.PopulateSubItemList();
        }
    }
}
