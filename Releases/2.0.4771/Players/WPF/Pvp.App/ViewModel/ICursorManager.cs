using System;
using System.Linq;

namespace Pvp.App.ViewModel
{
    public interface ICursorManager
    {
        void ShowCursor();
        void HideCursor();
        bool IsCursorVisible { get; }
    }
}