//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="marshl">
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
//       "There need not be, there would not be, any real change in our designs, only in our means."
//        - Saruman
//
//      [_The Lord of the Rings_, II / ii: "The Council of Elrond"]
//
//-----------------------------------------------------------------------
namespace LobsterLite
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security;
    using System.Threading;
    using LobsterModel;
    using Mono.Options;

    /// <summary>
    /// The main executable class
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The program entry point
        /// </summary>
        /// <param name="args">The command line arguments</param>
        /// <returns>A nonzero value if an error occurred, otherwise 0</returns>
        public static int Main(string[] args)
        {
            bool showHelp = false;

            string fileToDelete = null;
            string fileToUpdate = null;
            string fileToInsert = null;

            var optionSet = new OptionSet()
            {
                { "h|help",  "show this message and exit",
                  v => showHelp = v != null },
                 { "d|delete=",
                    "Deletes the given file from the database",
                  (string v) => fileToDelete = v },
                 { "p|push=",
                    "Pushes the given file to the database",
                  (string v) => fileToUpdate = v },
                 { "i|insert=",
                    "Inserts the given file into the database",
                  (string v) => fileToInsert = v },
            };

            List<string> extraArguments;
            try
            {
                extraArguments = optionSet.Parse(args);
            }
            catch (OptionException e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("Try '--help' for more information.");
                return 1;
            }

            // If the number of arguments is wrong, then show the help
            if (showHelp || extraArguments.Count != 1)
            {
                Program.ShowHelp(optionSet);
                return 0;
            }

            // Any messages logged should also be written to the console
            MessageLog.Instance.EventListener += new EventHandler<MessageLogEventArgs>((object o, MessageLogEventArgs e) =>
            {
                Console.WriteLine(e.Message.ToString());
            });

            int argumentCount = new[] { fileToDelete, fileToUpdate, fileToInsert }.Count(x => x != null);
            if (argumentCount > 1)
            {
                Console.WriteLine("Too many arguments");
                return 1;
            }

            string codeSourcePath = extraArguments?[0];
            if (codeSourcePath == null)
            {
                ShowHelp(optionSet);
                return 1;
            }

            bool result = OpenDatabaseConnection(codeSourcePath, out DatabaseConnection databaseConnection);

            // If the database connection was successful
            if (result)
            {
                // If the user wants to delete/update/insert a file
                if (argumentCount == 1)
                {
                    // Disable auto-pushing
                    databaseConnection.Config.AllowAutomaticClobbing = false;

                    // Get the path from among those arguments
                    string filepath = new[] { fileToDelete, fileToUpdate, fileToInsert }.First(x => x != null);

                    // And get the managed objects for that file
                    Tuple<DirectoryWatcher, WatchedFile> tuple = databaseConnection.GetWatchedNodeForPath(filepath);
                    if (tuple == null)
                    {
                        Console.WriteLine($"The file could not be found: {filepath}");
                        return 1;
                    }

                    try
                    {
                        if (fileToDelete != null)
                        {
                            databaseConnection.DeleteDatabaseFile(tuple.Item1, tuple.Item2);
                        }
                        else if (fileToUpdate != null)
                        {
                            databaseConnection.UpdateDatabaseFile(tuple.Item1, filepath);
                        }
                        else if (fileToInsert != null)
                        {
                            databaseConnection.InsertFile(tuple.Item1, filepath);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"An exception occurred: {ex}");
                        return 1;
                    }
                }
                else
                {
                    // Otherwise keep the connection open and wait for files to change
                    var exitEvent = new ManualResetEvent(false);
                    Console.CancelKeyPress += (sender, eventArgs) =>
                    {
                        eventArgs.Cancel = true;
                        exitEvent.Set();
                    };
                    MessageLog.LogInfo("Awaiting file updates. Press Ctrl+C to exit");
                    exitEvent.WaitOne();
                }
            }

            MessageLog.Close();
            return result ? 0 : 1;
        }

        /// <summary>
        /// Prompts the user for a password and returns it as a <see cref="SecureString"/>
        /// </summary>
        /// <remarks><see cref="https://stackoverflow.com/questions/3404421/password-masking-console-application"/></remarks>
        /// <returns>The password the user entered</returns>
        public static SecureString PromptforPassword()
        {
            Console.WriteLine("Enter password:");
            var pwd = new SecureString();
            while (true)
            {
                ConsoleKeyInfo i = Console.ReadKey(true);
                if (i.Key == ConsoleKey.Enter)
                {
                    break;
                }
                else if (i.Key == ConsoleKey.Backspace)
                {
                    if (pwd.Length > 0)
                    {
                        pwd.RemoveAt(pwd.Length - 1);
                        Console.Write("\b \b");
                    }
                }
                else if (i.KeyChar != '\u0000')
                {
                    // KeyChar == '\u0000' if the key pressed does not correspond to a printable character, e.g. F1, Pause-Break, etc
                    pwd.AppendChar(i.KeyChar);
                    Console.Write("*");
                }
            }

            return pwd;
        }

        /// <summary>
        /// Opens a database connection for the given CodeSource.
        /// The user will be prompted to select which connection should be used and the password for it.
        /// </summary>
        /// <param name="codeSourcePath">The full path of the CodeSource directory to open the connection for.</param>
        /// <param name="databaseConnection">The created <see cref="DatabaseConnection"/> if it is opened successfully, otherwise null.</param>
        /// <returns>True if the database connection is opened successfully, otherwise false.</returns>
        private static bool OpenDatabaseConnection(string codeSourcePath, out DatabaseConnection databaseConnection)
        {
            databaseConnection = null;
            if (!Directory.Exists(codeSourcePath))
            {
                MessageLog.LogError($"Could not find directory {codeSourcePath}");
                return false;
            }

            SettingsView modelSettings = new SettingsView();
            FileInfo configFile = new FileInfo(Path.Combine(codeSourcePath, modelSettings.DatabaseConfigFileName));

            if (!configFile.Exists)
            {
                MessageLog.LogError($"Could not find database config file {configFile.FullName}");
                return false;
            }

            CodeSourceConfigLoader loader = new CodeSourceConfigLoader();
            CodeSourceConfig config;
            loader.LoadCodeSourceConfig(codeSourcePath, out config);
            if (config == null)
            {
                MessageLog.LogError("The database configuration file could not be loaded.");
                return false;
            }

            if (config.ConnectionConfigList.Count == 0)
            {
                MessageLog.LogError("No connection configurations were found in the CodeSource config");
                return false;
            }

            Console.WriteLine($"PLease select a connection [0-{config.ConnectionConfigList.Count - 1}]:");
            for (int i = 0; i < config.ConnectionConfigList.Count; ++i)
            {
                var connectionConfig = config.ConnectionConfigList[i];
                Console.WriteLine($"{i}) {connectionConfig.Name} ({connectionConfig.Host}:{connectionConfig.Port}:{connectionConfig.SID})");
            }

            int connectionIndex;
            if (!int.TryParse(Console.ReadLine(), out connectionIndex) || connectionIndex < 0 || connectionIndex >= config.ConnectionConfigList.Count)
            {
                MessageLog.LogError("Invalid selection");
                return false;
            }

            var selectedConfig = config.ConnectionConfigList[connectionIndex];
            var securePassword = PromptforPassword();
            try
            {
                databaseConnection = DatabaseConnection.CreateDatabaseConnection(selectedConfig, securePassword);
            }
            catch (CreateConnectionException ex)
            {
                Console.WriteLine($"An error occurred when connecting to the database: {ex}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Displays a help message for the given OptionSet
        /// </summary>
        /// <param name="optionSet">The option set to display the help for.</param>
        private static void ShowHelp(OptionSet optionSet)
        {
            Console.WriteLine("Usage: .\\LobsterLite.exe CodeSource [--command filepath]");
            Console.WriteLine("If no commands are given, then this program will wait for files to be changed and the automatically push them.");
            optionSet.WriteOptionDescriptions(Console.Out);
        }
    }
}
