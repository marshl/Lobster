using System;
using System.Runtime.InteropServices;

namespace Lobster
{
    /// <summary>
    /// http://stackoverflow.com/questions/5131534/how-to-get-windows-native-look-for-the-net-treeview
    /// </summary>
    public class NativeListView : System.Windows.Forms.ListView
    {
        [DllImport( "uxtheme.dll", CharSet = CharSet.Unicode )]
        private extern static int SetWindowTheme( IntPtr hWnd, string pszSubAppName,
                                                string pszSubIdList );

        protected override void CreateHandle()
        {
            base.CreateHandle();
            SetWindowTheme( this.Handle, "EXPLORER", null );
        }
    }
}
