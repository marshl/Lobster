//-----------------------------------------------------------------------
// <copyright file="Common.cs" company="marshl">
// Copyright 2015, Liam Marshall, marshl.
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
namespace Lobster
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Windows.Forms;
    using System.Xml;
    using System.Xml.Schema;
    using Properties;

    /// <summary>
    /// A storage place for helper functions
    /// </summary>
    public abstract class Common
    {
        /// <summary>
        /// Found here http://stackoverflow.com/questions/50744/wait-until-file-is-unlocked-in-net?lq=1
        /// </summary>
        /// <param name="fullPath">The location of the file.</param>
        /// <param name="mode">The FileMode the file should be opened with.</param>
        /// <param name="access">The File Access rules the file should be opened with.</param>
        /// <param name="share"> The FileShare rules the file should be opened with.</param>
        /// <returns>A FileStream of the given file, if it could be opened.</returns>
        public static FileStream WaitForFile(string fullPath, FileMode mode, FileAccess access, FileShare share)
        {
            for (int numTries = 0; numTries < 10; numTries++)
            {
                try
                {
                    FileStream fs = new FileStream(fullPath, mode, access, share);

                    fs.ReadByte();
                    fs.Seek(0, SeekOrigin.Begin);

                    return fs;
                }
                catch (IOException)
                {
                    Thread.Sleep(50);
                }
            }

            return null;
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
        /// <returns>A new object of type T if it passes validation.</returns>
        public static T DeserialiseXmlFileUsingSchema<T>(string xmlFilename, string schemaFilename) where T : new()
        {
            bool error = false;
            XmlReaderSettings readerSettings = null;
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
                    error = true;
                    if (e.Severity == XmlSeverityType.Warning)
                    {
                        MessageLog.LogWarning(e.Message);
                    }
                    else if (e.Severity == XmlSeverityType.Error)
                    {
                        MessageLog.LogError(e.Message);
                    }
                });
            }

            T obj = new T();
            System.Xml.Serialization.XmlSerializer xmls = new System.Xml.Serialization.XmlSerializer(typeof(T));
            using (StreamReader streamReader = new StreamReader(xmlFilename))
            {
                XmlReader xmlReader = XmlReader.Create(streamReader, readerSettings);
                obj = (T)xmls.Deserialize(xmlReader);
                xmlReader.Close();
            }

            if (error)
            {
                throw new XmlSchemaValidationException("One or more validation errors occurred. Check the log for more information.");
            }

            return obj;
        }

        /// <summary>
        /// Parses a text file with the format key=value and adds the values to a map. 
        /// </summary>
        /// <param name="filename">The name of the file to parse.</param>
        /// <param name="outMap">The map to populate with the contents of the file.</param>
        public static void LoadFileIntoMap(string filename, out Dictionary<string, string> outMap)
        {
            outMap = new Dictionary<string, string>();
            StreamReader reader = new StreamReader(filename);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();

                // Trim anything after a # (comments)
                if (line.Contains('#'))
                {
                    line = line.Substring(line.IndexOf('#'));
                }

                // Ignore lines that do not contains an =
                if (line.Length == 0 || !line.Contains('='))
                {
                    continue;
                }

                string[] split = line.Split('=');
                string extension = split[0];
                string type = split[1];

                outMap.Add(extension, type);
            }

            reader.Close();
        }

        /// <summary>
        /// Displays a MessageBox with the given text and caption.
        /// </summary>
        /// <param name="caption">The MessageBox text.</param>
        /// <param name="text">The MessageBox caption.</param>
        /// <returns>The <see cref="DialogResult"/> of the displayed message box.</returns>
        public static DialogResult ShowErrorMessage(string caption, string text)
        {
            return MessageBox.Show(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
        }

        /// <summary>
        /// Displays a folder browser dialog and retuns the result.
        /// </summary>
        /// <param name="description">The description to be displayed on the dialog.</param>
        /// <param name="startingPath">The path for the dialog explorer to start.</param>
        /// <returns>The path of the directory that was selected, null if one wasn't selected.</returns>
        public static string PromptForDirectory(string description, string startingPath)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = description;
            fbd.SelectedPath = startingPath;

            DialogResult result = fbd.ShowDialog();
            if (result != DialogResult.OK)
            {
                return null;
            }

            return fbd.SelectedPath;
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
    }
}
