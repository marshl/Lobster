using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace Lobster
{
    public class DatabaseConnection
    {
        public DatabaseConfig dbConfig;

        public List<ClobType> clobTypeList;
        public Dictionary<ClobType, ClobDirectory> clobTypeToDirectoryMap;

        public DatabaseConnection( DatabaseConfig _config )
        {
            this.dbConfig = _config;
        }

        public void LoadClobTypes()
        {
            this.clobTypeList = new List<ClobType>();
            DirectoryInfo clobTypeDir = new DirectoryInfo( this.dbConfig.clobTypeDir );
            if ( !clobTypeDir.Exists )
            {
                MessageBox.Show( String.Format( "{0} could not be found.", clobTypeDir ), "ClobType Load Failed", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1 );
                MessageLog.Log( String.Format( "The directory {0} could not be found when loading {1}.", clobTypeDir, this.dbConfig.name ) );
                return;
            }

            foreach ( FileInfo file in clobTypeDir.GetFiles() )
            {
                try
                {
                    ClobType clobType = new ClobType();
                    XmlSerializer xmls = new XmlSerializer( typeof( ClobType ) );
                    StreamReader streamReader = new StreamReader( file.FullName );
                    XmlReader xmlReader = XmlReader.Create( streamReader );
                    clobType = (ClobType)xmls.Deserialize( xmlReader );
                    xmlReader.Close();
                    streamReader.Close();

                    if ( !clobType.enabled )
                    {
                        continue;
                    }

                    foreach ( ClobType.Table t in clobType.tables )
                    {
                        t.LinkColumns();
                    }

                    this.clobTypeList.Add( clobType );
                }
                catch ( InvalidOperationException _e )
                {
                    MessageBox.Show( "The ClobType " + file.Name + " failed to load. Check the log for more information.", "ClobType Load Failed", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1 );
                    MessageLog.Log( String.Format( "An InvalidOperationException was thrown when loading the ClobType {0}: {1}", file.Name, _e.Message ) );
                }
            }
        }

        public void LoadClobDirectories( LobsterModel _model )
        {
            this.clobTypeToDirectoryMap = new Dictionary<ClobType, ClobDirectory>();
            foreach ( ClobType clobType in this.clobTypeList )
            {
                ClobDirectory clobDir = new ClobDirectory();
                clobDir.clobType = clobType;
                clobDir.parentModel = _model;
                bool result = this.PopulateClobDirectory( clobDir );
                if ( result )
                {
                    this.clobTypeToDirectoryMap.Add( clobType, clobDir );
                }
            }
        }

        public bool PopulateClobDirectory( ClobDirectory _clobDirectory )
        {
            DirectoryInfo info = new DirectoryInfo( Path.Combine( this.dbConfig.codeSource, _clobDirectory.clobType.directory ) );
            if ( !info.Exists )
            {
                MessageLog.Log( "Folder could not be found: " + info.FullName );
                LobsterMain.OnErrorMessage( "Folder not found", "Folder \"" + info.FullName + "\" could not be found for ClobType " + _clobDirectory.clobType.name );
                return false;
            }
            _clobDirectory.rootClobNode = new ClobNode( info, _clobDirectory );
            if ( _clobDirectory.clobType.includeSubDirectories )
            {
                PopulateClobNodeDirectories_r( _clobDirectory.rootClobNode, _clobDirectory );
            }
            return true;
        }

        public void PopulateClobNodeDirectories_r( ClobNode _clobNode, ClobDirectory _clobDirectory )
        {
            DirectoryInfo[] subDirs = _clobNode.dirInfo.GetDirectories();
            foreach ( DirectoryInfo subDir in subDirs )
            {
                ClobNode childNode = new ClobNode( subDir, _clobDirectory );
                PopulateClobNodeDirectories_r( childNode, _clobDirectory );
                _clobNode.childNodes.Add( childNode );
            }
        }

        public void GetWorkingFiles( ref List<ClobFile> _workingFiles )
        {
            if ( this.clobTypeToDirectoryMap == null )
            {
                return;
            }

            foreach ( KeyValuePair<ClobType, ClobDirectory> pair in this.clobTypeToDirectoryMap )
            {
                pair.Value.GetWorkingFiles( ref _workingFiles );
            }
        }
    }
}
