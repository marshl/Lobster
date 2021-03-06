﻿//-----------------------------------------------------------------------
// <copyright file="Utils.cs" company="marshl">
// Copyright 2016, Liam Marshall, marshl.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//-----------------------------------------------------------------------
//
//      'Very useful, no doubt, that was to Saruman; yet it seems that he was not content.'
//          -- Gandalf
//
//      [ _The Lord of the Rings_, III/xi: "The Palantir"]
//
//-----------------------------------------------------------------------
namespace LobsterModel
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Threading;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;
    using Properties;

    /// <summary>
    /// A storage place for helper functions
    /// </summary>
    public abstract class Utils
    {
        /// <summary>
        /// The last retreived linker timestamp
        /// </summary>
        private static DateTime? linkerTimestamp;

        /// <summary>
        /// Attempts to read a file, even if the file is locked. Tries 10 times before throwing an IOException
        /// </summary>
        /// <param name="fullPath">The location of the file.</param>
        /// <param name="mode">The FileMode the file should be opened with.</param>
        /// <param name="access">The File Access rules the file should be opened with.</param>
        /// <param name="share"> The FileShare rules the file should be opened with.</param>
        /// <returns>A FileStream of the given file, if it could be opened.</returns>
        /// <remarks>Found here http://stackoverflow.com/questions/50744/wait-until-file-is-unlocked-in-net</remarks>
        public static FileStream WaitForFile(string fullPath, FileMode mode, FileAccess access, FileShare share)
        {
            Exception ex = null;
            for (int numTries = 0; numTries < 10; numTries++)
            {
                try
                {
                    FileStream fs = new FileStream(fullPath, mode, access, share);

                    fs.ReadByte();
                    fs.Seek(0, SeekOrigin.Begin);

                    return fs;
                }
                catch (Exception e)
                {
                    ex = e;
                    Thread.Sleep(50);
                }
            }

            throw ex;
        }

        /// <summary>
        /// From http://stackoverflow.com/questions/1600962/displaying-the-build-date
        /// </summary>
        /// <returns>The DateTime taht the assembly was linked</returns>
        public static DateTime RetrieveLinkerTimestamp()
        {
            if (linkerTimestamp.HasValue)
            {
                return linkerTimestamp.Value;
            }

            string filePath = System.Reflection.Assembly.GetCallingAssembly().Location;
            const int HeaderOffset = 60;
            const int LinkerTimestampOffset = 8;
            byte[] b = new byte[2048];

            using (var s = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                s.Read(b, 0, 2048);
            }

            int i = BitConverter.ToInt32(b, HeaderOffset);
            int secondsSince1970 = BitConverter.ToInt32(b, i + LinkerTimestampOffset);
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            dt = dt.AddSeconds(secondsSince1970);
            dt = dt.ToLocalTime();

            linkerTimestamp = dt;
            return linkerTimestamp.Value;
        }

        /// <summary>
        /// Returns the path in the directory of a temporary file with the name of the given original file plus a UID.
        /// </summary>
        /// <param name="originalFilename">The name of the original file, if it existed.</param>
        /// <returns>The path of the temporary file location.</returns>
        public static string GetTempFilepath(string originalFilename)
        {
            DirectoryInfo tempDir = new DirectoryInfo(Settings.Default.TempFileDirectory);
            if (!tempDir.Exists)
            {
                tempDir.Create();
            }

            return Path.Combine(tempDir.FullName, Guid.NewGuid() + originalFilename);
        }

        /// <summary>
        /// Converts any number of bytes into a human readable string containing the most significant byte size with 2 decimal places.
        /// </summary>
        /// <param name="byteCount">The number of bytes to convert.</param>
        /// <returns>The human readable string.</returns>
        public static string BytesToString(long byteCount)
        {
            // Longs run out around EB
            string[] suffixes = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
            if (byteCount == 0)
            {
                return "0" + suffixes[0];
            }

            long bytes = Math.Abs(byteCount);
            int suffixIndex = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, suffixIndex), 1);
            return $"{Math.Sign(byteCount) * num} {suffixes[suffixIndex]}";
        }

        /// <summary>
        /// Opens a file with the given name in explorer.
        /// </summary>
        /// <param name="fullName">The file to open in explorer.</param>
        public static void OpenFileInExplorer(string fullName)
        {
            Process.Start("explorer", $"/select,{fullName}");
        }

        /// <summary>
        /// Converts a SecureString to a System.String
        /// </summary>
        /// <param name="value">The secure string to convert.</param>
        /// <returns>The string representation of the string.</returns>
        /// <remarks>Found here: http://stackoverflow.com/questions/818704/how-to-convert-securestring-to-system-string</remarks>
        public static string SecureStringToString(SecureString value)
        {
            IntPtr valuePtr = IntPtr.Zero;
            try
            {
                valuePtr = Marshal.SecureStringToGlobalAllocUnicode(value);
                return Marshal.PtrToStringUni(valuePtr);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
            }
        }
    }
}
