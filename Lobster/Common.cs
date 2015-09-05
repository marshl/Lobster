using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Schema;

namespace Lobster
{
    class Common
    {
        /// <summary>
        /// 
        /// http://stackoverflow.com/questions/50744/wait-until-file-is-unlocked-in-net?lq=1
        /// </summary>
        /// <param name="fullPath"></param>
        /// <param name="mode"></param>
        /// <param name="access"></param>
        /// <param name="share"></param>
        /// <returns></returns>
        public static FileStream WaitForFile( string fullPath, FileMode mode, FileAccess access, FileShare share )
        {
            for ( int numTries = 0; numTries < 10; numTries++ )
            {
                try
                {
                    FileStream fs = new FileStream( fullPath, mode, access, share );

                    fs.ReadByte();
                    fs.Seek( 0, SeekOrigin.Begin );

                    return fs;
                }
                catch ( IOException )
                {
                    Thread.Sleep( 50 );
                }
            }

            return null;
        }

        //http://stackoverflow.com/questions/1600962/displaying-the-build-date
        public static DateTime RetrieveLinkerTimestamp()
        {
            string filePath = System.Reflection.Assembly.GetCallingAssembly().Location;
            const int c_PeHeaderOffset = 60;
            const int c_LinkerTimestampOffset = 8;
            byte[] b = new byte[2048];
            Stream s = null;

            try
            {
                s = new FileStream( filePath, FileMode.Open, FileAccess.Read );
                s.Read( b, 0, 2048 );
            }
            finally
            {
                if ( s != null )
                {
                    s.Close();
                }
            }

            int i = BitConverter.ToInt32( b, c_PeHeaderOffset );
            int secondsSince1970 = BitConverter.ToInt32( b, i + c_LinkerTimestampOffset );
            DateTime dt = new DateTime( 1970, 1, 1, 0, 0, 0, DateTimeKind.Utc );
            dt = dt.AddSeconds( secondsSince1970 );
            dt = dt.ToLocalTime();
            return dt;
        }

        public static T DeserialiseXmlFileUsingSchema<T>( string _xmlPath, string _schemaPath) where T : new()
        {
            XmlReaderSettings readerSettings = new XmlReaderSettings();
            readerSettings.Schemas.Add( null, _schemaPath );
            readerSettings.ValidationType = ValidationType.Schema;
            readerSettings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;
            readerSettings.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation;
            readerSettings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;

            bool error = false;
            readerSettings.ValidationEventHandler += new ValidationEventHandler(
            ( o, e ) =>
            {
                if ( e.Severity == XmlSeverityType.Warning )
                {
                    MessageLog.LogWarning( e.Message );
                }
                else if ( e.Severity == XmlSeverityType.Error )
                {
                    MessageLog.LogError( e.Message );
                }
                error = true;
            } );

            error = false;
            T obj = new T();
            System.Xml.Serialization.XmlSerializer xmls = new System.Xml.Serialization.XmlSerializer( typeof( T ) );
            StreamReader streamReader = new StreamReader( _xmlPath );
            XmlReader xmlReader = XmlReader.Create( streamReader, readerSettings );

            obj = (T)xmls.Deserialize( xmlReader );
            xmlReader.Close();
            streamReader.Close();

            if ( error )
            {
                throw new XmlSchemaValidationException( "One or more validation errors occurred. Check the log for more information." );
            }
            
            return obj;
        }

        public static void LoadFileIntoMap( string _path, out Dictionary<string, string> _map )
        {
            _map = new Dictionary<string, string>();
            StreamReader reader = new StreamReader( _path );
            string line;
            while ( ( line = reader.ReadLine() ) != null )
            {
                line = line.Trim();
                if ( line.Contains( '#' ) )
                {
                    line = line.Substring( line.IndexOf( '#' ) );
                }

                if ( line.Length == 0 || !line.Contains( '=' ) )
                {
                    continue;
                }

                string[] split = line.Split( '=' );
                string extension = split[0];
                string type = split[1];

                _map.Add( extension, type );
            }
            reader.Close();
        }
    }
}
