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
using Dzimchuk.Native;

namespace Dzimchuk.MediaEngine.Core
{
    public class UserDecisionEventArgs : EventArgs
    {
        public UserDecisionEventArgs(string message)
            : base()
        {
            _message = message;
        }

        private string _message;
        private bool _accept;

        public bool Accept
        {
            get { return _accept; }
            set { _accept = value; }
        }

        public string Message
        {
            get { return _message; }
        }
    }

    public class MessageReceivedEventArgs : EventArgs
    {
        private IntPtr _hwnd;
        private uint _msg;
        private IntPtr _wParam;
        private IntPtr _lParam;

        private IntPtr? _returnValue;

        public MessageReceivedEventArgs(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
            : base()
        {
            _hwnd = hWnd;
            _msg = msg;
            _wParam = wParam;
            _lParam = lParam;

            _returnValue = new Nullable<IntPtr>();
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

        public IntPtr? ReturnValue
        {
            get { return _returnValue; }
            set { _returnValue = value; }
        }
    }

    public class ErrorOccuredEventArgs : EventArgs
    {
        private string _message;

        public ErrorOccuredEventArgs(string message)
            : base()
        {
            _message = message;
        }

        public string Message
        {
            get { return _message; }
        }
    }

    public class InitSizeEventArgs : EventArgs
    {
        private GDI.SIZE _newSize;

        public InitSizeEventArgs(GDI.SIZE newSize) : base()
        {
            _newSize = newSize;
        }

        public GDI.SIZE NewVideSize
        {
            get { return _newSize; }
        }
    }

    public class DestinationRectangleChangedEventArgs : EventArgs
    {
        private GDI.RECT _newRect;

        public DestinationRectangleChangedEventArgs(GDI.RECT newRect)
            : base()
        {
            _newRect = newRect;
        }

        public GDI.RECT NewDestinationRectangle
        {
            get { return _newRect; }
        }
    }
}
