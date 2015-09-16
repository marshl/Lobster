using System;
using System.Collections.Generic;
using System.IO;

namespace Lobster
{
    /// <summary>
    /// All about them as they lay hung the darkness, hollow and immense,
    /// and they were oppressed by the loneliness and vastness of the dolven halls and
    /// endlessly branching stairs and passages.
    /// [ _The Lord of the Rings_, II/iv: "A Journey in the Dark"]
    /// </summary>
    public class ClobNode : IDisposable
    {
        public ClobNode( DirectoryInfo _dirInfo, ClobDirectory _baseDirectory )
        {
            this.dirInfo = _dirInfo;
            this.baseDirectory = _baseDirectory;
            this.CreateFileWatchers();
        }

        public DirectoryInfo dirInfo;
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
        
        public void CreateFileWatchers()
        {
            this.fileWatcher = new FileSystemWatcher();
            this.fileWatcher.Path = this.dirInfo.FullName;
            this.fileWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime;
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

        public void SetFileWatchersEnabled( bool _enabled )
        {
            this.fileAttributeWatcher.EnableRaisingEvents = this.fileWatcher.EnableRaisingEvents = _enabled;
        }

        private void OnFileAttributeChange( object sender, FileSystemEventArgs e )
        {
            this.SetFileWatchersEnabled( false );
            this.baseDirectory.GetLocalFiles();
            this.SetFileWatchersEnabled( true );
        }

        public void OnFileChanged( object _source, FileSystemEventArgs _e )
        {
            this.SetFileWatchersEnabled( false );
            // Ensure that the file changed exists and is not a directory
            if (!File.Exists( _e.FullPath ))
            {
                return;
            }

            ClobFile clobFile;
            if ( this.clobFileMap.TryGetValue( _e.Name.ToLower(), out clobFile ) )
            { 
                if ( clobFile.IsSynced && clobFile.IsEditable )
                {
                    this.baseDirectory.ClobType.ParentConnection.ParentModel.SendUpdateClobMessage( clobFile );
                }
            }
            this.SetFileWatchersEnabled( true );
        }

        public void OnFileCreated( object _source, FileSystemEventArgs _e )
        {
            this.SetFileWatchersEnabled( false );
            this.baseDirectory.GetLocalFiles();
            this.SetFileWatchersEnabled( true );
        }

        private void OnFileDeleted( object _sender, FileSystemEventArgs _e )
        {
            this.SetFileWatchersEnabled( false );
            this.baseDirectory.GetLocalFiles();
            this.SetFileWatchersEnabled( true );
        }

        private void OnFileRenamed( object _sender, FileSystemEventArgs _e )
        {
            this.SetFileWatchersEnabled( false );
            this.baseDirectory.GetLocalFiles();
            this.SetFileWatchersEnabled( true );
        }

        public void AddLocalClobFile( FileInfo _fileInfo )
        {
            ClobFile clobFile = new ClobFile( this.baseDirectory );
            clobFile.parentClobDirectory = this.baseDirectory;

            clobFile.localClobFile = new LocalClobFile();
            clobFile.localClobFile.fileInfo = _fileInfo;

            this.clobFileMap.Add( _fileInfo.Name.ToLower(), clobFile );
            this.baseDirectory.FileList.Add( clobFile );
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

        public void Dispose()
        {
            this.fileAttributeWatcher.Dispose();
            this.fileWatcher.Dispose();
        }
    }
}
