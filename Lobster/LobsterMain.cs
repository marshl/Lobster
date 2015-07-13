using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Media;
using System.Windows.Forms;

namespace Lobster
{
    public partial class LobsterMain : Form
    {
        private LobsterModel lobsterModel;

        // TODO: Die singleton, die!
        public static LobsterMain instance;

        private ClobNode currentNode;

        public LobsterMain()
        {
            instance = this;
            this.InitializeComponent();
            this.lobsterModel = new LobsterModel();
            
            bool result = this.lobsterModel.LoadDatabaseConfig();
            if ( !result )
            {
                //this.Close();
                return;
            }
            this.lobsterModel.LoadClobTypes();
            //this.lobsterModel.RetrieveDatabaseFiles();
            //this.lobsterModel.CompareFilesToDatabase();
            this.lobsterModel.RequeryDatabase();
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
            foreach ( ClobNode child in _clobNode.childNodes )
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

            this.currentNode = clobNode;
            this.PopulateListView( clobNode );
        }

        private void PopulateListView( ClobNode _clobNode )
        {
            this.fileListView.Items.Clear();
            DirectoryInfo nodeDirInfo = _clobNode.dirInfo;

            foreach ( KeyValuePair<string, ClobFile> pair in _clobNode.clobFileMap )
            {
                ClobFile clobFile = pair.Value;
                Debug.Assert( clobFile.localClobFile != null );
                if ( clobFile.localClobFile != null )
                {
                    clobFile.localClobFile.fileInfo.Refresh();
                }
                
                ListViewItem item = this.GetClobFileRow( clobFile );
                this.fileListView.Items.Add( item );
            }
            // If it is the root node, also display any database only files
            if ( _clobNode.baseDirectory.rootClobNode == _clobNode )
            {
                foreach ( ClobFile dbClobFIle in _clobNode.baseDirectory.databaseOnlyFiles )
                {
                    ListViewItem item = this.GetClobFileRow( dbClobFIle );
                    this.fileListView.Items.Add( item );
                }
            }
            this.fileListView.AutoResizeColumns( ColumnHeaderAutoResizeStyle.HeaderSize );
        }

        public ListViewItem GetClobFileRow( ClobFile _clobFile )
        {
            string filename = _clobFile.localClobFile != null ? _clobFile.localClobFile.fileInfo.Name : _clobFile.dbClobFile.filename;
            int imageHandle;
            string dateValue = "";
            string status;
            Color foreColour;
            if ( _clobFile.localClobFile != null )
            {
                imageHandle = !_clobFile.localClobFile.fileInfo.IsReadOnly ? 1 : 2;
                dateValue = _clobFile.localClobFile.fileInfo.LastAccessTime.ToShortDateString();
                if ( _clobFile.dbClobFile != null )
                {
                    status = "Synchronised with Database";
                    foreColour = Color.Black;
                }
                else if ( _clobFile.localClobFile.fileInfo.Exists )
                {
                    status = "Local Only";
                    foreColour = Color.Green;
                }
                else
                {
                    status = "Deleted";
                    foreColour = Color.Red;
                }
            }
            else
            {
                imageHandle = 3;
                status = "Database Only";
                foreColour = Color.Gray;
            }

            ListViewItem item = new ListViewItem( filename, imageHandle );
            ListViewItem.ListViewSubItem[] subItems = new ListViewItem.ListViewSubItem[]
            {
                new ListViewItem.ListViewSubItem( item, dateValue ),
                new ListViewItem.ListViewSubItem( item, status ),
            };
            item.Tag = _clobFile;
            item.ForeColor = foreColour;
            item.SubItems.AddRange( subItems );
            return item;
        }

        private void PopulateWorkingFileView()
        {
            this.workingFileList.Items.Clear();
            List<ClobFile> workingFiles = new List<ClobFile>();
            this.lobsterModel.GetWorkingFiles( ref workingFiles );

            foreach ( ClobFile clobFile in workingFiles )
            {
                ListViewItem item = this.GetClobFileRow( clobFile );
                this.workingFileList.Items.Add( item );
            }
            this.workingFileList.AutoResizeColumns( ColumnHeaderAutoResizeStyle.HeaderSize );
        }

        private void listView1_MouseClick( object sender, MouseEventArgs e )
        {
            if ( e.Button == MouseButtons.Right )
            {
                if ( fileListView.FocusedItem.Bounds.Contains( e.Location ) == true )
                {
                    contextMenuStrip1.Show( Cursor.Position );
                    ClobFile clobFile = (ClobFile)fileListView.FocusedItem.Tag;
                    this.ShowClobFileContextMenu( clobFile, fileListView );
                }
            }
        }

        private void ShowClobFileContextMenu( ClobFile _clobFile, ListView _fileListView )
        {
            contextMenuStrip1.Tag = _fileListView.FocusedItem;

            insertToolStripMenuItem.Enabled = _clobFile.dbClobFile == null && _clobFile.localClobFile.fileInfo.Exists;
            clobToolStripMenuItem.Enabled = _clobFile.localClobFile != null && _clobFile.dbClobFile != null;
            diffWithDatabaseToolStripMenuItem.Enabled = _clobFile.localClobFile != null && _clobFile.dbClobFile != null
                && !new List<string> { ".png", ".gif", ".bmp" }.Contains( Path.GetExtension( _clobFile.localClobFile.fileInfo.Name ) );

            showInExplorerToolStripMenuItem.Enabled = _clobFile.localClobFile != null && _clobFile.localClobFile.fileInfo.Exists;
            openDatabaseToolStripMenuItem.Enabled = _clobFile.dbClobFile != null;
        }

        private void insertToolStripMenuItem_Click( object sender, EventArgs e )
        {
            ToolStripItem item = (ToolStripItem)sender;
            ListViewItem listItem = (ListViewItem)item.GetCurrentParent().Tag;
            ClobFile clobFile = (ClobFile)listItem.Tag;
            bool result;

            if ( clobFile.parentClobDirectory.clobType.componentTypeColumn != null )
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
                listItem = this.GetClobFileRow( clobFile );
            }
            this.OnFileInsertComplete( clobFile, result );
        }

        private void showInExplorerToolStripMenuItem_Click( object sender, EventArgs e )
        {
            ToolStripItem item = (ToolStripItem)sender;
            ListViewItem listItem = (ListViewItem)item.GetCurrentParent().Tag;
            ClobFile clobFile = (ClobFile)listItem.Tag;
            Debug.Assert( clobFile.localClobFile != null );
            Process.Start( "explorer", "/select," + clobFile.localClobFile.fileInfo.FullName );
        }

        private void diffWithDatabaseToolStripMenuItem_Click( object sender, EventArgs e )
        {
            ToolStripItem item = (ToolStripItem)sender;
            ListViewItem listItem = (ListViewItem)item.GetCurrentParent().Tag;
            ClobFile clobFile = (ClobFile)listItem.Tag;
            Debug.Assert( clobFile.localClobFile != null && clobFile.dbClobFile != null );

            FileInfo tempFile = this.lobsterModel.SendDownloadClobDataToFileMessage( clobFile );
            if ( tempFile == null )
            {
                LobsterMain.OnErrorMessage( "Diff with Database", "An error ocurred when fetching the file data." );
                return;
            }

            MessageLog.Log( "Temp file \"" + tempFile.FullName + "\" created" );
            this.lobsterModel.tempFileList.Add( tempFile );

            Process.Start( "tortoisemerge", "/mine:" + tempFile.FullName + " /theirs:" + clobFile.localClobFile.fileInfo.FullName );
        }

        private void clobToolStripMenuItem_Click( object sender, EventArgs e )
        {
            ToolStripItem item = (ToolStripItem)sender;
            ListViewItem listItem = (ListViewItem)item.GetCurrentParent().Tag;
            ClobFile clobFile = (ClobFile)listItem.Tag;
            Debug.Assert( clobFile.localClobFile != null && clobFile.dbClobFile != null );
            if ( clobFile.localClobFile.fileInfo.IsReadOnly )
            {
                DialogResult result = MessageBox.Show( 
                    clobFile.localClobFile.fileInfo.Name + " is locked. Are you sure you want to clob it?",
                    "File is Locked",
                    MessageBoxButtons.OKCancel );
                if ( result != DialogResult.OK )
                {
                    return;
                } 
            }

            this.lobsterModel.SendUpdateClobMessage( clobFile );
        }

        private void LobsterMain_Resize( object sender, EventArgs e )
        {
            if ( this.WindowState == FormWindowState.Minimized )
            {
                this.Hide();
                this.notifyIcon1.ShowBalloonTip( 2000, "Lobster has been minimised", "Click on the notification icon to show it again", ToolTipIcon.Info );
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
            Debug.Assert( _clobFile.localClobFile != null );
            this.notifyIcon1.ShowBalloonTip( Program.BALLOON_TOOLTIP_DURATION_MS,
                _result ? "File Inserted" : "File Insert Failed",
                _clobFile.localClobFile.fileInfo.Name,
                _result ? ToolTipIcon.Info : ToolTipIcon.Error );

            ( _result ? SystemSounds.Beep : SystemSounds.Exclamation ).Play();
        }

        public void OnFileUpdateComplete( ClobFile _clobFile, bool _result )
        {
            Debug.Assert( _clobFile.localClobFile != null );
            this.notifyIcon1.ShowBalloonTip( Program.BALLOON_TOOLTIP_DURATION_MS,
                _result ? "Database Updated" : "Database Update Failed",
                _clobFile.localClobFile.fileInfo.Name,
                _result ? ToolTipIcon.Info : ToolTipIcon.Error );

            ( _result ? SystemSounds.Beep : SystemSounds.Exclamation ).Play();
        }

        public static void OnErrorMessage( string _caption, string _text )
        {
            DialogResult result = MessageBox.Show( _text, _caption, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1 );
        }

        private void LobsterMain_FormClosed( object sender, FormClosedEventArgs e )
        {
            foreach ( FileInfo tempFile in this.lobsterModel.tempFileList )
            {
                MessageLog.Log( "Temporary file \"" + tempFile.FullName + "\" deleted" );
                File.Delete( tempFile.FullName );
            }
        }

        private void MainTabControl_Selecting( object sender, TabControlCancelEventArgs e )
        {
            this.RefreshClobLists();
        }

        private void workingFileList_MouseClick( object sender, MouseEventArgs e )
        {
            if ( e.Button == MouseButtons.Right )
            {
                if ( workingFileList.FocusedItem.Bounds.Contains( e.Location ) == true )
                {
                    contextMenuStrip1.Show( Cursor.Position );
                    ClobFile clobFile = (ClobFile)workingFileList.FocusedItem.Tag;
                    this.ShowClobFileContextMenu( clobFile, workingFileList);
                }
            }
        }

        private void openDatabaseToolStripMenuItem_Click( object sender, EventArgs e )
        {
            ToolStripItem item = (ToolStripItem)sender;
            ListViewItem listItem = (ListViewItem)item.GetCurrentParent().Tag;
            ClobFile clobFile = (ClobFile)listItem.Tag;

            FileInfo tempFile = this.lobsterModel.SendDownloadClobDataToFileMessage( clobFile );
            if ( tempFile == null )
            {
                LobsterMain.OnErrorMessage( "Open Database", "An error ocurred when fetching the file data." );
                return;
            }
            
            MessageLog.Log( "Temp file \"" + tempFile.FullName + "\" created" );
            this.lobsterModel.tempFileList.Add( tempFile );

            Process.Start( tempFile.FullName );
        }

        private void requeryDatabaseButton_Click( object sender, EventArgs e )
        {
            this.lobsterModel.RequeryDatabase();
            this.RefreshClobLists();
        }

        private void RefreshClobLists()
        {
            if ( this.MainTabControl.SelectedTab == this.workingFileTab )
            {
                this.PopulateWorkingFileView();
            }
            else if ( this.MainTabControl.SelectedTab == this.treeViewTab )
            {
                this.PopulateListView( this.currentNode );
            }
        }
    }
}
