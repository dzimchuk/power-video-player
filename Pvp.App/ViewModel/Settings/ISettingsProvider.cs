using System;
using System.Linq;

namespace Pvp.App.ViewModel.Settings
{
    internal interface ISettingsProvider
    {
        event EventHandler<SettingChangeEventArgs> SettingChanged;

        void Set<T>(string name, T value);

        T Get<T>(string name, T defaultValue);

        void Save();
    }
}
