namespace Lobster
{
    partial class DatatypePicker
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
            this.datatypeComboBox = new System.Windows.Forms.ComboBox();
            this.datatypeAccept = new System.Windows.Forms.Button();
            this.datatypeCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // datatypeComboBox
            // 
            this.datatypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.datatypeComboBox.Location = new System.Drawing.Point(12, 12);
            this.datatypeComboBox.Name = "datatypeComboBox";
            this.datatypeComboBox.Size = new System.Drawing.Size(282, 28);
            this.datatypeComboBox.TabIndex = 0;
            // 
            // datatypeAccept
            // 
            this.datatypeAccept.Location = new System.Drawing.Point(88, 76);
            this.datatypeAccept.Name = "datatypeAccept";
            this.datatypeAccept.Size = new System.Drawing.Size(100, 35);
            this.datatypeAccept.TabIndex = 2;
            this.datatypeAccept.Text = "OK";
            this.datatypeAccept.UseVisualStyleBackColor = true;
            this.datatypeAccept.Click += new System.EventHandler(this.datatypeAccept_Click_1);
            // 
            // datatypeCancel
            // 
            this.datatypeCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.datatypeCancel.Location = new System.Drawing.Point(194, 76);
            this.datatypeCancel.Name = "datatypeCancel";
            this.datatypeCancel.Size = new System.Drawing.Size(100, 35);
            this.datatypeCancel.TabIndex = 3;
            this.datatypeCancel.Text = "Cancel";
            this.datatypeCancel.UseVisualStyleBackColor = true;
            // 
            // DatatypePicker
            // 
            this.AcceptButton = this.datatypeAccept;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.datatypeCancel;
            this.ClientSize = new System.Drawing.Size(306, 123);
            this.Controls.Add(this.datatypeCancel);
            this.Controls.Add(this.datatypeAccept);
            this.Controls.Add(this.datatypeComboBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DatatypePicker";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "DatatypePicker";
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.ComboBox datatypeComboBox;
        private System.Windows.Forms.Button datatypeAccept;
        private System.Windows.Forms.Button datatypeCancel;
    }
}