using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Schema;
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
                MessageLog.LogWarning( String.Format( "The directory {0} could not be found when loading {1}.", clobTypeDir, this.dbConfig.name ) );
                return;
            }

            foreach ( FileInfo file in clobTypeDir.GetFiles( "*.xml" ) )
            {
                try
                {
                    MessageLog.LogInfo( "Loading ClobType file {0}", file.FullName );
                    ClobType clobType = Common.DeserialiseXmlFileUsingSchema<ClobType>( file.FullName, "LobsterSettings/ClobType.xsd" );
                    
                    if ( !clobType.enabled )
                    {
                        MessageLog.LogWarning( "The ClobType file {0} was not loaded as it was marked as disabled.", file.FullName );
                        continue;
                    }

                    clobType.Initialise();

                    this.clobTypeList.Add( clobType );
                }
                catch ( Exception _e )
                {
                    if ( _e is InvalidOperationException || _e is XmlException || _e is XmlSchemaValidationException || _e is IOException )
                    {
                        MessageBox.Show( "The ClobType " + file.Name + " failed to load. Check the log for more information.", "ClobType Load Failed",
                            MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1 );
                        MessageLog.LogError( "An error occurred when loading the ClobType {0}: {1}", file.Name, _e.Message );
                        return;
                    }
                    throw;
                }
            }
        }

        public void PopulateClobDirectories( LobsterModel _model )
        {
            this.clobTypeToDirectoryMap = new Dictionary<ClobType, ClobDirectory>();
            foreach ( ClobType clobType in this.clobTypeList )
            {
                ClobDirectory clobDir = new ClobDirectory();
                clobDir.clobType = clobType;
                clobDir.parentModel = _model;
                bool success = this.PopulateClobDirectory( clobDir );
                if ( success )
                {
                    this.clobTypeToDirectoryMap.Add( clobType, clobDir );
                }
            }
        }

        private bool PopulateClobDirectory( ClobDirectory _clobDirectory )
        {
            DirectoryInfo info = new DirectoryInfo( Path.Combine( this.dbConfig.codeSource, _clobDirectory.clobType.directory ) );
            if ( !info.Exists )
            {
                MessageLog.LogWarning( "{0} could not be found.", info.FullName );
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
