//-----------------------------------------------------------------------
// <copyright file="ClobType.cs" company="marshl">
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
//
//      They were on ponies, and each pony was slung about with all kinds of baggages, packages,
//      parcels, and paraphernalia.
//          [ _The Hobbit_, II "Roast Mutton" ] 
//
// </copyright>
//-----------------------------------------------------------------------

namespace Lobster
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Xml.Serialization;

    [XmlType( TypeName = "clobtype" )]
    public class ClobType : ICloneable
    {
        [DisplayName( "Name" )]
        [Description( "The display name" )]
        public string name { get; set; }

        [DisplayName( "Directory" )]
        [Description( "The name of the directory in CodeSource to be used for this ClobType. Directory separators can be used." )]
        public string directory { get; set; }

        [DisplayName( "Include Subdirectories" )]
        [Description( "Whether or not all subdirectories under the specified folder should also be used." )]
        public bool includeSubDirectories { get; set; }

        [XmlIgnore]
        public string fileLocation;

        [XmlIgnore]
        public DatabaseConnection ParentConnection { get; private set; }

        [DisplayName( "Table List" )]
        [Description( "The tables used by this ClobType" )]
        public List<Table> tables { get; set; }

        public void Initialise( DatabaseConnection databaseConnection)
        {
            this.tables.ForEach( x => x.LinkColumns() );
            this.ParentConnection = databaseConnection;
        }

        public static void Serialise( string _fullpath, ClobType _clobType )
        {
            XmlSerializer xmls = new XmlSerializer( typeof( ClobType ) );
            using ( StreamWriter streamWriter = new StreamWriter( _fullpath ) )
            {
                xmls.Serialize( streamWriter, _clobType );
            }
        }

        public object Clone()
        {
            ClobType copy = new ClobType();
            copy.name = this.name;
            copy.directory = this.directory;
            copy.includeSubDirectories = this.includeSubDirectories;

            copy.tables = new List<Table>();
            if ( this.tables != null )
            {
                foreach ( Table table in this.tables )
                {
                    copy.tables.Add( (Table)table.Clone() );
                }
            }
            copy.tables.ForEach( x => x.LinkColumns() );
            copy.fileLocation = this.fileLocation;

            return copy;
        }
    }
}
