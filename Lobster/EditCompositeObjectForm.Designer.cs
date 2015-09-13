namespace Lobster
{
    abstract partial class EditCompositeObjectForm<T>
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
        protected virtual void InitializeComponent()
        {
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.applyButton = new System.Windows.Forms.Button();
            this.editObjectPropertyGrid = new System.Windows.Forms.PropertyGrid();
            this.editObjectTabControl = new System.Windows.Forms.TabControl();
            this.editObjectTabPage = new System.Windows.Forms.TabPage();
            this.subItemTabPage = new System.Windows.Forms.TabPage();
            this.editSubItemButton = new System.Windows.Forms.Button();
            this.removeSubItemButton = new System.Windows.Forms.Button();
            this.addSubItemButton = new System.Windows.Forms.Button();
            this.subItemListView = new System.Windows.Forms.ListView();
            this.editObjectTabControl.SuspendLayout();
            this.editObjectTabPage.SuspendLayout();
            this.subItemTabPage.SuspendLayout();
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
            // editObjectPropertyGrid
            // 
            this.editObjectPropertyGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.editObjectPropertyGrid.CategoryForeColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.editObjectPropertyGrid.Location = new System.Drawing.Point(6, 6);
            this.editObjectPropertyGrid.Name = "editObjectPropertyGrid";
            this.editObjectPropertyGrid.Size = new System.Drawing.Size(734, 437);
            this.editObjectPropertyGrid.TabIndex = 11;
            // 
            // editObjectTabControl
            // 
            this.editObjectTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.editObjectTabControl.Controls.Add(this.editObjectTabPage);
            this.editObjectTabControl.Controls.Add(this.subItemTabPage);
            this.editObjectTabControl.Location = new System.Drawing.Point(12, 12);
            this.editObjectTabControl.Multiline = true;
            this.editObjectTabControl.Name = "editObjectTabControl";
            this.editObjectTabControl.SelectedIndex = 0;
            this.editObjectTabControl.Size = new System.Drawing.Size(754, 482);
            this.editObjectTabControl.TabIndex = 12;
            // 
            // editObjectTabPage
            // 
            this.editObjectTabPage.Controls.Add(this.editObjectPropertyGrid);
            this.editObjectTabPage.Location = new System.Drawing.Point(4, 29);
            this.editObjectTabPage.Name = "editObjectTabPage";
            this.editObjectTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.editObjectTabPage.Size = new System.Drawing.Size(746, 449);
            this.editObjectTabPage.TabIndex = 0;
            this.editObjectTabPage.Text = "[EditObjectTabPageName]";
            this.editObjectTabPage.UseVisualStyleBackColor = true;
            // 
            // subItemTabPage
            // 
            this.subItemTabPage.Controls.Add(this.editSubItemButton);
            this.subItemTabPage.Controls.Add(this.removeSubItemButton);
            this.subItemTabPage.Controls.Add(this.addSubItemButton);
            this.subItemTabPage.Controls.Add(this.subItemListView);
            this.subItemTabPage.Location = new System.Drawing.Point(4, 29);
            this.subItemTabPage.Name = "subItemTabPage";
            this.subItemTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.subItemTabPage.Size = new System.Drawing.Size(746, 449);
            this.subItemTabPage.TabIndex = 1;
            this.subItemTabPage.Text = "[SubItemTabPageName]";
            this.subItemTabPage.UseVisualStyleBackColor = true;
            // 
            // editSubItemButton
            // 
            this.editSubItemButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.editSubItemButton.Location = new System.Drawing.Point(210, 411);
            this.editSubItemButton.Name = "editSubItemButton";
            this.editSubItemButton.Size = new System.Drawing.Size(96, 32);
            this.editSubItemButton.TabIndex = 15;
            this.editSubItemButton.Text = "Edit";
            this.editSubItemButton.UseVisualStyleBackColor = true;
            this.editSubItemButton.Click += new System.EventHandler(this.editSubItemButton_Click);
            // 
            // removeSubItemButton
            // 
            this.removeSubItemButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.removeSubItemButton.Location = new System.Drawing.Point(108, 411);
            this.removeSubItemButton.Name = "removeSubItemButton";
            this.removeSubItemButton.Size = new System.Drawing.Size(96, 32);
            this.removeSubItemButton.TabIndex = 14;
            this.removeSubItemButton.Text = "Remove";
            this.removeSubItemButton.UseVisualStyleBackColor = true;
            this.removeSubItemButton.Click += new System.EventHandler(this.removeSubItemButton_Click);
            // 
            // addSubItemButton
            // 
            this.addSubItemButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.addSubItemButton.Location = new System.Drawing.Point(6, 411);
            this.addSubItemButton.Name = "addSubItemButton";
            this.addSubItemButton.Size = new System.Drawing.Size(96, 32);
            this.addSubItemButton.TabIndex = 13;
            this.addSubItemButton.Text = "Add";
            this.addSubItemButton.UseVisualStyleBackColor = true;
            this.addSubItemButton.Click += new System.EventHandler(this.addSubItemButton_click);
            // 
            // subItemListView
            // 
            this.subItemListView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.subItemListView.Location = new System.Drawing.Point(6, 6);
            this.subItemListView.Name = "subItemListView";
            this.subItemListView.Size = new System.Drawing.Size(368, 399);
            this.subItemListView.TabIndex = 0;
            this.subItemListView.UseCompatibleStateImageBehavior = false;
            this.subItemListView.View = System.Windows.Forms.View.List;
            // 
            // EditCompositeObjectForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(778, 544);
            this.Controls.Add(this.editObjectTabControl);
            this.Controls.Add(this.applyButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Name = "EditCompositeObjectForm";
            this.Text = "EditDatabaseConnection";
            this.editObjectTabControl.ResumeLayout(false);
            this.editObjectTabPage.ResumeLayout(false);
            this.subItemTabPage.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        protected System.Windows.Forms.Button okButton;
        protected System.Windows.Forms.Button cancelButton;
        protected System.Windows.Forms.Button applyButton;
        protected System.Windows.Forms.PropertyGrid editObjectPropertyGrid;
        protected System.Windows.Forms.TabControl editObjectTabControl;
        protected System.Windows.Forms.TabPage editObjectTabPage;
        protected System.Windows.Forms.TabPage subItemTabPage;
        protected System.Windows.Forms.ListView subItemListView;
        protected System.Windows.Forms.Button removeSubItemButton;
        protected System.Windows.Forms.Button addSubItemButton;
        protected System.Windows.Forms.Button editSubItemButton;
    }
}