using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Lobster
{
    [XmlType("DatabaseConfig")]
    public class DatabaseConnection
    {
        [DisplayName( "Name" )]
        [Description( "The name of the connection. This is for display purposes only." )]
        public string name { get; set; }

        [DisplayName( "Host" )]
        [Description( "The host of the database." )]
        [Category( "Database" )]
        public string host { get; set; }

        [DisplayName( "Port" )]
        [Description( "The port the database is listening on. Usually 1521 for Oracle." )]
        [Category( "Database" )]
        public string port { get; set; }

        [DisplayName( "SID" )]
        [Description( "The Oracle System ID of the database." )]
        [Category( "Database" )]
        public string sid { get; set; }

        [DisplayName( "User Name" )]
        [Description( "The name of the user/schema to connect as. It is important to connect as a user with the privileges to access every table that could be modified by Lobster." )]
        [Category( "Database" )]
        public string username { get; set; }

        [DisplayName( "Password" )]
        [Description( "The password to connect with." )]
        [Category( "Database" )]
        public string password { get; set; }

        [DisplayName( "CodeSource Directory" )]
        [Description( "This is the location of the CodeSource directory that is used for this database. If it is invalid, Lobster will prompt you as it starts up." )]
        [Editor( typeof( FolderNameEditor ), typeof( UITypeEditor ) )]
        [Category( "Directories" )]
        public string codeSource { get; set; }

        [DisplayName( "Pooling" )]
        [Description( "If pooling is enabled, when Lobster connects to the Oracle database Oracle will remember the connection for a time, and reuse it if the same computer connects using the same connection string." )]
        public bool usePooling { get; set; }

        [DisplayName( "ClobType Directory" )]
        [Description( "ClobTypes are Lobster specific Xml files for describing the different tables located on the database and the rules that govern them." )]
        [Editor( typeof( FolderNameEditor ), typeof( UITypeEditor ) )]
        [Category( "Directories" )]
        public string clobTypeDir { get; set; }

        [XmlIgnore]
        public string fileLocation;

        [XmlIgnore]
        public List<ClobType> clobTypeList { get; set; }

        [XmlIgnore]
        public Dictionary<ClobType, ClobDirectory> clobTypeToDirectoryMap;

        public DatabaseConnection()
        {

        }

        public DatabaseConnection( DatabaseConnection _other )
        {
            this.name = _other.name;
            this.host = _other.host;
            this.sid = _other.sid;
            this.port = _other.port;
            this.username = _other.username;
            this.password = _other.password;
            this.codeSource = _other.codeSource;
            this.usePooling = _other.usePooling;
            this.clobTypeDir = _other.clobTypeDir;
            this.fileLocation = _other.fileLocation;
        }

        public static void Serialise( string _fullpath, DatabaseConnection _connection )
        {
            XmlSerializer xmls = new XmlSerializer( typeof( DatabaseConnection ) );
            using ( StreamWriter streamWriter = new StreamWriter( _fullpath ) )
            {
                xmls.Serialize( streamWriter, _connection );
            }
        }

        public void LoadClobTypes()
        {
            this.clobTypeList = new List<ClobType>();
            DirectoryInfo dirInfo = new DirectoryInfo( this.clobTypeDir );
            if ( !dirInfo.Exists )
            {
                MessageBox.Show( this.clobTypeDir + " could not be found.", "ClobType Load Failed", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1 );
                MessageLog.LogWarning( "The directory " + dirInfo + " could not be found when loading " + this.clobTypeDir );
                return;
            }

            foreach ( FileInfo file in dirInfo.GetFiles( "*.xml" ) )
            {
                try
                {
                    MessageLog.LogInfo( "Loading ClobType file " + file.FullName );
                    ClobType clobType = Common.DeserialiseXmlFileUsingSchema<ClobType>( file.FullName, "LobsterSettings/ClobType.xsd" );
                    
                    if ( !clobType.enabled )
                    {
                        MessageLog.LogWarning( "The ClobType file " + file.FullName + " was not loaded as it was marked as disabled." );
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
                        MessageLog.LogError( "An error occurred when loading the ClobType " + file.Name + " " + _e );
                        continue;
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
            DirectoryInfo info = new DirectoryInfo( Path.Combine( this.codeSource, _clobDirectory.clobType.directory ) );
            if ( !info.Exists )
            {
                MessageLog.LogWarning( info.FullName + " could not be found." );
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
