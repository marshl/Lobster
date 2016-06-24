using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using LobsterModel;
using Mono.Options;
using System.Linq;
using System.Threading;

namespace LobsterLite
{
    class Program
    {
        static int Main(string[] args)
        {
            MessageLog messageLog = MessageLog.Initialise();

            bool show_help = false;
            string username = "XVIEWMGR";
            int port = 1521;
            //string password = null;
            string hostname = null;
            string sid = null;
            string directory = null;

            var optionSet = new OptionSet()
            {
                { "u|username=",
                    "the username to log in as.\n" +
                        "default xviewmgr.",
                  (string v) => username = v },
                { "r|port=",
                    "the port to connect to\n" +
                        "default 1521.",
                  (int v) => port = v },
                 /*{ "p|password=",
                    "the password of the user\n",
                  (string v) => password = v },*/
                 { "o|host=",
                    "the host to connect to\n",
                  (string v) => hostname = v },
                 { "s|sid=",
                    "the Oracle Service ID\n",
                  (string v) => sid = v },
                { "d|directory=",
                    "the CodeSource directory to compare with.\n",
                  (string v) => directory = v },
                { "h|help",  "show this message and exit",
                  v => show_help = v != null },
            };

            List<string> extra;
            try
            {
                extra = optionSet.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write("gman: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `gman --help' for more information.");
                return 1;
            }

            if (show_help)
            {
                ShowHelp(optionSet);
                return 0;
            }

            string message;
            if (extra.Count < 2)
            {
                Console.WriteLine("Not enough arguments.");
                ShowHelp(optionSet);
                return 1;
            }
            else if (extra.Count > 2)
            {
                Console.WriteLine("Too many arguments");
                ShowHelp(optionSet);
                return 1;
            }

            /*if (password == null || hostname == null || sid == null || directory == null)
            {
                ShowHelp(p);
                return;
            }*/
            string codeSourcePath = extra[0];
            string password = extra[1];
            SecureString securePassword = new SecureString();
            foreach (char c in password)
            {
                securePassword.AppendChar(c);
            }

            var exitEvent = new ManualResetEvent(false);

            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                exitEvent.Set();
            };

            bool result = Run(codeSourcePath, securePassword);

            exitEvent.WaitOne();
            MessageLog.Close();
            return result ? 0 : 1;
        }

        private static bool Run(string codeSourcePath, SecureString password)
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

            DatabaseConfig config = DatabaseConfig.LoadDatabaseConfig(configFile.FullName);
            if (config == null)
            {
                MessageLog.LogError("The database configuration file could not be loaded.");
                return false;
            }

            DatabaseConnection connection = Model.SetDatabaseConnection(config, password);

            return true;
        }

        /// <summary>
        /// Displays a help message for the given OptionSet
        /// </summary>
        /// <param name="optionSet">The option set to display the help for.</param>
        private static void ShowHelp(OptionSet optionSet)
        {
            /*Console.WriteLine("Usage: greet [OPTIONS]+ message");
            Console.WriteLine("Greet a list of individuals with an optional message.");
            Console.WriteLine("If no message is specified, a generic greeting is used.");
            Console.WriteLine();*/
            Console.WriteLine("Options:");
            optionSet.WriteOptionDescriptions(Console.Out);
        }
    }
}
