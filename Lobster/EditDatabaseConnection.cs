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

            this.applyButton.Enabled = !this.isNewObject;
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void PopulateSubItemList()
        {
            this.subItemListView.Clear();

            for (int i = 0; i < this.workingObject.ClobTypeList.Count; ++i)
            {
                ClobType clobType = this.workingObject.ClobTypeList[i];
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
            if (this.workingObject.CodeSource == null
             || this.workingObject.ClobTypeDir == null
             || this.workingObject.Host == null
             || this.workingObject.Name == null
             || this.workingObject.Password == null
             || this.workingObject.Port == null
             || this.workingObject.SID == null
             || this.workingObject.Username == null)
            {
                MessageBox.Show("All fields must be completed before saving.", "Validation Errors");
                return false;
            }

            if (this.isNewObject)
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

                this.workingObject.FileLocation = sfd.FileName;
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
                DatabaseConnection.SerialiseToFile(this.workingObject.FileLocation, this.workingObject);
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Cannot save DatabaseConnection. " + this.workingObject.FileLocation + " is locked.");
                return false;
            }

            this.originalObject = this.workingObject;
            this.workingObject = (DatabaseConnection)this.originalObject.Clone();

            this.isNewObject = false;
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
            clobType.ParentConnection = this.workingObject;
            EditClobType editForm = new EditClobType(clobType, true);
            DialogResult result = editForm.ShowDialog();

            if (result == DialogResult.OK)
            {
                this.workingObject.ClobTypeList.Add(editForm.originalObject);
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
            ClobType clobType = this.workingObject.ClobTypeList[clobTypeIndex];
            this.workingObject.ClobTypeList.Remove(clobType);
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
            ClobType clobType = this.workingObject.ClobTypeList[clobTypeIndex];

            EditClobType editForm = new EditClobType(clobType, false);
            DialogResult ctResult = editForm.ShowDialog();
            this.workingObject.ClobTypeList[clobTypeIndex] = editForm.originalObject;
            this.PopulateSubItemList();
        }
    }
}
