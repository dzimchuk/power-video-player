using System;
using System.Linq;
using System.Windows.Input;
using Pvp.App.ViewModel;
using Pvp.Core.Wpf;

namespace Pvp.App.Service
{
    internal class MouseWheelInterpreter : IMouseWheelInterpreter
    {
        public int Interpret(EventArgs args)
        {
            var delta = 0;

            var mouseWheelArgs = args as MouseWheelEventArgs;
            if (mouseWheelArgs != null)
            {
                delta = mouseWheelArgs.Delta;
            }
            else
            {
                var mwArgs = args as MWMouseWheelEventArgs;
                if (mwArgs != null)
                {
                    delta = mwArgs.Delta;
                }
            }

            return delta;
        }
    }
}