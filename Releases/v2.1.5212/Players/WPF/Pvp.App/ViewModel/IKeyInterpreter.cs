using System;
using System.Linq;

namespace Pvp.App.ViewModel
{
    public interface IKeyInterpreter
    {
        KeyCombination Interpret(EventArgs args);
    }
}