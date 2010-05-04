/* ****************************************************************************
 *
 * Copyright (c) Andrei Dzimchuk. All rights reserved.
 *
 * This software is subject to the Microsoft Public License (Ms-PL). 
 * A copy of the license can be found in the license.htm file included 
 * in this distribution.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using Dzimchuk.Native;

/* 
 * MFVideoNormalizedRect
 * MFVideoAspectRatioMode
 * MFVideoRenderPrefs
 * IMFGetService
 * IMFVideoDisplayControl
 * IEVRFilterConfig
*/
namespace Dzimchuk.DirectShow
{
    [StructLayout(LayoutKind.Sequential), ComVisible(false)]
	public struct MFVideoNormalizedRect 
	{
		public float left;  
        public float top;  
        public float right;  
        public float bottom;
	}

    [Flags, ComVisible(false)]
    public enum MFVideoAspectRatioMode 
    {
        MFVideoARMode_None               = 0x00000000,
        MFVideoARMode_PreservePicture    = 0x00000001,
        MFVideoARMode_PreservePixel      = 0x00000002,
        MFVideoARMode_NonLinearStretch   = 0x00000004,
        MFVideoARMode_Mask               = 0x00000007 
    }

    [Flags, ComVisible(false)]
    public enum MFVideoRenderPrefs 
    {
        MFVideoRenderPrefs_DoNotRenderBorder       = 0x00000001,
        MFVideoRenderPrefs_DoNotClipToDevice       = 0x00000002,
        MFVideoRenderPrefs_AllowOutputThrottling   = 0x00000004,
        MFVideoRenderPrefs_ForceOutputThrottling   = 0x00000008,
        MFVideoRenderPrefs_ForceBatching           = 0x00000010,
        MFVideoRenderPrefs_AllowBatching           = 0x00000020,
        MFVideoRenderPrefs_ForceScaling            = 0x00000040,
        MFVideoRenderPrefs_AllowScaling            = 0x00000080,
        MFVideoRenderPrefs_Mask                    = 0x000000ff 
    }
    
    [ComVisible(true), ComImport,
    GuidAttribute("fa993888-4383-415a-a930-dd472a8cf6f7"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFGetService
    {
        [PreserveSig]
        int GetService(
            [In] ref Guid guidService,
            [In] ref Guid riid,
            [Out, MarshalAs(UnmanagedType.Interface)] out object ppvObject);
    }

    [ComVisible(true), ComImport,
    GuidAttribute("a490b1e4-ab84-4d31-a1b2-181e03b1077a"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFVideoDisplayControl
    {
        [PreserveSig]
        int GetNativeVideoSize( 
            [In, Out] ref GDI.SIZE pszVideo,
            [In, Out] ref GDI.SIZE pszARVideo);
        
        [PreserveSig]
        int GetIdealVideoSize( 
            [In, Out] ref GDI.SIZE pszMin,
            [In, Out] ref GDI.SIZE pszMax);
        
        [PreserveSig]
        int SetVideoPosition( 
            [In] ref MFVideoNormalizedRect pnrcSource,
            [In] ref GDI.RECT prcDest);
        
        [PreserveSig]
        int GetVideoPosition( 
            [Out] out MFVideoNormalizedRect pnrcSource,
            [Out] out GDI.RECT prcDest);
        
        [PreserveSig]
        int SetAspectRatioMode( 
            MFVideoAspectRatioMode dwAspectRatioMode);
        
        [PreserveSig]
        int GetAspectRatioMode( 
            out MFVideoAspectRatioMode pdwAspectRatioMode);
        
        [PreserveSig]
        int SetVideoWindow( 
            IntPtr hwndVideo);
        
        [PreserveSig]
        int GetVideoWindow( 
            out IntPtr phwndVideo);
        
        [PreserveSig]
        int RepaintVideo();
        
        [PreserveSig]
        int GetCurrentImage( 
            [In, Out] ref BITMAPINFOHEADER pBih,
            out IntPtr pDib, // [out] BYTE** lpDib
            out int pcbDib,
            [In, Out] ref long pTimeStamp);
        
        [PreserveSig]
        int SetBorderColor( 
            int Clr);
        
        [PreserveSig]
        int GetBorderColor( 
            out int pClr);
        
        [PreserveSig]
        int SetRenderingPrefs( 
            MFVideoRenderPrefs dwRenderFlags);
        
        [PreserveSig]
        int GetRenderingPrefs( 
            out MFVideoRenderPrefs pdwRenderFlags);
        
        [PreserveSig]
        int SetFullscreen( 
            bool fFullscreen);

        [PreserveSig]
        int GetFullscreen( 
            out bool pfFullscreen);
    }

    [ComVisible(true), ComImport,
    GuidAttribute("83E91E85-82C1-4ea7-801D-85DC50B75086"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IEVRFilterConfig
    {
        [PreserveSig]
        int SetNumberOfStreams(int dwMaxStreams);
        
        [PreserveSig]
        int GetNumberOfStreams(out int pdwMaxStreams);
    }
}
