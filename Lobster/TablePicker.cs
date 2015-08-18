using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Lobster
{
    public partial class TablePicker : Form
    {
        public TablePicker( ClobType _clobType )
        {
            InitializeComponent();
            Debug.Assert( _clobType.tables.Count > 0 );
            foreach ( ClobType.Table table in _clobType.tables )
            {
                this.tableCombo.Items.Add( table );
            }

            this.tableCombo.SelectedIndex = 0;
        }

        private void button1_Click( object sender, EventArgs e )
        {
            if ( this.tableCombo.SelectedIndex != -1 )
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }
}
