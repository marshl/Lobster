//-----------------------------------------------------------------------
// <copyright file="CodeSourceConfigView.cs" company="marshl">
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
//      To their surprise they came upon dark pools fed by threads of
//      water trickling down from some source higher up the valley.
//
//      [ _The Lord of the Rings_, III/iI: "The Land of Shadow"]
//
//-----------------------------------------------------------------------
namespace LobsterWpf.ViewModels
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using LobsterModel;

    /// <summary>
    /// The ViewModel for the CodeSourceConfig class.
    /// </summary>
    public class CodeSourceConfigView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CodeSourceConfigView"/> class.
        /// </summary>
        /// <param name="codeSourceConfig">The database config to use as the model of this view.</param>
        public CodeSourceConfigView(CodeSourceConfig codeSourceConfig)
        {
            this.BaseConfig = codeSourceConfig;
            this.ConnectionConfigViewList = new ObservableCollection<ConnectionConfigView>(
                this.BaseConfig.ConnectionConfigList.Select(item => new ConnectionConfigView(item)));
        }

        /// <summary>
        /// The event to be raised when a property is changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets or sets the list of connection config views (each mapped to the corresponding ConnectionConfig of the BaseConfig).
        /// </summary>
        public ObservableCollection<ConnectionConfigView> ConnectionConfigViewList { get; set; }

        /// <summary>
        /// Gets the underlying model object.
        /// </summary>
        public CodeSourceConfig BaseConfig { get; }

        /// <summary>
        /// Gets or sets a value indicating whether changes have been made to any of the fields in this config.
        /// </summary>
        public bool ChangesMade { get; set; } = false;

        /// <summary>
        /// Gets the name of the CodeSource.
        /// </summary>
        public string Name
        {
            get
            {
                return this.BaseConfig.Name;
            }
        }

        /// <summary>
        /// Gets the location of the file that the CodeSourceCOnfig was loaded from.
        /// </summary>
        public string FileLocation
        {
            get
            {
                return this.BaseConfig.FileLocation;
            }
        }

        /// <summary>
        /// Implementation of the INotifyPropertyChange, to tell WPF when a data value has changed
        /// </summary>
        /// <param name="propertyName">The name of the property that has changed.</param>
        /// <remarks>This method is called by the Set accessor of each property.
        /// The CallerMemberName attribute that is applied to the optional propertyName
        /// parameter causes the property name of the caller to be substituted as an argument.</remarks>
        private void NotifyPropertyChanged(string propertyName = "")
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
