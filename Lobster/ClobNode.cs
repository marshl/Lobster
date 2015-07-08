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
        public ClobNode( DirectoryInfo _dirInfo, LobsterModel _model, ClobDirectory _baseDirectory )
        {
            this.dirInfo = _dirInfo;
            this.model = _model;
            this.baseDirectory = _baseDirectory;
            this.CreateFileWatchers();
        }

        public DirectoryInfo dirInfo;
        private LobsterModel model;
        private ClobDirectory baseDirectory;

        public List<ClobNode> childNodes = new List<ClobNode>();
        public Dictionary<string, ClobFile> clobFileMap = new Dictionary<string, ClobFile>();

        public void GetWorkingFiles( ref List<ClobFile> _workingFiles )
        {
            foreach ( KeyValuePair<string, ClobFile> pair in this.clobFileMap )
            {
                if ( !pair.Value.fileInfo.IsReadOnly )
                {
                    _workingFiles.Add( pair.Value );
                }
            }

            foreach ( ClobNode node in this.childNodes )
            {
                node.GetWorkingFiles( ref _workingFiles );
            }
        }

        public ClobFile FindFileWithName( string _filename )
        {
            ClobFile clobFile;
            if ( this.clobFileMap.TryGetValue( _filename, out clobFile ) )
            {
                return clobFile;
            }

            foreach ( ClobNode subDir in this.childNodes )
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
            fileWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime;// | NotifyFilters.Attributes;
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
            if ( this.clobFileMap.TryGetValue( _e.Name.ToLower(), out clobFile ) )
            { 
                FileInfo fileInfo = new FileInfo( _e.FullPath );
                if ( !fileInfo.IsReadOnly && clobFile.status == ClobFile.STATUS.SYNCHRONISED )
                {
                    this.model.SendUpdateClobMessage( clobFile );
                }
            }
        }

        public void OnFileCreated( object _source, FileSystemEventArgs _e )
        {
            ClobFile clobFile = this.AddClobFile( new FileInfo( _e.FullPath ) );
            clobFile.status = ClobFile.STATUS.LOCAL_ONLY;
        }

        private void OnFileDeleted( object _sender, FileSystemEventArgs _e )
        {
            if ( this.clobFileMap.ContainsKey( _e.Name.ToLower() ) )
            {
                this.clobFileMap.Remove( _e.Name.ToLower() );
                this.baseDirectory.fullpathClobMap.Remove( _e.Name.ToLower() );
                LobsterMain.instance.OnDirectoryStructureChanged();
            }
        }

        public ClobFile AddClobFile( FileInfo fileInfo )
        {
            ClobFile clobFile = new ClobFile( fileInfo, this, this.baseDirectory );
            string key = fileInfo.Name;
            try
            {
                this.baseDirectory.fullpathClobMap.Add( fileInfo.FullName.ToLower(), clobFile );
                this.clobFileMap.Add( key.ToLower(), clobFile );
            }
            catch ( Exception _e )
            {
                MessageLog.Log( "A file with the name '" + key + "' already exists" + _e.Message );
            }
            return clobFile;
        }
    }
}
