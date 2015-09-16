namespace Lobster
{
    using System.Collections.Generic;
    using System.IO;
    using System.Windows.Forms;
    /// <summary>
    /// A box without hinges, key, or lid,
    /// Yet golden treasure inside is hid,
    /// [ _The Hobbit_, V: "Riddles in the Dark"]
    /// </summary>
    public class ClobDirectory
    {
        /// <summary>
        /// The ClobType that controls this directory
        /// </summary>
        public ClobType ClobType { get; private set; }

        public DatabaseConnection ParentConnection { get; private set; }
        public List<ClobFile> FileList { get; private set; }
        public List<DBClobFile> DatabaseFileList { get; set; }
        public List<ClobFile> DatabaseOnlyFiles { get; private set; }
        public ClobNode RootClobNode { get; set; }

        public ClobDirectory(ClobType clobType, DatabaseConnection dbConnection)
        {
            this.ClobType = clobType;
            this.ParentConnection = dbConnection;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="codeSourceDirectory"></param>
        /// <returns></returns>
        public bool Populate(string codeSourceDirectory)
        {
            DirectoryInfo info = new DirectoryInfo(Path.Combine(codeSourceDirectory, this.ClobType.directory));
            if (!info.Exists)
            {
                MessageLog.LogWarning(info.FullName + " could not be found.");
                LobsterMain.OnErrorMessage("Folder not found", "Folder \"" + info.FullName + "\" could not be found for ClobType " + this.ClobType.name);
                return false;
            }
            this.RootClobNode = new ClobNode(info, this);
            if (this.ClobType.includeSubDirectories)
            {
                this.PopulateClobNodeDirectories_r(this.RootClobNode);
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clobNode"></param>
        /// <param name="clobDirectory"></param>
        private void PopulateClobNodeDirectories_r(ClobNode clobNode)
        {
            DirectoryInfo[] subDirs = clobNode.dirInfo.GetDirectories();
            foreach (DirectoryInfo subDir in subDirs)
            {
                ClobNode childNode = new ClobNode(subDir, this);
                PopulateClobNodeDirectories_r(childNode);
                clobNode.childNodes.Add(childNode);
            }
        }

        /// <summary>
        /// Gets all editable files within this directory, and stores them in the given list.
        /// </summary>
        /// <param name="workingFileList">The list of files to populate.</param>
        public void GetWorkingFiles(ref List<ClobFile> workingFileList)
        {
            this.RootClobNode.GetWorkingFiles(ref workingFileList);
        }

        /// <summary>
        /// Finds all files in all directories under this directory, and links them to the database files.
        /// </summary>
        public void GetLocalFiles()
        {
            this.FileList = new List<ClobFile>();
            this.RootClobNode.RepopulateFileLists_r();
            this.LinkLocalAndDatabaseFiles();

            // The UI will have to be refreshed
            LobsterMain.instance.UpdateUIThread();
        }

        /// <summary>
        /// Links all database files to their corresponding local files.
        /// Files that are not found locally are stored as "database-only".
        /// </summary>
        private void LinkLocalAndDatabaseFiles()
        {
            // Reset the database only list
            this.DatabaseOnlyFiles = new List<ClobFile>();

            // Break any existing connections to clob files
            this.FileList.ForEach(x => x.dbClobFile = null);

            foreach (DBClobFile dbClobFile in this.DatabaseFileList)
            {
                List<ClobFile> matchingFiles = this.FileList.FindAll(x => x.localClobFile.fileInfo.Name.ToLower() == dbClobFile.filename.ToLower());

                // Link all matching local files to that database file
                if (matchingFiles.Count > 0)
                {
                    matchingFiles.ForEach(x => x.dbClobFile = dbClobFile);
                    if (matchingFiles.Count > 1)
                    {
                        MessageLog.LogWarning("Multiple local files have been found for the database file " + dbClobFile.filename + " from the table " + dbClobFile.table.FullName);
                        matchingFiles.ForEach(x => MessageLog.LogWarning(x.localClobFile.fileInfo.FullName));
                        MessageLog.LogWarning("Updating any of those files will update the same database file.");
                    }
                }
                else // If it has no local file to link it, then add it to the database only list
                {
                    ClobFile dbOnlyClob = new ClobFile(this);
                    dbOnlyClob.dbClobFile = dbClobFile;
                    this.DatabaseOnlyFiles.Add(dbOnlyClob);
                }
            }
        }
    }
}
