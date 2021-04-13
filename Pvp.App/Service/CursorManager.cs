using System;
using System.Linq;
using Pvp.App.ViewModel;
using Pvp.Core.Native;

namespace Pvp.App.Service
{
    internal class CursorManager : ICursorManager
    {
        private IntPtr _previousCursor = IntPtr.Zero;

        public void ShowCursor()
        {
            if (_previousCursor != IntPtr.Zero)
            {
                WindowsManagement.SetCursor(_previousCursor);
            }
        }

        public void HideCursor()
        {
            var previousCursor = WindowsManagement.SetCursor(IntPtr.Zero);
            if (previousCursor != IntPtr.Zero)
            {
                _previousCursor = previousCursor;
            }
        }

        public bool IsCursorVisible
        {
            get { return WindowsManagement.GetCursor() != IntPtr.Zero; }
        }
    }
}