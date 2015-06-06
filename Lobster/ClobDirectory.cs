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
        public List<ClobFile> fileList;
        public Dictionary<string, ClobFile> fileMap;
        public DataGridView dataGridView;

        public void LoadFiles()
        {
            this.fileList = new List<ClobFile>();
            this.fileMap = new Dictionary<string, ClobFile>();
            DirectoryInfo clobTypeDir = Directory.CreateDirectory( this.parentModel.dbConfig.codeSource + "/" + this.clobType.directory );
            foreach ( FileInfo file in clobTypeDir.GetFiles() )
            {
                ClobFile clobFile = new ClobFile();
                clobFile.filename = file.Name;
                clobFile.lastModified = file.LastWriteTime;
                clobFile.parentClobDirectory = this;
                this.fileMap.Add( file.Name, clobFile );
                this.fileList.Add( clobFile );
            }

            FileSystemWatcher fileWatcher = new FileSystemWatcher();
            fileWatcher.Path = clobTypeDir.FullName;
            fileWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime;
            fileWatcher.Changed += new FileSystemEventHandler( OnFileChanged );
            fileWatcher.Created += new FileSystemEventHandler( OnFileCreated );
            fileWatcher.EnableRaisingEvents = true;
        }

        public void CompareToDatabase()
        {
            // Assume the file is local only
            foreach ( ClobFile file in this.fileList )
            {
                file.status = ClobFile.STATUS.LOCAL_ONLY;
            }

            OracleConnection con = this.parentModel.oracleCon;
            OracleCommand command = con.CreateCommand();
            command.CommandText = "SELECT " + this.clobType.mnemonicColumn + " FROM " + this.clobType.schema + "." + this.clobType.table;
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

        public void OnFileChanged( object _source, FileSystemEventArgs _e )
        {
            if ( File.Exists( _e.FullPath ) )
            {
                ClobFile clobFile = this.fileMap[_e.Name];
                FileInfo fileInfo = new FileInfo( _e.FullPath );
                clobFile.lastModified = fileInfo.LastWriteTime;
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
