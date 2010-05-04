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
	public class WindowsManagement
	{
		public delegate int EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
		public delegate int EnumChildProc(IntPtr hWnd, IntPtr lParam);
		
		#region Structures
		[StructLayout(LayoutKind.Sequential)]
		public struct TRACKMOUSEEVENT 
		{
			public int cbSize;
			public uint dwFlags;
			public IntPtr hwndTrack;
			public uint dwHoverTime;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct WINDOWPLACEMENT
		{
			public int length;
			public int flags;
			public int showCmd;
			public GDI.POINT ptMinPosition;
			public GDI.POINT ptMaxPosition;
			public GDI.RECT rcNormalPosition;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct COPYDATASTRUCT
		{
			public int dwData;
			public int cbData;
			public IntPtr lpData;
		}

		[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
		public struct CREATESTRUCT
		{
			public IntPtr lpCreateParams;
			public IntPtr hInstance;
			public IntPtr hMenu;
			public IntPtr hwndParent;
			public int cy;
			public int cx;
			public int y;
			public int x;
			public int style;
			[MarshalAs(UnmanagedType.LPTStr)]
			public string lpszName;
			[MarshalAs(UnmanagedType.LPTStr)]
			public string lpszClass;
			public int dwExStyle;
		}

		#endregion

		#region Funtions
		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		public static extern int TrackPopupMenuEx(IntPtr hMenu,
													int uFlags,
													int x,
													int y,
													IntPtr hWnd,
													IntPtr ignore);

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		public static extern int SetForegroundWindow(IntPtr hWnd);

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		public static extern int ScrollWindow(IntPtr hwnd, int cx, int cy, 
												ref GDI.RECT rectScroll,
												ref GDI.RECT rectClip);

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		public static extern short GetKeyState(int keyCode);

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		public static extern int GetWindowLong(IntPtr hWnd, int index);

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		public static extern int SetWindowLong(IntPtr hWnd, int index, int dwNewLong);

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		public static extern int GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);
		
		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		public static extern int ShowWindow(IntPtr hWnd, int nCmdShow);

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		public static extern int ShowWindowAsync(IntPtr hWnd, int nCmdShow);

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		public static extern int SendMessage(IntPtr hWnd, int Msg,	IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		public static extern int SendMessageTimeout(
			IntPtr hWnd, 
			int Msg, 
			IntPtr wParam, 
			IntPtr lParam, 
			int fuFlags,
			int uTimeout,
			out int lpdwResult
			);

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		public static extern int PostMessage(IntPtr hWnd, int Msg,	IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		public static extern int IsIconic(IntPtr hWnd);

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		public static extern int EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		public static extern int EnumChildWindows(IntPtr hWndParent, EnumChildProc lpEnumFunc, IntPtr lParam);

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		public static extern uint RegisterWindowMessage([MarshalAs(UnmanagedType.LPTStr)]string lpString);

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		public static extern int SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, int uFlags);

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		public static extern int MoveWindow(IntPtr hWnd, int X, int Y, 
			int nWidth, int nHeight, [MarshalAs(UnmanagedType.Bool)] bool bRepaint);

		[DllImport("comctl32.dll", CharSet=CharSet.Auto)]
		public static extern int _TrackMouseEvent(ref TRACKMOUSEEVENT lpEventTrack);

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		public static extern int CheckMenuItem(IntPtr hmenu, int uIDCheckItem, int uCheck);
		
		#endregion

		#region Defines
		public const int MF_BYCOMMAND      = 0x00000000;
		public const int MF_BYPOSITION     = 0x00000400;

		public const int MF_UNCHECKED      = 0x00000000;
		public const int MF_CHECKED        = 0x00000008;
		
		public const int WS_CHILD = 0x40000000;
		public const int WS_CLIPSIBLINGS = 0x04000000;
		
		public const int GWL_STYLE = -16;
		public const int WS_SYSMENU = 0x00080000;
		public const int WS_MINIMIZEBOX = 0x00020000;
		public const int WS_MAXIMIZEBOX = 0x00010000;
		public const int WPF_ASYNCWINDOWPLACEMENT = 0x0004;
		public const int WPF_RESTORETOMAXIMIZED = 0x0002;
		public const int WPF_SETMINPOSITION = 0x0001;
		
		public const int SW_HIDE = 0;
		public const int SW_MAXIMIZE = 3;
		public const int SW_MINIMIZE = 6;
		public const int SW_RESTORE = 9;
		public const int SW_SHOW = 5;
		public const int SW_SHOWMAXIMIZED = 3;
		public const int SW_SHOWMINIMIZED = 2;
		public const int SW_SHOWMINNOACTIVE = 7;
		public const int SW_SHOWNA = 8;
		public const int SW_SHOWNOACTIVATE = 4;
		public const int SW_SHOWNORMAL = 1;

		public const int SMTO_ABORTIFHUNG = 0x0002;
		public const int SMTO_BLOCK = 0x0001;
		public const int SMTO_NORMAL = 0x0000;
		public const int SMTO_NOTIMEOUTIFNOTHUNG = 0x0008;

		public static readonly IntPtr HWND_TOP       = (IntPtr) 0;
		public static readonly IntPtr HWND_BOTTOM    = (IntPtr) 1;
		public static readonly IntPtr HWND_TOPMOST   = new IntPtr(-1);
		public static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);

		public const int SWP_NOSIZE       =  0x0001;
		public const int SWP_NOMOVE       =  0x0002;
		public const int SWP_NOZORDER     =  0x0004;
		public const int SWP_NOREDRAW     =  0x0008;
		public const int SWP_NOACTIVATE   =  0x0010;
		public const int SWP_FRAMECHANGED =  0x0020; /* The frame changed: send WM_NCCALCSIZE */
		public const int SWP_SHOWWINDOW   =  0x0040;
		public const int SWP_HIDEWINDOW   =  0x0080;
		public const int SWP_NOCOPYBITS   =  0x0100;
		public const int SWP_NOOWNERZORDER=  0x0200;  /* Don't do owner Z ordering */
		public const int SWP_NOSENDCHANGING= 0x0400;  /* Don't send WM_WINDOWPOSCHANGING */

		public const int SWP_DRAWFRAME    =  SWP_FRAMECHANGED;
		public const int SWP_NOREPOSITION =  SWP_NOOWNERZORDER;

		public const int SWP_DEFERERASE   =  0x2000;
		public const int SWP_ASYNCWINDOWPOS= 0x4000;

		public const uint TME_HOVER      = 0x00000001;
		public const uint TME_LEAVE      = 0x00000002;
		public const uint TME_NONCLIENT  = 0x00000010;
		public const uint TME_QUERY      = 0x40000000;
		public const uint ME_CANCEL      = 0x80000000;

		public const uint HOVER_DEFAULT  = 0xFFFFFFFF;

		#endregion
	}
}
