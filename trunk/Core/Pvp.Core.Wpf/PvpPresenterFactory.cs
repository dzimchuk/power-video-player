using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Interop;

namespace Pvp.Core.Wpf
{
    internal static class PvpPresenterFactory
    {
        public static IPvpPresenterHook GetPvpPresenter(D3DImage image)
        {
            return new PvpPresenter2(image);
        }
    }
}
