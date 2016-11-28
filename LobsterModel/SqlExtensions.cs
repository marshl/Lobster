//-----------------------------------------------------------------------
// <copyright file="SqlExtensions.cs" company="marshl">
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
//      'I looked then and saw that his robes, which had seemed white, were not
//       so, but were woven of all colours.and if he moved they shimmered 
//       and changed hue so that the eye was bewildered.'
//          - Gandalf the Grey
//
//      [ _The Lord of the Rings_, II/ii: "The Council of Elrond"]
//
//-----------------------------------------------------------------------
namespace LobsterModel
{
    using System.Data.Common;
    using System.Text.RegularExpressions;

    /// <summary>
    /// S static class contain extension methods for DbConnection classes.
    /// </summary>
    public static class SqlExtensions
    {
        /// <summary>
        /// Returns whether the command text contains the given parameter.
        /// </summary>
        /// <param name="command">The command to search.</param>
        /// <param name="parameter">The parameter to search for.</param>
        /// <returns>True if the command contains the parameter, otherwise false.</returns>
        public static bool ContainsParameter(this DbCommand command, string parameter)
        {
            // Use a lookahead assertion to ensure the parameter isn't followed by a wordy character
            // That way a parameter like :p_filename and :p_filename_without_extension can't be confused
            return Regex.IsMatch(command.CommandText, $":{parameter}(?![\\w])");
        }
    }
}
