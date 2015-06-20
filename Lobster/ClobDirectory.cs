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
        public Dictionary<string, ClobFile> fileMap = new Dictionary<string, ClobFile>();
        public DataGridView dataGridView;
        public ClobNode rootClobNode;

        public void CompareToDatabase()
        {
            // Assume the file is local only
            foreach ( KeyValuePair<string, ClobFile> pair in this.fileMap )
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
            
            OracleDataReader reader = command.ExecuteReader();

            while ( reader.Read() )
            {
                string mnemonic = reader.GetString( 0 ) + ".xml";
                // Any files found on the database are "synchronised" 
                if ( this.fileMap.ContainsKey( mnemonic ) )
                {
                    this.fileMap[mnemonic].status = ClobFile.STATUS.SYNCHRONISED;
                }
            }
            command.Dispose();
        }

        public void CreateFileWatchers()
        {
            FileSystemWatcher fileWatcher = new FileSystemWatcher();
            fileWatcher.Path = this.rootClobNode.dirInfo.FullName;
            fileWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime;
            fileWatcher.Changed += new FileSystemEventHandler( OnFileChanged );
            fileWatcher.Created += new FileSystemEventHandler( OnFileCreated );
            fileWatcher.EnableRaisingEvents = true;
        }

        public void OnFileChanged( object _source, FileSystemEventArgs _e )
        {
            if ( File.Exists( _e.FullPath ) )
            {
                ClobFile clobFile = this.fileMap[_e.FullPath];
                FileInfo fileInfo = new FileInfo( _e.FullPath );
                if ( !fileInfo.IsReadOnly )
                {
                    clobFile.UpdateDatabase();
                }
            }
        }

        public void OnFileCreated( object _source, FileSystemEventArgs _e )
        {
            Console.WriteLine( "!!!" );
        }
    }
}
