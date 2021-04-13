using System;
using System.Runtime.InteropServices;

namespace Pvp.Core.Nwnd
{
    [ComVisible(true), ComImport,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("7DCDEE21-0AAF-4A2F-8928-FDC0043FC2C9")]
    internal interface IMediaWindowCallback
    {
        [PreserveSig]
        int OnMessageReceived(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
    }
}