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
        private LobsterModel lobsterModel;

        public LobsterMain( LobsterModel _lobsterModel )
        {
            this.lobsterModel = _lobsterModel;
            InitializeComponent();

            this.PopulateTreeView();
        }

        private void PopulateTreeView()
        {
            TreeNode rootNode;
            rootNode = new TreeNode( "CodeSource", 0, 0 );
            rootNode.Expand();
            foreach ( ClobDirectory clobDir in this.lobsterModel.clobDirectories )
            {
                TreeNode dirNode = new TreeNode( clobDir.rootClobNode.dirInfo.Name, 0, 0 );
                dirNode.Tag = clobDir.rootClobNode;

                this.GetDirectories( clobDir.rootClobNode, dirNode );

                rootNode.Nodes.Add( dirNode );
            }
            treeView1.Nodes.Add( rootNode );
        }

        private void GetDirectories( ClobNode _clobNode, TreeNode _treeNode )
        {
            TreeNode aNode;
            foreach ( ClobNode child in _clobNode.subDirs )
            {
                aNode = new TreeNode( child.dirInfo.Name );
                aNode.Tag = child;
                aNode.ImageKey = "folder";
                
                GetDirectories( child, aNode );
                _treeNode.Nodes.Add( aNode );
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
            DirectoryInfo nodeDirInfo = clobNode.dirInfo;
            ListViewItem.ListViewSubItem[] subItems;
            ListViewItem item = null;

            foreach ( ClobFile clobFile in clobNode.clobFiles )
            {
                item = new ListViewItem( clobFile.fileInfo.Name, 1 );
                subItems = new ListViewItem.ListViewSubItem[]
                {
                    new ListViewItem.ListViewSubItem(item, clobFile.fileInfo.LastAccessTime.ToShortDateString()),
                    new ListViewItem.ListViewSubItem(item, clobFile.status.ToString() ),
                };
                item.Tag = clobFile;
                item.SubItems.AddRange( subItems );
                listView1.Items.Add( item );
            }
            listView1.AutoResizeColumns( ColumnHeaderAutoResizeStyle.HeaderSize );
        }

        private void listView1_MouseClick( object sender, MouseEventArgs e )
        {
            if ( e.Button == MouseButtons.Right )
            {
                if ( listView1.FocusedItem.Bounds.Contains( e.Location ) == true )
                {
                    contextMenuStrip1.Show( Cursor.Position );
                    contextMenuStrip1.Tag = listView1.FocusedItem;//.Tag;
                }
            }
        }

        private void insertToolStripMenuItem_Click( object sender, EventArgs e )
        {
            ToolStripItem item = (ToolStripItem)sender;
            //ClobFile clobFile = (ClobFile)item.GetCurrentParent().Tag;
            ListViewItem listItem = (ListViewItem)item.GetCurrentParent().Tag;
            ClobFile clobFile = (ClobFile)listItem.Tag;
            if ( clobFile.InsertIntoDatabase() )
            {
                listItem.SubItems[2].Text = clobFile.status.ToString();
            }
        }
    }
}
