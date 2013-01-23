using System;
using System.Runtime.InteropServices;

namespace Pvp.Core.Wpf
{
    [ComVisible(true), ComImport,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("DACEB68E-8716-41F5-85DC-7F5F5D97CC65")]
    internal interface IEvrPresenterMediaWindowManager
    {
        [PreserveSig]
        int GetMediaWindow(out IntPtr phwnd);
    }
}