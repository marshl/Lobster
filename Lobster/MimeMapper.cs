using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Lobster
{
    public class MimeMapper
    {
        public Dictionary<string, string> extensionToMimeType = new Dictionary<string, string>();
        public MimeMapper( string _path )
        {
            StreamReader reader = new StreamReader( _path );
            string line;
            while ( ( line = reader.ReadLine() ) != null )
            {
                line = line.Trim();
                if ( line.Contains( '#' ) )
                {
                    line = line.Substring( line.IndexOf( '#' ) );
                }

                if ( line.Length == 0 || !line.Contains('=') )
                {
                    continue;
                }
                
                string[] split = line.Split( '=' );
                string extension = split[0];
                string type = split[1];

                this.extensionToMimeType.Add( extension, type );
            }
            reader.Close();
        }
    }
}
