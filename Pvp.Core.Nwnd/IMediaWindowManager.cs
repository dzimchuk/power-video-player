using System;
using System.Runtime.InteropServices;
using Pvp.Core.DirectShow;

namespace Pvp.Core.Nwnd
{
    [ComVisible(true), ComImport,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("DACEB68E-8716-41F5-85DC-7F5F5D97CC65")]
    internal interface IMediaWindowManager
    {
        [PreserveSig]
        int CreateMediaWindow(out IntPtr phwnd, IntPtr hParent, int x, int y, int nWidth, int nHeight, int dwStyle);

        [PreserveSig]
        int SetRunning(bool bRunning, IVMRWindowlessControl VMRWindowlessControl,
            IVMRWindowlessControl9 VMRWindowlessControl9, IMFVideoDisplayControl MFVideoDisplayControl);

        [PreserveSig]
        int InvalidateMediaWindow();

        [PreserveSig]
        int SetLogo(IntPtr hBitmap);

        [PreserveSig]
        int ShowLogo(bool show);

        [PreserveSig]
        int RegisterCallback(IMediaWindowCallback pCallback);
    }
}