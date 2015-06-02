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
                TabPage clobTab = new TabPage();
                clobTab.Name = clobType.directory;
                clobTab.Padding = new Padding( 3 );
                clobTab.Size = new Size( 970, 655 );
                clobTab.Text = clobType.directory;
                clobTab.UseVisualStyleBackColor = true;

                this.MainTabControl.Controls.Add( clobTab );

                DataGridViewTextBoxColumn fileNameColumn = new DataGridViewTextBoxColumn(); ;
                fileNameColumn.HeaderText = "File Name";
                fileNameColumn.Name = "FileName";
                fileNameColumn.ReadOnly = true;
                // 
                // LastModified
                // 
                DataGridViewTextBoxColumn lastModifiedColumn = new DataGridViewTextBoxColumn();
                lastModifiedColumn.HeaderText = "Last Modified";
                lastModifiedColumn.Name = "LastModified";
                lastModifiedColumn.ReadOnly = true;
                // 
                // LastClobbed
                // 
                DataGridViewTextBoxColumn lastClobbedColumn = new DataGridViewTextBoxColumn();
                lastClobbedColumn.HeaderText = "Last Clobbed";
                lastClobbedColumn.Name = "LastClobbed";
                lastClobbedColumn.ReadOnly = true;
                // 
                // Create
                // 
                DataGridViewButtonColumn createColumn = new DataGridViewButtonColumn();
                createColumn.HeaderText = "Create";
                createColumn.Name = "Create";
                createColumn.ReadOnly = true;
                // 
                // Reclob
                // 
                DataGridViewButtonColumn reclobColumn = new DataGridViewButtonColumn();
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


                foreach ( KeyValuePair<string, ClobFile> pair in clobType.fileMap )
                {
                    ClobFile file = pair.Value;
                    dataGridView.Rows.Add( 
                        file.filename, 
                        file.lastModified,
                        file.lastModified,
                        "Create", 
                        "Reclob" );
                }
                dataGridView.Sort( dataGridView.Columns["FileName"], ListSortDirection.Ascending );
                clobType.dataGridView = dataGridView;
                this.dataGridList.Add( dataGridView );
            }
        }

        private void dataGridView_CellContentClick( object _sender, DataGridViewCellEventArgs _e )
        {
            var senderGrid = (DataGridView)_sender;

            if ( senderGrid.Columns[_e.ColumnIndex] is DataGridViewButtonColumn 
              && _e.RowIndex >= 0 )
            {
                switch ( senderGrid.Columns[_e.ColumnIndex].Name )
                {
                    case "Create":
                    {
                        throw new NotImplementedException();
                    }
                    case "Reclob":
                    {
                        for ( int i = 0; i < this.dataGridList.Count; ++i )
                        {
                            if ( this.dataGridList[i] == senderGrid )
                            {
                                this.lobsterModel.clobTypes[i].fileList[_e.RowIndex].ClobToDatabase();
                                break;
                            }
                        }
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
}
