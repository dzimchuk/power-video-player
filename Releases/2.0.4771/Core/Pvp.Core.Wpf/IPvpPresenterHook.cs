using System;
using System.Linq;

namespace Pvp.Core.Wpf
{
    internal interface IPvpPresenterHook : IDisposable
    {
        void HookUp(object rcwPresenter);
    }
}