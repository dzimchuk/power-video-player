using System;
using System.Linq;
using Pvp.App.ViewModel.Settings;

namespace Pvp.App.Messaging
{
    internal class EditKeyCombinationEventArgs : EventArgs
    {
        public EditKeyCombinationEventArgs(KeyCombinationItem keyCombinationItem)
        {
            KeyCombinationItem = keyCombinationItem;
        }

        public KeyCombinationItem KeyCombinationItem { get; private set; }
    }
}