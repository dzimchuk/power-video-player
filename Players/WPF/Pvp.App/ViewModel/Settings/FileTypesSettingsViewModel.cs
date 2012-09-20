using System;
using GalaSoft.MvvmLight;

namespace Pvp.App.ViewModel.Settings
{
    internal class FileTypesSettingsViewModel : ViewModelBase, ISettingsViewModel
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

        public bool AnyChanges { get; private set; }
    }
}