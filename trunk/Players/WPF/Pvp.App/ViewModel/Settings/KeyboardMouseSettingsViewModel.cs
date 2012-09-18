using System;
using GalaSoft.MvvmLight;

namespace Pvp.App.ViewModel.Settings
{
    internal class KeyboardMouseSettingsViewModel : ViewModelBase
    {
        private readonly ISettingsProvider _settingsProvider;

        public KeyboardMouseSettingsViewModel(ISettingsProvider settingsProvider)
        {
            _settingsProvider = settingsProvider;
            Load();
        }

        private void Load()
        {

        }

        public void Persist()
        {

        }
    }
}