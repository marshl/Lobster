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
namespace LobsterWpf
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
        /// Gets or sets the settigns view for the model.
        /// </summary>
        public LobsterModel.SettingsView ModelSettings { get; set; }

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
                Settings.Default.Save();
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
                Settings.Default.Save();
            }
        }

        /// <summary>
        /// Gets or sets a the extensions that a file must have to be diffed (space delimeted string).
        /// </summary>
        public string DiffableExtensions
        {
            get
            {
                string[] array = new string[Settings.Default.DiffableExtensions.Count];
                Settings.Default.DiffableExtensions.CopyTo(array, 0);
                return string.Join(" ", array);
            }

            set
            {
                Settings.Default.DiffableExtensions = new StringCollection();
                Settings.Default.DiffableExtensions.AddRange(value.Split(' '));
                Settings.Default.Save();
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
                Settings.Default.Save();
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
                Settings.Default.Save();
            }
        }
    }
}
