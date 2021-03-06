using System;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace Pvp.App.ViewModel.Settings
{
    internal class GeneralSettingsViewModel : ViewModelBase, ISettingsViewModel
    {
        private readonly ISettingsProvider _settingsProvider;
        private readonly IFileSelector _fileSelector;

        private bool _startFullScreen;
        private bool _autoPlay;
        private bool _rememberVolume;
        private bool _centerWindow;
        private bool _showLogo;
        private bool _topMost;
        private string _screenshotsFolder;

        private ICommand _chooseFolderCommand;

        public GeneralSettingsViewModel(ISettingsProvider settingsProvider, IFileSelector fileSelector)
        {
            _settingsProvider = settingsProvider;
            _fileSelector = fileSelector;
            Load();
        }
        
        private void Load()
        {
            _startFullScreen = StartFullScreenOriginal;
            _autoPlay = AutoPlayOriginal;
            _rememberVolume = RememberVolumeOriginal;
            _centerWindow = CenterWindowOriginal;
            _showLogo = ShowLogoOriginal;
            _topMost = TopMostOriginal;
            _screenshotsFolder = ScreenshotsFolderOriginal;
        }

        private bool TopMostOriginal
        {
            get { return _settingsProvider.Get("TopMost", false); }
        }

        private bool ShowLogoOriginal
        {
            get { return _settingsProvider.Get("ShowLogo", true); }
        }

        private bool CenterWindowOriginal
        {
            get { return _settingsProvider.Get("CenterWindow", true); }
        }

        private bool RememberVolumeOriginal
        {
            get { return _settingsProvider.Get("RememberVolume", true); }
        }

        private bool AutoPlayOriginal
        {
            get { return _settingsProvider.Get("AutoPlay", true); }
        }

        private bool StartFullScreenOriginal
        {
            get { return _settingsProvider.Get("StartFullScreen", false); }
        }

        private string ScreenshotsFolderOriginal
        {
            get { return _settingsProvider.Get("ScreenshotsFolder", DefaultSettings.SreenshotsFolder); }
        }

        public void Persist()
        {
        	_settingsProvider.Set("StartFullScreen", _startFullScreen);
            _settingsProvider.Set("AutoPlay", _autoPlay);
            _settingsProvider.Set("RememberVolume", _rememberVolume);
            _settingsProvider.Set("CenterWindow", _centerWindow);
            _settingsProvider.Set("ShowLogo", _showLogo);
            _settingsProvider.Set("TopMost", _topMost);
            _settingsProvider.Set("ScreenshotsFolder", _screenshotsFolder);
        }

        public bool StartFullScreen
        {
            get { return _startFullScreen; }
            set
            {
                _startFullScreen = value;
                RaisePropertyChanged("StartFullScreen");
            }
        }

        public bool TopMost
        {
            get { return _topMost; }
            set
            {
                _topMost = value;
                RaisePropertyChanged("TopMost");
            }
        }

        public bool ShowLogo
        {
            get { return _showLogo; }
            set
            {
                _showLogo = value;
                RaisePropertyChanged("ShowLogo");
            }
        }

        public bool CenterWindow
        {
            get { return _centerWindow; }
            set
            {
                _centerWindow = value;
                RaisePropertyChanged("CenterWindow");
            }
        }

        public bool RememberVolume
        {
            get { return _rememberVolume; }
            set
            {
                _rememberVolume = value;
                RaisePropertyChanged("RememberVolume");
            }
        }

        public bool AutoPlay
        {
            get { return _autoPlay; }
            set
            {
                _autoPlay = value;
                RaisePropertyChanged("AutoPlay");
            }
        }

        public string ScreenshotsFolder
        {
            get { return _screenshotsFolder; }
            set
            {
                if (value == _screenshotsFolder) return;
                _screenshotsFolder = value;
                RaisePropertyChanged("ScreenshotsFolder");
            }
        }

        public ICommand ChooseFolderCommand
        {
            get
            {
                if (_chooseFolderCommand == null)
                {
                    _chooseFolderCommand = new RelayCommand(
                        () =>
                            {
                                var folder = _fileSelector.SelectFolder(_screenshotsFolder);
                                if (!string.IsNullOrEmpty(folder))
                                {
                                    ScreenshotsFolder = folder;
                                }
                            });
                }

                return _chooseFolderCommand;
            }
        }

        public bool AnyChanges
        {
            get
            {
                return _autoPlay != AutoPlayOriginal || _centerWindow != CenterWindowOriginal
                       || _rememberVolume != RememberVolumeOriginal || _showLogo != ShowLogoOriginal 
                       || _startFullScreen != StartFullScreenOriginal || _topMost != TopMostOriginal
                       || (!string.IsNullOrEmpty(_screenshotsFolder) && !string.IsNullOrEmpty(ScreenshotsFolderOriginal) && !_screenshotsFolder.Equals(ScreenshotsFolderOriginal));
            }
        }
    }
}