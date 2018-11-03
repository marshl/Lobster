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
namespace LobsterLite
{
    using System;
    using System.Collections.Generic;
    using System.IO;
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

            var optionSet = new OptionSet()
            {
                { "h|help",  "show this message and exit",
                  v => showHelp = v != null },
                //{"codesource=", "The code source directory", (string path) => codeSourcePath = path }
            };

            /*
             * options:
             * codesource 
             * codesource update file
             * codesource insert file
             * codesource delete file
             */

            MessageLog.Instance.EventListener += new EventHandler<MessageLogEventArgs>(delegate (Object o, MessageLogEventArgs e) { Console.WriteLine(e.Message.ToString()); });

            List<string> extra;
            try
            {
                extra = optionSet.Parse(args);
            }
            catch (OptionException e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("Try '--help' for more information.");
                return 1;
            }

            if (showHelp)
            {
                Program.ShowHelp(optionSet);
                return 0;
            }

            /*if (extra.Count < 2)
            {
                Console.WriteLine("Not enough arguments.");
                Program.ShowHelp(optionSet);
                return 1;
            }
            else if (extra.Count > 2)
            {
                Console.WriteLine("Too many arguments");
                Program.ShowHelp(optionSet);
                return 1;
            }*/

            string codeSourcePath = extra?[0];
            if (codeSourcePath == null)
            {
                ShowHelp(optionSet);
                return 1;
            }


            var exitEvent = new ManualResetEvent(false);

            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                exitEvent.Set();
            };

            bool result = Run(codeSourcePath);
            if (result)
            {
                exitEvent.WaitOne();
            }

            MessageLog.Close();
            return result ? 0 : 1;
        }

        /// <summary>
        /// Executes the program with the given arguments
        /// </summary>
        /// <param name="codeSourcePath">The directory of the CodeSource folder to use.</param>
        /// <param name="password">The password to connect to the database with.</param>
        /// <returns>Returns true if no error occurred, otherwise false.</returns>
        private static bool Run(string codeSourcePath)
        {
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
            if (!Int32.TryParse(Console.ReadLine(), out connectionIndex) || connectionIndex < 0 || connectionIndex >= config.ConnectionConfigList.Count)
            {
                MessageLog.LogError("Invalid selection");
                return false;
            }

            var selectedConfig = config.ConnectionConfigList[connectionIndex];
            Console.WriteLine("Please enter the password");
            string password = Console.ReadLine();
            SecureString securePassword = new SecureString();
            foreach (char c in password)
            {
                securePassword.AppendChar(c);
            }
            try
            {
                var connection = DatabaseConnection.CreateDatabaseConnection(selectedConfig, securePassword);
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
            Console.WriteLine("Usage: LobsterLite CodeSource password [OPTIONS]");
            optionSet.WriteOptionDescriptions(Console.Out);
        }
    }
}
