using System;
using System.Linq;
using Pvp.App.ViewModel;
using Pvp.Core.Native;

namespace Pvp.App.Service
{
    internal class CursorManager : ICursorManager
    {
        private IntPtr _previousCursor = IntPtr.Zero;
        private bool _isCursorVisible = true;

        public void ShowCursor()
        {
            if (_previousCursor != IntPtr.Zero)
            {
                WindowsManagement.SetCursor(_previousCursor);
            }

            _isCursorVisible = true;
        }

        public void HideCursor()
        {
            var previousCursor = WindowsManagement.SetCursor(IntPtr.Zero);
            if (previousCursor != IntPtr.Zero)
                _previousCursor = previousCursor;

            _isCursorVisible = false;
        }

        public bool IsCursorVisible
        {
            get { return _isCursorVisible; }
        }
    }
}