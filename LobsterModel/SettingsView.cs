//-----------------------------------------------------------------------
// <copyright file="SettingsView.cs" company="marshl">
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
//      "Bless us and splash us, my precioussss! I guess it's a choice feast; at least a
//      tasty morsel it'd make us, gollum!"
//          -- Gollum
//      [ _The Hobbit_, V: "Riddles in the Dark"]
//
//-----------------------------------------------------------------------
namespace LobsterModel
{
    using Properties;

    /// <summary>
    /// A view access to all of the editable settings for the model.
    /// </summary>
    public class SettingsView
    {
        /// <summary>
        /// Gets or sets a value indicating whether files should be automatically backed up.
        /// </summary>
        public bool IsBackupEnabled
        {
            get
            {
                return Settings.Default.BackupEnabled;
            }

            set
            {
                Settings.Default.BackupEnabled = value;
                Settings.Default.Save();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether messages with the [SENSITIVE] type should be written out to log.
        /// </summary>
        public bool LogSenstiveMessages
        {
            get
            {
                return Settings.Default.LogSensitiveMessages;
            }

            set
            {
                Settings.Default.LogSensitiveMessages = value;
                Settings.Default.Save();
            }
        }

        /// <summary>
        /// Gets or sets the duration during which file events for files already updated should be ignored.
        /// </summary>
        public int FileUpdateTimeoutMilliseconds
        {
            get
            {
                return Settings.Default.FileUpdateTimeoutMilliseconds;
            }

            set
            {
                Settings.Default.FileUpdateTimeoutMilliseconds = value;
                Settings.Default.Save();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether Clob or Xml type files should have footers appended to them.
        /// </summary>
        public bool AppendFooterToDatabaseFiles
        {
            get
            {
                return Settings.Default.AppendFooterToDatabaseFiles;
            }

            set
            {
                Settings.Default.AppendFooterToDatabaseFiles = value;
                Settings.Default.Save();
            }
        }
    }
}
