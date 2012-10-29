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

namespace Pvp.Core.Native
{
    /// <summary>
    /// 
    /// </summary>
    public class WindowsManagement
    {
        public delegate int EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
        public delegate int EnumChildProc(IntPtr hWnd, IntPtr lParam);
        public delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        
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

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
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

        [StructLayout(LayoutKind.Sequential)]
        public struct WNDCLASSEX
        {
            public uint cbSize;
            public uint style;
            [MarshalAs(UnmanagedType.FunctionPtr)]
            public WndProc lpfnWndProc;
            public int cbClsExtra;
            public int cbWndExtra;
            public IntPtr hInstance;
            public IntPtr hIcon;
            public IntPtr hCursor;
            public IntPtr hbrBackground;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpszMenuName;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpszClassName;
            public IntPtr hIconSm;
        }


        #endregion

        #region Funtions
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int TrackPopupMenuEx(IntPtr hMenu,
                                                    int uFlags,
                                                    int x,
                                                    int y,
                                                    IntPtr hWnd,
                                                    IntPtr ignore);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int ScrollWindow(IntPtr hwnd, int cx, int cy, 
                                                ref GDI.RECT rectScroll,
                                                ref GDI.RECT rectClip);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern short GetKeyState(int keyCode);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int GetWindowLong(IntPtr hWnd, int index);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int SetWindowLong(IntPtr hWnd, int index, int dwNewLong);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);
        
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int SendMessage(IntPtr hWnd, int Msg,	IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int SendMessageTimeout(
            IntPtr hWnd, 
            int Msg, 
            IntPtr wParam, 
            IntPtr lParam, 
            int fuFlags,
            int uTimeout,
            out int lpdwResult
            );

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int PostMessage(IntPtr hWnd, int Msg,	IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int IsIconic(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int EnumChildWindows(IntPtr hWndParent, EnumChildProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern uint RegisterWindowMessage([MarshalAs(UnmanagedType.LPTStr)]string lpString);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, int uFlags);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int MoveWindow(IntPtr hWnd, int X, int Y, 
            int nWidth, int nHeight, [MarshalAs(UnmanagedType.Bool)] bool bRepaint);

        [DllImport("comctl32.dll", CharSet = CharSet.Unicode)]
        public static extern int _TrackMouseEvent(ref TRACKMOUSEEVENT lpEventTrack);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int CheckMenuItem(IntPtr hmenu, int uIDCheckItem, int uCheck);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern short RegisterClassEx([In] ref WNDCLASSEX lpwcx);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr CreateWindowEx(
           uint dwExStyle,
           string lpClassName,
           string lpWindowName,
           uint dwStyle,
           int x,
           int y,
           int nWidth,
           int nHeight,
           IntPtr hWndParent,
           IntPtr hMenu,
           IntPtr hInstance,
           IntPtr lpParam);

        [DllImport("user32.dll")]
        public static extern bool GetClientRect(IntPtr hWnd, out GDI.RECT lpRect);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, out GDI.RECT lpRect);

        [DllImport("user32.dll")]
        public static extern bool InvalidateRect(IntPtr hWnd, ref GDI.RECT lpRect, bool bErase);

        [DllImport("user32.dll")]
        public static extern IntPtr DefWindowProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

        [DllImport("user32.dll")]
        public static extern IntPtr SetCursor(IntPtr hCursor);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DestroyWindow(IntPtr hwnd);

        #endregion

        #region Defines
        public const int MF_BYCOMMAND      = 0x00000000;
        public const int MF_BYPOSITION     = 0x00000400;

        public const int MF_UNCHECKED      = 0x00000000;
        public const int MF_CHECKED        = 0x00000008;
               
        public const int GWL_STYLE = -16;
        public const int GWL_EXSTYLE = -20;

        public const int WS_OVERLAPPED = 0x00000000;
        public const int WS_POPUP = -2147483648; // 0x80000000
        public const int WS_VISIBLE = 0x10000000;
        public const int WS_CHILD = 0x40000000;
        public const int WS_CLIPSIBLINGS = 0x04000000;
        public const int WS_SYSMENU = 0x00080000;
        public const int WS_MINIMIZEBOX = 0x00020000;
        public const int WS_MAXIMIZEBOX = 0x00010000;

        public const int WS_EX_DLGMODALFRAME = 0x0001;

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

        [Flags]
        public enum ClassStyles : uint
        {
            CS_VREDRAW = 0x0001,
            CS_HREDRAW = 0x0002,
            CS_DBLCLKS = 0x0008,
            CS_OWNDC = 0x0020,
            CS_CLASSDC = 0x0040,
            CS_PARENTDC = 0x0080,
            CS_NOCLOSE = 0x0200,
            CS_SAVEBITS = 0x0800,
            CS_BYTEALIGNCLIENT = 0x1000,
            CS_BYTEALIGNWINDOW = 0x2000,
            CS_GLOBALCLASS = 0x4000,
            CS_IME = 0x00010000,
            CS_DROPSHADOW = 0x00020000
        }

        public const int
            IDC_ARROW = 32512,
            IDC_IBEAM = 32513,
            IDC_WAIT = 32514,
            IDC_CROSS = 32515,
            IDC_UPARROW = 32516,
            IDC_SIZE = 32640,
            IDC_ICON = 32641,
            IDC_SIZENWSE = 32642,
            IDC_SIZENESW = 32643,
            IDC_SIZEWE = 32644,
            IDC_SIZENS = 32645,
            IDC_SIZEALL = 32646,
            IDC_NO = 32648,
            IDC_HAND = 32649,
            IDC_APPSTARTING = 32650,
            IDC_HELP = 32651;

        #endregion
    }
}
