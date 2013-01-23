using System;

namespace Pvp.App.ViewModel.Settings
{
    internal class SettingChangeEventArgs : EventArgs
    {
        public string SettingName { get; private set; }

        public SettingChangeEventArgs(string settingName)
        {
            SettingName = settingName;
        }
    }
}