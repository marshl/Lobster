using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace Lobster
{
    /// <summary>
    /// "Who are you and what do you want?" he asked gruffly, standing in front of them and towering
    /// tall above Gandalf.
    /// </summary>
    public partial class DatatypePicker : Form
    {
        public DatatypePicker( Column _column )
        {
            InitializeComponent();
            Debug.Assert( _column.MimeTypeList.Count > 0 );
            foreach ( string str in _column.MimeTypeList )
            {
                this.datatypeComboBox.Items.Add( str );
            }
            this.datatypeComboBox.SelectedIndex = 0;
        }

        private void datatypeAccept_Click_1( object sender, EventArgs e )
        {
            if ( datatypeComboBox.Text != "" )
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }
}
