using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

namespace Lobster
{
    public class ClobDirectory
    {
        public ClobType clobType;

        public LobsterModel parentModel;
        public List<ClobFile> clobFileList;
        public List<DBClobFile> dbClobFileList;
        public List<ClobFile> databaseOnlyFiles;
        public DataGridView dataGridView;
        public ClobNode rootClobNode;

        public void GetWorkingFiles( ref List<ClobFile> _workingFiles )
        {
            this.rootClobNode.GetWorkingFiles( ref _workingFiles );
        }

        public void CompareLocalFilesToDB()
        {
            // Reset the database only list
            this.databaseOnlyFiles = new List<ClobFile>();

            // Break any existing connections to clob files
            this.clobFileList.ForEach( x => x.dbClobFile = null );

            foreach ( DBClobFile dbClobFile in this.dbClobFileList )
            { 
                Debug.Assert( dbClobFile.filename != null );
                List<ClobFile> matchingFiles = this.clobFileList.FindAll( x => x.localClobFile.fileInfo.Name.ToLower() == dbClobFile.filename.ToLower() );

                // Link all matching local files to that database file
                if ( matchingFiles.Count > 0 )
                {
                    matchingFiles.ForEach( x => x.dbClobFile = dbClobFile );
                    if ( matchingFiles.Count > 1 )
                    {
                        MessageLog.LogWarning( "Multiple local files have been found for the database file " + dbClobFile.filename + " from the table " + dbClobFile.table.FullName );
                        matchingFiles.ForEach( x => MessageLog.LogWarning( x.localClobFile.fileInfo.FullName ) );
                    }
                }
                else // If it has no local file to link it, then add it to the database only list
                {
                    ClobFile dbOnlyClob = new ClobFile( this );
                    dbOnlyClob.dbClobFile = dbClobFile;
                    this.databaseOnlyFiles.Add( dbOnlyClob );
                }
            }
        }

        public void RefreshFileLists()
        {
            this.clobFileList = new List<ClobFile>();
            this.rootClobNode.RepopulateFileLists_r();
            this.CompareLocalFilesToDB();

            LobsterMain.instance.UpdateUIThread();
        }
    }
}
