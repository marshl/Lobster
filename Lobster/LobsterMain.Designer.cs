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
            this.fileTreeView = new System.Windows.Forms.TreeView();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.fileListView = new System.Windows.Forms.ListView();
            this.nameColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lastModifiedColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.statusColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.MainTabControl = new System.Windows.Forms.TabControl();
            this.workingFileTab = new System.Windows.Forms.TabPage();
            this.workingFileList = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.insertToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clobToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showInExplorerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.diffWithDatabaseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.treeViewTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.treeViewSplitContainer)).BeginInit();
            this.treeViewSplitContainer.Panel1.SuspendLayout();
            this.treeViewSplitContainer.Panel2.SuspendLayout();
            this.treeViewSplitContainer.SuspendLayout();
            this.MainTabControl.SuspendLayout();
            this.workingFileTab.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // treeViewTab
            // 
            this.treeViewTab.Controls.Add(this.treeViewSplitContainer);
            this.treeViewTab.Location = new System.Drawing.Point(4, 29);
            this.treeViewTab.Name = "treeViewTab";
            this.treeViewTab.Padding = new System.Windows.Forms.Padding(3);
            this.treeViewTab.Size = new System.Drawing.Size(1114, 655);
            this.treeViewTab.TabIndex = 0;
            this.treeViewTab.Text = "Tree View";
            this.treeViewTab.UseVisualStyleBackColor = true;
            // 
            // treeViewSplitContainer
            // 
            this.treeViewSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
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
            this.treeViewSplitContainer.Size = new System.Drawing.Size(1108, 649);
            this.treeViewSplitContainer.SplitterDistance = 280;
            this.treeViewSplitContainer.SplitterWidth = 6;
            this.treeViewSplitContainer.TabIndex = 0;
            // 
            // fileTreeView
            // 
            this.fileTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fileTreeView.ImageIndex = 0;
            this.fileTreeView.ImageList = this.imageList1;
            this.fileTreeView.Location = new System.Drawing.Point(0, 0);
            this.fileTreeView.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.fileTreeView.Name = "fileTreeView";
            this.fileTreeView.SelectedImageIndex = 0;
            this.fileTreeView.Size = new System.Drawing.Size(280, 649);
            this.fileTreeView.TabIndex = 0;
            this.fileTreeView.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeView1_NodeMouseClick);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "folder");
            this.imageList1.Images.SetKeyName(1, "document");
            this.imageList1.Images.SetKeyName(2, "lock");
            // 
            // fileListView
            // 
            this.fileListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.nameColumn,
            this.lastModifiedColumn,
            this.statusColumn});
            this.fileListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fileListView.FullRowSelect = true;
            this.fileListView.Location = new System.Drawing.Point(0, 0);
            this.fileListView.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.fileListView.MultiSelect = false;
            this.fileListView.Name = "fileListView";
            this.fileListView.Size = new System.Drawing.Size(822, 649);
            this.fileListView.SmallImageList = this.imageList1;
            this.fileListView.TabIndex = 0;
            this.fileListView.UseCompatibleStateImageBehavior = false;
            this.fileListView.View = System.Windows.Forms.View.Details;
            this.fileListView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.listView1_MouseClick);
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
            this.MainTabControl.Controls.Add(this.treeViewTab);
            this.MainTabControl.Controls.Add(this.workingFileTab);
            this.MainTabControl.Location = new System.Drawing.Point(12, 12);
            this.MainTabControl.Name = "MainTabControl";
            this.MainTabControl.SelectedIndex = 0;
            this.MainTabControl.Size = new System.Drawing.Size(1122, 688);
            this.MainTabControl.TabIndex = 1;
            this.MainTabControl.Selecting += new System.Windows.Forms.TabControlCancelEventHandler(this.MainTabControl_Selecting);
            // 
            // workingFileTab
            // 
            this.workingFileTab.Controls.Add(this.workingFileList);
            this.workingFileTab.Location = new System.Drawing.Point(4, 29);
            this.workingFileTab.Name = "workingFileTab";
            this.workingFileTab.Padding = new System.Windows.Forms.Padding(3);
            this.workingFileTab.Size = new System.Drawing.Size(1114, 655);
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
            this.workingFileList.Size = new System.Drawing.Size(1108, 649);
            this.workingFileList.SmallImageList = this.imageList1;
            this.workingFileList.TabIndex = 1;
            this.workingFileList.UseCompatibleStateImageBehavior = false;
            this.workingFileList.View = System.Windows.Forms.View.Details;
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
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.insertToolStripMenuItem,
            this.clobToolStripMenuItem,
            this.showInExplorerToolStripMenuItem,
            this.diffWithDatabaseToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(258, 124);
            // 
            // insertToolStripMenuItem
            // 
            this.insertToolStripMenuItem.Name = "insertToolStripMenuItem";
            this.insertToolStripMenuItem.Size = new System.Drawing.Size(257, 30);
            this.insertToolStripMenuItem.Text = "Insert Into Database";
            this.insertToolStripMenuItem.Click += new System.EventHandler(this.insertToolStripMenuItem_Click);
            // 
            // clobToolStripMenuItem
            // 
            this.clobToolStripMenuItem.Name = "clobToolStripMenuItem";
            this.clobToolStripMenuItem.Size = new System.Drawing.Size(257, 30);
            this.clobToolStripMenuItem.Text = "Force Clob";
            this.clobToolStripMenuItem.Click += new System.EventHandler(this.clobToolStripMenuItem_Click);
            // 
            // showInExplorerToolStripMenuItem
            // 
            this.showInExplorerToolStripMenuItem.Name = "showInExplorerToolStripMenuItem";
            this.showInExplorerToolStripMenuItem.Size = new System.Drawing.Size(257, 30);
            this.showInExplorerToolStripMenuItem.Text = "Show In Explorer";
            this.showInExplorerToolStripMenuItem.Click += new System.EventHandler(this.showInExplorerToolStripMenuItem_Click);
            // 
            // diffWithDatabaseToolStripMenuItem
            // 
            this.diffWithDatabaseToolStripMenuItem.Name = "diffWithDatabaseToolStripMenuItem";
            this.diffWithDatabaseToolStripMenuItem.Size = new System.Drawing.Size(257, 30);
            this.diffWithDatabaseToolStripMenuItem.Text = "Diff with Database";
            this.diffWithDatabaseToolStripMenuItem.Click += new System.EventHandler(this.diffWithDatabaseToolStripMenuItem_Click);
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "Lobster";
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon1_MouseClick);
            // 
            // LobsterMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1146, 712);
            this.Controls.Add(this.MainTabControl);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "LobsterMain";
            this.Text = "Lobster";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.LobsterMain_FormClosed);
            this.Resize += new System.EventHandler(this.LobsterMain_Resize);
            this.treeViewTab.ResumeLayout(false);
            this.treeViewSplitContainer.Panel1.ResumeLayout(false);
            this.treeViewSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.treeViewSplitContainer)).EndInit();
            this.treeViewSplitContainer.ResumeLayout(false);
            this.MainTabControl.ResumeLayout(false);
            this.workingFileTab.ResumeLayout(false);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabPage treeViewTab;
        private System.Windows.Forms.TabControl MainTabControl;
        private System.Windows.Forms.SplitContainer treeViewSplitContainer;
        private System.Windows.Forms.TreeView fileTreeView;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.ListView fileListView;
        private System.Windows.Forms.ColumnHeader nameColumn;
        private System.Windows.Forms.ColumnHeader lastModifiedColumn;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem insertToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clobToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showInExplorerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem diffWithDatabaseToolStripMenuItem;
        private System.Windows.Forms.ColumnHeader statusColumn;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.TabPage workingFileTab;
        private System.Windows.Forms.ListView workingFileList;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
    }
}

