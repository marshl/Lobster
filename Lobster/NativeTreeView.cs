//-----------------------------------------------------------------------
// <copyright file="NativeTreeView.cs" company="marshl">
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
//      Then she let her hand fall, and the light faded, and suddenly she laughed again, and lo! 
//      she was shrunken: a slender elf-woman, clad in simple white, whose gentle voice was soft and sad.
// 
//      [ _The Lord of the Rings_, II/vii: "The Mirror of Galadriel"]
//
//-----------------------------------------------------------------------
namespace Lobster
{
    /// <summary>
    /// An alternative for the Windows Forms TreeView that uses the appearance of Explorer
    /// http://stackoverflow.com/questions/5131534/how-to-get-windows-native-look-for-the-net-treeview
    /// </summary>
    public class NativeTreeView : System.Windows.Forms.TreeView
    {
        /// <summary>
        /// Override for CreateHandle, setting the window theme to Explorer as well as performing the base action.
        /// </summary>
        protected override void CreateHandle()
        {
            base.CreateHandle();
            NativeMethods.SetWindowTheme(this.Handle, "explorer", null);
        }
    }
}
