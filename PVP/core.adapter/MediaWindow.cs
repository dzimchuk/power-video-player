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
using Dzimchuk.DirectShow;
using Dzimchuk.Native;
using System.Runtime.InteropServices;

namespace Dzimchuk.MediaEngine.Core
{
    internal class MediaWindow : NativeWindow, IMediaWindow
    {
        private IntPtr _hwnParent;
        private bool _keepOpen;
        private bool _running;

        #region Imported functions from nwnd.dll

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

        #endregion
        
        public MediaWindow(IntPtr hwndParent, int nWidth, int nHeight, bool keepOpen)
        {
            IntPtr hwnd = CreateMediaWindow(hwndParent, nWidth, nHeight);
            AssignHandle(hwnd);

            _hwnParent = hwndParent;
            _keepOpen = keepOpen;
        }
        
        protected override void WndProc(ref Message m)
        {
            if (MessageReceived != null)
                MessageReceived(this, new MessageReceivedEventArgs(m.HWnd, (uint)m.Msg, m.WParam, m.LParam));
            
            base.WndProc(ref m);
        }

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        public IntPtr HostHandle
        {
            get { return _hwnParent; }
        }

        public bool KeepOpen
        {
            get { return _keepOpen; }
        }

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
            _running = true;
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
            if (disposing && _running && Recreate != null) // when disposing is FALSE we are not on UI thread anyway
                Recreate(null, EventArgs.Empty);
        }

        public event EventHandler Recreate;

        public bool IsRunning
        {
            get { return _running; }
        }
    }
}
