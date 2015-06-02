using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace Lobster
{
    [Serializable]
    public class ClobType
    {
        [XmlIgnore]
        public LobsterModel parentModel;

        public string directory;
        public string schema;
        public string table;
        public string mnemonicColumn;
        public string clobColumn;

        [XmlIgnore]
        public List<ClobFile> fileList;
        [XmlIgnore]
        public Dictionary<string, ClobFile> fileMap;
        [XmlIgnore]
        public DataGridView dataGridView;

        public void LoadFiles( string _codeSource )
        {
            this.fileList = new List<ClobFile>();
            this.fileMap = new Dictionary<string, ClobFile>();
            DirectoryInfo clobTypeDir = Directory.CreateDirectory( _codeSource + "/" + this.directory );
            foreach ( FileInfo file in clobTypeDir.GetFiles() )
            {
                ClobFile clobFile = new ClobFile();
                clobFile.filename = file.Name;
                clobFile.lastModified = file.LastWriteTime;
                this.fileMap.Add( file.Name, clobFile );
                clobFile.parentClobType = this;
                this.fileList.Add( clobFile );
            }

            FileSystemWatcher fileWatcher = new FileSystemWatcher();
            fileWatcher.Path = clobTypeDir.FullName;
            fileWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime;
            fileWatcher.Changed += new FileSystemEventHandler( OnFileChanged );
            fileWatcher.Created += new FileSystemEventHandler( OnFileCreated );
            fileWatcher.EnableRaisingEvents = true;
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
                    clobFile.ClobToDatabase();
                }
            }
            Console.WriteLine( "!!!" );
        }

        public void OnFileCreated( object _source, FileSystemEventArgs _e )
        {
            Console.WriteLine( "!!!" );
        }
    }
}
