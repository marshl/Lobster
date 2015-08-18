using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
    }
}
