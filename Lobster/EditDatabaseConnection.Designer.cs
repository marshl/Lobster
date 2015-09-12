namespace Lobster
{
    partial class EditDatabaseConnection
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose( bool disposing )
        {
            if ( disposing && ( components != null ) )
            {
                components.Dispose();
            }
            base.Dispose( disposing );
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.applyButton = new System.Windows.Forms.Button();
            this.databaseConnectionPropertyGrid = new System.Windows.Forms.PropertyGrid();
            this.editDatabaseConnectionTabControl = new System.Windows.Forms.TabControl();
            this.databaseConnectionTabPage = new System.Windows.Forms.TabPage();
            this.clobTypeTabPage = new System.Windows.Forms.TabPage();
            this.editClobTypeButton = new System.Windows.Forms.Button();
            this.removeClobTypeButton = new System.Windows.Forms.Button();
            this.addClobTypeButton = new System.Windows.Forms.Button();
            this.clobTypeListView = new System.Windows.Forms.ListView();
            this.editDatabaseConnectionTabControl.SuspendLayout();
            this.databaseConnectionTabPage.SuspendLayout();
            this.clobTypeTabPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.okButton.Location = new System.Drawing.Point(466, 500);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(96, 32);
            this.okButton.TabIndex = 0;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cancelButton.Location = new System.Drawing.Point(670, 500);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(96, 32);
            this.cancelButton.TabIndex = 1;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // applyButton
            // 
            this.applyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.applyButton.Location = new System.Drawing.Point(568, 500);
            this.applyButton.Name = "applyButton";
            this.applyButton.Size = new System.Drawing.Size(96, 32);
            this.applyButton.TabIndex = 2;
            this.applyButton.Text = "Apply";
            this.applyButton.UseVisualStyleBackColor = true;
            this.applyButton.Click += new System.EventHandler(this.applyButton_Click);
            // 
            // databaseConnectionPropertyGrid
            // 
            this.databaseConnectionPropertyGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.databaseConnectionPropertyGrid.CategoryForeColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.databaseConnectionPropertyGrid.Location = new System.Drawing.Point(6, 6);
            this.databaseConnectionPropertyGrid.Name = "databaseConnectionPropertyGrid";
            this.databaseConnectionPropertyGrid.Size = new System.Drawing.Size(734, 437);
            this.databaseConnectionPropertyGrid.TabIndex = 11;
            // 
            // editDatabaseConnectionTabControl
            // 
            this.editDatabaseConnectionTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.editDatabaseConnectionTabControl.Controls.Add(this.databaseConnectionTabPage);
            this.editDatabaseConnectionTabControl.Controls.Add(this.clobTypeTabPage);
            this.editDatabaseConnectionTabControl.Location = new System.Drawing.Point(12, 12);
            this.editDatabaseConnectionTabControl.Multiline = true;
            this.editDatabaseConnectionTabControl.Name = "editDatabaseConnectionTabControl";
            this.editDatabaseConnectionTabControl.SelectedIndex = 0;
            this.editDatabaseConnectionTabControl.Size = new System.Drawing.Size(754, 482);
            this.editDatabaseConnectionTabControl.TabIndex = 12;
            // 
            // databaseConnectionTabPage
            // 
            this.databaseConnectionTabPage.Controls.Add(this.databaseConnectionPropertyGrid);
            this.databaseConnectionTabPage.Location = new System.Drawing.Point(4, 29);
            this.databaseConnectionTabPage.Name = "databaseConnectionTabPage";
            this.databaseConnectionTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.databaseConnectionTabPage.Size = new System.Drawing.Size(746, 449);
            this.databaseConnectionTabPage.TabIndex = 0;
            this.databaseConnectionTabPage.Text = "Database Connection";
            this.databaseConnectionTabPage.UseVisualStyleBackColor = true;
            // 
            // clobTypeTabPage
            // 
            this.clobTypeTabPage.Controls.Add(this.editClobTypeButton);
            this.clobTypeTabPage.Controls.Add(this.removeClobTypeButton);
            this.clobTypeTabPage.Controls.Add(this.addClobTypeButton);
            this.clobTypeTabPage.Controls.Add(this.clobTypeListView);
            this.clobTypeTabPage.Location = new System.Drawing.Point(4, 29);
            this.clobTypeTabPage.Name = "clobTypeTabPage";
            this.clobTypeTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.clobTypeTabPage.Size = new System.Drawing.Size(746, 449);
            this.clobTypeTabPage.TabIndex = 1;
            this.clobTypeTabPage.Text = "Clob Types";
            this.clobTypeTabPage.UseVisualStyleBackColor = true;
            // 
            // editClobTypeButton
            // 
            this.editClobTypeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.editClobTypeButton.Location = new System.Drawing.Point(210, 411);
            this.editClobTypeButton.Name = "editClobTypeButton";
            this.editClobTypeButton.Size = new System.Drawing.Size(96, 32);
            this.editClobTypeButton.TabIndex = 15;
            this.editClobTypeButton.Text = "Edit";
            this.editClobTypeButton.UseVisualStyleBackColor = true;
            this.editClobTypeButton.Click += new System.EventHandler(this.editClobTypeButton_Click);
            // 
            // removeClobTypeButton
            // 
            this.removeClobTypeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.removeClobTypeButton.Location = new System.Drawing.Point(108, 411);
            this.removeClobTypeButton.Name = "removeClobTypeButton";
            this.removeClobTypeButton.Size = new System.Drawing.Size(96, 32);
            this.removeClobTypeButton.TabIndex = 14;
            this.removeClobTypeButton.Text = "Remove";
            this.removeClobTypeButton.UseVisualStyleBackColor = true;
            this.removeClobTypeButton.Click += new System.EventHandler(this.removeClobTypeButton_Click);
            // 
            // addClobTypeButton
            // 
            this.addClobTypeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.addClobTypeButton.Location = new System.Drawing.Point(6, 411);
            this.addClobTypeButton.Name = "addClobTypeButton";
            this.addClobTypeButton.Size = new System.Drawing.Size(96, 32);
            this.addClobTypeButton.TabIndex = 13;
            this.addClobTypeButton.Text = "Add";
            this.addClobTypeButton.UseVisualStyleBackColor = true;
            this.addClobTypeButton.Click += new System.EventHandler(this.addClobTypeButton_Click);
            // 
            // clobTypeListView
            // 
            this.clobTypeListView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.clobTypeListView.Location = new System.Drawing.Point(6, 6);
            this.clobTypeListView.Name = "clobTypeListView";
            this.clobTypeListView.Size = new System.Drawing.Size(368, 399);
            this.clobTypeListView.TabIndex = 0;
            this.clobTypeListView.UseCompatibleStateImageBehavior = false;
            // 
            // EditDatabaseConnection
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(778, 544);
            this.Controls.Add(this.editDatabaseConnectionTabControl);
            this.Controls.Add(this.applyButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Name = "EditDatabaseConnection";
            this.Text = "EditDatabaseConnection";
            this.editDatabaseConnectionTabControl.ResumeLayout(false);
            this.databaseConnectionTabPage.ResumeLayout(false);
            this.clobTypeTabPage.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button applyButton;
        private System.Windows.Forms.PropertyGrid databaseConnectionPropertyGrid;
        private System.Windows.Forms.TabControl editDatabaseConnectionTabControl;
        private System.Windows.Forms.TabPage databaseConnectionTabPage;
        private System.Windows.Forms.TabPage clobTypeTabPage;
        private System.Windows.Forms.ListView clobTypeListView;
        private System.Windows.Forms.Button removeClobTypeButton;
        private System.Windows.Forms.Button addClobTypeButton;
        private System.Windows.Forms.Button editClobTypeButton;
    }
}