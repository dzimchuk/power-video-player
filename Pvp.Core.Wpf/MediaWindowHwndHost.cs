using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using Dzimchuk.Native;

namespace Dzimchuk.MediaEngine.Core
{
    internal class MediaWindowHwndHost : HwndHost
    {
        private IMediaWindow _mediaWindow;

        public MediaWindowHwndHost()
        {
            VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
            HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
        }

        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            var hostHandle = WindowsManagement.CreateWindowEx(0,
                                                "static",
                                                null,
                                                WindowsManagement.WS_VISIBLE | WindowsManagement.WS_CHILD | WindowsManagement.WS_CLIPSIBLINGS,
                                                0,
                                                0,
                                                0,
                                                0,
                                                hwndParent.Handle,
                                                IntPtr.Zero,
                                                IntPtr.Zero,
                                                IntPtr.Zero);

            _mediaWindow = new DefaultMediaWindow(hostHandle);

            return new HandleRef(this, hostHandle);
        }

        protected override void DestroyWindowCore(HandleRef hwnd)
        {
            if (_mediaWindow != null)
                _mediaWindow.Dispose();

            WindowsManagement.DestroyWindow(hwnd.Handle);
        }

        // if messages are going to be handled by MessageHook handler, we might need to provide the following implementation:
        // handled = false;
        // return IntPtr.Zero;
        //
        // if we are not going to handle messages, just leave the default implementation
        protected override IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            return base.WndProc(hwnd, msg, wParam, lParam, ref handled);
        }

        public IMediaWindow MediaWindow
        {
            get { return _mediaWindow; }
        }
    }
}
