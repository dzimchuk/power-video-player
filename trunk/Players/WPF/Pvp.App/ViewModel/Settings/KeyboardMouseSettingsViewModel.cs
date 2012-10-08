using System;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace Pvp.App.ViewModel.Settings
{
    internal class KeyboardMouseSettingsViewModel : ViewModelBase, ISettingsViewModel
    {
        private readonly ISettingsProvider _settingsProvider;
        private readonly IDialogService _dialogService;

        private ICommand _enterKeyCommand;
        private ICommand _clearCommand;
        private ICommand _clearAllCommand;
        private ICommand _defaultsCommand;

        private MouseWheelAction _mouseWheelAction;

        public KeyboardMouseSettingsViewModel(ISettingsProvider settingsProvider, IDialogService dialogService)
        {
            _settingsProvider = settingsProvider;
            _dialogService = dialogService;

            Load();
        }

        public ICommand EnterKeyCommand
        {
            get
            {
                if (_enterKeyCommand == null)
                {
                    _enterKeyCommand = new RelayCommand(() => _dialogService.ShowEnterKeyWindow());
                }

                return _enterKeyCommand;
            }
        }

        public ICommand ClearCommand
        {
            get
            {
                if (_clearCommand == null)
                {
                    _clearCommand = new RelayCommand(() => Console.WriteLine());
                }

                return _clearCommand;
            }
        }

        public ICommand ClearAllCommand
        {
            get
            {
                if (_clearAllCommand == null)
                {
                    _clearAllCommand = new RelayCommand(() => Console.WriteLine());
                }

                return _clearAllCommand;
            }
        }

        public ICommand DefaultsCommand
        {
            get
            {
                if (_defaultsCommand == null)
                {
                    _defaultsCommand = new RelayCommand(() => Console.WriteLine());
                }

                return _defaultsCommand;
            }
        }

        public MouseWheelAction MouseWheelAction
        {
            get { return _mouseWheelAction; }
            set
            {
                if (Equals(value, _mouseWheelAction)) return;
                _mouseWheelAction = value;
                RaisePropertyChanged("MouseWheelAction");
            }
        }

        private void Load()
        {
            _mouseWheelAction = MouseWheelActionOriginal;
        }

        private MouseWheelAction MouseWheelActionOriginal
        {
            get { return _settingsProvider.Get(SettingsConstants.MouseWheelAction, DefaultSettings.MouseWheekAction); }
        }

        public void Persist()
        {
            _settingsProvider.Set(SettingsConstants.MouseWheelAction, _mouseWheelAction);
        }

        public bool AnyChanges { get { return _mouseWheelAction != MouseWheelActionOriginal; } }
    }
}