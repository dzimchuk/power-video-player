using System;
using GalaSoft.MvvmLight;

namespace Pvp.App.ViewModel.Settings
{
    internal class VideoSettingsViewModel : ViewModelBase
    {
        private readonly ISettingsProvider _settingsProvider;

        public VideoSettingsViewModel(ISettingsProvider settingsProvider)
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