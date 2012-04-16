using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pvp.App.ViewModel
{
    internal interface IWindowHandleProvider
    {
        IntPtr Handle { get; }
    }
}
