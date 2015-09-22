namespace Lobster
{
    using Properties;
    using System;
    using System.IO;
    using System.Windows.Forms;

    /// <summary>
    /// 'So!' said the Messenger. 'Then thou art the spokesman, old greybeard?'
    /// [ _The Lord of the Rings_, V/x: "The Black Gate Opens"]
    /// </summary>
    public class EditDatabaseConnection : EditCompositeObjectForm<DatabaseConnection>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="_original"></param>
        /// <param name="_newConnection"></param>
        public EditDatabaseConnection(DatabaseConnection _original, bool _newConnection) : base(_original, _newConnection)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        protected override void InitializeComponent()
        {
            base.InitializeComponent();
            this.Text = "Edit Database Connection";
            this.editObjectTabPage.Text = "Database Connection";
            this.subItemTabPage.Text = "Clob Types";

            this.applyButton.Enabled = !this.IsNewObject;
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void PopulateSubItemList()
        {
            this.subItemListView.Clear();

            for (int i = 0; i < this.WorkingObject.ClobTypeList.Count; ++i)
            {
                ClobType clobType = this.WorkingObject.ClobTypeList[i];
                ListViewItem item = new ListViewItem(clobType.Name);

                this.subItemListView.Items.Add(item);
                item.Tag = i;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override bool ValidateChanges()
        {
            if (this.WorkingObject.CodeSource == null
             || this.WorkingObject.ClobTypeDir == null
             || this.WorkingObject.Host == null
             || this.WorkingObject.Name == null
             || this.WorkingObject.Password == null
             || this.WorkingObject.Port == null
             || this.WorkingObject.SID == null
             || this.WorkingObject.Username == null)
            {
                MessageBox.Show("All fields must be completed before saving.", "Validation Errors");
                return false;
            }

            if (this.IsNewObject)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.InitialDirectory = Path.Combine(Directory.GetCurrentDirectory(), Settings.Default.ConnectionDir);
                sfd.AddExtension = true;
                sfd.Filter = "eXtensible Markup Language File (*.xml)|*.xml";
                sfd.FileName = "NewConnection.xml";
                DialogResult result = sfd.ShowDialog();
                if (result != DialogResult.OK)
                {
                    return false;
                }

                this.WorkingObject.FileLocation = sfd.FileName;
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override bool ApplyChanges()
        {
            try
            {
                DatabaseConnection.SerialiseToFile(this.WorkingObject.FileLocation, this.WorkingObject);
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Cannot save DatabaseConnection. " + this.WorkingObject.FileLocation + " is locked.");
                return false;
            }

            this.OriginalObject = this.WorkingObject;
            this.WorkingObject = (DatabaseConnection)this.OriginalObject.Clone();

            this.IsNewObject = false;
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void addSubItemButton_click(object sender, EventArgs e)
        {
            ClobType clobType = new ClobType();
            clobType.ParentConnection = this.WorkingObject;
            EditClobType editForm = new EditClobType(clobType, true);
            DialogResult result = editForm.ShowDialog();

            if (result == DialogResult.OK)
            {
                this.WorkingObject.ClobTypeList.Add(editForm.OriginalObject);
                this.PopulateSubItemList();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void removeSubItemButton_Click(object sender, EventArgs e)
        {
            if (this.subItemListView.SelectedItems.Count == 0)
            {
                MessageBox.Show("Select a Database Connection first.");
                return;
            }

            int clobTypeIndex = (int)this.subItemListView.SelectedItems[0].Tag;
            ClobType clobType = this.WorkingObject.ClobTypeList[clobTypeIndex];
            this.WorkingObject.ClobTypeList.Remove(clobType);
            this.PopulateSubItemList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void editSubItemButton_Click(object sender, EventArgs e)
        {
            if (this.subItemListView.SelectedItems.Count == 0)
            {
                MessageBox.Show("Select a Database Connection first.");
                return;
            }

            int clobTypeIndex = (int)this.subItemListView.SelectedItems[0].Tag;
            ClobType clobType = this.WorkingObject.ClobTypeList[clobTypeIndex];

            EditClobType editForm = new EditClobType(clobType, false);
            DialogResult ctResult = editForm.ShowDialog();
            this.WorkingObject.ClobTypeList[clobTypeIndex] = editForm.OriginalObject;
            this.PopulateSubItemList();
        }
    }
}
