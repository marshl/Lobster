//-----------------------------------------------------------------------
// <copyright file="SearchRule.cs" company="marshl">
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
//      The outlet was blocked with some barrier, but not of stone: soft 
//      and a little yielding it seemed, and yet strong and impervious; 
//      air filtered through, hut not a glimmer of any light.
//
//      [ _The Lord of the Rings_, IV/ix: "Shelob's Lair"]
//
//-----------------------------------------------------------------------
namespace LobsterModel
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;

    /// <summary>
    /// Used to find all files in a directory that match a list of rules.
    /// </summary>
    [DataContract]
    [XmlInclude(typeof(IncludeRule))]
    [XmlInclude(typeof(ExcludeRule))]
    public abstract class SearchRule
    {
        /// <summary>
        /// Gets or sets the file pattern to match against.
        /// </summary>
        [DataMember]
        public string Pattern { get; set; }

        /// <summary>
        /// Gets all files in the given directory that fulfil the given search rules.
        /// </summary>
        /// <param name="directoryPath">The directory to get the files for.</param>
        /// <param name="rules">A list of <see cref="SearchRule"/>s in order of application (that is, an inclusion rule applied after an exclusion rule that apply to the same file will include that file).</param>
        /// <returns>The list of files that match the rules.</returns>
        public static List<string> GetFiles(string directoryPath, List<SearchRule> rules)
        {
            var fileList = new List<string>();

            DirectoryInfo dirInfo = new DirectoryInfo(directoryPath);
            if (!dirInfo.Exists)
            {
                return fileList;
            }

            if (rules.Count(x => x is IncludeRule) == 0)
            {
                rules.Insert(0, new IncludeRule() { Pattern = "*" });
            }

            foreach (SearchRule rule in rules)
            {
                var files = dirInfo.GetFiles(rule.Pattern, SearchOption.AllDirectories).Select(x => x.FullName);

                if (rule is IncludeRule)
                {
                    fileList = fileList.Union(files).ToList();
                }
                else if (rule is ExcludeRule)
                {
                    fileList = fileList.Except(files).ToList();
                }
            }

            return fileList;
        }

        /// <summary>
        /// Gets all directories in the given directory that fulfil the given search rules.
        /// </summary>
        /// <param name="directoryPath">The directory to get the directories for.</param>
        /// <param name="rules">A list of <see cref="SearchRule"/>s in order of application (that is, an inclusion rule applied after an exclusion rule that apply to the same directory will include that directory).</param>
        /// <returns>The list of directories that match the rules.</returns>
        public static List<string> GetDirectories(string directoryPath, List<SearchRule> rules)
        {
            var fileList = new List<string>();

            DirectoryInfo dirInfo = new DirectoryInfo(directoryPath);
            if (!dirInfo.Exists)
            {
                return fileList;
            }

            if (rules.Count(x => x is IncludeRule) == 0)
            {
                rules.Insert(0, new IncludeRule() { Pattern = "*" });
            }

            foreach (SearchRule rule in rules)
            {
                var files = dirInfo.GetDirectories(rule.Pattern, SearchOption.AllDirectories).Select(x => x.FullName);

                if (rule is IncludeRule)
                {
                    fileList = fileList.Union(files).ToList();
                }
                else if (rule is ExcludeRule)
                {
                    fileList = fileList.Except(files).ToList();
                }
            }

            return fileList;
        }

        /// <summary>
        /// A search rule that includes the files that match it
        /// </summary>
        [DataContract]
        public class IncludeRule : SearchRule
        {
        }

        /// <summary>
        /// A search rule that excludes any files that match it
        /// </summary>
        [DataContract]
        public class ExcludeRule : SearchRule
        {
        }
    }
}
