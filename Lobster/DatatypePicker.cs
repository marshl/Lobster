using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lobster
{
    public partial class DatatypePicker : Form
    {
        public DatatypePicker( ClobType.Column _column )
        {
            InitializeComponent();
            Debug.Assert( _column.mimeTypes.Count > 0 );
            foreach ( string str in _column.mimeTypes )
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
