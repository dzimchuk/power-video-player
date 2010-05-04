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
using Dzimchuk.Native;

/* 
 * VMR_ASPECT_RATIO_MODE
 * VMRRenderPrefs
 * VMRMode
 * VMRVIDEOSTREAMINFO
 * DDCOLORKEY
 * NORMALIZEDRECT
 * IVMRWindowlessControl
 * IVMRFilterConfig
 * IVMRImageCompositor
 * IVMRAspectRatioControl
*/
namespace Dzimchuk.DirectShow
{
	[ComVisible(false)]
	public enum VMR_ASPECT_RATIO_MODE
	{
		VMR_ARMODE_NONE,
		VMR_ARMODE_LETTER_BOX
	}

	[Flags, ComVisible(false)]
	public enum VMRRenderPrefs
	{
		RenderPrefs_ForceOffscreen               = 0x00000001,
		RenderPrefs_ForceOverlays                = 0x00000002,
		RenderPrefs_AllowOverlays                = 0x00000000,
		RenderPrefs_AllowOffscreen               = 0x00000000,
		RenderPrefs_DoNotRenderColorKeyAndBorder = 0x00000008,
		RenderPrefs_RestrictToInitialMonitor     = 0x00000010,
		RenderPrefs_PreferAGPMemWhenMixing       = 0x00000020,
		RenderPrefs_Mask                         = 0x0000003f
	}

	[ComVisible(false)]
	public enum VMRMode
	{
		VMRMode_Windowed  = 0x00000001,
		VMRMode_Windowless  = 0x00000002,
		VMRMode_Renderless  = 0x00000004,
		VMRMode_Mask  = 0x00000007
	}

	[StructLayout(LayoutKind.Sequential), ComVisible(false)]
	public struct VMRVIDEOSTREAMINFO 
	{
		public IntPtr			pddsVideoSurface;
		public int				dwWidth;
		public int				dwHeight;
		public int				dwStrmID;
		public float			fAlpha;
		public DDCOLORKEY		ddClrKey;
		public NORMALIZEDRECT	rNormal;
	}

	[StructLayout(LayoutKind.Sequential), ComVisible(false)]
	public struct DDCOLORKEY
	{
		public int dw1;
		public int dw2;
	}

	[StructLayout(LayoutKind.Sequential), ComVisible(false)]
	public struct NORMALIZEDRECT
	{
		public float left;
		public float top;
		public float right;
		public float bottom;
	}
	
	[ComVisible(true), ComImport,
	GuidAttribute("0eb1088c-4dcd-46f0-878f-39dae86a51b7"),
	InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IVMRWindowlessControl
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
		int GetAspectRatioMode(out VMR_ASPECT_RATIO_MODE lpAspectRatioMode);

		[PreserveSig]
		int SetAspectRatioMode(VMR_ASPECT_RATIO_MODE AspectRatioMode);

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

		[PreserveSig]
		int SetColorKey(int Clr);

		[PreserveSig]
		int GetColorKey(out int lpClr);
	}

	[ComVisible(true), ComImport,
	GuidAttribute("9e5530c5-7034-48b4-bb46-0b8a6efc8e36"),
	InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IVMRFilterConfig
	{
		[PreserveSig]
		int SetImageCompositor([In] IVMRImageCompositor lpVMRImgCompositor);

		[PreserveSig]
		int SetNumberOfStreams(int dwMaxStreams);

		[PreserveSig]
		int GetNumberOfStreams(out int pdwMaxStreams);

		[PreserveSig]
		int SetRenderingPrefs(VMRRenderPrefs dwRenderFlags); // a combination of VMRRenderingPrefs flags
			
		[PreserveSig]
		int GetRenderingPrefs(out VMRRenderPrefs pdwRenderFlags);

		[PreserveSig]
		int SetRenderingMode(VMRMode Mode);
					
		[PreserveSig]
		int GetRenderingMode(out VMRMode pMode);
	}

	[ComVisible(true), ComImport,
	GuidAttribute("7a4fb5af-479f-4074-bb40-ce6722e43c82"),
	InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IVMRImageCompositor
	{
		[PreserveSig]
		int InitCompositionTarget([In, MarshalAs(UnmanagedType.IUnknown)] object pD3DDevice,
					 IntPtr pddsRenderTarget);

		[PreserveSig]
		int TermCompositionTarget([In, MarshalAs(UnmanagedType.IUnknown)] object pD3DDevice,
			IntPtr pddsRenderTarget);

		[PreserveSig]
		int SetStreamMediaType(int dwStrmID, [In] ref AMMediaType pmt,
							  [In, MarshalAs(UnmanagedType.Bool)] bool fTexture);

		[PreserveSig]
		int CompositeImage([In, MarshalAs(UnmanagedType.IUnknown)] object pD3DDevice,
			IntPtr pddsRenderTarget, [In] ref AMMediaType pmtRenderTarget,
			long rtStart, long rtEnd, int dwClrBkGnd, [In] ref VMRVIDEOSTREAMINFO pVideoStreamInfo,
			int cStreams);
	}

    [ComVisible(true), ComImport,
	GuidAttribute("ede80b5c-bad6-4623-b537-65586c9f8dfd"),
	InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IVMRAspectRatioControl
    {
        [PreserveSig]
		int GetAspectRatioMode(out VMR_ASPECT_RATIO_MODE lpdwARMode);
        
        [PreserveSig]
		int SetAspectRatioMode(VMR_ASPECT_RATIO_MODE dwARMode);
        
    }
}
