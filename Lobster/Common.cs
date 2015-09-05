using Newtonsoft.Json;
using RestSharp;
using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;

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
    }
}
