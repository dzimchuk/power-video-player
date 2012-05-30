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
using Pvp.Core.DirectShow;
using Pvp.Core.MediaEngine;
using Pvp.Core.Native;

namespace Pvp.Core.Nwnd
{
    public class MediaWindow : IMediaWindow, IMediaWindowCallback
    {
        private static Guid CLSID_MediaWindowManager = new Guid("6F6EF4A2-2B39-4050-9FD2-E24065299518");

        private IMediaWindowManager _manager;
        private readonly IntPtr _hWnd;

        public MediaWindow(IntPtr hwndParent, int x, int y, int nWidth, int nHeight, int dwStyle)
        {
            IntPtr hwnd;
            Initialize(out hwnd, hwndParent, x, y, nWidth, nHeight, dwStyle);

            _manager.RegisterCallback(this);
            _hWnd = hwnd;
        }

        private void Initialize(out IntPtr phwnd, IntPtr hParent, int x, int y, int nWidth, int nHeight, int dwStyle)
        {
            object factoryObject = null;
            object managerObject = null;
            try
            {
                var hr = ClassFactory.GetClassFactory(ref CLSID_MediaWindowManager, ref ClassFactory.IID_ClassFactory, out factoryObject);
                Marshal.ThrowExceptionForHR(hr);

                var factory = (IClassFactory)factoryObject;

                var iidMediaWindow = typeof(IMediaWindowManager).GUID;
                hr = factory.CreateInstance(null, ref iidMediaWindow, out managerObject);
                Marshal.ThrowExceptionForHR(hr);

                _manager = (IMediaWindowManager)managerObject;
                managerObject = null;

                Marshal.ThrowExceptionForHR(_manager.CreateMediaWindow(out phwnd, hParent, x, y, nWidth, nHeight, dwStyle));
            }
            finally
            {
                if (factoryObject != null)
                {
                    Marshal.FinalReleaseComObject(factoryObject);
                }

                if (managerObject != null)
                {
                    Marshal.FinalReleaseComObject(managerObject);
                }
            }
        }

        public IntPtr Handle
        {
            get
            {
                return _hWnd;
            }
        }

        int IMediaWindowCallback.OnMessageReceived(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            if (MessageReceived != null)
                MessageReceived(this, new MessageReceivedEventArgs(hWnd, msg, wParam, lParam));

            return DsHlp.S_OK;
        }

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        public void Invalidate()
        {
            _manager.InvalidateMediaWindow();
        }

        public void Move(ref GDI.RECT rcDest)
        {
            WindowsManagement.MoveWindow(Handle, rcDest.left, rcDest.top, rcDest.right - rcDest.left, rcDest.bottom - rcDest.top, true);
        }

        public void SetRendererInterfaces(IVMRWindowlessControl VMRWindowlessControl,
            IVMRWindowlessControl9 VMRWindowlessControl9,
            IMFVideoDisplayControl MFVideoDisplayControl)
        {
            _manager.SetRunning(true, VMRWindowlessControl, VMRWindowlessControl9, MFVideoDisplayControl);
        }

        public void SetLogo(IntPtr logo)
        {
            _manager.SetLogo(logo);
        }

        public void ShowLogo(bool logo)
        {
            _manager.ShowLogo(logo);
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
            if (disposing && _manager != null)
            {
                _manager.SetRunning(false, null, null, null);

                Marshal.FinalReleaseComObject(_manager);
                _manager = null;
            }
        }
    }
}
