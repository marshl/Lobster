using Oracle.DataAccess.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lobster
{
    public class ClobDirectory
    {
        public ClobType clobType;

        public LobsterModel parentModel;
        public Dictionary<string, ClobFile> fullpathClobMap = new Dictionary<string, ClobFile>();
        public Dictionary<string, ClobFile> filenameClobMap = new Dictionary<string, ClobFile>();
        public DataGridView dataGridView;
        public ClobNode rootClobNode;

        public void CompareToDatabase()
        {
            // Assume the file is local only
            foreach ( KeyValuePair<string, ClobFile> pair in this.fullpathClobMap )
            {
                pair.Value.status = ClobFile.STATUS.LOCAL_ONLY;
            }

            OracleConnection con = this.parentModel.oracleCon;
            OracleCommand command = con.CreateCommand();

            if ( this.clobType.hasParentTable )
            {
                command.CommandText = "SELECT parent." + this.clobType.parentMnemonicColumn
                    + " FROM " + this.clobType.schema + "." + this.clobType.parentTable + " parent"
                    + " JOIN " + this.clobType.schema + "." + this.clobType.table + " child"
                    + " ON child." + this.clobType.mnemonicColumn + " = parent." + this.clobType.parentIDColumn;
            }
            else
            {
                command.CommandText = "SELECT " + this.clobType.mnemonicColumn + " FROM " + this.clobType.schema + "." + this.clobType.table;
            }

            OracleDataReader reader;
            try
            {
                reader = command.ExecuteReader();
            }
            catch ( Exception _e )
            {
                Console.WriteLine( "Error comparing to database: " + _e.Message );
                return;
            }

            while ( reader.Read() )
            {
                string mnemonic = reader.GetString( 0 ) + ".xml";
                // Any files found on the database are "synchronised" 
                if ( this.filenameClobMap.ContainsKey( mnemonic ) )
                {
                    this.filenameClobMap[mnemonic].status = ClobFile.STATUS.SYNCHRONISED;
                }
            }
            command.Dispose();
        }

        public void CreateFileWatchers()
        {
            FileSystemWatcher fileWatcher = new FileSystemWatcher();
            fileWatcher.Path = this.rootClobNode.dirInfo.FullName;
            fileWatcher.IncludeSubdirectories = true;
            fileWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime;
            fileWatcher.Changed += new FileSystemEventHandler( OnFileChanged );
            fileWatcher.Created += new FileSystemEventHandler( OnFileCreated );
            fileWatcher.Deleted += new FileSystemEventHandler( OnFileDeleted );
            fileWatcher.EnableRaisingEvents = true;
        }

        public void OnFileChanged( object _source, FileSystemEventArgs _e )
        {
            if ( File.Exists( _e.FullPath ) )
            {
                ClobFile clobFile = this.fullpathClobMap[_e.FullPath];
                FileInfo fileInfo = new FileInfo( _e.FullPath );
                if ( !fileInfo.IsReadOnly && clobFile.status == ClobFile.STATUS.SYNCHRONISED )
                {
                    clobFile.UpdateDatabase();
                }
            }
        }

        public void OnFileCreated( object _source, FileSystemEventArgs _e )
        {
            Console.WriteLine( "!!!" );
        }

        private void OnFileDeleted( object sender, FileSystemEventArgs e )
        {
            Console.WriteLine( "!!!" );
        }
    }
}
