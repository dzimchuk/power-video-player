using System;
using System.Linq;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using Pvp.Core.Native;

namespace Pvp.Core.Wpf
{
    internal class MediaWindowHwndHost : HwndHost
    {
        public MediaWindowHwndHost()
        {
            VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
            HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
        }
         
        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            var hostHandle = WindowsManagement.CreateWindowEx(0,
                "static",
                "",
                WindowsManagement.WS_VISIBLE | WindowsManagement.WS_CHILD | WindowsManagement.WS_CLIPSIBLINGS,
                0,
                0,
                0,
                0,
                hwndParent.Handle,
                IntPtr.Zero,
                IntPtr.Zero,
                IntPtr.Zero);

            return new HandleRef(this, hostHandle);
        }

        protected override void DestroyWindowCore(HandleRef hwnd)
        {
            WindowsManagement.DestroyWindow(hwnd.Handle);
        }

        // if messages are going to be handled by MessageHook handler, we might need to provide the following implementation:
        // handled = false;
        // return IntPtr.Zero;
        //
        // if we are not going to handle messages, just leave the default implementation
        protected override IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            handled = false;
            return IntPtr.Zero;
            // return base.WndProc(hwnd, msg, wParam, lParam, ref handled);
        }
    }
}
