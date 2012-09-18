using System;
using GalaSoft.MvvmLight;

namespace Pvp.App.ViewModel.Settings
{
    internal class FileTypesSettingsViewModel : ViewModelBase
    {
        private readonly ISettingsProvider _settingsProvider;

        public FileTypesSettingsViewModel(ISettingsProvider settingsProvider)
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