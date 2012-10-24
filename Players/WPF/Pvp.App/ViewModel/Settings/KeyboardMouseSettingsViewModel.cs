using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace Pvp.App.ViewModel.Settings
{
    internal class KeyboardMouseSettingsViewModel : ViewModelBase, ISettingsViewModel
    {
        private readonly ISettingsProvider _settingsProvider;
        private readonly IDialogService _dialogService;
        private readonly ISelectedKeyCombinationItemResolver _selectedKeyCombinationItemResolver;

        private ICommand _enterKeyCommand;
        private ICommand _clearCommand;
        private ICommand _clearAllCommand;
        private ICommand _defaultsCommand;
        private ICommand _selectedItemChangedCommand;

        private MouseWheelAction _mouseWheelAction;
        private List<KeyCombinationItem> _keys;
        private List<KeyCombinationItem> _originalKeys;
        private List<KeyCombinationItem> _defaultKeys;

        private KeyCombinationItem _selectedItem;

        public KeyboardMouseSettingsViewModel(ISettingsProvider settingsProvider, IDialogService dialogService,
                                              ISelectedKeyCombinationItemResolver selectedKeyCombinationItemResolver)
        {
            _settingsProvider = settingsProvider;
            _dialogService = dialogService;
            _selectedKeyCombinationItemResolver = selectedKeyCombinationItemResolver;

            Load();
        }

        public ICommand SelectedItemChangedCommand
        {
            get
            {
                if (_selectedItemChangedCommand == null)
                {
                    _selectedItemChangedCommand = new RelayCommand<EventArgs>(args => { _selectedItem = _selectedKeyCombinationItemResolver.Resolve(args); });
                }

                return _selectedItemChangedCommand;
            }
        }

        public ICommand EnterKeyCommand
        {
            get
            {
                if (_enterKeyCommand == null)
                {
                    _enterKeyCommand = new RelayCommand<EventArgs>(args =>
                                                                       {
                                                                           var item = _selectedKeyCombinationItemResolver.Resolve(args);
                                                                           if (item != null)
                                                                           {
                                                                               item.KeyCombination = _dialogService.ShowEnterKeyWindow();
                                                                               NotifySettingsViewModel();
                                                                           }
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
                    _clearCommand = new RelayCommand(() =>
                                                         {
                                                             if (_selectedItem != null)
                                                             {
                                                                 _selectedItem.KeyCombination = new KeyCombination();
                                                                 NotifySettingsViewModel();
                                                             }
                                                         });
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
                    _clearAllCommand = new RelayCommand(() =>
                                                            {
                                                                var emptyCombination = new KeyCombination();
                                                                foreach (var item in _keys)
                                                                {
                                                                    item.KeyCombination = emptyCombination;
                                                                }

                                                                NotifySettingsViewModel();
                                                            });
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
                    _defaultsCommand = new RelayCommand(() =>
                                                            {
                                                                for (int i = 0; i < _keys.Count; i++)
                                                                {
                                                                    _keys[i].KeyCombination = _defaultKeys[i].KeyCombination;
                                                                }

                                                                NotifySettingsViewModel();
                                                            });
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

            _defaultKeys = new List<KeyCombinationItem>();
            foreach (var pair in DefaultSettings.KeyMap)
            {
                _defaultKeys.Add(new KeyCombinationItem
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

        private void NotifySettingsViewModel()
        {
            RaisePropertyChanged(string.Empty); // forces SettingsViewModel to check AnyChanges
        }

        public bool AnyChanges
        {
            get { return _originalKeys.Where((t, i) => t != _keys[i]).Any() || _mouseWheelAction != MouseWheelActionOriginal; }
        }
    }
}