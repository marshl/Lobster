using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using LobsterModel.Properties;

namespace LobsterModel
{
    public class CodeSourceConfigLoader
    {
        public List<CodeSourceConfig> CodeSourceConfigList { get; set; } = new List<CodeSourceConfig>();

        public CodeSourceConfigLoader()
        {

        }

        public event EventHandler<CodeSourceLoadErrorEventArgs> CodeSourceLoadErrorEvent;

        /// <summary>
        /// Deserialises the given file into a new <see cref="DatabaseConnection"/>.
        /// </summary>
        /// <param name="codeSourceDirectory">The location of the file to deserialise.</param>
        /// <returns>The new <see cref="DatabaseConnection"/>.</returns>
        public bool LoadCodeSourceConfig(string codeSourceDirectory, ref CodeSourceConfig codeSourceConfig)
        {
            // TODO: Make this private
            MessageLog.LogInfo($"Loading Lobster CodeSource Directory {codeSourceDirectory}");

            FileInfo configFile = new FileInfo(Path.Combine(codeSourceDirectory, Settings.Default.DatabaseConfigFileName));

            if (!configFile.Exists)
            {
                MessageLog.LogError($"{configFile.FullName} could not be found.");
                this.OnCodeSourceLoadError(codeSourceDirectory);
                return false;
            }

            try
            {
                DataContractSerializer xmls = new DataContractSerializer(typeof(CodeSourceConfig));
                using (FileStream fileStream = new FileStream(configFile.FullName, FileMode.Open, FileAccess.Read))
                {
                    codeSourceConfig = (CodeSourceConfig)xmls.ReadObject(fileStream);
                    codeSourceConfig.FileLocation = configFile.FullName;
                    foreach (ConnectionConfig config in codeSourceConfig.ConnectionConfigList)
                    {
                        config.Parent = codeSourceConfig;
                    }
                }

                return true;
            }
            catch (Exception e) when (e is FileNotFoundException || e is InvalidOperationException || e is SerializationException || e is IOException)
            {
                MessageLog.LogError($"An error occurred when loading the CodeSourceConfig {configFile.FullName}: {e}");
                this.OnCodeSourceLoadError(codeSourceDirectory, e);
                return false;
            }
        }

        private void OnCodeSourceLoadError(string codeSourceDirectory)
        {
            this.CodeSourceLoadErrorEvent?.Invoke(this, new CodeSourceLoadErrorEventArgs(codeSourceDirectory));
        }

        private void OnCodeSourceLoadError(string codeSourceDirectory, Exception ex)
        {
            this.CodeSourceLoadErrorEvent?.Invoke(this, new CodeSourceLoadErrorEventArgs(codeSourceDirectory, ex));
        }

        public class CodeSourceLoadErrorEventArgs : EventArgs
        {
            public string FilePath { get; }

            public Exception ExceptionObj { get; }

            public CodeSourceLoadErrorEventArgs(string filePath)
            {
                this.FilePath = filePath;
            }

            public CodeSourceLoadErrorEventArgs(string filePath, Exception ex)
            {
                this.FilePath = filePath;
                this.ExceptionObj = ex;
            }
        }

        /// <summary>
        /// Loads each of the  <see cref="DatabaseConfig"/> files in the connection directory, and returns the list.
        /// </summary>
        /// <returns>All valid config files in the connection directory.</returns>
        public void Load()
        {
            if (Settings.Default.CodeSourceDirectories == null || Settings.Default.CodeSourceDirectories.Count == 0)
            {
                return;
            }

            List<string> invalidDirectories = new List<string>();
            foreach (string directoryName in Settings.Default.CodeSourceDirectories)
            {
                string configFile = Path.Combine(directoryName, Settings.Default.DatabaseConfigFileName);
                if (!File.Exists(configFile))
                {
                    invalidDirectories.Add(directoryName);
                    continue;
                }

                CodeSourceConfig config = null;
                if (this.LoadCodeSourceConfig(directoryName, ref config))
                {
                    this.CodeSourceConfigList.Add(config);
                }
                else
                {
                    invalidDirectories.Add(directoryName);
                }
            }

            foreach (string str in invalidDirectories)
            {
                Settings.Default.CodeSourceDirectories.Remove(str);
            }

            Settings.Default.Save();
        }

        /// <summary>
        /// Initialises the given directory with the necessary configuration files and folders.
        /// </summary>
        /// <param name="directory">The directory to initialise.</param>
        /// <param name="newConfig">The database configuration file that is created.</param>
        /// <param name="name">The mnemonic for the CodeSource directory.</param>
        /// <returns>True if the directory was set up  correctly, false if there was an issue setting it up, or if the directory already has them.</returns>
        public static bool InitialiseCodeSourceDirectory(string directory, string name, ref CodeSourceConfig newConfig)
        {
            try
            {
                Directory.CreateDirectory(Path.Combine(directory, Settings.Default.ClobTypeDirectoryName));
                newConfig = new CodeSourceConfig();
                newConfig.FileLocation = Path.Combine(directory, Settings.Default.DatabaseConfigFileName);
                newConfig.Name = name;
                newConfig.SerialiseToFile();

                if (!AddCodeSourceDirectory(directory))
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                return false;
            }
        }

        /// <summary>
        /// Adds the given directory to the CodeSource dirctory list.
        /// </summary>
        /// <param name="directoryName">The directory to add.</param>
        /// <returns>True if the directory is new and was added ok, false if the directory was already being used.</returns>
        public static bool AddCodeSourceDirectory(string directoryName)
        {
            if (Settings.Default.CodeSourceDirectories == null)
            {
                Settings.Default.CodeSourceDirectories = new System.Collections.Specialized.StringCollection();
            }
            else if (Settings.Default.CodeSourceDirectories.Contains(directoryName))
            {
                return false;
            }

            Settings.Default.CodeSourceDirectories.Add(directoryName);
            Settings.Default.Save();
            return true;
        }

        /// <summary>
        /// Validates that the given direcotry is currently a valid CodeSource folder (it has the correct files and folders).
        /// </summary>
        /// <param name="directory">The directory to check.</param>
        /// <param name="errorMessage">The error message to return (if any).</param>
        /// <returns>True if the directory is valid, otherwise false (with the error message initialised).</returns>
        public static bool ValidateCodeSourceLocation(string directory, ref string errorMessage)
        {
            if (Settings.Default.CodeSourceDirectories != null && Settings.Default.CodeSourceDirectories.Contains(directory))
            {
                errorMessage = $"The chosen directory {directory} has already been used.";
                return false;
            }

            DirectoryInfo dirInfo = new DirectoryInfo(directory);
            if (!dirInfo.Exists)
            {
                errorMessage = $"The chosen directory {directory} does not exist, or is not a directory";
                return false;
            }

            DirectoryInfo lobsterTypeFolder = new DirectoryInfo(Path.Combine(directory, Settings.Default.ClobTypeDirectoryName));
            if (!lobsterTypeFolder.Exists)
            {
                errorMessage = $"The chosen folder must contain a directory named {Settings.Default.ClobTypeDirectoryName}";
                return false;
            }

            FileInfo configFile = new FileInfo(Path.Combine(dirInfo.FullName, Settings.Default.DatabaseConfigFileName));
            if (!configFile.Exists)
            {
                errorMessage = $"The chosen folder must contain a file named {Settings.Default.DatabaseConfigFileName}";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates whether the given directory is a vliad place to set as a new CodeSOurce location.
        /// </summary>
        /// <param name="directory">The directory to validate.</param>
        /// <param name="errorMessage">Any error message that occurred, if any.</param>
        /// <returns>True if the location is valid, otherwise false.</returns>
        public static bool ValidateNewCodeSourceLocation(string directory, ref string errorMessage)
        {
            if (Settings.Default.CodeSourceDirectories != null && Settings.Default.CodeSourceDirectories.Contains(directory))
            {
                errorMessage = $"The chosen directory {directory} has already been used.";
                return false;
            }

            DirectoryInfo dirInfo = new DirectoryInfo(directory);
            if (!dirInfo.Exists)
            {
                errorMessage = $"The chosen directory {directory} does not exist, or is not a directory";
                return false;
            }

            FileInfo configFile = new FileInfo(Path.Combine(directory, Settings.Default.DatabaseConfigFileName));
            if (configFile.Exists)
            {
                errorMessage = $"The chosen folder already contains a file named {Settings.Default.DatabaseConfigFileName}";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Removes the CodeSource folder with the given name from the known list.
        /// </summary>
        /// <param name="directoryName">The  name of the directory to remove.</param>
        /// <returns>True if the directory already exists and was removed, otherwise false.</returns>
        public static bool RemoveCodeSource(string directoryName)
        {
            if (Settings.Default.CodeSourceDirectories == null || !Settings.Default.CodeSourceDirectories.Contains(directoryName))
            {
                return false;
            }

            Settings.Default.CodeSourceDirectories.Remove(directoryName);
            return true;
        }
    }
}
