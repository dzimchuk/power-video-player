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

namespace Dzimchuk.Native
{
	/// <summary>
	/// 
	/// </summary>
	public class GDI
	{
		
		#region Structures
		[StructLayout(LayoutKind.Sequential)]
		public struct POINT
		{
			public int x;
			public int y;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct RECT
		{
			public int left;
			public int top;
			public int right;
			public int bottom;
		}

        [StructLayout(LayoutKind.Sequential)]
        public struct SIZE
        {
            public int cx;
            public int cy;
        }

		[StructLayout(LayoutKind.Sequential)]
		public struct MONITORINFO
		{
			public int cbSize;
			public RECT rcMonitor;
			public RECT rcWork;
			public int dwFlags;
		}

		#endregion

		#region Functions
		[DllImport("gdi32.dll", CharSet=CharSet.Auto)]
		public static extern int PtInRegion(IntPtr hRgn, int x, int y);

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		public static extern int GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		public static extern IntPtr MonitorFromWindow(IntPtr hWnd, int dwFlags);

		#endregion

		#region Defines
		public const int MONITOR_DEFAULTTONULL     = 0x00000000;
		public const int MONITOR_DEFAULTTOPRIMARY  = 0x00000001;
		public const int MONITOR_DEFAULTTONEAREST  = 0x00000002;

		public const int MONITORINFOF_PRIMARY      = 0x00000001;
		#endregion
	}
}
