using System;
using System.Collections.Generic;
using System.IO;

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
        public ClobDirectory baseDirectory;

        public List<ClobNode> childNodes = new List<ClobNode>();
        public Dictionary<string, ClobFile> clobFileMap;

        public void GetWorkingFiles( ref List<ClobFile> _workingFiles )
        {
            foreach ( KeyValuePair<string, ClobFile> pair in this.clobFileMap )
            {
                ClobFile clobFile = pair.Value;
                if ( clobFile.localClobFile != null && !clobFile.localClobFile.fileInfo.IsReadOnly )
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
            fileWatcher.Renamed += new RenamedEventHandler( OnFileRenamed );
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
                if ( clobFile.localClobFile != null && clobFile.localClobFile.fileInfo.IsReadOnly == false )
                {
                    this.model.SendUpdateClobMessage( clobFile );
                }
            }
        }

        public void OnFileCreated( object _source, FileSystemEventArgs _e )
        {
            this.baseDirectory.RefreshFileLists();
        }

        private void OnFileDeleted( object _sender, FileSystemEventArgs _e )
        {
            this.baseDirectory.RefreshFileLists();
        }

        private void OnFileRenamed( object _sender, FileSystemEventArgs _e )
        {
            this.baseDirectory.RefreshFileLists();
        }

        public void AddLocalClobFile( FileInfo _fileInfo )
        {
            ClobFile clobFile = new ClobFile( this.baseDirectory );
            clobFile.parentClobDirectory = this.baseDirectory;

            clobFile.localClobFile = new LocalClobFile();
            clobFile.localClobFile.fileInfo = _fileInfo;

            this.clobFileMap.Add( _fileInfo.Name, clobFile );
            this.baseDirectory.filenameClobMap.Add( _fileInfo.Name, clobFile );
        }

        public void RepopulateFileLists_r()
        {
            this.clobFileMap = new Dictionary<string, ClobFile>();
            foreach ( FileInfo fileInfo in this.dirInfo.GetFiles() )
            {
                this.AddLocalClobFile( fileInfo );
            }

            foreach ( ClobNode child in this.childNodes )
            {
                child.RepopulateFileLists_r();
            }
        }
    }
}
