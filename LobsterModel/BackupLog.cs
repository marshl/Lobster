using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LobsterModel
{
    public class BackupLog
    {
        private Dictionary<string, List<FileBackup>> BackupDictionary;

        public BackupLog()
        {
            this.BackupDictionary = new Dictionary<string, List<FileBackup>>();
        }

        public void AddBackup(string originalFilename, string backupFilename)
        {
            FileBackup file = new FileBackup(originalFilename, backupFilename);

            List<FileBackup> files = this.GetBackupsForFile(originalFilename);
            if (files == null)
            {
                files = new List<FileBackup>();
                this.BackupDictionary.Add(originalFilename, files);
            }

            files.Add(file);
        }

        public List<FileBackup> GetBackupsForFile(string filename)
        {
            List<FileBackup> result;
            this.BackupDictionary.TryGetValue(filename, out result);
            return result;
        }
    }
}
