using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Lobster
{
    public class ClobDirectory
    {
        public ClobType clobType;

        public LobsterModel parentModel;
        public Dictionary<string, ClobFile> filenameClobMap = new Dictionary<string, ClobFile>();
        public Dictionary<string, DBClobFile> databaseClobMap = new Dictionary<string, DBClobFile>();
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
            foreach ( KeyValuePair<string, ClobFile> pair in this.filenameClobMap )
            {
                pair.Value.dbClobFile = null;
            }

            foreach ( KeyValuePair<string, DBClobFile> pair in this.databaseClobMap )
            {
                DBClobFile dbClobFile = pair.Value;
                Debug.Assert( dbClobFile.filename != null );
                ClobFile clobFile;
                if ( this.filenameClobMap.TryGetValue( dbClobFile.filename, out clobFile ) )
                {
                    clobFile.dbClobFile = dbClobFile;
                }
                else
                {
                    ClobFile dbOnlyClob = new ClobFile( this );
                    dbOnlyClob.dbClobFile = dbClobFile;
                    this.databaseOnlyFiles.Add( dbOnlyClob );
                }
            }
        }

        public void RefreshFileLists()
        {
            this.filenameClobMap.Clear();
            this.rootClobNode.RepopulateFileLists_r();
            this.CompareLocalFilesToDB();

            LobsterMain.instance.UpdateUIThread();
        }
    }
}
