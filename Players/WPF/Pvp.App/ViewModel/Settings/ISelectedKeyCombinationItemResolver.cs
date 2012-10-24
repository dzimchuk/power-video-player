using System;
using System.Linq;

namespace Pvp.App.ViewModel.Settings
{
    public interface ISelectedKeyCombinationItemResolver
    {
        KeyCombinationItem Resolve(EventArgs args);
    }
}