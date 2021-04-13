using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Pvp.Core.Wpf
{
    public class MWKeyEventArgs : RoutedEventArgs
    {
        public MWKeyEventArgs(RoutedEvent routedEvent, Key key, ModifierKeys modifierKeys) : base(routedEvent)
        {
            Key = key;
            Modifiers = modifierKeys;
        }

        public Key Key { get; private set; }

        public ModifierKeys Modifiers { get; private set; }
    }
}