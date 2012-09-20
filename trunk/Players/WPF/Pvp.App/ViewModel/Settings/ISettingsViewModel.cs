using System;
using System.Linq;

namespace Pvp.App.ViewModel.Settings
{
    internal interface ISettingsViewModel
    {
        bool AnyChanges { get; }
    }
}