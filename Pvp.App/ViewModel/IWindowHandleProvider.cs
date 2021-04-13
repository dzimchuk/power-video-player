using System;
using System.Linq;

namespace Pvp.App.ViewModel
{
    public interface IWindowHandleProvider
    {
        IntPtr Handle { get; }
    }
}