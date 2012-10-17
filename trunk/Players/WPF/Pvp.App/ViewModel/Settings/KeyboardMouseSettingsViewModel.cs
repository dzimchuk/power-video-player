using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Pvp.App.Messaging;

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
        private List<KeyCombinationItem> _keys;
        private List<KeyCombinationItem> _originalKeys; 

        public KeyboardMouseSettingsViewModel(ISettingsProvider settingsProvider, IDialogService dialogService)
        {
            _settingsProvider = settingsProvider;
            _dialogService = dialogService;

            Messenger.Default.Register<EventMessage>(this, OnEvent);

            Load();
        }

        private void OnEvent(EventMessage message)
        {
            if (message.Content == Event.EditKeyCombination)
            {
                EnterKeyCommand.Execute(((EditKeyCombinationEventArgs)message.EventArgs).KeyCombinationItem);
            }
        }

        public ICommand EnterKeyCommand
        {
            get
            {
                if (_enterKeyCommand == null)
                {
                    _enterKeyCommand = new RelayCommand<KeyCombinationItem>(item =>
                                                                                {
                                                                                    item.KeyCombination = _dialogService.ShowEnterKeyWindow();
                                                                                    RaisePropertyChanged("CheckMe");
                                                                                });
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

        public IEnumerable<KeyCombinationItem> Keys
        {
            get { return _keys; }
        }

        private void Load()
        {
            _mouseWheelAction = MouseWheelActionOriginal;

            var keys = _settingsProvider.Get(SettingsConstants.KeyMap, DefaultSettings.KeyMap);
            _originalKeys = new List<KeyCombinationItem>();
            foreach (var pair in keys)
            {
                _originalKeys.Add(new KeyCombinationItem
                                      {
                                          Key = pair.Key,
                                          KeyCombination = pair.Value
                                      });
            }

            _keys = new List<KeyCombinationItem>(_originalKeys.Select(i => i.Clone()));
        }

        private MouseWheelAction MouseWheelActionOriginal
        {
            get { return _settingsProvider.Get(SettingsConstants.MouseWheelAction, DefaultSettings.MouseWheekAction); }
        }

        public void Persist()
        {
            _settingsProvider.Set(SettingsConstants.MouseWheelAction, _mouseWheelAction);

            var keys = _keys.ToDictionary(i => i.Key, i => i.KeyCombination);
            _settingsProvider.Set(SettingsConstants.KeyMap, keys);
        }

        public bool AnyChanges
        {
            get
            {
                return _originalKeys.Where((t, i) => t != _keys[i]).Any() || _mouseWheelAction != MouseWheelActionOriginal;
            }
        }
    }
}