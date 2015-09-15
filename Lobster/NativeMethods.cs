using System;
using System.Runtime.InteropServices;

namespace Lobster
{
    class NativeMethods
    {
        [DllImport( "uxtheme.dll", CharSet = CharSet.Unicode )]
        public extern static int SetWindowTheme( IntPtr hWnd, string pszSubAppName,
                                                string pszSubIdList );
    }
}
