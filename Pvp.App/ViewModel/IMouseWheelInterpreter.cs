using System;
using System.Linq;

namespace Pvp.App.ViewModel
{
    public interface IMouseWheelInterpreter
    {
        int Interpret(EventArgs args);
    }
}