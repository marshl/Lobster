//-----------------------------------------------------------------------
// <copyright file="ClobColumnNotFoundException.cs" company="marshl">
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
//      "Where is it? Where iss it?" Bilbo heard him crying. "Losst it is, my precious, lost, lost! 
//      Curse us and crush us, my precious is lost!"
//  
//      [ _The Hobbit_, V: "Riddles in the Dark"]
//
// </copyright>
//-----------------------------------------------------------------------
namespace Lobster
{
    using System;

    /// <summary>
    /// An exception thrown when a column isn't found that matches the given mime type
    /// </summary>
    public class ClobColumnNotFoundException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClobColumnNotFoundException"/> class.
        /// </summary>
        /// <param name="databaseFile">The file that a CLOB_DATA column could not be found for.</param>
        public ClobColumnNotFoundException(DBClobFile databaseFile)
            : base("The clob column for file " + databaseFile.Filename + " of mimetype " + databaseFile.MimeType + " could not be found the table " + databaseFile.ParentTable.FullName)
        {
        }
    }
}
