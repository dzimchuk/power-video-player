using System;

namespace Pvp.Core.MediaEngine
{
    public class MessageReceivedEventArgs : EventArgs
    {
        private readonly IntPtr _hwnd;
        private readonly uint _msg;
        private readonly IntPtr _wParam;
        private readonly IntPtr _lParam;

        public MessageReceivedEventArgs(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            _hwnd = hWnd;
            _msg = msg;
            _wParam = wParam;
            _lParam = lParam;

            ReturnValue = new IntPtr?();
        }

        public IntPtr HWnd
        {
            get { return _hwnd; }
        }

        public uint Msg
        {
            get { return _msg; }
        }

        public IntPtr WParam
        {
            get { return _wParam; }
        }

        public IntPtr LParam
        {
            get { return _lParam; }
        }

        public IntPtr? ReturnValue { get; set; }
    }
}