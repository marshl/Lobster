using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace LobsterWpf
{
    public class ColumnView : INotifyPropertyChanged
    {
        /// <summary>
        /// The event to be raised when a property is changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Implementation of the INotifyPropertyChange, to tell WPF when a data value has changed
        /// </summary>
        /// <param name="propertyName">The name of the property that has changed.</param>
        /// <remarks>This method is called by the Set accessor of each property.
        /// The CallerMemberName attribute that is applied to the optional propertyName
        /// parameter causes the property name of the caller to be substituted as an argument.</remarks>
        private void NotifyPropertyChanged(string propertyName = "")
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
