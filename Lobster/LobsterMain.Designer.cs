using System;

namespace Lobster
{
    partial class LobsterMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LobsterMain));
            this.treeViewTab = new System.Windows.Forms.TabPage();
            this.treeViewSplitContainer = new System.Windows.Forms.SplitContainer();
            this.fileTreeView = new Lobster.NativeTreeView();
            this.fileViewImageList = new System.Windows.Forms.ImageList(this.components);
            this.fileListView = new Lobster.NativeListView();
            this.nameColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lastModifiedColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.statusColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.MainTabControl = new System.Windows.Forms.TabControl();
            this.connectionTabPage = new System.Windows.Forms.TabPage();
            this.editConnectionButton = new System.Windows.Forms.Button();
            this.removeConnectionButton = new System.Windows.Forms.Button();
            this.newConnectionButton = new System.Windows.Forms.Button();
            this.connectionListView = new Lobster.NativeListView();
            this.connectionNameColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.connectionHostColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.connectionPortColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.connectionSIDColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.connectionCodeSourceColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.connectionPoolingColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.workingFileTab = new System.Windows.Forms.TabPage();
            this.workingFileList = new Lobster.NativeListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.connectButton = new System.Windows.Forms.Button();
            this.ribbonImageList = new System.Windows.Forms.ImageList(this.components);
            this.clobFileContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.insertToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clobToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showInExplorerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.diffWithDatabaseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openDatabaseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lobsterNotificationIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.refreshButton = new System.Windows.Forms.Button();
            this.insertButton = new System.Windows.Forms.Button();
            this.reclobButton = new System.Windows.Forms.Button();
            this.exploreButton = new System.Windows.Forms.Button();
            this.diffWithDBButton = new System.Windows.Forms.Button();
            this.pullDBFileButton = new System.Windows.Forms.Button();
            this.treeViewTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.treeViewSplitContainer)).BeginInit();
            this.treeViewSplitContainer.Panel1.SuspendLayout();
            this.treeViewSplitContainer.Panel2.SuspendLayout();
            this.treeViewSplitContainer.SuspendLayout();
            this.MainTabControl.SuspendLayout();
            this.connectionTabPage.SuspendLayout();
            this.workingFileTab.SuspendLayout();
            this.clobFileContextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // treeViewTab
            // 
            this.treeViewTab.Controls.Add(this.treeViewSplitContainer);
            this.treeViewTab.Location = new System.Drawing.Point(4, 29);
            this.treeViewTab.Name = "treeViewTab";
            this.treeViewTab.Padding = new System.Windows.Forms.Padding(3);
            this.treeViewTab.Size = new System.Drawing.Size(1138, 567);
            this.treeViewTab.TabIndex = 0;
            this.treeViewTab.Text = "Tree View";
            this.treeViewTab.UseVisualStyleBackColor = true;
            // 
            // treeViewSplitContainer
            // 
            this.treeViewSplitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.treeViewSplitContainer.Location = new System.Drawing.Point(3, 3);
            this.treeViewSplitContainer.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.treeViewSplitContainer.Name = "treeViewSplitContainer";
            // 
            // treeViewSplitContainer.Panel1
            // 
            this.treeViewSplitContainer.Panel1.Controls.Add(this.fileTreeView);
            // 
            // treeViewSplitContainer.Panel2
            // 
            this.treeViewSplitContainer.Panel2.Controls.Add(this.fileListView);
            this.treeViewSplitContainer.Size = new System.Drawing.Size(1132, 561);
            this.treeViewSplitContainer.SplitterDistance = 313;
            this.treeViewSplitContainer.SplitterWidth = 6;
            this.treeViewSplitContainer.TabIndex = 0;
            // 
            // fileTreeView
            // 
            this.fileTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fileTreeView.FullRowSelect = true;
            this.fileTreeView.HideSelection = false;
            this.fileTreeView.ImageIndex = 0;
            this.fileTreeView.ImageList = this.fileViewImageList;
            this.fileTreeView.Location = new System.Drawing.Point(0, 0);
            this.fileTreeView.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.fileTreeView.Name = "fileTreeView";
            this.fileTreeView.SelectedImageIndex = 0;
            this.fileTreeView.ShowNodeToolTips = true;
            this.fileTreeView.Size = new System.Drawing.Size(313, 561);
            this.fileTreeView.TabIndex = 0;
            this.fileTreeView.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.fileTreeView_NodeMouseClick);
            // 
            // fileViewImageList
            // 
            this.fileViewImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("fileViewImageList.ImageStream")));
            this.fileViewImageList.TransparentColor = System.Drawing.Color.Transparent;
            this.fileViewImageList.Images.SetKeyName(0, "folder");
            this.fileViewImageList.Images.SetKeyName(1, "Page.png");
            this.fileViewImageList.Images.SetKeyName(2, "lock");
            this.fileViewImageList.Images.SetKeyName(3, "attach_file");
            // 
            // fileListView
            // 
            this.fileListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.nameColumn,
            this.lastModifiedColumn,
            this.statusColumn});
            this.fileListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fileListView.FullRowSelect = true;
            this.fileListView.HideSelection = false;
            this.fileListView.Location = new System.Drawing.Point(0, 0);
            this.fileListView.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.fileListView.MultiSelect = false;
            this.fileListView.Name = "fileListView";
            this.fileListView.Size = new System.Drawing.Size(813, 561);
            this.fileListView.SmallImageList = this.fileViewImageList;
            this.fileListView.TabIndex = 0;
            this.fileListView.UseCompatibleStateImageBehavior = false;
            this.fileListView.View = System.Windows.Forms.View.Details;
            this.fileListView.SelectedIndexChanged += new System.EventHandler(this.fileListView_SelectedIndexChanged);
            this.fileListView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.fileListView_MouseClick);
            // 
            // nameColumn
            // 
            this.nameColumn.Text = "Name";
            // 
            // lastModifiedColumn
            // 
            this.lastModifiedColumn.Text = "Last Modified";
            // 
            // statusColumn
            // 
            this.statusColumn.Text = "Status";
            // 
            // MainTabControl
            // 
            this.MainTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MainTabControl.Controls.Add(this.connectionTabPage);
            this.MainTabControl.Controls.Add(this.treeViewTab);
            this.MainTabControl.Controls.Add(this.workingFileTab);
            this.MainTabControl.Location = new System.Drawing.Point(0, 100);
            this.MainTabControl.Name = "MainTabControl";
            this.MainTabControl.SelectedIndex = 0;
            this.MainTabControl.Size = new System.Drawing.Size(1146, 600);
            this.MainTabControl.TabIndex = 1;
            this.MainTabControl.Selecting += new System.Windows.Forms.TabControlCancelEventHandler(this.MainTabControl_Selecting);
            // 
            // connectionTabPage
            // 
            this.connectionTabPage.Controls.Add(this.editConnectionButton);
            this.connectionTabPage.Controls.Add(this.removeConnectionButton);
            this.connectionTabPage.Controls.Add(this.newConnectionButton);
            this.connectionTabPage.Controls.Add(this.connectionListView);
            this.connectionTabPage.Location = new System.Drawing.Point(4, 29);
            this.connectionTabPage.Name = "connectionTabPage";
            this.connectionTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.connectionTabPage.Size = new System.Drawing.Size(1138, 567);
            this.connectionTabPage.TabIndex = 2;
            this.connectionTabPage.Text = "Connections";
            this.connectionTabPage.UseVisualStyleBackColor = true;
            // 
            // editConnectionButton
            // 
            this.editConnectionButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.editConnectionButton.Enabled = false;
            this.editConnectionButton.Location = new System.Drawing.Point(830, 529);
            this.editConnectionButton.Name = "editConnectionButton";
            this.editConnectionButton.Size = new System.Drawing.Size(96, 32);
            this.editConnectionButton.TabIndex = 18;
            this.editConnectionButton.Text = "Edit";
            this.editConnectionButton.UseVisualStyleBackColor = true;
            this.editConnectionButton.Click += new System.EventHandler(this.editConnectionButton_Click);
            // 
            // removeConnectionButton
            // 
            this.removeConnectionButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.removeConnectionButton.Enabled = false;
            this.removeConnectionButton.Location = new System.Drawing.Point(1034, 529);
            this.removeConnectionButton.Name = "removeConnectionButton";
            this.removeConnectionButton.Size = new System.Drawing.Size(96, 32);
            this.removeConnectionButton.TabIndex = 17;
            this.removeConnectionButton.Text = "Remove";
            this.removeConnectionButton.UseVisualStyleBackColor = true;
            this.removeConnectionButton.Click += new System.EventHandler(this.removeConnectionButton_Click);
            // 
            // newConnectionButton
            // 
            this.newConnectionButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.newConnectionButton.Location = new System.Drawing.Point(932, 529);
            this.newConnectionButton.Name = "newConnectionButton";
            this.newConnectionButton.Size = new System.Drawing.Size(96, 32);
            this.newConnectionButton.TabIndex = 16;
            this.newConnectionButton.Text = "New";
            this.newConnectionButton.UseVisualStyleBackColor = true;
            this.newConnectionButton.Click += new System.EventHandler(this.newConnectionButton_Click);
            // 
            // connectionListView
            // 
            this.connectionListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.connectionListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.connectionNameColumn,
            this.connectionHostColumn,
            this.connectionPortColumn,
            this.connectionSIDColumn,
            this.connectionCodeSourceColumn,
            this.connectionPoolingColumn});
            this.connectionListView.FullRowSelect = true;
            this.connectionListView.HideSelection = false;
            this.connectionListView.Location = new System.Drawing.Point(8, 6);
            this.connectionListView.MultiSelect = false;
            this.connectionListView.Name = "connectionListView";
            this.connectionListView.Size = new System.Drawing.Size(1122, 519);
            this.connectionListView.TabIndex = 0;
            this.connectionListView.UseCompatibleStateImageBehavior = false;
            this.connectionListView.View = System.Windows.Forms.View.Details;
            this.connectionListView.SelectedIndexChanged += new System.EventHandler(this.connectionListView_SelectedIndexChanged);
            // 
            // connectionNameColumn
            // 
            this.connectionNameColumn.Text = "Name";
            this.connectionNameColumn.Width = 78;
            // 
            // connectionHostColumn
            // 
            this.connectionHostColumn.Text = "Host";
            this.connectionHostColumn.Width = 81;
            // 
            // connectionPortColumn
            // 
            this.connectionPortColumn.Text = "Port";
            // 
            // connectionSIDColumn
            // 
            this.connectionSIDColumn.Text = "SID";
            // 
            // connectionCodeSourceColumn
            // 
            this.connectionCodeSourceColumn.Text = "Codesource Directory";
            this.connectionCodeSourceColumn.Width = 181;
            // 
            // connectionPoolingColumn
            // 
            this.connectionPoolingColumn.Text = "Pooling";
            this.connectionPoolingColumn.Width = 83;
            // 
            // workingFileTab
            // 
            this.workingFileTab.Controls.Add(this.workingFileList);
            this.workingFileTab.Location = new System.Drawing.Point(4, 29);
            this.workingFileTab.Name = "workingFileTab";
            this.workingFileTab.Padding = new System.Windows.Forms.Padding(3);
            this.workingFileTab.Size = new System.Drawing.Size(1138, 567);
            this.workingFileTab.TabIndex = 1;
            this.workingFileTab.Text = "Working Files";
            this.workingFileTab.UseVisualStyleBackColor = true;
            // 
            // workingFileList
            // 
            this.workingFileList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.workingFileList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.workingFileList.FullRowSelect = true;
            this.workingFileList.Location = new System.Drawing.Point(3, 3);
            this.workingFileList.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.workingFileList.MultiSelect = false;
            this.workingFileList.Name = "workingFileList";
            this.workingFileList.Size = new System.Drawing.Size(1132, 561);
            this.workingFileList.SmallImageList = this.fileViewImageList;
            this.workingFileList.TabIndex = 1;
            this.workingFileList.UseCompatibleStateImageBehavior = false;
            this.workingFileList.View = System.Windows.Forms.View.Details;
            this.workingFileList.SelectedIndexChanged += new System.EventHandler(this.workingFileList_SelectedIndexChanged);
            this.workingFileList.MouseClick += new System.Windows.Forms.MouseEventHandler(this.workingFileList_MouseClick);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Name";
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Last Modified";
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Status";
            // 
            // connectButton
            // 
            this.connectButton.Enabled = false;
            this.connectButton.ImageKey = "connect";
            this.connectButton.ImageList = this.ribbonImageList;
            this.connectButton.Location = new System.Drawing.Point(12, 12);
            this.connectButton.Name = "connectButton";
            this.connectButton.Size = new System.Drawing.Size(82, 82);
            this.connectButton.TabIndex = 1;
            this.connectButton.Text = "Connect";
            this.connectButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.connectButton.UseVisualStyleBackColor = true;
            this.connectButton.Click += new System.EventHandler(this.connectButton_Click);
            // 
            // ribbonImageList
            // 
            this.ribbonImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ribbonImageList.ImageStream")));
            this.ribbonImageList.TransparentColor = System.Drawing.Color.Transparent;
            this.ribbonImageList.Images.SetKeyName(0, "refresh");
            this.ribbonImageList.Images.SetKeyName(1, "connect");
            this.ribbonImageList.Images.SetKeyName(2, "insert");
            this.ribbonImageList.Images.SetKeyName(3, "reclob");
            this.ribbonImageList.Images.SetKeyName(4, "diff");
            this.ribbonImageList.Images.SetKeyName(5, "pull");
            this.ribbonImageList.Images.SetKeyName(6, "exploerTo");
            // 
            // clobFileContextMenuStrip
            // 
            this.clobFileContextMenuStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.clobFileContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.insertToolStripMenuItem,
            this.clobToolStripMenuItem,
            this.showInExplorerToolStripMenuItem,
            this.diffWithDatabaseToolStripMenuItem,
            this.openDatabaseToolStripMenuItem});
            this.clobFileContextMenuStrip.Name = "contextMenuStrip1";
            this.clobFileContextMenuStrip.Size = new System.Drawing.Size(285, 154);
            // 
            // insertToolStripMenuItem
            // 
            this.insertToolStripMenuItem.Name = "insertToolStripMenuItem";
            this.insertToolStripMenuItem.Size = new System.Drawing.Size(284, 30);
            this.insertToolStripMenuItem.Text = "Insert Into Database";
            this.insertToolStripMenuItem.Click += new System.EventHandler(this.insertToolStripMenuItem_Click);
            // 
            // clobToolStripMenuItem
            // 
            this.clobToolStripMenuItem.Name = "clobToolStripMenuItem";
            this.clobToolStripMenuItem.Size = new System.Drawing.Size(284, 30);
            this.clobToolStripMenuItem.Text = "Force Clob";
            this.clobToolStripMenuItem.Click += new System.EventHandler(this.clobToolStripMenuItem_Click);
            // 
            // showInExplorerToolStripMenuItem
            // 
            this.showInExplorerToolStripMenuItem.Name = "showInExplorerToolStripMenuItem";
            this.showInExplorerToolStripMenuItem.Size = new System.Drawing.Size(284, 30);
            this.showInExplorerToolStripMenuItem.Text = "Show In Explorer";
            this.showInExplorerToolStripMenuItem.Click += new System.EventHandler(this.showInExplorerToolStripMenuItem_Click);
            // 
            // diffWithDatabaseToolStripMenuItem
            // 
            this.diffWithDatabaseToolStripMenuItem.Name = "diffWithDatabaseToolStripMenuItem";
            this.diffWithDatabaseToolStripMenuItem.Size = new System.Drawing.Size(284, 30);
            this.diffWithDatabaseToolStripMenuItem.Text = "Diff with Database";
            this.diffWithDatabaseToolStripMenuItem.Click += new System.EventHandler(this.diffWithDatabaseToolStripMenuItem_Click);
            // 
            // openDatabaseToolStripMenuItem
            // 
            this.openDatabaseToolStripMenuItem.Name = "openDatabaseToolStripMenuItem";
            this.openDatabaseToolStripMenuItem.ShortcutKeyDisplayString = "";
            this.openDatabaseToolStripMenuItem.Size = new System.Drawing.Size(284, 30);
            this.openDatabaseToolStripMenuItem.Text = "Open Database Version";
            this.openDatabaseToolStripMenuItem.Click += new System.EventHandler(this.openDatabaseToolStripMenuItem_Click);
            // 
            // lobsterNotificationIcon
            // 
            this.lobsterNotificationIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("lobsterNotificationIcon.Icon")));
            this.lobsterNotificationIcon.Text = "Lobster";
            this.lobsterNotificationIcon.Visible = true;
            this.lobsterNotificationIcon.MouseClick += new System.Windows.Forms.MouseEventHandler(this.lobsterNotificationIcon_MouseClick);
            // 
            // refreshButton
            // 
            this.refreshButton.AccessibleDescription = "";
            this.refreshButton.Enabled = false;
            this.refreshButton.ImageKey = "refresh";
            this.refreshButton.ImageList = this.ribbonImageList;
            this.refreshButton.Location = new System.Drawing.Point(100, 12);
            this.refreshButton.Name = "refreshButton";
            this.refreshButton.Size = new System.Drawing.Size(82, 82);
            this.refreshButton.TabIndex = 3;
            this.refreshButton.Text = "Refresh";
            this.refreshButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.refreshButton.UseVisualStyleBackColor = true;
            this.refreshButton.Click += new System.EventHandler(this.refreshButtonClick);
            // 
            // insertButton
            // 
            this.insertButton.Enabled = false;
            this.insertButton.ImageKey = "insert";
            this.insertButton.ImageList = this.ribbonImageList;
            this.insertButton.Location = new System.Drawing.Point(188, 12);
            this.insertButton.Name = "insertButton";
            this.insertButton.Size = new System.Drawing.Size(82, 82);
            this.insertButton.TabIndex = 4;
            this.insertButton.Text = "Insert";
            this.insertButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.insertButton.UseVisualStyleBackColor = true;
            this.insertButton.Click += new System.EventHandler(this.insertButton_Click);
            // 
            // reclobButton
            // 
            this.reclobButton.Enabled = false;
            this.reclobButton.ImageKey = "reclob";
            this.reclobButton.ImageList = this.ribbonImageList;
            this.reclobButton.Location = new System.Drawing.Point(276, 12);
            this.reclobButton.Name = "reclobButton";
            this.reclobButton.Size = new System.Drawing.Size(82, 82);
            this.reclobButton.TabIndex = 5;
            this.reclobButton.Text = "Reclob";
            this.reclobButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.reclobButton.UseVisualStyleBackColor = true;
            this.reclobButton.Click += new System.EventHandler(this.reclobButton_Click);
            // 
            // exploreButton
            // 
            this.exploreButton.Enabled = false;
            this.exploreButton.ImageKey = "exploerTo";
            this.exploreButton.ImageList = this.ribbonImageList;
            this.exploreButton.Location = new System.Drawing.Point(365, 13);
            this.exploreButton.Name = "exploreButton";
            this.exploreButton.Size = new System.Drawing.Size(82, 82);
            this.exploreButton.TabIndex = 6;
            this.exploreButton.Text = "Explore";
            this.exploreButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.exploreButton.UseVisualStyleBackColor = true;
            this.exploreButton.Click += new System.EventHandler(this.exploreButton_click);
            // 
            // diffWithDBButton
            // 
            this.diffWithDBButton.Enabled = false;
            this.diffWithDBButton.ImageKey = "diff";
            this.diffWithDBButton.ImageList = this.ribbonImageList;
            this.diffWithDBButton.Location = new System.Drawing.Point(454, 13);
            this.diffWithDBButton.Name = "diffWithDBButton";
            this.diffWithDBButton.Size = new System.Drawing.Size(82, 82);
            this.diffWithDBButton.TabIndex = 7;
            this.diffWithDBButton.Text = "Diff";
            this.diffWithDBButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.diffWithDBButton.UseVisualStyleBackColor = true;
            this.diffWithDBButton.Click += new System.EventHandler(this.diffWithDBButton_Click);
            // 
            // pullDBFileButton
            // 
            this.pullDBFileButton.Enabled = false;
            this.pullDBFileButton.ImageKey = "pull";
            this.pullDBFileButton.ImageList = this.ribbonImageList;
            this.pullDBFileButton.Location = new System.Drawing.Point(542, 13);
            this.pullDBFileButton.Name = "pullDBFileButton";
            this.pullDBFileButton.Size = new System.Drawing.Size(82, 82);
            this.pullDBFileButton.TabIndex = 8;
            this.pullDBFileButton.Text = "Pull";
            this.pullDBFileButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.pullDBFileButton.UseVisualStyleBackColor = true;
            this.pullDBFileButton.Click += new System.EventHandler(this.pullDBFileButton_Click);
            // 
            // LobsterMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1146, 712);
            this.Controls.Add(this.pullDBFileButton);
            this.Controls.Add(this.diffWithDBButton);
            this.Controls.Add(this.exploreButton);
            this.Controls.Add(this.reclobButton);
            this.Controls.Add(this.insertButton);
            this.Controls.Add(this.connectButton);
            this.Controls.Add(this.refreshButton);
            this.Controls.Add(this.MainTabControl);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "LobsterMain";
            this.Text = "Lobster";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.LobsterMain_FormClosed);
            this.treeViewTab.ResumeLayout(false);
            this.treeViewSplitContainer.Panel1.ResumeLayout(false);
            this.treeViewSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.treeViewSplitContainer)).EndInit();
            this.treeViewSplitContainer.ResumeLayout(false);
            this.MainTabControl.ResumeLayout(false);
            this.connectionTabPage.ResumeLayout(false);
            this.workingFileTab.ResumeLayout(false);
            this.clobFileContextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabPage treeViewTab;
        private System.Windows.Forms.TabControl MainTabControl;
        private System.Windows.Forms.SplitContainer treeViewSplitContainer;
        private System.Windows.Forms.ImageList fileViewImageList;
        private System.Windows.Forms.ColumnHeader nameColumn;
        private System.Windows.Forms.ColumnHeader lastModifiedColumn;
        private System.Windows.Forms.ContextMenuStrip clobFileContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem insertToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clobToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showInExplorerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem diffWithDatabaseToolStripMenuItem;
        private System.Windows.Forms.ColumnHeader statusColumn;
        private System.Windows.Forms.NotifyIcon lobsterNotificationIcon;
        private System.Windows.Forms.TabPage workingFileTab;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ToolStripMenuItem openDatabaseToolStripMenuItem;
        private System.Windows.Forms.TabPage connectionTabPage;
        private System.Windows.Forms.ListView connectionListView;
        private System.Windows.Forms.ColumnHeader connectionNameColumn;
        private System.Windows.Forms.ColumnHeader connectionHostColumn;
        private System.Windows.Forms.ColumnHeader connectionPortColumn;
        private System.Windows.Forms.ColumnHeader connectionSIDColumn;
        private System.Windows.Forms.ColumnHeader connectionCodeSourceColumn;
        private System.Windows.Forms.ColumnHeader connectionPoolingColumn;
        private System.Windows.Forms.Button connectButton;
        private System.Windows.Forms.Button refreshButton;
        private System.Windows.Forms.ImageList ribbonImageList;
        private System.Windows.Forms.Button insertButton;
        private System.Windows.Forms.Button reclobButton;
        private System.Windows.Forms.Button exploreButton;
        private System.Windows.Forms.Button diffWithDBButton;
        private System.Windows.Forms.Button pullDBFileButton;
        private NativeTreeView fileTreeView;
        private NativeListView workingFileList;
        private System.Windows.Forms.Button editConnectionButton;
        private System.Windows.Forms.Button removeConnectionButton;
        private System.Windows.Forms.Button newConnectionButton;
        private NativeListView fileListView;
    }
}

