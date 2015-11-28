using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LobsterModel
{
    public class FileBackup
    {
        public string OriginalFilename { get; }
        public string BackupFilename { get; }
        public DateTime DateCreated { get; }

        public FileBackup(string originalFIlename, string backupFilename )
        {
            this.OriginalFilename = originalFIlename;
            this.BackupFilename = backupFilename;
            this.DateCreated = DateTime.Now;
        }
    }
}
