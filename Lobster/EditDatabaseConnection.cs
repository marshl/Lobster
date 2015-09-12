using System.IO;
using System.Windows.Forms;

namespace Lobster
{
    public partial class EditDatabaseConnection : Form
    {
        public DatabaseConnection originalConnection;
        public DatabaseConnection workingConnection;

        private bool isNewConnection;

        public EditDatabaseConnection()
        {
            InitializeComponent();
        }

        public EditDatabaseConnection( ref DatabaseConnection _orignalConfig, bool _newConnection )
        {
            this.originalConnection = _orignalConfig;
            this.isNewConnection = _newConnection;

            this.workingConnection = new DatabaseConnection( this.originalConnection );
            this.InitializeComponent();
            this.databaseConnectionPropertyGrid.SelectedObject = this.workingConnection.clobTypeList;
        }

        private bool ValidateChanges()
        {
            if ( this.workingConnection.codeSource == null
                || this.workingConnection.clobTypeDir == null
                || this.workingConnection.host == null
                || this.workingConnection.name == null
                || this.workingConnection.password == null
                || this.workingConnection.port == null
                || this.workingConnection.sid == null 
                || this.workingConnection.username == null )
            {
                MessageBox.Show( "All fields must be completed before saving.", "Validation Errors" );
                return false;
            }

            if ( this.isNewConnection )
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
                this.workingConnection.fileLocation = sfd.FileName;
            }
            return true;
        }

        private void ApplyChanges()
        {
            this.originalConnection = this.workingConnection;
            this.workingConnection = new DatabaseConnection( this.originalConnection );
            DatabaseConnection.Serialise( this.originalConnection.fileLocation, this.originalConnection );
            this.isNewConnection = false;
        }

        private void okButton_Click( object sender, System.EventArgs e )
        {
            if ( !this.ValidateChanges() )
            {
                return;
            }
            this.ApplyChanges();
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

        private void addClobTypeButton_Click( object sender, System.EventArgs e )
        {

        }

        private void removeClobTypeButton_Click( object sender, System.EventArgs e )
        {

        }

        private void editClobTypeButton_Click( object sender, System.EventArgs e )
        {

        }
    }
}
