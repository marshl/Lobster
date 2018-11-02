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
//      'But it is a heavy burden. So heavy that none could lay it
//      on another.I do not lay it on you.But if you take it freely, I will say that
//      your choice is right'
//          -- Elrond
//
//      [ _The Lord of the Rings_, II/ii: "The Council of Elrond"]
//
//-----------------------------------------------------------------------
namespace LobsterWpf.ViewModels
{
    using System.Collections.Specialized;
    using Properties;

    /// <summary>
    /// The view for both the model settings and the view settings.
    /// </summary>
    public class SettingsView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsView"/> class.
        /// </summary>
        public SettingsView()
        {
            this.ModelSettings = new LobsterModel.SettingsView();
        }

        /// <summary>
        /// Gets the settings view for the model.
        /// </summary>
        public LobsterModel.SettingsView ModelSettings { get; }

        /// <summary>
        /// Gets or sets the name of the program used to diff files.
        /// </summary>
        public string DiffProgramName
        {
            get
            {
                return Settings.Default.DiffProgramName;
            }

            set
            {
                Settings.Default.DiffProgramName = value;
            }
        }

        /// <summary>
        /// Gets or sets the string format for diffing files with the DiffProgramName.
        /// </summary>
        public string DiffProgramArguments
        {
            get
            {
                return Settings.Default.DiffProgramArguments;
            }

            set
            {
                Settings.Default.DiffProgramArguments = value;
            }
        }
        
        /// <summary>
        /// Gets or sets the path of the file used as a success sound effect.
        /// </summary>
        public string SuccessSoundFile
        {
            get
            {
                return Settings.Default.SuccessSoundFile;
            }

            set
            {
                Settings.Default.SuccessSoundFile = value;
            }
        }

        /// <summary>
        /// Gets or sets the path of the file used as a failure sound effect.
        /// </summary>
        public string FailureSoundFile
        {
            get
            {
                return Settings.Default.FailureSoundFile;
            }

            set
            {
                Settings.Default.FailureSoundFile = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether backup files should be deleted.
        /// </summary>
        public bool DeleteBackups
        {
            get
            {
                return Settings.Default.DeleteBackupFiles;
            }

            set
            {
                Settings.Default.DeleteBackupFiles = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of days after which backup files should be deleted.
        /// </summary>
        public int BackupTimeoutDays
        {
            get
            {
                return Settings.Default.BackupFileLifetimeDays;
            }

            set
            {
                Settings.Default.BackupFileLifetimeDays = value;
            }
        }

        /// <summary>
        /// Applies the changes to the settings to the user settings file.
        /// </summary>
        public void ApplyChanges()
        {
            Settings.Default.Save();
            this.ModelSettings.ApplyChanges();
        }

        /// <summary>
        /// Resets the settings back to the contents of the user settings file.
        /// </summary>
        public void Reset()
        {
            Settings.Default.Reload();
            this.ModelSettings.Reset();
        }
    }
}
