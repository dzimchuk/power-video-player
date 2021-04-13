using System;
using System.Linq;

namespace Pvp.App.Messaging
{
    internal class KeyboardMouseActionEventArgs : EventArgs
    {
        public string Action { get; private set; }

        public KeyboardMouseActionEventArgs(string action)
        {
            Action = action;
        }
    }
}