using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lobster
{
    public class ClobNode
    {
        public ClobNode( DirectoryInfo _dirInfo, LobsterModel _model )
        {
            this.dirInfo = _dirInfo;
            this.model = _model;
            this.CreateFileWatchers();
        }

        public DirectoryInfo dirInfo;
        private LobsterModel model;

        public List<ClobNode> subDirs = new List<ClobNode>();
        public Dictionary<string, ClobFile> clobFileMap = new Dictionary<string, ClobFile>();

        public ClobFile FindFileWithName( string _filename )
        {
            ClobFile clobFile;
            if ( this.clobFileMap.TryGetValue( _filename, out clobFile ) )
            {
                return clobFile;
            }

            foreach ( ClobNode subDir in this.subDirs )
            {
                clobFile = subDir.FindFileWithName( _filename );
                if ( clobFile != null )
                {
                    return clobFile;
                }
            }

            return null;
        }

        public void CreateFileWatchers()
        {
            FileSystemWatcher fileWatcher = new FileSystemWatcher();
            fileWatcher.Path = this.dirInfo.FullName;
            //fileWatcher.IncludeSubdirectories = true;
            fileWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime;
            fileWatcher.Changed += new FileSystemEventHandler( OnFileChanged );
            fileWatcher.Created += new FileSystemEventHandler( OnFileCreated );
            fileWatcher.Deleted += new FileSystemEventHandler( OnFileDeleted );
            fileWatcher.EnableRaisingEvents = true;
        }

        public void OnFileChanged( object _source, FileSystemEventArgs _e )
        {
            if ( !File.Exists( _e.FullPath ) )
            {
                return;
            }

            ClobFile clobFile;
            if ( this.clobFileMap.TryGetValue( Path.GetFileNameWithoutExtension( _e.Name ), out clobFile ) )
            { 
                FileInfo fileInfo = new FileInfo( _e.FullPath );
                if ( !fileInfo.IsReadOnly && clobFile.status == ClobFile.STATUS.SYNCHRONISED )
                {
                    bool result = this.model.UpdateDatabaseClob( clobFile );
                    
                    LobsterMain.instance.OnFileUpdateComplete( clobFile, result );
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
