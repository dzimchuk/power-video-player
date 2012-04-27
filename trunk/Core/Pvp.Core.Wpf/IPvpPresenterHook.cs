using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pvp.Core.Wpf
{
    internal interface IPvpPresenterHook : IDisposable
    {
        void HookUp(object rcwPresenter);
    }
}
