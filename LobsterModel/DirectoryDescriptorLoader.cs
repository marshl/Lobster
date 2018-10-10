//-----------------------------------------------------------------------
// <copyright file="DirectoryDescriptorLoader.cs" company="marshl">
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
namespace LobsterModel
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml;
    using System.Xml.Serialization;
    using Properties;

    /// <summary>
    /// A utility class for loading <see cref="DirectoryDescriptor"/>s.
    /// </summary>
    public class DirectoryDescriptorLoader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DirectoryDescriptorLoader"/> class.
        /// </summary>
        /// <param name="directoryDescriptorFolder">A path to the folder where the descriptors will be loaded from.</param>
        public DirectoryDescriptorLoader(string directoryDescriptorFolder)
        {
            this.DescriptorFolderPath = directoryDescriptorFolder;
        }

        /// <summary>
        /// The event called whenever an error occurs when loading the a <see cref="DirectoryDescriptor"/>.
        /// </summary>
        public event EventHandler<DirectoryDescriptorLoadErrorEventArgs> DirectoryDescriptorLoadError;

        /// <summary>
        /// Gets the folder where the directory descriptors are loaded from.
        /// </summary>
        public string DescriptorFolderPath { get; }

        /// <summary>
        /// Loads each of the  <see cref="DirectoryDescriptor"/> files in the connection directory, and returns the list.
        /// </summary>
        /// <returns>All valid config files in the connection directory.</returns>
        public List<DirectoryDescriptor> GetDirectoryDescriptorList()
        {
            List<DirectoryDescriptor> dirDescList = new List<DirectoryDescriptor>();

            DirectoryInfo dir = new DirectoryInfo(this.DescriptorFolderPath);

            if (!dir.Exists)
            {
                return dirDescList;
            }

            foreach (FileInfo filename in dir.GetFiles("*.xml"))
            {
                DirectoryDescriptor dirDesc;
                if (this.LoadDirectoryDescriptor(filename.FullName, out dirDesc))
                {
                    dirDescList.Add(dirDesc);
                }
            }

            return dirDescList;
        }

        /// <summary>
        /// Loads the directory descriptor with the given filepath and returns it.
        /// </summary>
        /// <param name="filePath">The full path of the file to load.</param>
        /// <param name="directoryDescriptor">The loaded directory descriptor.</param>
        /// <returns>True uf the descriptor was loaded successfully, otherwise false.</returns>
        public bool LoadDirectoryDescriptor(string filePath, out DirectoryDescriptor directoryDescriptor)
        {
            MessageLog.LogInfo($"Loading DirectoryDescriptor {filePath}");
            try
            {
                string schema = Settings.Default.DirectoryDescriptorSchemaFilename;
                XmlSerializer xmls = new XmlSerializer(typeof(DirectoryDescriptor));
                using (FileStream stream = new FileStream(filePath, FileMode.Open))
                {
                    directoryDescriptor = (DirectoryDescriptor)xmls.Deserialize(stream);
                }

                directoryDescriptor.FilePath = filePath;
                return true;
            }
            catch (Exception ex) when (ex is FileNotFoundException || ex is InvalidOperationException || ex is XmlException || ex is IOException)
            {
                MessageLog.LogError($"An error occurred when loading the DirectoryDescriptor {filePath}: {ex}");
                this.OnDirectoryDescriptorLoadError(new DirectoryDescriptorLoadErrorEventArgs(filePath, ex));
                directoryDescriptor = null;
                return false;
            }
        }

        /// <summary>
        /// Invokes <see cref="DirectoryDescriptorLoadError"/> with the given event arguments.
        /// </summary>
        /// <param name="eventArgs">The event arguments.</param>
        protected virtual void OnDirectoryDescriptorLoadError(DirectoryDescriptorLoadErrorEventArgs eventArgs)
        {
            this.DirectoryDescriptorLoadError?.Invoke(this, eventArgs);
        }

        /// <summary>
        /// The event arguments for the <see cref="DirectoryDescriptorLoadError"/> event.
        /// </summary>
        public class DirectoryDescriptorLoadErrorEventArgs : EventArgs
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="DirectoryDescriptorLoadErrorEventArgs"/> class.
            /// </summary>
            /// <param name="filePath">The path of the <see cref="DirectoryDescriptor"/> file that failed to load.</param>
            /// <param name="ex">The exception that was raised when loading the <see cref="DirectoryDescriptor"/>.</param>
            public DirectoryDescriptorLoadErrorEventArgs(string filePath, Exception ex)
            {
                this.FilePath = filePath;
                this.RaisedException = ex;
            }

            /// <summary>
            /// Gets the path of the <see cref="DirectoryDescriptor"/> that failed to load.
            /// </summary>
            public string FilePath { get; }

            /// <summary>
            /// Gets the exception that was raised when loading the <see cref="DirectoryDescriptor"/>.
            /// </summary>
            public Exception RaisedException { get; }
        }
    }
}
