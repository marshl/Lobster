using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lobster
{
    public partial class DatatypePicker : Form
    {
        public DatatypePicker( ClobType _clobType )
        {
            InitializeComponent();
            foreach ( string str in _clobType.dataTypes )
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
