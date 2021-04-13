using System;
using System.Linq;
using System.Windows;

namespace Pvp.Core.Wpf
{
    public class MWMouseWheelEventArgs : RoutedEventArgs
    {
        public int Delta { get; private set; }

        public MWMouseWheelEventArgs(RoutedEvent routedEvent, int delta) : base(routedEvent)
        {
            Delta = delta;
        }
    }
}