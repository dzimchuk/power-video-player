using System;
using System.Linq;

namespace Pvp.App.Messaging
{
    internal class KeyboardActionEventArgs : EventArgs
    {
        public string Action { get; private set; }

        public KeyboardActionEventArgs(string action)
        {
            Action = action;
        }
    }
}