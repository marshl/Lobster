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
        public LobsterMain()
        {
            InitializeComponent();

            MessageLog messageLog = new MessageLog();
            messageLog.textBox = this.logTextBox;

            this.lobsterModel = new LobsterModel();

            this.lobsterModel.LoadDatabaseConfig();
            
            this.lobsterModel.LoadClobTypes();

            if ( this.lobsterModel.OpenConnection() )
            {
                this.lobsterModel.CompareToDatabase();
            }


            this.dataGridList = new List<DataGridView>();
            foreach ( ClobDirectory clobDirectory in this.lobsterModel.clobDirectories )
            {
                DataGridView dataGridView = this.CreateClobTab( clobDirectory );
                this.dataGridList.Add( dataGridView );
            }
        }

        private DataGridView CreateClobTab( ClobDirectory _clobDirectory )
        {
            TabPage clobTab = new TabPage();
            clobTab.Name = _clobDirectory.clobType.directory;
            clobTab.Padding = new Padding( 3 );
            clobTab.Size = new Size( 970, 655 );
            clobTab.Text = _clobDirectory.clobType.directory;
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


            foreach ( KeyValuePair<string, ClobFile> pair in _clobDirectory.fileMap )
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
            _clobDirectory.dataGridView = dataGridView;
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

            ClobDirectory clobDirectory = null;
            for ( int i = 0; i < this.dataGridList.Count; ++i )
            {
                if ( this.dataGridList[i] == senderGrid )
                {
                    clobDirectory = this.lobsterModel.clobDirectories[i];
                    break;
                }
            }

            switch ( senderGrid.Columns[_e.ColumnIndex].Name )
            {
                case "Create":
                    {
                        clobDirectory.fileList[_e.RowIndex].InsertIntoDatabase();
                        break;
                    }
                case "Reclob":
                    {
                        clobDirectory.fileList[_e.RowIndex].UpdateDatabase();
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
