using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using LobsterModel;
using System.Collections.ObjectModel;

namespace LobsterWpf.ViewModels
{
    public class CodeSourceConfigView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseConfigView"/> class.
        /// </summary>
        /// <param name="codeSourceConfig">The database config to use as the model of this view.</param>
        public CodeSourceConfigView(CodeSourceConfig codeSourceConfig)
        {
            this.BaseConfig = codeSourceConfig;
            this.ConnectionConfigViewList = new ObservableCollection<ConnectionConfigView>(
                this.BaseConfig.ConnectionConfigList.Select(item => new ConnectionConfigView(item)));
        }

        public ObservableCollection<ConnectionConfigView> ConnectionConfigViewList { get; set; }

        /// <summary>
        /// The event to be raised when a property is changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets the underlying model object.
        /// </summary>
        public CodeSourceConfig BaseConfig { get; }

        /// <summary>
        /// Gets or sets a value indicating whether changes have been made to any of the fields in this config.
        /// </summary>
        public bool ChangesMade { get; set; } = false;

        public string Name
        {
            get
            {
                return this.BaseConfig.Name;
            }
        }

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
