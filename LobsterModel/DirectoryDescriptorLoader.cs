using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using LobsterModel.Properties;

namespace LobsterModel
{
    class DirectoryDescriptorLoader
    {
        public List<DirectoryDescriptor> loadedDescriptors = new List<DirectoryDescriptor>();

        public string DescriptorFolderPath { get; }

        public DirectoryDescriptorLoader(string directoryDescriptorFolder)
        {
            this.DescriptorFolderPath = directoryDescriptorFolder;
        }

        /// <summary>
        /// Loads each of the  <see cref="DatabaseConfig"/> files in the connection directory, and returns the list.
        /// </summary>
        /// <param name="clobTypeDirectory">The directory to load the clob types from.</param>
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
        /// Loads the clob type with the given filepath and returns it.
        /// </summary>
        /// <param name="filePath">The full path of the file to load.</param>
        /// <returns>The ClobType, if loaded successfully, otherwise null.</returns>
        public bool LoadDirectoryDescriptor(string filePath, out DirectoryDescriptor directoryDescriptor)
        {
            MessageLog.LogInfo($"Loading ClobType {filePath}");
            try
            {
                string schema = Settings.Default.ClobTypeSchemaFilename;
                XmlSerializer xmls = new XmlSerializer(typeof(DirectoryDescriptor));
                FileStream stream = new FileStream(filePath, FileMode.Open);
                directoryDescriptor = (DirectoryDescriptor)xmls.Deserialize(stream);
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

        public event EventHandler DirectoryDescriptorLoadError;

        protected virtual void OnDirectoryDescriptorLoadError(EventArgs e)
        {
            DirectoryDescriptorLoadError?.Invoke(this, e);
        }

        class DirectoryDescriptorLoadErrorEventArgs : EventArgs
        {
            public string FilePath { get; }

            public Exception RaisedException { get; }

            public DirectoryDescriptorLoadErrorEventArgs(string filePath, Exception ex)
            {
                this.FilePath = filePath;
                this.RaisedException = ex;
            }
        }
    }
}
