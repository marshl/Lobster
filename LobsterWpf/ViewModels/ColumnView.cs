//-----------------------------------------------------------------------
// <copyright file="ColumnView.cs" company="marshl">
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
//      Monoliths of
//      black marble, they rose to great capitals carved in many strange figures of
//      beasts and leaves; and far above in shadow the wide vaulting gleamed with dull
//      gold, inset with flowing traceries of many colours.
//
//      [ _The Lord of the Rings_, V/i: "Minas Tirith"]
//
//-----------------------------------------------------------------------
namespace LobsterWpf.ViewModels
{
    using System;
    using System.ComponentModel;
    using LobsterModel;

    /// <summary>
    /// The mode-view for a LObsterMOdel.Column
    /// </summary>
    [Obsolete]
    public class ColumnView : INotifyPropertyChanged
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnView"/> class.
        /// </summary>
        /// <param name="column">The column to use as a base for this view.</param>
        public ColumnView(Column column)
        {
            this.BaseColumn = column;
        }

        /// <summary>
        /// The event to be raised when a property is changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets the base column for this view.
        /// </summary>
        public Column BaseColumn { get; }

        /// <summary>
        /// Gets or sets the name of the column.
        /// </summary>
        public string Name
        {
            get
            {
                return this.BaseColumn.Name;
            }

            set
            {
                this.BaseColumn.Name = value;
                this.NotifyPropertyChanged("Name");
            }
        }

        /// <summary>
        /// Gets or sets the sequence of this column.
        /// </summary>
        public string Sequence
        {
            get
            {
                return this.BaseColumn.Sequence;
            }

            set
            {
                this.BaseColumn.Sequence = value;
                this.NotifyPropertyChanged("Sequence");
            }
        }

        /// <summary>
        /// Gets or sets the purpose of the column.
        /// </summary>
        public Column.Purpose ColumnPurpose
        {
            get
            {
                return this.BaseColumn.ColumnPurpose;
            }

            set
            {
                this.BaseColumn.ColumnPurpose = value;
                this.NotifyPropertyChanged("ColumnPurpose");
            }
        }

        /// <summary>
        /// Gets or sets the datatype of the column.
        /// </summary>
        public Column.Datatype? DataType
        {
            get
            {
                return this.BaseColumn.DataType;
            }

            set
            {
                this.BaseColumn.DataType = value;
                this.NotifyPropertyChanged("DataType");
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
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
