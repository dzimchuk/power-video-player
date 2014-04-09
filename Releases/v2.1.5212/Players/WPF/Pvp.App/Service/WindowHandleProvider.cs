using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pvp.App.ViewModel;
using System.Windows.Interop;
using System.Windows;

namespace Pvp.App.Service
{
    internal class WindowHandleProvider : IWindowHandleProvider
    {
        private IntPtr? _handle;

        public IntPtr Handle
        {
            get 
            {
                if (_handle == null)
                    _handle = new WindowInteropHelper(Application.Current.MainWindow).Handle;

                return _handle.Value;
            }
        }
    }
}
