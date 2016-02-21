//-----------------------------------------------------------------------
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
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Xml;
    using System.Xml.Schema;
    using Properties;

    /// <summary>
    /// A storage place for helper functions
    /// </summary>
    public abstract class Utils
    {
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
            string filePath = System.Reflection.Assembly.GetCallingAssembly().Location;
            const int HeaderOffset = 60;
            const int LinkerTimestampOffset = 8;
            byte[] b = new byte[2048];
            Stream s = null;

            try
            {
                s = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                s.Read(b, 0, 2048);
            }
            finally
            {
                if (s != null)
                {
                    s.Close();
                }
            }

            int i = BitConverter.ToInt32(b, HeaderOffset);
            int secondsSince1970 = BitConverter.ToInt32(b, i + LinkerTimestampOffset);
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            dt = dt.AddSeconds(secondsSince1970);
            dt = dt.ToLocalTime();
            return dt;
        }

        /// <summary>
        /// Parses the given file and returns an objcet of the templated type.
        /// Throws anXmlSchemaValidationException if the file does not pass the schema check.
        /// </summary>
        /// <typeparam name="T">The type of the object to parse the file as, and the type returned.</typeparam>
        /// <param name="xmlFilename">The location of the xml file to parse.</param>
        /// <param name="schemaFilename">The location of the schema file to validate the XML with.</param>
        /// <param name="result">A new object of type T if it passes validation.</param>
        /// <returns>A value indicating whether the file deserialised successfully, otherwise false.</returns>
        public static bool DeserialiseXmlFileUsingSchema<T>(string xmlFilename, string schemaFilename, out T result) where T : SerializableObject, new()
        {
            T obj = new T();
            XmlReaderSettings readerSettings = null;
            List<ValidationEventArgs> validationEvents = new List<ValidationEventArgs>();
            if (schemaFilename != null)
            {
                readerSettings = new XmlReaderSettings();
                readerSettings.Schemas.Add(null, schemaFilename);
                readerSettings.ValidationType = ValidationType.Schema;
                readerSettings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;
                readerSettings.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation;
                readerSettings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;

                readerSettings.ValidationEventHandler += new ValidationEventHandler(
                (o, e) =>
                {
                    if (e.Severity == XmlSeverityType.Warning)
                    {
                        MessageLog.LogWarning(e.Message);
                    }
                    else if (e.Severity == XmlSeverityType.Error)
                    {
                        MessageLog.LogError(e.Message);
                    }

                    validationEvents.Add(e);
                });
            }

            System.Xml.Serialization.XmlSerializer xmls = new System.Xml.Serialization.XmlSerializer(typeof(T));

            try
            {
                using (StreamReader streamReader = new StreamReader(xmlFilename))
                {
                    XmlReader xmlReader = XmlReader.Create(streamReader, readerSettings);
                    obj = (T)xmls.Deserialize(xmlReader);
                    xmlReader.Close();
                    obj.ErrorList = validationEvents.Select(item => item.Message).ToList();
                    result = obj;
                    return true;
                }
            }
            catch (Exception ex ) when ( ex is InvalidOperationException || ex is UnauthorizedAccessException)
            {
                obj = new T();
                obj.ErrorList.Add(ex.Message);
                result = obj;
                return false;
            }
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
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
            if (byteCount == 0)
            {
                return "0" + suf[0];
            }

            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString() + " " + suf[place];
        }

        /// <summary>
        /// Opens a file with the given name in explorer.
        /// </summary>
        /// <param name="fullName">The file to open in explorer.</param>
        public static void OpenFileInExplorer(string fullName)
        {
            Process.Start("explorer", $"/select,{fullName}");
        }
    }
}
