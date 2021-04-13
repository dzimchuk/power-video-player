using System;

namespace Pvp.Core.MediaEngine
{
    public class ErrorOccuredEventArgs : EventArgs
    {
        private readonly string _message;

        public ErrorOccuredEventArgs(string message)
        {
            _message = message;
        }

        public string Message
        {
            get { return _message; }
        }
    }
}