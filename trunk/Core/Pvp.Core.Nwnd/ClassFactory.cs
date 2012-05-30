using System;
using System.Runtime.InteropServices;

namespace Pvp.Core.Nwnd
{
    internal static class ClassFactory
    {
        [DllImport("nwnd.dll", EntryPoint = "DllGetClassObject")]
        public static extern int GetClassFactory(ref Guid clsid, ref Guid riid, [MarshalAs(UnmanagedType.IUnknown)]
                                                 out object ppv);

        public static Guid IID_ClassFactory = new Guid("00000001-0000-0000-C000-000000000046");
    }
}
