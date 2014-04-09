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

namespace Pvp.Core.Wpf
{
    internal class MediaWindow2 : IMediaWindow
    {
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        private static Guid CLSID_MediaWindowManager = new Guid("6F6EF4A2-2B39-4050-9FD2-E24065299518");

        private IEvrPresenterMediaWindowManager _manager;
        private IntPtr _hMediaWindow; // slight performance optimization

        public MediaWindow2()
        {
            Initialize();
        }

        private void Initialize()
        {
            object factoryObject = null;
            object managerObject = null;
            try
            {
                var hr = ClassFactory.GetEvrPresenterClassFactory(ref CLSID_MediaWindowManager, ref ClassFactory.IID_ClassFactory, out factoryObject);
                Marshal.ThrowExceptionForHR(hr);

                var factory = (IClassFactory)factoryObject;

                var iidMediaWindow = typeof(IEvrPresenterMediaWindowManager).GUID;
                hr = factory.CreateInstance(null, ref iidMediaWindow, out managerObject);
                Marshal.ThrowExceptionForHR(hr);

                _manager = (IEvrPresenterMediaWindowManager) managerObject;
                managerObject = null;

                Marshal.ThrowExceptionForHR(_manager.GetMediaWindow(out _hMediaWindow));
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
                if (_manager == null)
                {
                    throw new InvalidOperationException("MediaWindowManager has not been initialized.");
                }

                IntPtr hwnd;
                Marshal.ThrowExceptionForHR(_manager.GetMediaWindow(out hwnd));
                return hwnd;
            }
        }

        public void Invalidate()
        {
        }

        private GDI.RECT? _clientRect;
        private GDI.RECT? _windowRect;
        public void Move(GDI.RECT rcDest)
        {
            if (_clientRect == null)
            {
                GDI.RECT rect;
                WindowsManagement.GetClientRect(_hMediaWindow, out rect);
                _clientRect = rect;
            }
                
            if (_windowRect == null)
            {
                GDI.RECT rect;
                WindowsManagement.GetWindowRect(_hMediaWindow, out rect);
                _windowRect = rect;
            }

            if ((_clientRect.Value.right - _clientRect.Value.left) != (rcDest.right - rcDest.left) ||
                (_clientRect.Value.bottom - _clientRect.Value.top) != (rcDest.bottom - rcDest.top)) 
            {
                GDI.RECT rect;
                rect.left = rect.top = 0;
                rect.right = (rcDest.right - rcDest.left) + ((_windowRect.Value.right - _windowRect.Value.left) - (_clientRect.Value.right - _clientRect.Value.left));
                rect.bottom = (rcDest.bottom - rcDest.top) + ((_windowRect.Value.bottom - _windowRect.Value.top) - (_clientRect.Value.bottom - _clientRect.Value.top));

                if (WindowsManagement.MoveWindow(_hMediaWindow, rect.left, rect.top, rect.right, rect.bottom, false) != 0)
                {
                    _windowRect = rect;
                    _clientRect = rcDest;
                }
            }
        }

        public void SetRendererInterfaces(IVMRWindowlessControl VMRWindowlessControl, IVMRWindowlessControl9 VMRWindowlessControl9, IMFVideoDisplayControl MFVideoDisplayControl)
        {
        }

        ~MediaWindow2()
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
                Marshal.FinalReleaseComObject(_manager);
                _manager = null;
            }
        }
    }
}