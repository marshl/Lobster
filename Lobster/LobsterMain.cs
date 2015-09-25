//-----------------------------------------------------------------------
// <copyright file="LobsterMain.cs" company="marshl">
// Copyright 2015, Liam Marshall, marshl.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//-----------------------------------------------------------------------
//
//      One Ring to rule them all,
//      One Ring to find them,
//      One Ring to bring them all
//      and in the Darkness bind them.
//          -- Enscription on the Ring of Power
//
//      [ _The Lord of the Rings_, I/ii: "The Shadow of the Past"]
//
//-----------------------------------------------------------------------
namespace Lobster
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Media;
    using System.Windows.Forms;
    using Properties;

    /// <summary>
    /// The main Lobster form.
    /// </summary>
    public partial class LobsterMain : Form
    {
        /// <summary>
        /// The model used by this form.
        /// </summary>
        private LobsterModel model;

        /// <summary>
        /// The sound that is played when an action is completed successfully.
        /// </summary>
        private SoundPlayer successSoundPlayer;

        /// <summary>
        /// The sound that is played when an action fails.
        /// </summary>
        private SoundPlayer failureSoundPlayer;

        /// <summary>
        /// Initializes a new instance of the <see cref="LobsterMain"/> class.
        /// </summary>
        public LobsterMain()
        {
            Instance = this;
            this.InitializeComponent();

            this.model = new LobsterModel();
            
            this.PopulateConnectionList(this.model.ConnectionList);

            this.successSoundPlayer = new SoundPlayer(
                Path.Combine(
                    Settings.Default.SettingsDirectoryName,
                    Settings.Default.MediaDirectoryName,
                    Settings.Default.SuccessSoundFilename));

            this.failureSoundPlayer = new SoundPlayer(
                Path.Combine(
                    Settings.Default.SettingsDirectoryName,
                    Settings.Default.MediaDirectoryName,
                    Settings.Default.FailureSoundFilename));
        }

        /// <summary>
        /// The single instance of this form.
        /// </summary>
        /// TODO: Die singleton, die!
        public static LobsterMain Instance { get; set; }

        /// <summary>
        /// The callback for when a <see cref="ClobFile"/> insert command was completed (successfully or not).
        /// </summary>
        /// <param name="clobFile">The file that the command was run on.</param>
        /// <param name="result">Whether the file was inserted into the database.</param>
        /// TODO: Remove this
        public void OnFileInsertComplete(ClobFile clobFile, bool result)
        {
            Debug.Assert(clobFile.LocalFile != null, "The local file must not be null.");

            this.lobsterNotificationIcon.ShowBalloonTip(
                Settings.Default.BalloonPopupDurationMS,
                result ? "File Inserted" : "File Insert Failed",
                clobFile.LocalFile.FileInfo.Name,
                result ? ToolTipIcon.Info : ToolTipIcon.Error);

            (result ? this.successSoundPlayer : this.failureSoundPlayer).Play();
        }

        /// <summary>
        /// The callback for when a <see cref="ClobFile"/> update command was completed (successfully or not).
        /// </summary>
        /// <param name="clobFile">The file that the command was run on.</param>
        /// <param name="result">Whether the file was updated successfully.</param>
        /// TODO: Remove this
        public void OnFileUpdateComplete(ClobFile clobFile, bool result)
        {
            Debug.Assert(clobFile.LocalFile != null, "The local file must not be null");

            this.lobsterNotificationIcon.ShowBalloonTip(
                Settings.Default.BalloonPopupDurationMS,
                result ? "Database Updated" : "Database Update Failed",
                clobFile.LocalFile.FileInfo.Name,
                result ? ToolTipIcon.Info : ToolTipIcon.Error);

            (result ? this.successSoundPlayer : this.failureSoundPlayer).Play();
        }

        /// <summary>
        /// The callback to refresh the file listings when a change on the file system is made.
        /// </summary>
        /// TODO: Remove this
        public void UpdateUIThread()
        {
            this.Invoke((MethodInvoker)delegate
            {
                this.RefreshClobLists();
            });
        }

        /// <summary>
        /// Fills the connectionListView with given list of <see cref="DatabaseConnection"/>s.
        /// </summary>
        /// <param name="connectionList">The list of connections the list should be populated with.</param>
        private void PopulateConnectionList(List<DatabaseConnection> connectionList)
        {
            this.connectionListView.Items.Clear();
            for (int i = 0; i < connectionList.Count; ++i)
            {
                DatabaseConnection connection = connectionList[i];
                ListViewItem item = new ListViewItem(connection.Name);
                string address = connection.Host;

                ListViewItem.ListViewSubItem[] subItems = new ListViewItem.ListViewSubItem[]
                {
                    new ListViewItem.ListViewSubItem(item, address),
                    new ListViewItem.ListViewSubItem(item, connection.Port),
                    new ListViewItem.ListViewSubItem(item, connection.SID),
                    new ListViewItem.ListViewSubItem(item, connection.CodeSource),
                    new ListViewItem.ListViewSubItem(item, connection.UsePooling.ToString()),
                };
                item.SubItems.AddRange(subItems);
                item.Tag = i;
                this.connectionListView.Items.Add(item);
            }

            this.connectionListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        /// <summary>
        /// Populates the directory tree view with the directory structure in the model.
        /// </summary>
        private void PopulateDirectoryTreeView()
        {
            Debug.Assert(this.model.CurrentConnection != null, "The current connection must not be null");
            this.fileTreeView.Nodes.Clear();

            // Use the folder name as the root element
            DatabaseConnection dbc = this.model.CurrentConnection;
            TreeNode rootNode = new TreeNode(Path.GetFileName(dbc.CodeSource) ?? "CodeSource", 0, 0);
            foreach (KeyValuePair<ClobType, ClobDirectory> pair in dbc.ClobTypeToDirectoryMap)
            {
                ClobDirectory clobDir = pair.Value;
                TreeNode dirNode = new TreeNode(clobDir.RootClobNode.DirInfo.Name, 0, 0);
                dirNode.Tag = clobDir.RootClobNode;
                this.PopulateTreeNode_r(dirNode);
                rootNode.Nodes.Add(dirNode);
            }

            this.fileTreeView.Nodes.Add(rootNode);
            rootNode.Expand();
        }

        /// <summary>
        /// Recursively populates the given tree node from the fileTreeView.
        /// </summary>
        /// <param name="treeNode">The tree node to recursively populate.</param>
        private void PopulateTreeNode_r(TreeNode treeNode)
        {
            ClobNode clobNode = (ClobNode)treeNode.Tag;
            foreach (ClobNode child in clobNode.ChildNodes)
            {
                TreeNode node = new TreeNode(child.DirInfo.Name);
                node.Tag = child;
                node.ImageKey = "folder";

                this.PopulateTreeNode_r(node);
                treeNode.Nodes.Add(node);
            }
        }

        /// <summary>
        /// Creates a <see cref="ListViewItem"/> for the given <see cref="ClobFile"/>
        /// with the columns: Filename / LastAccessTime / Status
        /// </summary>
        /// <param name="clobFile">The <see cref="ClobFile"/> to create a <see cref="ListViewItem"/> for.</param>
        /// <returns>The <see cref="ListViewItem"/> for the </returns>
        private ListViewItem GetListViewRowForClobFile(ClobFile clobFile)
        {
            Debug.Assert(
                clobFile.LocalFile != null || clobFile.DatabaseFile != null,
                "The file must be located locally or on the database");
            string filename;
            int imageHandle;
            string dateValue = string.Empty;
            string status;
            Color foreColour;

            if (clobFile.IsDbOnly)
            {
                filename = clobFile.DatabaseFile.Filename;
                imageHandle = 3;
                status = "Database Only";
                foreColour = Color.Gray;
            }
            else
            {
                filename = clobFile.LocalFile.FileInfo.Name;
                imageHandle = clobFile.IsEditable ? 1 : 2;
                dateValue = clobFile.LocalFile.FileInfo.LastAccessTime.ToShortDateString();
                if (clobFile.IsSynced)
                {
                    status = "Synchronised";
                    foreColour = Color.Black;
                }
                else if (clobFile.IsLocalOnly)
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

            ListViewItem item = new ListViewItem(filename, imageHandle);
            ListViewItem.ListViewSubItem[] subItems = new ListViewItem.ListViewSubItem[]
            {
                new ListViewItem.ListViewSubItem(item, dateValue),
                new ListViewItem.ListViewSubItem(item, status),
            };

            item.Tag = clobFile;
            item.ForeColor = foreColour;
            item.SubItems.AddRange(subItems);
            return item;
        }

        /// <summary>
        /// The event callback for when a user clicks on a node in the file tree view.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void fileTreeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            TreeNode newSelected = e.Node;
            fileListView.Items.Clear();
            ClobNode clobNode = (ClobNode)newSelected.Tag;
            if (clobNode == null)
            {
                return;
            }

            this.PopulateFileListView(clobNode);
            this.UpdateRibbonButtons();
        }

        /// <summary>
        /// Populates the file list view with the files that are located in the given <see cref="ClobNode"/>.
        /// If the ClobNode is the root node of the tree, database only files are also added to list view.
        /// </summary>
        /// <param name="clobNode">The ClobNode whose child files will be displayed.</param>
        private void PopulateFileListView(ClobNode clobNode)
        {
            Debug.Assert(clobNode != null, "The clob node must not be null");
            this.fileListView.Items.Clear();
            DirectoryInfo nodeDirInfo = clobNode.DirInfo;

            foreach (KeyValuePair<string, ClobFile> pair in clobNode.ClobFileMap)
            {
                ClobFile clobFile = pair.Value;
                Debug.Assert(clobFile.LocalFile != null, "The ClobFile must have a local file for it to be displayed in the file list view");
                if (clobFile.LocalFile != null)
                {
                    clobFile.LocalFile.FileInfo.Refresh();
                }

                ListViewItem item = this.GetListViewRowForClobFile(clobFile);
                this.fileListView.Items.Add(item);
            }

            // If it is the root node, also display any database only files
            if (clobNode.BaseClobDirectory.RootClobNode == clobNode)
            {
                foreach (ClobFile databaseFile in clobNode.BaseClobDirectory.DatabaseOnlyFiles)
                {
                    ListViewItem item = this.GetListViewRowForClobFile(databaseFile);
                    this.fileListView.Items.Add(item);
                }
            }

            this.fileListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        /// <summary>
        /// Populates the working file view with all unlocked files in the model.
        /// </summary>
        private void PopulateWorkingFileView()
        {
            this.workingFileList.Items.Clear();
            List<ClobFile> workingFiles = new List<ClobFile>();
            this.model.CurrentConnection.GetWorkingFiles(ref workingFiles);

            foreach (ClobFile clobFile in workingFiles)
            {
                ListViewItem item = this.GetListViewRowForClobFile(clobFile);
                this.workingFileList.Items.Add(item);
            }

            if (workingFiles.Count > 0)
            {
                this.workingFileList.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            }
        }

        /// <summary>
        /// The callback for when a user clicks anywhere in the file list view.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void fileListView_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && fileListView.FocusedItem.Bounds.Contains(e.Location) == true)
            {
                this.clobFileContextMenuStrip.Show(Cursor.Position);
                ClobFile clobFile = (ClobFile)fileListView.FocusedItem.Tag;
                this.ShowClobFileContextMenu(clobFile, this.fileListView);
            }
        }

        /// <summary>
        /// OPens a context menu strip for the given <see cref="ClobFile"/>.
        /// </summary>
        /// <param name="clobFile">The ClobFile that is the context of the menu.</param>
        /// <param name="fileListView">The list view that the context menu is opened for (tree or working tab list views)</param>
        private void ShowClobFileContextMenu(ClobFile clobFile, ListView fileListView)
        {
            clobFileContextMenuStrip.Tag = this.fileListView.FocusedItem;

            insertToolStripMenuItem.Enabled = clobFile.IsLocalOnly;
            clobToolStripMenuItem.Enabled = clobFile.IsSynced;

            // Only enable diffing if the file extension is within the diffable list
            string extension = Path.GetExtension(clobFile.LocalFile.FileInfo.Name);
            diffWithDatabaseToolStripMenuItem.Enabled = clobFile.IsSynced
                && Settings.Default.DiffableExtensions.Contains(extension);

            this.showInExplorerToolStripMenuItem.Enabled = clobFile.LocalFile != null;
            this.openDatabaseToolStripMenuItem.Enabled = clobFile.DatabaseFile != null;
        }

        /// <summary>
        /// The callback for when the insert file ribbon button is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments.</param>
        private void insertToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripItem item = (ToolStripItem)sender;
            ListViewItem listItem = (ListViewItem)item.GetCurrentParent().Tag;
            ClobFile clobFile = (ClobFile)listItem.Tag;
            this.InsertClobFile(clobFile);
        }

        /// <summary>
        /// Inserts the given ClobFile into the database and updates the UI.
        /// </summary>
        /// <param name="clobFile">The ClobFile to insert into the database.</param>
        private void InsertClobFile(ClobFile clobFile)
        {
            Debug.Assert(clobFile.IsLocalOnly, "A ClobFile that is already on the database cannot be inserted again.");
            Table table;
            ClobDirectory clobDir = clobFile.ParentClobNode.BaseClobDirectory;

            // If there is > 1 tables in this ClobType, ask the user for which one to use
            if (clobDir.ClobType.Tables.Count > 1)
            {
                TablePicker tablePicker = new TablePicker(clobDir.ClobType);
                DialogResult dialogResult = tablePicker.ShowDialog();
                if (dialogResult != DialogResult.OK)
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
            Column mimeTypeColumn = table.columns.Find(x => x.ColumnPurpose == Column.Purpose.MIME_TYPE);
            if (mimeTypeColumn != null)
            {
                DatatypePicker typePicker = new DatatypePicker(mimeTypeColumn);
                DialogResult dialogResult = typePicker.ShowDialog();
                if (dialogResult != DialogResult.OK)
                {
                    return;
                }

                mimeType = typePicker.datatypeComboBox.Text;
            }

            bool result = this.model.SendInsertClobMessage(clobFile, table, mimeType);
            if (result)
            {
                this.RefreshClobLists();
            }

            this.OnFileInsertComplete(clobFile, result);
        }

        /// <summary>
        /// The callback for when the show in explorer button is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void showInExplorerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripItem item = (ToolStripItem)sender;
            ListViewItem listItem = (ListViewItem)item.GetCurrentParent().Tag;
            ClobFile clobFile = (ClobFile)listItem.Tag;
            this.ExploreToClobFile(clobFile);
        }

        /// <summary>
        /// The callback for when the diff with database tool strip item is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void diffWithDatabaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripItem item = (ToolStripItem)sender;
            ListViewItem listItem = (ListViewItem)item.GetCurrentParent().Tag;
            ClobFile clobFile = (ClobFile)listItem.Tag;
            this.DiffClobFileWithDatabase(clobFile);
        }

        /// <summary>
        /// The callback for when the clob to database tool strip item is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void clobToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripItem item = (ToolStripItem)sender;
            ListViewItem listItem = (ListViewItem)item.GetCurrentParent().Tag;
            ClobFile clobFile = (ClobFile)listItem.Tag;
            this.ReclobFile(clobFile);
        }

        /// <summary>
        /// The callback for when the notification icon is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void lobsterNotificationIcon_MouseClick(object sender, MouseEventArgs e)
        {
            this.Show();
            this.BringToFront();
            this.WindowState = FormWindowState.Normal;
        }

        /// <summary>
        /// The callback for when the form is closed, performing one last clean up.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="args">The event arguments.</param>
        private void LobsterMain_FormClosed(object sender, FormClosedEventArgs args)
        {
            foreach (FileInfo tempFile in this.model.TempFileList)
            {
                try
                {
                    File.Delete(tempFile.FullName);
                    MessageLog.LogInfo("Temporary file deleted " + tempFile.FullName);
                }
                catch (IOException e)
                {
                    MessageLog.LogWarning("Failed to delete temporary file " + tempFile.FullName + " " + e);
                }
            }
        }

        /// <summary>
        /// The callack for when the tab is changed.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void MainTabControl_Selecting(object sender, TabControlCancelEventArgs e)
        {
            if (this.model.CurrentConnection == null)
            {
                e.Cancel = true;
                MessageBox.Show("You must first connect to a database. To make connect, first select a connection from the list, then press the Connect button.");
                return;
            }

            if (this.MainTabControl.SelectedTab == this.treeViewTab)
            {
                this.RefreshClobLists();
            }
            else if (this.MainTabControl.SelectedTab == this.workingFileTab)
            {
                this.RefreshClobLists();
            }

            this.refreshButton.Enabled = this.MainTabControl.SelectedTab == this.treeViewTab || this.MainTabControl.SelectedTab == this.workingFileTab;
        }

        /// <summary>
        /// The callback for when a user right clicks anywhere within the working file ListView.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void workingFileList_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (workingFileList.FocusedItem.Bounds.Contains(e.Location) == true)
                {
                    clobFileContextMenuStrip.Show(Cursor.Position);
                    ClobFile clobFile = (ClobFile)this.workingFileList.FocusedItem.Tag;
                    this.ShowClobFileContextMenu(clobFile, this.workingFileList);
                }
            }
        }

        /// <summary>
        /// The callback for when a user clicks the "Open Database Version" option in the file list context menu.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void openDatabaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripItem item = (ToolStripItem)sender;
            ListViewItem listItem = (ListViewItem)item.GetCurrentParent().Tag;
            ClobFile clobFile = (ClobFile)listItem.Tag;
            this.PullDatabaseFile(clobFile);
        }

        /// <summary>
        /// Updates the file lists and action ribbon, depending on which tab is open.
        /// </summary>
        private void RefreshClobLists()
        {
            if (this.MainTabControl.SelectedTab == this.workingFileTab)
            {
                this.PopulateWorkingFileView();
            }
            else if (this.MainTabControl.SelectedTab == this.treeViewTab)
            {
                if (this.fileTreeView.SelectedNode != null
                    && !this.fileTreeView.Nodes.Contains(this.fileTreeView.SelectedNode))
                {
                    this.PopulateFileListView((ClobNode)this.fileTreeView.SelectedNode.Tag);
                }
            }

            this.UpdateRibbonButtons();
        }

        /// <summary>
        /// The callback for when the user presses the connect button.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void connectButton_Click(object sender, EventArgs e)
        {
            Debug.Assert(this.connectionListView.SelectedItems.Count <= 1, "Multiple connections must not be selected");
            if (this.connectionListView.SelectedItems.Count == 0)
            {
                MessageBox.Show("You must first select a connection.");
                return;
            }

            LoadingForm loadingForm = new LoadingForm();
            loadingForm.Show();

            int configIndex = (int)this.connectionListView.SelectedItems[0].Tag;
            DatabaseConnection connection = this.model.ConnectionList[configIndex];
            bool result = this.model.SetDatabaseConnection(connection);
            if (result)
            {
                this.PopulateDirectoryTreeView();
                this.MainTabControl.SelectTab(1);
                this.refreshButton.Enabled = true;
            }

            loadingForm.Close();
        }

        /// <summary>
        /// The callback for when the refresh files button is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void refreshButtonClick(object sender, EventArgs e)
        {
            if (this.model.CurrentConnection != null)
            {
                this.model.RebuildLocalAndDatabaseFileLists();
                this.RefreshClobLists();
            }
        }

        /// <summary>
        /// The callback for when the selected item of the connection list is changed.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void connectionListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.UpdateRibbonButtons();
            this.UpdateConnectionButtons();
        }

        /// <summary>
        /// UPdates the enabledness of the connection editor buttons.
        /// </summary>
        private void UpdateConnectionButtons()
        {
            this.removeConnectionButton.Enabled = this.editConnectionButton.Enabled
                = this.connectionListView.SelectedItems.Count > 0;
        }

        /// <summary>
        /// The callback for when the selected file in the file list view changes.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void fileListView_SelectedIndexChanged(object sender, EventArgs e)
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

            // Only enable the Reclob buttons if the file is synchronised
            this.reclobButton.Enabled = this.diffWithDBButton.Enabled = selectedFile != null && selectedFile.IsSynced;

            // Only enable diffing if the file extension is within the diffable list
            if (this.diffWithDBButton.Enabled)
            {
                string extension = Path.GetExtension(selectedFile.LocalFile.FileInfo.Name);
                this.diffWithDBButton.Enabled = Settings.Default.DiffableExtensions.Contains(extension);
            }

            // Only enable the Explore To button if the file exists locally
            this.exploreButton.Enabled = selectedFile != null && selectedFile.LocalFile != null;

            // Only enable the Pull button if the file exists on the database
            this.pullDBFileButton.Enabled = selectedFile != null && selectedFile.DatabaseFile != null;

            // Only enable the connection button if on the connection page
            this.connectButton.Enabled = this.MainTabControl.SelectedTab == this.connectionTabPage && this.connectionListView.SelectedItems.Count > 0;
        }

        /// <summary>
        /// The callback for when the insert file is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void insertButton_Click(object sender, EventArgs e)
        {
            ClobFile clobFile = this.GetCurrentlySelectedClobFile();
            if (clobFile != null)
            {
                this.InsertClobFile(clobFile);
            }
            else
            {
                this.ShowNoFileSelectedMessage();
            }
        }

        /// <summary>
        /// Displays a generic warning message for when the user attempts to perform
        /// a ClobFile action with no file selected
        /// </summary>
        private void ShowNoFileSelectedMessage()
        {
            MessageBox.Show(
                "No files selected",
                "You must select a file before you can perform this action.",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        /// <summary>
        /// The callback for when the reclob button is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void reclobButton_Click(object sender, EventArgs e)
        {
            ClobFile clobFile = this.GetCurrentlySelectedClobFile();
            if (clobFile != null)
            {
                this.ReclobFile(clobFile);
            }
            else
            {
                this.ShowNoFileSelectedMessage();
            }
        }

        /// <summary>
        /// Pushes the contents of the given ClobFile to the database.
        /// </summary>
        /// <param name="clobFile">The ClobFile to push to the database.</param>
        private void ReclobFile(ClobFile clobFile)
        {
            Debug.Assert(clobFile.IsSynced, "The file must be synchronised to reclob it");
            if (clobFile.LocalFile.FileInfo.IsReadOnly)
            {
                DialogResult result = MessageBox.Show(
                    clobFile.LocalFile.FileInfo.Name + " is locked. Are you sure you want to clob it?",
                    "File is Locked",
                    MessageBoxButtons.OKCancel);

                if (result != DialogResult.OK)
                {
                    return;
                }
            }

            this.model.SendUpdateClobMessage(clobFile);
        }

        /// <summary>
        /// The callback for when the user clicks the show in explorer button.
        /// OPens the location of the file in a new instance of windows explorer.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void exploreButton_click(object sender, EventArgs e)
        {
            ClobFile clobFile = this.GetCurrentlySelectedClobFile();
            if (clobFile != null)
            {
                this.ExploreToClobFile(clobFile);
            }
            else
            {
                this.ShowNoFileSelectedMessage();
            }
        }

        /// <summary>
        /// Opens a new instance of windows explorer at the location of the given file.
        /// </summary>
        /// <param name="clobFIle">The file to explore to.</param>
        private void ExploreToClobFile(ClobFile clobFIle)
        {
            Debug.Assert(clobFIle.LocalFile != null, "The file must exist locally to be able to explore to it");

            // Windows Explorer command line arguments: https://support.microsoft.com/en-us/kb/152457
            Process.Start("explorer", "/select," + clobFIle.LocalFile.FileInfo.FullName);
        }

        /// <summary>
        /// The callback for when the selected index of the working file list changes.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void workingFileList_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.UpdateRibbonButtons();
        }

        /// <summary>
        /// The callback for when the diff ribbon button is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void diffWithDBButton_Click(object sender, EventArgs e)
        {
            ClobFile clobFile = this.GetCurrentlySelectedClobFile();
            if (clobFile != null)
            {
                this.DiffClobFileWithDatabase(clobFile);
            }
            else
            {
                this.ShowNoFileSelectedMessage();
            }
        }

        /// <summary>
        /// Diffs the local file of the given <see cref="ClobFIle"/> with its corresponding database version 
        /// and opens the result in a user selected diffing program.
        /// </summary>
        /// <param name="clobFile">The file to diff.</param>
        private void DiffClobFileWithDatabase(ClobFile clobFile)
        {
            Debug.Assert(clobFile.IsSynced, "The ClobFile must be synchronised to diff it.");

            FileInfo tempFile = this.model.SendDownloadClobDataToFileMessage(clobFile);
            if (tempFile == null)
            {
                Common.ShowErrorMessage("Diff with Database", "An error ocurred when fetching the file data.");
                return;
            }

            MessageLog.LogInfo("Temporary file created: " + tempFile.FullName);
            this.model.TempFileList.Add(tempFile);

            string args = string.Format(
                Settings.Default.DiffProgramArguments,
                tempFile.FullName,
                clobFile.LocalFile.FileInfo.FullName);

            Process.Start(Settings.Default.DiffProgramName, args);
        }

        /// <summary>
        /// The callback for when the pull file button is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void pullDBFileButton_Click(object sender, EventArgs e)
        {
            ClobFile clobFile = this.GetCurrentlySelectedClobFile();
            if (clobFile != null)
            {
                this.PullDatabaseFile(clobFile);
            }
            else
            {
                this.ShowNoFileSelectedMessage();
            }
        }

        /// <summary>
        /// Pulls the given ClobFile data from the database, stores it in a temp file and then executes that file.
        /// </summary>
        /// <param name="clobFile">The ClobFile to download.</param>
        private void PullDatabaseFile(ClobFile clobFile)
        {
            Debug.Assert(clobFile.DatabaseFile != null, "The file must exist on the database");
            FileInfo tempFile = this.model.SendDownloadClobDataToFileMessage(clobFile);
            if (tempFile == null)
            {
                Common.ShowErrorMessage("Open Database", "An error ocurred when fetching the file data.");
                return;
            }

            MessageLog.LogInfo("Temporary file created: " + tempFile.FullName);
            this.model.TempFileList.Add(tempFile);

            Process.Start(tempFile.FullName);
        }

        /// <summary>
        /// Gets the ClobFile that is currently selected on the file list tab or the working file list tab 
        /// (depending on which is currently open).
        /// </summary>
        /// <returns>The ClobFile that is selected if one is, otherwise null.</returns>
        private ClobFile GetCurrentlySelectedClobFile()
        {
            ClobFile selectedFile = null;

            if (this.MainTabControl.SelectedTab == this.treeViewTab
                && this.fileListView.SelectedItems.Count > 0)
            {
                selectedFile = (ClobFile)this.fileListView.SelectedItems[0].Tag;
            }
            else if (this.MainTabControl.SelectedTab == this.workingFileTab
                && this.workingFileList.SelectedItems.Count > 0)
            {
                selectedFile = (ClobFile)this.workingFileList.SelectedItems[0].Tag;
            }

            return selectedFile;
        }

        /// <summary>
        /// The callback for when the edit connection button is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void editConnectionButton_Click(object sender, EventArgs e)
        {
            Debug.Assert(this.connectionListView.SelectedItems.Count <= 1, "Only a single connection should be selected");
            if (this.connectionListView.SelectedItems.Count == 0)
            {
                MessageBox.Show("You must first select a connection.");
                return;
            }

            int configIndex = (int)this.connectionListView.SelectedItems[0].Tag;
            DatabaseConnection connectionRef = this.model.ConnectionList[configIndex];

            EditDatabaseConnection editForm = new EditDatabaseConnection(connectionRef, false);
            DialogResult result = editForm.ShowDialog();
            this.model.ConnectionList[configIndex] = editForm.OriginalObject;
            this.PopulateConnectionList(this.model.ConnectionList);
        }

        /// <summary>
        /// The callback for when the new connection button is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void newConnectionButton_Click(object sender, EventArgs e)
        {
            DatabaseConnection newConnection = new DatabaseConnection();
            newConnection.ParentModel = this.model;
            newConnection.FileLocation = Path.Combine(Settings.Default.ConnectionDir, "NewConnection.xml");
            EditDatabaseConnection editForm = new EditDatabaseConnection(newConnection, true);
            DialogResult result = editForm.ShowDialog();
            if (result == DialogResult.OK)
            {
                this.model.ConnectionList.Add(editForm.OriginalObject);
                this.PopulateConnectionList(this.model.ConnectionList);
            }
        }

        /// <summary>
        /// Callback for when the remove connection button is clicked.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void removeConnectionButton_Click(object sender, EventArgs e)
        {
            Debug.Assert(this.connectionListView.SelectedItems.Count <= 1, "Only a single connection should be selected");
            if (this.connectionListView.SelectedItems.Count == 0)
            {
                MessageBox.Show("You must first select a connection.");
                return;
            }

            int configIndex = (int)this.connectionListView.SelectedItems[0].Tag;
            DatabaseConnection databaseConnection = this.model.ConnectionList[configIndex];

            DialogResult result = MessageBox.Show(
                "Are you sure you want to permanently delete the connection " + databaseConnection.Name ?? "New Connection" + "?",
                "Remove Connection",
                MessageBoxButtons.OKCancel);

            if (result == DialogResult.OK)
            {
                File.Delete(databaseConnection.FileLocation);
                this.model.ConnectionList.RemoveAt(configIndex);
                this.PopulateConnectionList(this.model.ConnectionList);
            }
        }
    }
}
