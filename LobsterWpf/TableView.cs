//-----------------------------------------------------------------------
// <copyright file="TableView.cs" company="marshl">
// Copyright 2015, Liam Marshall, marshl.
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
    using System.Collections.Generic;
    using System.ComponentModel;
    using LobsterModel;

    /// <summary>
    /// The model-view of the LobserModel.Table
    /// </summary>
    public class TableView : INotifyPropertyChanged
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TableView"/> class.
        /// </summary>
        /// <param name="table">The model table to use as a basis for this view.</param>
        public TableView(Table table)
        {
            this.TableObject = table;
        }

        /// <summary>
        /// The event to be raised when a property is changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets the underlying Table object 
        /// </summary>
        public Table TableObject { get; }

        /// <summary>
        /// Gets or sets the schema/user that this table belongs to.
        /// </summary>
        public string Schema
        {
            get
            {
                return this.TableObject.Schema;
            }

            set
            {
                this.TableObject.Schema = value;
                this.NotifyPropertyChanged("Schema");
            }
        }

        /// <summary>
        /// Gets or sets the name of this table in the database.
        /// </summary>
        public string Name
        {
            get
            {
                return this.TableObject.Name;
            }

            set
            {
                this.TableObject.Name = value;
                this.NotifyPropertyChanged("Name");
            }
        }

        /// <summary>
        /// Gets or sets the extension that will be added to the mnemonic to create the guesstimate local file name. 
        /// If the default extension is not set, then .xml will be used
        /// </summary>
        public string DefaultExtension
        {
            get
            {
                return this.TableObject.DefaultExtension;
            }

            set
            {
                this.TableObject.DefaultExtension = value;
                this.NotifyPropertyChanged("DefaultExtension");
            }
        }

        /// <summary>
        /// Gets or sets the columns in this table that Lobster needs.
        /// </summary>
        public List<Column> Columns
        {
            get
            {
                return this.TableObject.Columns;
            }

            set
            {
                this.TableObject.Columns = value;
                this.NotifyPropertyChanged("Columns");
            }
        }

        /// <summary>
        /// Gets or sets the parent table of this table, if this table is part of a parent-child relationship.
        /// </summary>
        public Table ParentTable
        {
            get
            {
                return this.TableObject.ParentTable;
            }

            set
            {
                this.TableObject.ParentTable = value;
                this.NotifyPropertyChanged("ParentTable");
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
