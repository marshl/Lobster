//-----------------------------------------------------------------------
// <copyright file="NativeMethods.cs" company="marshl">
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
namespace Lobster
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Methods for calling native dlls.
    /// </summary>
    public class NativeMethods
    {
        /// <summary>
        /// Uses t=the uxtheme dll to set a window theme to windows native.
        /// </summary>
        /// <param name="hWnd">The window handle.</param>
        /// <param name="pszSubAppName">The sub-appication name.</param>
        /// <param name="pszSubIdList">The sub ID list.</param>
        /// <returns>The result.</returns>
        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        public static extern int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string pszSubIdList);
    }
}
