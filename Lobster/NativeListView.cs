﻿using System;
using System.Runtime.InteropServices;

namespace Lobster
{
    /// <summary>
    /// http://stackoverflow.com/questions/5131534/how-to-get-windows-native-look-for-the-net-treeview
    /// 
    /// She stood before Frodo seeming now tall beyond measurement,
    ///  and beautiful beyond enduring, terrible and worshipful.
    /// 
    /// [ _The Lord of the Rings_, II/vii: "The Mirror of Galadriel"]
    /// </summary>
    public class NativeListView : System.Windows.Forms.ListView
    {
        protected override void CreateHandle()
        {
            base.CreateHandle();
            NativeMethods.SetWindowTheme( this.Handle, "EXPLORER", null );
        }
    }
}
