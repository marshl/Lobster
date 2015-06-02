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
    public partial class LobsterMain : Form
    {
        LobsterModel lobsterModel;

        public List<DataGridView> dataGridList;
        public LobsterMain( LobsterModel _lobsterModel )
        {
            this.lobsterModel = _lobsterModel;
            InitializeComponent();

            this.dataGridList = new List<DataGridView>();
            foreach ( ClobType clobType in this.lobsterModel.clobTypes )
            {
                DataGridView dataGridView = this.CreateClobTab( clobType );
                this.dataGridList.Add( dataGridView );
            }
        }

        private DataGridView CreateClobTab( ClobType _clobType )
        {
            TabPage clobTab = new TabPage();
            clobTab.Name = _clobType.directory;
            clobTab.Padding = new Padding( 3 );
            clobTab.Size = new Size( 970, 655 );
            clobTab.Text = _clobType.directory;
            clobTab.UseVisualStyleBackColor = true;

            this.MainTabControl.Controls.Add( clobTab );

            DataGridViewTextBoxColumn fileNameColumn = new DataGridViewTextBoxColumn(); ;
            fileNameColumn.HeaderText = "File Name";
            fileNameColumn.Name = "FileName";
            fileNameColumn.ReadOnly = true;

            // LastModified
            DataGridViewTextBoxColumn lastModifiedColumn = new DataGridViewTextBoxColumn();
            lastModifiedColumn.HeaderText = "Last Modified";
            lastModifiedColumn.Name = "LastModified";
            lastModifiedColumn.ReadOnly = true;

            // LastClobbed
            DataGridViewTextBoxColumn lastClobbedColumn = new DataGridViewTextBoxColumn();
            lastClobbedColumn.HeaderText = "Last Clobbed";
            lastClobbedColumn.Name = "LastClobbed";
            lastClobbedColumn.ReadOnly = true;

            // Create
            DataGridViewDisableButtonColumn createColumn = new DataGridViewDisableButtonColumn();
            createColumn.HeaderText = "Create";
            createColumn.Name = "Create";
            createColumn.ReadOnly = true;

            // Reclob
            DataGridViewDisableButtonColumn reclobColumn = new DataGridViewDisableButtonColumn();
            reclobColumn.HeaderText = "Reclob";
            reclobColumn.Name = "Reclob";
            reclobColumn.ReadOnly = true;

            DataGridView dataGridView = new DataGridView();
            dataGridView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView.Columns.AddRange( new DataGridViewColumn[] {
                    fileNameColumn,
                    lastModifiedColumn,
                    lastClobbedColumn,
                    createColumn,
                    reclobColumn
                } );
            dataGridView.Location = new Point( 6, 6 );
            dataGridView.Name = "dataGridView";
            dataGridView.RowTemplate.Height = 28;
            dataGridView.Size = new Size( 958, 643 );
            dataGridView.AllowUserToAddRows = false;
            dataGridView.AllowUserToDeleteRows = false;
            clobTab.Controls.Add( dataGridView );

            dataGridView.CellContentClick += new DataGridViewCellEventHandler( this.dataGridView_CellContentClick );


            foreach ( KeyValuePair<string, ClobFile> pair in _clobType.fileMap )
            {
                ClobFile file = pair.Value;
                int index = dataGridView.Rows.Add(
                    file.filename,
                    file.lastModified,
                    file.lastModified,
                    "Create",
                    "Reclob" );
                DataGridViewRow row = dataGridView.Rows[index];
                DataGridViewDisableButtonCell createButton = (DataGridViewDisableButtonCell)row.Cells["Create"];
                createButton.Enabled = (file.status == ClobFile.STATUS.LOCAL_ONLY);

                DataGridViewDisableButtonCell reclobButton = (DataGridViewDisableButtonCell)row.Cells["Reclob"];
                reclobButton.Enabled = ( file.status != ClobFile.STATUS.LOCAL_ONLY );
            }
            dataGridView.Sort( dataGridView.Columns["FileName"], ListSortDirection.Ascending );
            _clobType.dataGridView = dataGridView;
            return dataGridView;
        }

        private void dataGridView_CellContentClick( object _sender, DataGridViewCellEventArgs _e )
        {
            var senderGrid = (DataGridView)_sender;

            if ( !( senderGrid.Columns[_e.ColumnIndex] is DataGridViewDisableButtonColumn )
              || _e.RowIndex < 0 )
            {
                return;
            }
            DataGridViewDisableButtonCell cell = senderGrid.Rows[_e.RowIndex].Cells[_e.ColumnIndex] as DataGridViewDisableButtonCell;
            if ( cell != null && !cell.Enabled )
            {
                return;
            }

            ClobType clobType = null;
            for ( int i = 0; i < this.dataGridList.Count; ++i )
            {
                if ( this.dataGridList[i] == senderGrid )
                {
                    clobType = this.lobsterModel.clobTypes[i];
                    break;
                }
            }

            switch ( senderGrid.Columns[_e.ColumnIndex].Name )
            {
                case "Create":
                    {
                        clobType.fileList[_e.RowIndex].InsertIntoDatabase();
                        break;
                    }
                case "Reclob":
                    {
                        clobType.fileList[_e.RowIndex].UpdateDatabase();
                        break;
                    }
                default:
                    {
                        throw new InvalidEnumArgumentException();
                    }
            }
        }
    }
}
