using CustomUIControls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
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

        // TODO: Die singleton, die!
        public static LobsterMain instance;

        public LobsterMain()
        {
            instance = this;
            this.InitializeComponent();
            this.lobsterModel = new LobsterModel();
            this.CreateTreeView();
        }

        private void CreateTreeView()
        {
            TreeNode rootNode;
            rootNode = new TreeNode( "CodeSource", 0, 0 );
            rootNode.Expand();
            foreach ( ClobDirectory clobDir in this.lobsterModel.clobDirectories )
            {
                TreeNode dirNode = new TreeNode( clobDir.rootClobNode.dirInfo.Name, 0, 0 );
                dirNode.Tag = clobDir.rootClobNode;
                this.PopulateTreeNode_r( clobDir.rootClobNode, dirNode );
                rootNode.Nodes.Add( dirNode );
            }
            this.fileTreeView.Nodes.Add( rootNode );
        }

        private void PopulateTreeNode_r( ClobNode _clobNode, TreeNode _treeNode )
        {
            foreach ( ClobNode child in _clobNode.subDirs )
            {
                TreeNode aNode = new TreeNode( child.dirInfo.Name );
                aNode.Tag = child;
                aNode.ImageKey = "folder";
                
                PopulateTreeNode_r( child, aNode );
                _treeNode.Nodes.Add( aNode );
            }
        }
        
        private void treeView1_NodeMouseClick( object sender, TreeNodeMouseClickEventArgs e )
        {
            TreeNode newSelected = e.Node;
            fileListView.Items.Clear();
            ClobNode clobNode = (ClobNode)newSelected.Tag;
            if ( clobNode == null )
            {
                return;
            }
            DirectoryInfo nodeDirInfo = clobNode.dirInfo;

            foreach ( KeyValuePair<string, ClobFile> pair in clobNode.clobFileMap )
            {
                ClobFile clobFile = pair.Value;
                ListViewItem item = new ListViewItem( clobFile.fileInfo.Name, 1 );
                ListViewItem.ListViewSubItem[] subItems = new ListViewItem.ListViewSubItem[]
                {
                    new ListViewItem.ListViewSubItem(item, clobFile.fileInfo.LastAccessTime.ToShortDateString()),
                    new ListViewItem.ListViewSubItem(item, clobFile.status.ToString() ),
                };
                item.Tag = clobFile;
                item.ForeColor = clobFile.status == ClobFile.STATUS.LOCAL_ONLY ? Color.Green : Color.Black;
                item.SubItems.AddRange( subItems );
                this.fileListView.Items.Add( item );
            }
            this.fileListView.AutoResizeColumns( ColumnHeaderAutoResizeStyle.HeaderSize );
        }

        private void listView1_MouseClick( object sender, MouseEventArgs e )
        {
            if ( e.Button == MouseButtons.Right )
            {
                if ( fileListView.FocusedItem.Bounds.Contains( e.Location ) == true )
                {
                    contextMenuStrip1.Show( Cursor.Position );
                    ClobFile clobFile = (ClobFile)fileListView.FocusedItem.Tag;
                    contextMenuStrip1.Tag = fileListView.FocusedItem;
                    insertToolStripMenuItem.Enabled = clobFile.status == ClobFile.STATUS.LOCAL_ONLY;
                    clobToolStripMenuItem.Enabled = clobFile.status == ClobFile.STATUS.SYNCHRONISED;
                    diffWithDatabaseToolStripMenuItem.Enabled = !new List<string>{ ".png",".gif",".bmp" }.Contains( Path.GetExtension( clobFile.fileInfo.Name ) );
                }
            }
        }

        private void insertToolStripMenuItem_Click( object sender, EventArgs e )
        {
            ToolStripItem item = (ToolStripItem)sender;
            ListViewItem listItem = (ListViewItem)item.GetCurrentParent().Tag;
            ClobFile clobFile = (ClobFile)listItem.Tag;
            bool result;

            if ( clobFile.parentClobDirectory.clobType.dataTypeColumnName != null )
            {
                DatatypePicker typePicker = new DatatypePicker( clobFile.parentClobDirectory.clobType );
                DialogResult dialogResult = typePicker.ShowDialog();
                if ( dialogResult != DialogResult.OK )
                {
                    return;
                }
                string chosenType = typePicker.datatypeComboBox.Text;
                result = this.lobsterModel.SendInsertClobMessage( clobFile, chosenType );
            }
            else
            {
                result = this.lobsterModel.SendInsertClobMessage( clobFile, null );
            }

            if ( result )
            {
                listItem.SubItems[2].Text = clobFile.status.ToString();
            }
            this.OnFileInsertComplete( clobFile, result );
        }

        private void showInExplorerToolStripMenuItem_Click( object sender, EventArgs e )
        {
            ToolStripItem item = (ToolStripItem)sender;
            ListViewItem listItem = (ListViewItem)item.GetCurrentParent().Tag;
            ClobFile clobFile = (ClobFile)listItem.Tag;
            Process.Start( "explorer", "/select," + clobFile.fileInfo.FullName );
        }

        private void diffWithDatabaseToolStripMenuItem_Click( object sender, EventArgs e )
        {
            ToolStripItem item = (ToolStripItem)sender;
            ListViewItem listItem = (ListViewItem)item.GetCurrentParent().Tag;
            ClobFile clobFile = (ClobFile)listItem.Tag;

            string databaseContent = this.lobsterModel.SendGetClobDataMessage( clobFile );

            if ( databaseContent == null )
            {
                this.OnErrorMessage( "Diff with Database", "An error ocurred when fetching the file data." );
                return;
            }

            string tempName = Path.GetTempFileName();
            FileInfo fileInfo = new FileInfo( tempName );
            StreamWriter streamWriter = File.AppendText( tempName );
            //TODO
            streamWriter.Write( databaseContent );
            streamWriter.Close();
            streamWriter.Dispose();

            MessageLog.Log( "Temp file \"" + fileInfo.FullName + "\" created" );
            this.lobsterModel.tempFileList.Add( fileInfo );

            Process.Start( "tortoisemerge", "/mine:" + tempName + " /theirs:" + clobFile.fileInfo.FullName );
        }

        private void clobToolStripMenuItem_Click( object sender, EventArgs e )
        {
            ToolStripItem item = (ToolStripItem)sender;
            ListViewItem listItem = (ListViewItem)item.GetCurrentParent().Tag;
            ClobFile clobFile = (ClobFile)listItem.Tag;
            bool result = this.lobsterModel.SendUpdateClobMessage( clobFile );
            this.OnFileUpdateComplete( clobFile, result );
        }

        private void LobsterMain_Resize( object sender, EventArgs e )
        {
            if ( this.WindowState == FormWindowState.Minimized )
            {
                this.Hide();
            }
        }

        private void notifyIcon1_MouseClick( object sender, MouseEventArgs e )
        {
            this.Show();
            this.BringToFront();
            this.WindowState = FormWindowState.Normal;
        }

        public void OnFileInsertComplete( ClobFile _clobFile, bool _result )
        {
            this.notifyIcon1.ShowBalloonTip( Program.BALLOON_TOOLTIP_DURATION,
                _result ? "File Inserted" : "File Insert Failed",
                _clobFile.fileInfo.Name,
                _result ? ToolTipIcon.Info : ToolTipIcon.Error );
        }

        public void OnFileUpdateComplete( ClobFile _clobFile, bool _result )
        {
            this.notifyIcon1.ShowBalloonTip( Program.BALLOON_TOOLTIP_DURATION,
                _result ? "Database Updated" : "Database Update Failed",
                _clobFile.fileInfo.Name,
                _result ? ToolTipIcon.Info : ToolTipIcon.Error );
        }

        public void OnErrorMessage( string _caption, string _text )
        {
            DialogResult result = MessageBox.Show( _text, _caption, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1 );
        }

        private void LobsterMain_FormClosed( object sender, FormClosedEventArgs e )
        {
            foreach ( FileInfo tempFile in this.lobsterModel.tempFileList )
            {
                File.Delete( tempFile.FullName );
            }
        }
    }
}
