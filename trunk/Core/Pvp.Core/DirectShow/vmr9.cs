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
using System.Runtime.InteropServices;
using Pvp.Core.Native;

/* 
 * VMR9AspectRatioMode
 * VMR9RenderPrefs
 * VMR9Mode
 * VMR9VideoStreamInfo
 * VMR9NormalizedRect
 * VMR9_SampleFormat
 * IVMRWindowlessControl9
 * IVMRFilterConfig9
 * IVMRImageCompositor9
 * IVMRAspectRatioControl9
*/
namespace Pvp.Core.DirectShow
{
    [ComVisible(false)]
    public enum VMR9AspectRatioMode
    {
        VMR9ARMode_None,
        VMR9ARMode_LetterBox
    }

    [Flags, ComVisible(false)]
    public enum VMR9RenderPrefs
    {
        RenderPrefs9_DoNotRenderBorder         = 0x00000001, // app paints color keys
        RenderPrefs9_Mask                      = 0x00000001	 // OR of all above flags
    }

    [ComVisible(false)]
    public enum VMR9Mode
    {
        VMR9Mode_Windowed  = 0x00000001,
        VMR9Mode_Windowless  = 0x00000002,
        VMR9Mode_Renderless  = 0x00000004,
        VMR9Mode_Mask  = 0x00000007
    }

    [StructLayout(LayoutKind.Sequential), ComVisible(false)]
    public struct VMR9VideoStreamInfo
    {
        [MarshalAs(UnmanagedType.IUnknown)]
        public object				pddsVideoSurface;
        public int					dwWidth;
        public int					dwHeight;
        public int					dwStrmID;
        public float				fAlpha;
        public VMR9NormalizedRect	rNormal;
        public long					rtStart;
        public long					rtEnd;
        public VMR9_SampleFormat	SampleFormat;
    }

    [StructLayout(LayoutKind.Sequential), ComVisible(false)]
    public struct VMR9NormalizedRect
    {
        public float left;
        public float top;
        public float right;
        public float bottom;
    }

    [ComVisible(false)]
    public enum VMR9_SampleFormat
    {
        VMR9_SampleReserved      = 1,
        VMR9_SampleProgressiveFrame = 2,
        VMR9_SampleFieldInterleavedEvenFirst = 3,
        VMR9_SampleFieldInterleavedOddFirst = 4,
        VMR9_SampleFieldSingleEven = 5,
        VMR9_SampleFieldSingleOdd = 6
    }

    [ComVisible(true), ComImport,
    GuidAttribute("8f537d09-f85e-4414-b23b-502e54c79927"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IVMRWindowlessControl9
    {
        [PreserveSig]
        int GetNativeVideoSize(out int lpWidth, out int lpHeight, out int lpARWidth, out int lpARHeight);

        [PreserveSig]
        int GetMinIdealVideoSize(out int lpWidth, out int lpHeight);

        [PreserveSig]
        int GetMaxIdealVideoSize(out int lpWidth, out int lpHeight);

        [PreserveSig]
        int SetVideoPosition(/*[In] ref GDI.RECT*/ IntPtr lpSRCRect, [In] ref GDI.RECT lpDSTRect); // IntPtr is to be able to specify NULL (entire source frame)

        [PreserveSig]
        int GetVideoPosition([Out] out GDI.RECT lpSRCRect, [Out] out GDI.RECT lpDSTRect);

        [PreserveSig]
        int GetAspectRatioMode(out VMR9AspectRatioMode lpAspectRatioMode);

        [PreserveSig]
        int SetAspectRatioMode(VMR9AspectRatioMode AspectRatioMode);

        [PreserveSig]
        int SetVideoClippingWindow(IntPtr hwnd);

        [PreserveSig]
        int RepaintVideo(IntPtr hwnd, IntPtr hdc);

        [PreserveSig]
        int DisplayModeChanged();

        [PreserveSig]
        int GetCurrentImage(out IntPtr lpDib); // [out] BYTE** lpDib

        [PreserveSig]
        int SetBorderColor(int Clr);

        [PreserveSig]
        int GetBorderColor(out int lpClr);		
    }

    [ComVisible(true), ComImport,
    GuidAttribute("5a804648-4f66-4867-9c43-4f5c822cf1b8"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IVMRFilterConfig9
    {
        [PreserveSig]
        int SetImageCompositor([In] IVMRImageCompositor9 lpVMRImgCompositor);

        [PreserveSig]
        int SetNumberOfStreams(int dwMaxStreams);

        [PreserveSig]
        int GetNumberOfStreams(out int pdwMaxStreams);

        [PreserveSig]
        int SetRenderingPrefs(VMR9RenderPrefs dwRenderFlags); // a combination of VMR9RenderPrefs flags
            
        [PreserveSig]
        int GetRenderingPrefs(out VMR9RenderPrefs pdwRenderFlags);

        [PreserveSig]
        int SetRenderingMode(VMR9Mode Mode);
                    
        [PreserveSig]
        int GetRenderingMode(out int pMode);
    }

    [ComVisible(true), ComImport,
    GuidAttribute("4a5c89eb-df51-4654-ac2a-e48e02bbabf6"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IVMRImageCompositor9
    {
        [PreserveSig]
        int InitCompositionDevice([In, MarshalAs(UnmanagedType.IUnknown)] object pD3DDevice);

        [PreserveSig]
        int TermCompositionDevice([In, MarshalAs(UnmanagedType.IUnknown)] object pD3DDevice);

        [PreserveSig]
        int SetStreamMediaType(int dwStrmID, [In] ref AMMediaType pmt,
                              [In, MarshalAs(UnmanagedType.Bool)] bool fTexture);

        [PreserveSig]
        int CompositeImage([In, MarshalAs(UnmanagedType.IUnknown)] object pD3DDevice,
                     [In, MarshalAs(UnmanagedType.IUnknown)] object pddsRenderTarget,
                     [In] ref AMMediaType pmtRenderTarget, long rtStart, long rtEnd,
                     int dwClrBkGnd, [In] ref VMR9VideoStreamInfo pVideoStreamInfo,
                     int cStreams);
    }

    [ComVisible(true), ComImport,
    GuidAttribute("00d96c29-bbde-4efc-9901-bb5036392146"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IVMRAspectRatioControl9
    {
        [PreserveSig]
        int GetAspectRatioMode(out VMR9AspectRatioMode lpdwARMode);

        [PreserveSig]
        int SetAspectRatioMode(VMR9AspectRatioMode dwARMode);

    }
}