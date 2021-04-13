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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Pvp.Core.DirectShow;
using Pvp.Core.Native;

namespace Pvp.Core.MediaEngine
{
    /// <summary>
    /// A native window wrapper
    /// </summary>
    public class DefaultMediaWindow : IMediaWindow
    {
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;
        
        private const string WINDOW_CLASS_NAME = "PVP_MEDIA_WINDOW";
        private IntPtr _hwnd;

        private IVMRWindowlessControl  _VMRWindowlessControl;
        private IVMRWindowlessControl9 _VMRWindowlessControl9;
        private IMFVideoDisplayControl _MFVideoDisplayControl;

        private static IDictionary<IntPtr, WindowsManagement.WndProc> _procs =
            new Dictionary<IntPtr, WindowsManagement.WndProc>();
        private static WindowsManagement.WndProc _global_wnd_proc = WndProc; // prevent garbage collection of the delegate

        static DefaultMediaWindow()
        {
            WindowsManagement.WNDCLASSEX wcex = new WindowsManagement.WNDCLASSEX();
            wcex.cbSize = (uint)Marshal.SizeOf(wcex);
            wcex.style = (uint)(WindowsManagement.ClassStyles.CS_HREDRAW |
                                WindowsManagement.ClassStyles.CS_VREDRAW |
                                WindowsManagement.ClassStyles.CS_DBLCLKS);
            wcex.lpfnWndProc += _global_wnd_proc;
            wcex.cbClsExtra = 0;
            wcex.cbWndExtra = 0;
            wcex.hInstance = IntPtr.Zero;
            wcex.hIcon = IntPtr.Zero;
            wcex.hCursor = WindowsManagement.LoadCursor(IntPtr.Zero, WindowsManagement.IDC_ARROW);
            wcex.hbrBackground = IntPtr.Zero;
            wcex.lpszMenuName = null;
            wcex.lpszClassName = WINDOW_CLASS_NAME;
            wcex.hIconSm = IntPtr.Zero;

            WindowsManagement.RegisterClassEx(ref wcex);
        }

        private static IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            WindowsManagement.WndProc proc;
            if (_procs.TryGetValue(hWnd, out proc))
            {
                return proc(hWnd, msg, wParam, lParam);
            }
            else
            {
                return WindowsManagement.DefWindowProc(hWnd, msg, wParam, lParam);
            }
        }
        
        public DefaultMediaWindow(IntPtr hwndParent)
        {
            if (hwndParent == IntPtr.Zero)
                throw new ArgumentNullException(Resources.Resources.error_no_hwnd);
            
            _hwnd = CreateWindow(hwndParent);
            if (_hwnd == IntPtr.Zero)
                throw new Exception(Resources.Resources.error_cant_create_media_window);

            _procs.Add(_hwnd, OnWndProc);
        }

        ~DefaultMediaWindow()
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
            // no matter what, destroy the handle
            if (_hwnd != IntPtr.Zero)
            {
                System.Diagnostics.Debug.Assert(_procs.ContainsKey(_hwnd), "Handle wasn't found in the inernal collection.");
                _procs.Remove(_hwnd);
                System.Diagnostics.Debug.Assert(!_procs.ContainsKey(_hwnd), "Handle wasn't removed from the inernal collection.");
                
                WindowsManagement.DestroyWindow(_hwnd);
                _hwnd = IntPtr.Zero;
            }
        }

        private IntPtr CreateWindow(IntPtr hwndParent)
        {
            return WindowsManagement.CreateWindowEx(0,
                                                    WINDOW_CLASS_NAME, 
                                                    null,
                                                    WindowsManagement.WS_VISIBLE | WindowsManagement.WS_CHILD | WindowsManagement.WS_CLIPSIBLINGS,
                                                    0, 
                                                    0, 
                                                    0, 
                                                    0, 
                                                    hwndParent, 
                                                    IntPtr.Zero, 
                                                    IntPtr.Zero, 
                                                    IntPtr.Zero);
        }

        protected virtual IntPtr OnWndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            MessageReceivedEventArgs args = new MessageReceivedEventArgs(hWnd, msg, wParam, lParam);
            OnMessageReceived(args);

            if (!args.ReturnValue.HasValue)
            {
                switch (msg)
                {
                    case (int)WindowsMessages.WM_PAINT:
                        var processed = Paint();
                        if (processed)
                            args.ReturnValue = IntPtr.Zero;
                        break;
                    case (int)WindowsMessages.WM_ERASEBKGND:
                        args.ReturnValue = new IntPtr(1); // return non-zero to indicate no further erasing is required
                        break;
                    case (int)WindowsMessages.WM_DISPLAYCHANGE:
                        if (_VMRWindowlessControl != null)
                            _VMRWindowlessControl.DisplayModeChanged();
                        else if (_VMRWindowlessControl9 != null)
                            _VMRWindowlessControl9.DisplayModeChanged();
                        break;
                }
            }
            
            return args.ReturnValue ?? WindowsManagement.DefWindowProc(hWnd, msg, wParam, lParam);
        }

        protected virtual bool Paint()
        {
            if (_MFVideoDisplayControl != null)
            {
                _MFVideoDisplayControl.RepaintVideo();
            }
            else
            {
                GDI.PAINTSTRUCT ps;
                IntPtr hDC = GDI.BeginPaint(_hwnd, out ps);

                if (_VMRWindowlessControl != null)
                {
                    _VMRWindowlessControl.RepaintVideo(_hwnd, hDC);
                }
                else if (_VMRWindowlessControl9 != null)
                {
                    _VMRWindowlessControl9.RepaintVideo(_hwnd, hDC);
                }

                GDI.EndPaint(_hwnd, ref ps);
            }

            return true;
        }

        protected virtual void OnMessageReceived(MessageReceivedEventArgs args)
        {
            if (MessageReceived != null)
                MessageReceived(this, args);
        }

        public void Invalidate()
        {
            if (_hwnd != IntPtr.Zero)
            {
                GDI.RECT rcClient;
                WindowsManagement.GetClientRect(_hwnd, out rcClient);
                WindowsManagement.InvalidateRect(_hwnd, ref rcClient, false);
            }
        }

        public void Move(GDI.RECT rcDest)
        {
            if (_hwnd != null)
            {
                WindowsManagement.MoveWindow(_hwnd, rcDest.left, rcDest.top, rcDest.right - rcDest.left, rcDest.bottom - rcDest.top, true);
            }
        }

        public IntPtr Handle
        {
            get { return _hwnd; }
        }

        public void SetRendererInterfaces(IVMRWindowlessControl VMRWindowlessControl,
                                          IVMRWindowlessControl9 VMRWindowlessControl9,
                                          IMFVideoDisplayControl MFVideoDisplayControl)
        {
            _VMRWindowlessControl = VMRWindowlessControl;
            _VMRWindowlessControl9 = VMRWindowlessControl9;
            _MFVideoDisplayControl = MFVideoDisplayControl;
        }
    }
}
