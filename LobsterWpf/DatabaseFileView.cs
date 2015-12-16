using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media;
using LobsterModel;

namespace LobsterWpf
{
    public class DatabaseFileView : FileNodeView
    {
        private string localFilePath;

        public DatabaseFileView(ConnectionView connection, DBClobFile dbClobFile, string localFile) : base(connection)
        {
            this.databaseFile = dbClobFile;
            this.localFilePath = localFile;
        }

        public override bool CanBeDiffed
        {
            get
            {
                return this.localFilePath != null;
            }
        }

        public override bool CanBeExploredTo
        {
            get
            {
                return this.localFilePath != null;
            }
        }

        public override bool CanBeInserted
        {
            get
            {
                return false;
            }
        }

        public override bool CanBeUpdated
        {
            get
            {
                return this.localFilePath != null;
            }
        }

        private DBClobFile databaseFile;
        public override DBClobFile DatabaseFile
        {
            get
            {
                return this.databaseFile;
            }

            set
            {
                this.databaseFile = value;
                this.NotifyPropertyChanged("DatabaseFile");
            }
        }

        public override string FullName
        {
            get
            {
                return this.localFilePath;
            }

            set
            {
                this.localFilePath = value;
                this.NotifyPropertyChanged("FullName");
            }
        }

        public override bool IsReadOnly
        {
            get
            {
                if ( this.localFilePath == null )
                {
                    return false;
                }

                return (File.GetAttributes(this.FullName) & FileAttributes.ReadOnly) != 0;
            }
        }

        public override string Name
        {
            get
            {
                return this.databaseFile.Filename;
            }
        }

        public override string ForegroundColour
        {
            get
            {
                return (this.localFilePath != null ? Colors.Black : Colors.DodgerBlue).ToString();
            }
        }

        public override string GetFileSize()
        {
            if ( this.databaseFile != null )
            {
                return null;
            }

            return Utils.BytesToString(new FileInfo(this.FullName).Length);
        }

        public override void Refresh()
        {

        }

        protected override string GetImageUrl()
        {
            return this.IsReadOnly ? LockedFileUrl : NormalFileUrl;
        }
    }
}
