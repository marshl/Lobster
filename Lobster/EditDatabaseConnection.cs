using System.Windows.Forms;

namespace Lobster
{
    public partial class EditDatabaseConnection : Form
    {
        public DatabaseConfig originalConfig;
        public DatabaseConfig workingConfig;

        public EditDatabaseConnection()
        {
            InitializeComponent();
        }

        public EditDatabaseConnection( ref DatabaseConfig _orignalConfig )
        {
            this.originalConfig = _orignalConfig;
            this.workingConfig = new DatabaseConfig( this.originalConfig );
            InitializeComponent();

            this.databaseConnectionPropertyGrid.SelectedObject = this.workingConfig;
        }

        private void ApplyChanges()
        {
            this.originalConfig = this.workingConfig;
            this.workingConfig = new DatabaseConfig( this.originalConfig );
            DatabaseConfig.Serialise( this.originalConfig.fileLocation, this.originalConfig );
        }

        private void okButton_Click( object sender, System.EventArgs e )
        {
            this.ApplyChanges();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void applyButton_Click( object sender, System.EventArgs e )
        {
            this.ApplyChanges();
        }

        private void cancelButton_Click( object sender, System.EventArgs e )
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
