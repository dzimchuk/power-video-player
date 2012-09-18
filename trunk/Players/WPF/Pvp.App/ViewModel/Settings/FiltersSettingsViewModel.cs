using System;
using GalaSoft.MvvmLight;

namespace Pvp.App.ViewModel.Settings
{
    internal class FiltersSettingsViewModel : ViewModelBase
    {
        private readonly ISettingsProvider _settingsProvider;

        public FiltersSettingsViewModel(ISettingsProvider settingsProvider)
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