namespace Lobster
{
    partial class TablePicker
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
            this.tableCombo = new System.Windows.Forms.ComboBox();
            this.tablePickerAccept = new System.Windows.Forms.Button();
            this.tablePickerCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // tableCombo
            // 
            this.tableCombo.FormattingEnabled = true;
            this.tableCombo.Location = new System.Drawing.Point(12, 12);
            this.tableCombo.Name = "tableCombo";
            this.tableCombo.Size = new System.Drawing.Size(322, 28);
            this.tableCombo.TabIndex = 0;
            // 
            // tablePickerAccept
            // 
            this.tablePickerAccept.Location = new System.Drawing.Point(125, 64);
            this.tablePickerAccept.Name = "tablePickerAccept";
            this.tablePickerAccept.Size = new System.Drawing.Size(106, 37);
            this.tablePickerAccept.TabIndex = 1;
            this.tablePickerAccept.Text = "OK";
            this.tablePickerAccept.UseVisualStyleBackColor = true;
            this.tablePickerAccept.Click += new System.EventHandler(this.button1_Click);
            // 
            // tablePickerCancel
            // 
            this.tablePickerCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.tablePickerCancel.Location = new System.Drawing.Point(237, 64);
            this.tablePickerCancel.Name = "tablePickerCancel";
            this.tablePickerCancel.Size = new System.Drawing.Size(97, 37);
            this.tablePickerCancel.TabIndex = 2;
            this.tablePickerCancel.Text = "Cancel";
            this.tablePickerCancel.UseVisualStyleBackColor = true;
            // 
            // TablePicker
            // 
            this.AcceptButton = this.tablePickerAccept;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.tablePickerCancel;
            this.ClientSize = new System.Drawing.Size(346, 115);
            this.Controls.Add(this.tablePickerCancel);
            this.Controls.Add(this.tablePickerAccept);
            this.Controls.Add(this.tableCombo);
            this.Name = "TablePicker";
            this.Text = "TablePicker";
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.ComboBox tableCombo;
        private System.Windows.Forms.Button tablePickerAccept;
        private System.Windows.Forms.Button tablePickerCancel;
    }
}