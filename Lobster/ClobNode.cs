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

        private FileSystemWatcher fileWatcher;
        private FileSystemWatcher fileAttributeWatcher;

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
            if ( this.clobFileMap.TryGetValue( _filename.ToLower(), out clobFile ) )
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
            this.fileWatcher = new FileSystemWatcher();
            this.fileWatcher.Path = this.dirInfo.FullName;
            this.fileWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime;// | NotifyFilters.Attributes;
            this.fileWatcher.Changed += new FileSystemEventHandler( OnFileChanged );
            this.fileWatcher.Created += new FileSystemEventHandler( OnFileCreated );
            this.fileWatcher.Deleted += new FileSystemEventHandler( OnFileDeleted );
            this.fileWatcher.Renamed += new RenamedEventHandler( OnFileRenamed );
            this.fileWatcher.EnableRaisingEvents = true;

            this.fileAttributeWatcher = new FileSystemWatcher();
            this.fileAttributeWatcher.Path = this.dirInfo.FullName;
            this.fileAttributeWatcher.NotifyFilter = NotifyFilters.Attributes;
            this.fileAttributeWatcher.Changed += new FileSystemEventHandler( OnFileAttributeChange );
            this.fileAttributeWatcher.EnableRaisingEvents = true;
        }

        private void OnFileAttributeChange( object sender, FileSystemEventArgs e )
        {
            this.fileAttributeWatcher.EnableRaisingEvents = false;
            this.baseDirectory.RefreshFileLists();
            this.fileAttributeWatcher.EnableRaisingEvents = true;
        }

        public void OnFileChanged( object _source, FileSystemEventArgs _e )
        {
            // Ensure that is not a directory
            if ( !File.Exists( _e.FullPath ) )
            {
                return;
            }

            ClobFile clobFile;
            if ( this.clobFileMap.TryGetValue( _e.Name.ToLower(), out clobFile ) )
            { 
                if ( clobFile.localClobFile != null && clobFile.localClobFile.fileInfo.IsReadOnly == false )
                {
                    this.fileWatcher.EnableRaisingEvents = false;
                    this.model.SendUpdateClobMessage( clobFile );
                    this.fileWatcher.EnableRaisingEvents = true;
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
            if ( this.baseDirectory.filenameClobMap.ContainsKey( _fileInfo.Name ) )
            {
                LobsterMain.OnErrorMessage( "Duplicate File", "A duplicate file entry for " + _fileInfo.Name + " has been found. File names must be unique within a Clob Type (bug fix required)." );
                return;
            }

            ClobFile clobFile = new ClobFile( this.baseDirectory );
            clobFile.parentClobDirectory = this.baseDirectory;

            clobFile.localClobFile = new LocalClobFile();
            clobFile.localClobFile.fileInfo = _fileInfo;

            this.clobFileMap.Add( _fileInfo.Name.ToLower(), clobFile );
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
