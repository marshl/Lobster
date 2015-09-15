using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace Lobster
{
    /// <summary>
    /// 'Èxactly! And who are they to be? That seems to me what this Council
    /// has to decide, and all that it has to decide.'
    ///     - Bilbo Baggins
    /// 
    /// [ _The Lord of the Rings_, II/ii: "The Council of Elrond"]
    /// </summary>
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
