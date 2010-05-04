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

namespace Dzimchuk.DirectShow
{
	[StructLayout(LayoutKind.Sequential), ComVisible(false)]
	public struct BITMAPINFOHEADER
	{
		public int		biSize;
		public int		biWidth;
		public int		biHeight;
		public short	biPlanes;
		public short	biBitCount;
		public int		biCompression;
		public int		biSizeImage;
		public int		biXPelsPerMeter;
		public int		biYPelsPerMeter;
		public int		biClrUsed;
		public int		biClrImportant;
	}

	[StructLayout(LayoutKind.Sequential), ComVisible(false)]
	public struct VIDEOINFOHEADER
	{
		public GDI.RECT			rcSource;
		public GDI.RECT			rcTarget;
		public int				dwBitRate;
		public int				dwBitErrorRate;
		public long				AvgTimePerFrame;
		public BITMAPINFOHEADER	bmiHeader;
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct ControlFlagsUnion
	{
		[FieldOffset(0)]
		public int	dwControlFlags;
		[FieldOffset(0)]
		public int	dwReserved1;
	}
	
	[StructLayout(LayoutKind.Sequential), ComVisible(false)]
	public struct VIDEOINFOHEADER2
	{
		public GDI.RECT			rcSource;
		public GDI.RECT			rcTarget;
		public int				dwBitRate;
		public int				dwBitErrorRate;
		public long				AvgTimePerFrame;
		public int				dwInterlaceFlags;
		public int				dwCopyProtectFlags;
		public int				dwPictAspectRatioX; 
		public int				dwPictAspectRatioY; 
		public ControlFlagsUnion	dwControlFlags;
		public int				dwReserved2;
		public BITMAPINFOHEADER	bmiHeader;
	}

	[StructLayout(LayoutKind.Sequential), ComVisible(false)]
	public struct WAVEFORMATEX
	{
		public short	wFormatTag; 
		public short	nChannels; 
		public int		nSamplesPerSec; 
		public int		nAvgBytesPerSec; 
		public short	nBlockAlign; 
		public short	wBitsPerSample; 
		public short	cbSize; 
	}

	[StructLayout(LayoutKind.Sequential), ComVisible(false)]
	public struct MPEG1VIDEOINFO
	{
		public VIDEOINFOHEADER	hdr;
		public int			dwStartTimeCode;
		public int			cbSequenceHeader;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst=1)]
		public byte[]		bSequenceHeader;
	}

	[StructLayout(LayoutKind.Sequential), ComVisible(false)]
	public struct MPEG2VIDEOINFO
	{
		public VIDEOINFOHEADER2	hdr;
		public int				dwStartTimeCode;   
		public int				cbSequenceHeader;     
		public int				dwProfile;     
		public int				dwLevel;            
		public int				dwFlags;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=1)] 
		public int[]			dwSequenceHeader;     
	}

	[StructLayout(LayoutKind.Sequential), ComVisible(false)]
	public struct BITMAPINFO 
	{
		public BITMAPINFOHEADER    bmiHeader;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst=1)]
		public RGBQUAD[]           bmiColors;
	}

	[StructLayout(LayoutKind.Sequential), ComVisible(false)]
	public struct RGBQUAD 
	{
		public byte rgbBlue;
		public byte rgbGreen;
		public byte rgbRed;
		public byte rgbReserved;
	}
}