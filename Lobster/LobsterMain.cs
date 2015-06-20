using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
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
            foreach ( ClobDirectory clobDirectory in this.lobsterModel.clobDirectories )
            {
                DataGridView dataGridView = this.CreateClobTab( clobDirectory );
                this.dataGridList.Add( dataGridView );
            }

            PopulateTreeView();
        }

        private void PopulateTreeView()
        {
            TreeNode rootNode;
            /*DirectoryInfo info = new DirectoryInfo( @"C:\ICONWS\trunk\CodeSource" );
            if ( info.Exists )
            {
                rootNode = new TreeNode( "CodeSource", 0, 0 );
                rootNode.Tag = info;
                GetDirectories( info.GetDirectories(), rootNode );
                treeView1.Nodes.Add( rootNode );
            }*/
            rootNode = new TreeNode( "CodeSource", 0, 0 );
            //rootNode.Tag = this.lobsterModel.rootClobNode;
            rootNode.Expand();
            foreach ( ClobDirectory clobDir in this.lobsterModel.clobDirectories )
            {
                TreeNode dirNode = new TreeNode( clobDir.rootClobNode.dirInfo.Name, 0, 0 );
                dirNode.Tag = clobDir.rootClobNode;

                this.GetDirectories( clobDir.rootClobNode, dirNode );

                rootNode.Nodes.Add( dirNode );
            }

            //GetDirectories( this.lobsterModel.rootClobNode, rootNode );
            treeView1.Nodes.Add( rootNode );
        }

        private void GetDirectories( ClobNode _clobNode, TreeNode _treeNode )
        //private void GetDirectories( DirectoryInfo[] subDirs, TreeNode nodeToAddTo )
        {
            TreeNode aNode;
            //DirectoryInfo[] subSubDirs;
            //foreach ( DirectoryInfo subDir in subDirs )
            foreach ( ClobNode child in _clobNode.subDirs )
            {
                //aNode = new TreeNode( subDir.Name, 0, 0 );
                aNode = new TreeNode( child.dirInfo.Name );
                //aNode.Tag = subDir;
                aNode.Tag = child;
                aNode.ImageKey = "folder";
                //subSubDirs = subDir.GetDirectories();
                //if ( subDirs.Length != 0)
                {
                //    GetDirectories( subSubDirs, aNode );
                }
                GetDirectories( child, aNode );
                //nodeToAddTo.Nodes.Add( aNode );
                _treeNode.Nodes.Add( aNode );
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


            /*foreach ( KeyValuePair<string, ClobFile> pair in _clobDirectory.fileMap )
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
            }*/
            //dataGridView.Sort( dataGridView.Columns["FileName"], ListSortDirection.Ascending );
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

        private void treeView1_NodeMouseClick( object sender, TreeNodeMouseClickEventArgs e )
        {
            TreeNode newSelected = e.Node;
            listView1.Items.Clear();
            ClobNode clobNode = (ClobNode)newSelected.Tag;
            if ( clobNode == null )
            {
                return;
            }
            DirectoryInfo nodeDirInfo = clobNode.dirInfo;// (DirectoryInfo)newSelected.Tag;
            ListViewItem.ListViewSubItem[] subItems;
            ListViewItem item = null;

            /*foreach ( DirectoryInfo dir in nodeDirInfo.GetDirectories() )
            {
                item = new ListViewItem( dir.Name, 0 );
                subItems = new ListViewItem.ListViewSubItem[]
                {
                    new ListViewItem.ListViewSubItem(item,"Directory"),
                    new ListViewItem.ListViewSubItem( item, dir.LastAccessTime.ToShortDateString())
                };
                item.SubItems.AddRange( subItems );
                listView1.Items.Add( item );
            }*/
            /*foreach ( FileInfo file in nodeDirInfo.GetFiles())
            {
                item = new ListViewItem( file.Name, 1 );
                subItems = new ListViewItem.ListViewSubItem[]
                {
                    new ListViewItem.ListViewSubItem(item,"File"),
                    new ListViewItem.ListViewSubItem(item,file.LastAccessTime.ToShortDateString())
                };
                item.SubItems.AddRange( subItems );
                listView1.Items.Add( item );

            }*/
            foreach ( ClobFile clobFile in clobNode.clobFiles )
            {
                item = new ListViewItem( clobFile.fileInfo.Name, 1 );
                subItems = new ListViewItem.ListViewSubItem[]
                {
                    new ListViewItem.ListViewSubItem(item, "File"),
                    new ListViewItem.ListViewSubItem(item, clobFile.fileInfo.LastAccessTime.ToShortDateString())
                };
                item.SubItems.AddRange( subItems );
                listView1.Items.Add( item );
            }
            listView1.AutoResizeColumns( ColumnHeaderAutoResizeStyle.HeaderSize );
        }
    }
}
