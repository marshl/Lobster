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
            OracleConnection con = this.parentModel.OpenConnection();
            OracleCommand command = con.CreateCommand();

            if ( this.clobType.hasParentTable )
            {
                command.CommandText = "SELECT parent." + this.clobType.parentMnemonicColumn
                    + ( this.clobType.dataTypeColumnName != null ? ", child." + this.clobType.dataTypeColumnName : null )
                    + " FROM " + this.clobType.schema + "." + this.clobType.parentTable + " parent"
                    + " JOIN " + this.clobType.schema + "." + this.clobType.table + " child"
                    + " ON child." + this.clobType.mnemonicColumn + " = parent." + this.clobType.parentIDColumn;
            }
            else
            {
                command.CommandText = "SELECT " + this.clobType.mnemonicColumn
                    + ( this.clobType.dataTypeColumnName != null ? ", " + this.clobType.dataTypeColumnName : null )
                    + " FROM " + this.clobType.schema + "." + this.clobType.table;
            }

            OracleDataReader reader;
            try
            {
                reader = command.ExecuteReader();
            }
            catch ( Exception _e )
            {
                Console.WriteLine( "Error comparing to database: " + _e.Message );
                con.Close();
                return;
            }

            // Assume the file is local only
            foreach ( KeyValuePair<string, ClobFile> pair in this.fullpathClobMap )
            {
                pair.Value.status = ClobFile.STATUS.LOCAL_ONLY;
            }

            while ( reader.Read() )
            {
                string mnemonic = reader.GetString( 0 );

                string internalMnemonic = null;
                if ( mnemonic.Contains('/') )
                {
                    internalMnemonic = mnemonic.Substring( mnemonic.LastIndexOf( '/' ) + 1 );
                }

                // Any files found on the database are "synchronised" 
                ClobFile clobFile;
                if ( this.filenameClobMap.TryGetValue( internalMnemonic ?? mnemonic, out clobFile ) )
                {
                    clobFile.status = ClobFile.STATUS.SYNCHRONISED;
                    clobFile.databaseMnemonic = mnemonic;
                    clobFile.databaseType = reader.FieldCount > 1 ? reader.GetString( 1 ) : null;
                }
                else
                {
                    Console.WriteLine( "No file found for " + internalMnemonic +" / " + mnemonic );
                }
            }
            command.Dispose();
            con.Close();
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
                    this.parentModel.UpdateDatabaseClob( clobFile );
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
