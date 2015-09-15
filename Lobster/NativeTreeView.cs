using System;
using System.Runtime.InteropServices;

namespace Lobster
{
    /// <summary>
    /// http://stackoverflow.com/questions/5131534/how-to-get-windows-native-look-for-the-net-treeview
    /// 
    /// Then she let her hand fall, and the light faded, and suddenly she laughed again, and lo! 
    /// she was shrunken: a slender elf-woman, clad in simple white, whose gentle voice was soft and sad.
    /// 
    /// [ _The Lord of the Rings_, II/vii: "The Mirror of Galadriel"]
    /// </summary>
    public class NativeTreeView : System.Windows.Forms.TreeView
    {
        [DllImport( "uxtheme.dll", CharSet = CharSet.Unicode )]
        private extern static int SetWindowTheme( IntPtr hWnd, string pszSubAppName,
                                                string pszSubIdList );

        protected override void CreateHandle()
        {
            base.CreateHandle();
            SetWindowTheme( this.Handle, "explorer", null );
        }
    }
}
