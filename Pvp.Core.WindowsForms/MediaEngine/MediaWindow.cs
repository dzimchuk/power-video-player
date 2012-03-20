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
using System.Windows.Forms;
using Pvp.Core.DirectShow;
using Pvp.Core.Native;
using System.Runtime.InteropServices;

namespace Pvp.Core.MediaEngine
{
    internal class MediaWindow : NativeWindow, IMediaWindow
    {
        #region Imported functions from nwnd.dll

        private static class NwndWrapper
        {
            [DllImport("nwnd.dll")]
            public static extern IntPtr CreateMediaWindow(IntPtr hParent, int nWidth, int nHeight);

            [DllImport("nwnd.dll")]
            public static extern void SetRunning([MarshalAs(UnmanagedType.Bool)] bool bRunning,
                IVMRWindowlessControl pVMR, IVMRWindowlessControl9 pVMR9, IMFVideoDisplayControl pEVR);

            [DllImport("nwnd.dll")]
            public static extern void SetLogo(IntPtr hLogo);

            [DllImport("nwnd.dll")]
            public static extern void IsShowLogo([MarshalAs(UnmanagedType.Bool)] bool bShow);

            [DllImport("nwnd.dll")]
            public static extern void InvalidateMediaWindow();
        }

        private static class Nwnd64Wrapper
        {
            [DllImport("nwnd64.dll")]
            public static extern IntPtr CreateMediaWindow(IntPtr hParent, int nWidth, int nHeight);

            [DllImport("nwnd64.dll")]
            public static extern void SetRunning([MarshalAs(UnmanagedType.Bool)] bool bRunning,
                IVMRWindowlessControl pVMR, IVMRWindowlessControl9 pVMR9, IMFVideoDisplayControl pEVR);

            [DllImport("nwnd64.dll")]
            public static extern void SetLogo(IntPtr hLogo);

            [DllImport("nwnd64.dll")]
            public static extern void IsShowLogo([MarshalAs(UnmanagedType.Bool)] bool bShow);

            [DllImport("nwnd64.dll")]
            public static extern void InvalidateMediaWindow();
        }

        private static IntPtr CreateMediaWindow(IntPtr hParent, int nWidth, int nHeight)
        {
            return IntPtr.Size == 8 ? Nwnd64Wrapper.CreateMediaWindow(hParent, nWidth, nHeight) : NwndWrapper.CreateMediaWindow(hParent, nWidth, nHeight);
        }

        private static void SetRunning(bool bRunning, IVMRWindowlessControl pVMR, IVMRWindowlessControl9 pVMR9, IMFVideoDisplayControl pEVR)
        {
            if (IntPtr.Size == 8)
                Nwnd64Wrapper.SetRunning(bRunning, pVMR, pVMR9, pEVR);
            else
                NwndWrapper.SetRunning(bRunning, pVMR, pVMR9, pEVR);
        }

        public static void SetLogo(IntPtr hLogo)
        {
            if (IntPtr.Size == 8)
                Nwnd64Wrapper.SetLogo(hLogo);
            else
                NwndWrapper.SetLogo(hLogo);
        }

        public static void IsShowLogo(bool bShow)
        {
            if (IntPtr.Size == 8)
                Nwnd64Wrapper.IsShowLogo(bShow);
            else
                NwndWrapper.IsShowLogo(bShow);
        }

        public static void InvalidateMediaWindow()
        {
            if (IntPtr.Size == 8)
                Nwnd64Wrapper.InvalidateMediaWindow();
            else
                NwndWrapper.InvalidateMediaWindow();
        }

        #endregion
        
        public MediaWindow(IntPtr hwndParent, int nWidth, int nHeight)
        {
            IntPtr hwnd = CreateMediaWindow(hwndParent, nWidth, nHeight);
            AssignHandle(hwnd);
        }
        
        protected override void WndProc(ref Message m)
        {
            if (MessageReceived != null)
                MessageReceived(this, new MessageReceivedEventArgs(m.HWnd, (uint)m.Msg, m.WParam, m.LParam));
            
            base.WndProc(ref m);
        }

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        public void Invalidate()
        {
            InvalidateMediaWindow();
        }

        public void Move(ref GDI.RECT rcDest)
        {
            WindowsManagement.MoveWindow(Handle, rcDest.left, rcDest.top, rcDest.right - rcDest.left, rcDest.bottom - rcDest.top, true);
        }

        public void SetRendererInterfaces(IVMRWindowlessControl VMRWindowlessControl, 
                                          IVMRWindowlessControl9 VMRWindowlessControl9, 
                                          IMFVideoDisplayControl MFVideoDisplayControl)
        {
            SetRunning(true, VMRWindowlessControl, VMRWindowlessControl9, MFVideoDisplayControl);
        }

        ~MediaWindow()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            SetRunning(false, null, null, null);
            DestroyHandle();
        }
    }
}
