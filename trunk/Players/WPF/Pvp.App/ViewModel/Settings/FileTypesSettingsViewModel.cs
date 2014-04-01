using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace Pvp.App.ViewModel.Settings
{
    internal class FileTypesSettingsViewModel : ViewModelBase, ISettingsViewModel
    {
        private readonly IFileAssociator _fileAssociator;
        private readonly IDialogService _dialogService;

        private ICommand _selectAllCommand;
        private ICommand _clearAllCommand;
        private ICommand _showExternalUICommand;

        private Dictionary<string, bool> _originalStatus;
        private IEnumerable<FileTypeItem> _items;
        
        public FileTypesSettingsViewModel(IFileAssociator fileAssociator, IDialogService dialogService)
        {
            _fileAssociator = fileAssociator;
            _dialogService = dialogService;
            Load();
        }

        public IEnumerable<FileTypeItem> Items
        {
            get { return _items; }
        }

        public ICommand SelectAllCommand
        {
            get
            {
                if (_selectAllCommand == null)
                {
                    _selectAllCommand = new RelayCommand(() =>
                                                             {
                                                                 SelectAll(true);
                                                             });
                }

                return _selectAllCommand;
            }
        }

        private void SelectAll(bool selected)
        {
            foreach (var item in _items)
            {
                item.Selected = selected;
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
                                                                SelectAll(false);
                                                            });
                }

                return _clearAllCommand;
            }
        }

        private void Load()
        {
            if (!_fileAssociator.CanAssociate)
            {
                return;
            }

            var items = FileTypes.All.Select(t => new FileTypeItem { Extension = t }).ToList();
            
            _fileAssociator.SetStatus(items);
            items.ForEach(i => i.PropertyChanged += OnItemPropertyChanged);

            _originalStatus = items.ToDictionary(i => i.Extension, i => i.Selected);
            _items = items;
        }

        private void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChanged("FileTypes"); // make SettingsViewModel check AnyChanges
        }

        public void Persist()
        {
            if (!_fileAssociator.CanAssociate)
            {
                return;
            }

            _fileAssociator.Associate(_items);
        }

        public bool AnyChanges
        {
            get
            {
                return _fileAssociator.CanAssociate && _items.Any(i => _originalStatus[i.Extension] != i.Selected);
            }
        }

        public bool ShowFileTypes
        {
            get { return _fileAssociator.CanAssociate; }
        }

        public bool CanShowExternalUI
        {
            get { return _fileAssociator.CanShowExternalUI; }
        }

        public ICommand ShowExternalUICommand
        {
            get
            {
                if (_showExternalUICommand == null)
                {
                    _showExternalUICommand = new RelayCommand(() =>
                                                              {
                                                                  try
                                                                  {
                                                                      _fileAssociator.ShowExternalUI();
                                                                  }
                                                                  catch
                                                                  {
                                                                      _dialogService.DisplayError(Resources.Resources.settings_launch_external_ui_error);
                                                                  }
                                                              });
                }

                return _showExternalUICommand;
            }
        }
    }
}