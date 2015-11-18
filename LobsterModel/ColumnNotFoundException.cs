//-----------------------------------------------------------------------
// <copyright file="ColumnNotFoundException.cs" company="marshl">
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
//
//      "Where is it? Where iss it?" Bilbo heard him crying. "Losst it is, my precious, lost, lost! 
//      Curse us and crush us, my precious is lost!"
//  
//      [ _The Hobbit_, V: "Riddles in the Dark"]
//
//-----------------------------------------------------------------------
namespace LobsterModel
{
    using System;

    /// <summary>
    /// An exception thrown when a column isn't found that matches the given mime type
    /// </summary>
    [Serializable]
    public class ColumnNotFoundException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnNotFoundException"/> class.
        /// </summary>
        /// <param name="table">The table that the column could not be found on.</param>
        /// <param name="columnPurpose">The column purpose that could not be found </param>
        /// <param name="mimeType">The optional mime type that could not be found on any data column in the table.</param>
        /// <param name="filename">The optional name of the file that raised this exception when attempting to find the data column for it.</param>
        public ColumnNotFoundException(Table table, Column.Purpose columnPurpose, string mimeType = null, string filename = null)
            : base($"The {columnPurpose} column "
                  + (filename != null ? " for file " + filename : null)
                  + " "
                  + (mimeType != null ? " with mimetype " + mimeType : null)
                  + $" could not be found the table {table.FullName}")
        {
        }
    }
}
