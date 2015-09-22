using Lobster.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Media;
using System.Windows.Forms;

namespace Lobster
{
    /// <summary>
    /// 
    /// One Ring to rule them all,
    /// One Ring to find them,
    /// One Ring to bring them all
    /// and in the Darkness bind them.
    ///     -- Enscription on the Ring of Power
    /// 
    /// [ _The Lord of the Rings_, I/ii: "The Shadow of the Past"]
    /// </summary>
    public partial class LobsterMain : Form
    {
        private LobsterModel lobsterModel;

        // TODO: Die singleton, die!
        public static LobsterMain instance;

        private ClobNode currentNode;

        private SoundPlayer successSoundPlayer;
        private SoundPlayer failureSoundPlayer;

        public LobsterMain()
        {
            instance = this;
            this.InitializeComponent();
            this.lobsterModel = new LobsterModel();
            
            this.lobsterModel.LoadDatabaseConnections();

            this.PopulateConnectionList( this.lobsterModel.dbConnectionList );

            this.successSoundPlayer = new SoundPlayer( Program.SETTINGS_DIR + "/media/success.wav" );
            this.failureSoundPlayer = new SoundPlayer( Program.SETTINGS_DIR + "/media/failure.wav" );
        }

        private void PopulateConnectionList( List<DatabaseConnection> connectionList )
        {
            this.connectionListView.Items.Clear();
            for ( int i = 0; i < connectionList.Count; ++i )
            {
                DatabaseConnection connection = connectionList[i];
                ListViewItem item = new ListViewItem( connection.Name );
                string ipAddress = connection.Host;

                ListViewItem.ListViewSubItem[] subItems = new ListViewItem.ListViewSubItem[]
                {
                    new ListViewItem.ListViewSubItem( item, ipAddress ),
                    new ListViewItem.ListViewSubItem( item, connection.Port ),
                    new ListViewItem.ListViewSubItem( item, connection.SID ),
                    new ListViewItem.ListViewSubItem( item, connection.CodeSource ),
                    new ListViewItem.ListViewSubItem( item, connection.UsePooling.ToString() ),
                };
                item.SubItems.AddRange( subItems );
                item.Tag = i;
                this.connectionListView.Items.Add( item );
            }

            this.connectionListView.AutoResizeColumns( ColumnHeaderAutoResizeStyle.HeaderSize );
            if ( this.connectionListView.Items.Count > 0 )
            {
                this.connectionListView.Items[0].Selected = true; 
            }
        }

        private void PopulateDirectoryTreeView()
        {
            Debug.Assert( this.lobsterModel.currentConnection != null );
            this.fileTreeView.Nodes.Clear();
            // Use the folder name as the root element
            DatabaseConnection dbc = this.lobsterModel.currentConnection;
            TreeNode rootNode = new TreeNode( Path.GetFileName( dbc.CodeSource ) ?? "CodeSource", 0, 0 );
            foreach ( KeyValuePair<ClobType, ClobDirectory> pair in dbc.ClobTypeToDirectoryMap )
            {
                ClobDirectory clobDir = pair.Value;
                TreeNode dirNode = new TreeNode( clobDir.RootClobNode.DirInfo.Name, 0, 0 );
                dirNode.Tag = clobDir.RootClobNode;
                this.PopulateTreeNode_r( clobDir.RootClobNode, dirNode );
                rootNode.Nodes.Add( dirNode );
            }
            this.fileTreeView.Nodes.Add( rootNode );
            rootNode.Expand();
        }

        private void PopulateTreeNode_r( ClobNode _clobNode, TreeNode _treeNode )
        {
            foreach ( ClobNode child in _clobNode.ChildNodes )
            {
                TreeNode aNode = new TreeNode( child.DirInfo.Name );
                aNode.Tag = child;
                aNode.ImageKey = "folder";

                PopulateTreeNode_r( child, aNode );
                _treeNode.Nodes.Add( aNode );
            }
        }
        
        private void fileTreeView_NodeMouseClick( object sender, TreeNodeMouseClickEventArgs e )
        {
            TreeNode newSelected = e.Node;
            fileListView.Items.Clear();
            ClobNode clobNode = (ClobNode)newSelected.Tag;
            if ( clobNode == null )
            {
                return;
            }

            this.currentNode = clobNode;
            this.PopulateFileListView( clobNode );
            this.UpdateRibbonButtons();
        }

        private void PopulateFileListView( ClobNode _clobNode )
        {
            Debug.Assert( _clobNode != null );
            this.fileListView.Items.Clear();
            DirectoryInfo nodeDirInfo = _clobNode.DirInfo;

            foreach ( KeyValuePair<string, ClobFile> pair in _clobNode.ClobFileMap )
            {
                ClobFile clobFile = pair.Value;
                Debug.Assert( clobFile.LocalFile != null );
                if ( clobFile.LocalFile != null )
                {
                    clobFile.LocalFile.FileInfo.Refresh();
                }
                
                ListViewItem item = this.GetListViewRowForClobFile( clobFile );
                this.fileListView.Items.Add( item );
            }
            // If it is the root node, also display any database only files
            if ( _clobNode.BaseClobDirectory.RootClobNode == _clobNode )
            {
                foreach ( ClobFile dbClobFIle in _clobNode.BaseClobDirectory.DatabaseOnlyFiles )
                {
                    ListViewItem item = this.GetListViewRowForClobFile( dbClobFIle );
                    this.fileListView.Items.Add( item );
                }
            }
            this.fileListView.AutoResizeColumns( ColumnHeaderAutoResizeStyle.HeaderSize );
        }

        public ListViewItem GetListViewRowForClobFile( ClobFile _clobFile )
        {
            string filename = _clobFile.LocalFile != null ? _clobFile.LocalFile.FileInfo.Name : _clobFile.DatabaseFile.Filename;
            int imageHandle;
            string dateValue = "";
            string status;
            Color foreColour;

            if ( _clobFile.IsDbOnly )
            {
                imageHandle = 3;
                status = "Database Only";
                foreColour = Color.Gray;
            }
            else
            {
                imageHandle = _clobFile.IsEditable ? 1 : 2;
                dateValue = _clobFile.LocalFile.FileInfo.LastAccessTime.ToShortDateString();
                if ( _clobFile.IsSynced )
                {
                    status = "Synchronised";
                    foreColour = Color.Black;
                }
                else if ( _clobFile.IsLocalOnly )
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
            this.lobsterModel.currentConnection.GetWorkingFiles( ref workingFiles );

            foreach ( ClobFile clobFile in workingFiles )
            {
                ListViewItem item = this.GetListViewRowForClobFile( clobFile );
                this.workingFileList.Items.Add( item );
            }
            this.workingFileList.AutoResizeColumns( ColumnHeaderAutoResizeStyle.HeaderSize );
        }

        private void fileListView_MouseClick( object sender, MouseEventArgs e )
        {
            if ( e.Button == MouseButtons.Right && fileListView.FocusedItem.Bounds.Contains( e.Location ) == true )
            {
                clobFileContextMenuStrip.Show( Cursor.Position );
                ClobFile clobFile = (ClobFile)fileListView.FocusedItem.Tag;
                this.ShowClobFileContextMenu( clobFile, fileListView );
            }
        }

        private void ShowClobFileContextMenu( ClobFile _clobFile, ListView _fileListView )
        {
            clobFileContextMenuStrip.Tag = _fileListView.FocusedItem;

            insertToolStripMenuItem.Enabled = _clobFile.IsLocalOnly;
            clobToolStripMenuItem.Enabled = _clobFile.IsSynced;
            diffWithDatabaseToolStripMenuItem.Enabled = _clobFile.IsSynced
                && !new List<string> { ".png", ".gif", ".bmp" }.Contains( Path.GetExtension( _clobFile.LocalFile.FileInfo.Name ) );
            
            showInExplorerToolStripMenuItem.Enabled = _clobFile.LocalFile != null;
            openDatabaseToolStripMenuItem.Enabled = _clobFile.DatabaseFile != null;
        }

        private void insertToolStripMenuItem_Click( object sender, EventArgs e )
        {
            ToolStripItem item = (ToolStripItem)sender;
            ListViewItem listItem = (ListViewItem)item.GetCurrentParent().Tag;
            ClobFile clobFile = (ClobFile)listItem.Tag;
            Debug.Assert( clobFile != null );

            this.InsertClobFile( clobFile );
        }

        private void InsertClobFile( ClobFile _clobFile )
        {
            Table table;
            ClobDirectory clobDir = _clobFile.ParentClobNode.BaseClobDirectory;
            // If there is > 1 tables in this ClobType, ask the user for which one to use
            if (clobDir.ClobType.Tables.Count > 1 )
            {
                TablePicker tablePicker = new TablePicker(clobDir.ClobType );
                DialogResult dialogResult = tablePicker.ShowDialog();
                if ( dialogResult != DialogResult.OK )
                {
                    return;
                }
                table = tablePicker.tableCombo.SelectedItem as Table;
            }
            else
            {
                table = clobDir.ClobType.Tables[0];
            }

            // If the table has a MimeType column, ask the user for the type to use
            string mimeType = null;
            Column mimeTypeColumn = table.columns.Find( x => x.ColumnPurpose == Column.Purpose.MIME_TYPE );
            if ( mimeTypeColumn != null )
            {
                DatatypePicker typePicker = new DatatypePicker( mimeTypeColumn );
                DialogResult dialogResult = typePicker.ShowDialog();
                if ( dialogResult != DialogResult.OK )
                {
                    return;
                }
                mimeType = typePicker.datatypeComboBox.Text;
            }

            bool success = this.lobsterModel.SendInsertClobMessage( _clobFile, table, mimeType );
            if ( success )
            {
                this.RefreshClobLists();
            }
            this.OnFileInsertComplete( _clobFile, success );
        }

        private void showInExplorerToolStripMenuItem_Click( object sender, EventArgs e )
        {
            ToolStripItem item = (ToolStripItem)sender;
            ListViewItem listItem = (ListViewItem)item.GetCurrentParent().Tag;
            ClobFile clobFile = (ClobFile)listItem.Tag;
            this.ExploreToClobFile( clobFile );
        }

        private void diffWithDatabaseToolStripMenuItem_Click( object sender, EventArgs e )
        {
            ToolStripItem item = (ToolStripItem)sender;
            ListViewItem listItem = (ListViewItem)item.GetCurrentParent().Tag;
            ClobFile clobFile = (ClobFile)listItem.Tag;
            this.DiffClobFileWithDatabase( clobFile );
        }

        private void clobToolStripMenuItem_Click( object sender, EventArgs e )
        {
            ToolStripItem item = (ToolStripItem)sender;
            ListViewItem listItem = (ListViewItem)item.GetCurrentParent().Tag;
            ClobFile clobFile = (ClobFile)listItem.Tag;
            this.ReclobFile( clobFile );
        }
        
        private void lobsterNotificationIcon_MouseClick( object sender, MouseEventArgs e )
        {
            this.Show();
            this.BringToFront();
            this.WindowState = FormWindowState.Normal;
        }

        public void OnFileInsertComplete( ClobFile _clobFile, bool _result )
        {
            Debug.Assert( _clobFile.LocalFile != null );
            this.lobsterNotificationIcon.ShowBalloonTip( Program.BALLOON_TOOLTIP_DURATION_MS,
                _result ? "File Inserted" : "File Insert Failed",
                _clobFile.LocalFile.FileInfo.Name,
                _result ? ToolTipIcon.Info : ToolTipIcon.Error );
            
            ( _result ? this.successSoundPlayer : this.failureSoundPlayer ).Play();
        }

        public void OnFileUpdateComplete( ClobFile _clobFile, bool _result )
        {
            Debug.Assert( _clobFile.LocalFile != null );
            this.lobsterNotificationIcon.ShowBalloonTip( Program.BALLOON_TOOLTIP_DURATION_MS,
                _result ? "Database Updated" : "Database Update Failed",
                _clobFile.LocalFile.FileInfo.Name,
                _result ? ToolTipIcon.Info : ToolTipIcon.Error );

            ( _result ? this.successSoundPlayer : this.failureSoundPlayer ).Play();
        }

        public static void OnErrorMessage( string _caption, string _text )
        {
            DialogResult result = MessageBox.Show( _text, _caption, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1 );
        }

        private void LobsterMain_FormClosed( object sender, FormClosedEventArgs e )
        {
            foreach ( FileInfo tempFile in this.lobsterModel.tempFileList )
            {
                try
                {
                    File.Delete( tempFile.FullName );
                    MessageLog.LogInfo( "Temporary file deleted " + tempFile.FullName );
                }
                catch ( IOException _e )
                {
                    MessageLog.LogWarning( "Failed to delete temporary file " + tempFile.FullName + " " + _e );
                }
            }
        }

        private void MainTabControl_Selecting( object sender, TabControlCancelEventArgs e )
        {
            if ( this.lobsterModel.currentConnection == null )
            {
                e.Cancel = true;
                MessageBox.Show( "You must first connect to a database. To make connect, first select a connection from the list, then press the Connect button." );
                return;
            }

            if ( this.MainTabControl.SelectedTab == this.connectionTabPage )
            {
               
            }
            else if ( this.MainTabControl.SelectedTab == this.treeViewTab )
            {
                this.RefreshClobLists();
            }
            else if ( this.MainTabControl.SelectedTab == this.workingFileTab )
            {
                this.RefreshClobLists();
            }
            else
            {
                throw new ArgumentException( "Unknown tab " + this.MainTabControl.SelectedTab.Name );
            }

            this.refreshButton.Enabled = this.MainTabControl.SelectedTab == this.treeViewTab || this.MainTabControl.SelectedTab == this.workingFileTab;
        }

        private void workingFileList_MouseClick( object sender, MouseEventArgs e )
        {
            if ( e.Button == MouseButtons.Right )
            {
                if ( workingFileList.FocusedItem.Bounds.Contains( e.Location ) == true )
                {
                    clobFileContextMenuStrip.Show( Cursor.Position );
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
            this.PullDatabaseFile( clobFile );           
        }

        private void RefreshClobLists()
        {
            if ( this.MainTabControl.SelectedTab == this.workingFileTab )
            {
                this.PopulateWorkingFileView();
            }
            else if ( this.MainTabControl.SelectedTab == this.treeViewTab )
            {
                if ( this.currentNode != null )
                {
                    this.PopulateFileListView( this.currentNode );
                }
            }
            this.UpdateRibbonButtons();
        }

        public void UpdateUIThread()
        {
            this.Invoke( (MethodInvoker)delegate
            {
                RefreshClobLists();
            } );
        }

        private void connectButton_Click( object sender, EventArgs e )
        {
            if ( this.connectionListView.SelectedItems.Count == 0 )
            {
                MessageBox.Show( "You must first select a connection." );
                return;
            }

            if ( this.connectionListView.SelectedItems.Count > 1 )
            {
                throw new ArgumentException( "Cannot connect to more than one database at a time." );
            }

            LoadingForm loadingForm = new LoadingForm();
            loadingForm.Show();

            int configIndex = (int)this.connectionListView.SelectedItems[0].Tag;
            DatabaseConnection connection = this.lobsterModel.dbConnectionList[configIndex];
            bool result = this.lobsterModel.SetDatabaseConnection( connection );
            if ( result )
            {
                this.PopulateDirectoryTreeView();
                this.MainTabControl.SelectTab( 1 );
                this.refreshButton.Enabled = true;
            }
            loadingForm.Close();
            
        }

        private void refreshButtonClick( object sender, EventArgs e )
        {
            if ( this.lobsterModel.currentConnection != null )
            {
                this.lobsterModel.RequeryDatabase();
                this.RefreshClobLists();
            }
        }

        private void connectionListView_SelectedIndexChanged( object _sender, EventArgs _e )
        {
            this.UpdateRibbonButtons();
            this.UpdateConnectionButtons();
        }

        private void UpdateConnectionButtons()
        {
            this.removeConnectionButton.Enabled = this.editConnectionButton.Enabled
                = ( this.connectionListView.SelectedItems.Count > 0 );
        }

        private void fileListView_SelectedIndexChanged( object sender, EventArgs e )
        {
            this.UpdateRibbonButtons();
        }

        /// <summary>
        /// Sets the Enable property of the "Ribbon" buttons depending on program state
        /// </summary>
        private void UpdateRibbonButtons()
        {
            ClobFile selectedFile = this.GetCurrentlySelectedClobFile();

            // Only enable the Insert button if the file is local-only
            this.insertButton.Enabled = selectedFile != null && selectedFile.IsLocalOnly;

            // Only enable the Reclob and Diff buttons if the file is synchronised
            this.reclobButton.Enabled = this.diffWithDBButton.Enabled = selectedFile != null && selectedFile.IsSynced;

            // Only enable the Explore To button if the file exists locally
            this.exploreButton.Enabled = selectedFile != null && selectedFile.LocalFile != null;

            // Only enable the Pull button if the file exists on the database
            this.pullDBFileButton.Enabled = selectedFile != null && selectedFile.DatabaseFile != null;

            // Only enable the connection button if on the connection page
            this.connectButton.Enabled = this.MainTabControl.SelectedTab == this.connectionTabPage && this.connectionListView.SelectedItems.Count > 0;
        }

        private void insertButton_Click( object sender, EventArgs e )
        {
            ClobFile clobFile = this.GetCurrentlySelectedClobFile();
            if ( clobFile != null )
            {
                this.InsertClobFile( clobFile );
            }
            else
            {
                this.ShowNoFileSelectedMessage();
            }
        }

        private void ShowNoFileSelectedMessage()
        {
            MessageBox.Show( "No files selected", "You must select a file before you can perform this action.", MessageBoxButtons.OK, MessageBoxIcon.Warning );
        }

        private void reclobButton_Click( object sender, EventArgs e )
        {
            ClobFile clobFile = this.GetCurrentlySelectedClobFile();
            if ( clobFile != null )
            {
                this.ReclobFile( clobFile );
            }
            else
            {
                this.ShowNoFileSelectedMessage();
            }
        }

        private void ReclobFile( ClobFile _clobFile )
        {
            Debug.Assert( _clobFile.IsSynced );
            if ( _clobFile.LocalFile.FileInfo.IsReadOnly )
            {
                DialogResult result = MessageBox.Show( _clobFile.LocalFile.FileInfo.Name + " is locked. Are you sure you want to clob it?",
                    "File is Locked",
                    MessageBoxButtons.OKCancel );
                if ( result != DialogResult.OK )
                {
                    return;
                }
            }

            this.lobsterModel.SendUpdateClobMessage( _clobFile );
        }

        private void exploreButton_click( object sender, EventArgs e )
        {
            ClobFile clobFile = this.GetCurrentlySelectedClobFile();
            if ( clobFile != null )
            {
                this.ExploreToClobFile( clobFile );
            }
            else
            {
                this.ShowNoFileSelectedMessage();
            }
        }

        private void ExploreToClobFile( ClobFile _clobFile )
        {
            Debug.Assert( _clobFile.LocalFile != null );
            Process.Start( "explorer", "/select," + _clobFile.LocalFile.FileInfo.FullName );
        }

        private void workingFileList_SelectedIndexChanged( object sender, EventArgs e )
        {
            this.UpdateRibbonButtons();
        }

        private void diffWithDBButton_Click( object sender, EventArgs e )
        {
            ClobFile clobFile = this.GetCurrentlySelectedClobFile();
            if ( clobFile != null )
            {
                this.DiffClobFileWithDatabase( clobFile );
            }
            else
            {
                this.ShowNoFileSelectedMessage();
            }
        }

        private void DiffClobFileWithDatabase( ClobFile _clobFile )
        {
            Debug.Assert( _clobFile.IsSynced );

            FileInfo tempFile = this.lobsterModel.SendDownloadClobDataToFileMessage( _clobFile );
            if ( tempFile == null )
            {
                LobsterMain.OnErrorMessage( "Diff with Database", "An error ocurred when fetching the file data." );
                return;
            }

            MessageLog.LogInfo( "Temporary file created: " + tempFile.FullName );
            this.lobsterModel.tempFileList.Add( tempFile );

            Process.Start( "tortoisemerge", "/mine:" + tempFile.FullName + " /theirs:" + _clobFile.LocalFile.FileInfo.FullName );
        }

        private void pullDBFileButton_Click( object sender, EventArgs e )
        {
            ClobFile clobFile = this.GetCurrentlySelectedClobFile();
            if ( clobFile != null )
            {
                this.PullDatabaseFile( clobFile );
            }
            else
            {
                this.ShowNoFileSelectedMessage();
            }
        }

        private void PullDatabaseFile( ClobFile _clobFile )
        {
            Debug.Assert( _clobFile.DatabaseFile != null );
            FileInfo tempFile = this.lobsterModel.SendDownloadClobDataToFileMessage( _clobFile );
            if ( tempFile == null )
            {
                LobsterMain.OnErrorMessage( "Open Database", "An error ocurred when fetching the file data." );
                return;
            }

            MessageLog.LogInfo( "Temporary file created: " + tempFile.FullName );
            this.lobsterModel.tempFileList.Add( tempFile );

            Process.Start( tempFile.FullName );
        }

        private ClobFile GetCurrentlySelectedClobFile()
        {
            ClobFile selectedFile = null;

            if ( this.MainTabControl.SelectedTab == this.treeViewTab && this.fileListView.SelectedItems.Count > 0 )
            {
                selectedFile = (ClobFile)this.fileListView.SelectedItems[0].Tag;
            }
            else if ( this.MainTabControl.SelectedTab == this.workingFileTab && this.workingFileList.SelectedItems.Count > 0 )
            {
                selectedFile = (ClobFile)this.workingFileList.SelectedItems[0].Tag;
            }

            return selectedFile;
        }

        private void editConnectionButton_Click( object sender, EventArgs e )
        {
            Debug.Assert( this.connectionListView.SelectedItems.Count <= 1 );
            if ( this.connectionListView.SelectedItems.Count == 0 )
            {
                MessageBox.Show( "You must first select a connection." );
                return;
            }

            int configIndex = (int)this.connectionListView.SelectedItems[0].Tag;
            DatabaseConnection connectionRef = this.lobsterModel.dbConnectionList[configIndex];

            EditDatabaseConnection editForm = new EditDatabaseConnection( connectionRef, false );
            DialogResult result = editForm.ShowDialog();
            this.lobsterModel.dbConnectionList[configIndex] = editForm.OriginalObject;
            this.PopulateConnectionList( this.lobsterModel.dbConnectionList );
        }

        private void newConnectionButton_Click( object sender, EventArgs e )
        {
            DatabaseConnection newConnection = new DatabaseConnection();
            newConnection.ParentModel = this.lobsterModel;
            newConnection.FileLocation = Path.Combine(Settings.Default.ConnectionDir, "NewConnection.xml" ) ;
            EditDatabaseConnection editForm = new EditDatabaseConnection( newConnection, true );
            DialogResult result = editForm.ShowDialog();
            if ( result == DialogResult.OK )
            {
                this.lobsterModel.dbConnectionList.Add( editForm.OriginalObject );
                this.PopulateConnectionList( this.lobsterModel.dbConnectionList );
            }
        }

        private void removeConnectionButton_Click( object sender, EventArgs e )
        {
            Debug.Assert( this.connectionListView.SelectedItems.Count <= 1 );
            if ( this.connectionListView.SelectedItems.Count == 0 )
            {
                MessageBox.Show( "You must first select a connection." );
                return;
            }

            int configIndex = (int)this.connectionListView.SelectedItems[0].Tag;
            DatabaseConnection databaseConnection = this.lobsterModel.dbConnectionList[configIndex];

            DialogResult result = MessageBox.Show( 
                "Are you sure you want to permanently delete the connection " + databaseConnection.Name ?? "New Connection" + "?",
                "Remove Connection", MessageBoxButtons.OKCancel );

            if ( result == DialogResult.OK )
            {
                File.Delete( databaseConnection.FileLocation );
                this.lobsterModel.dbConnectionList.RemoveAt( configIndex );
                this.PopulateConnectionList( this.lobsterModel.dbConnectionList );
            }
        }

        private void openFileDialog1_FileOk( object sender, System.ComponentModel.CancelEventArgs e )
        {

        }
    }
}
