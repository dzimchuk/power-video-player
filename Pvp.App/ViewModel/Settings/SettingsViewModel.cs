using System;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Pvp.App.Messaging;

namespace Pvp.App.ViewModel.Settings
{
    internal class SettingsViewModel : ViewModelBase, ISettingsViewModel
    {
        private readonly FileTypesSettingsViewModel _fileTypesSettingsViewModel;
        private readonly KeyboardMouseSettingsViewModel _keyboardMouseSettingsViewModel;
        private readonly FiltersSettingsViewModel _filtersSettingsViewModel;
        private readonly VideoSettingsViewModel _videoSettingsViewModel;
        private readonly GeneralSettingsViewModel _generalSettingsViewModel;

        private ICommand _okCommand;
        private ICommand _cancelCommand;
        private ICommand _applyCommand;

        private readonly ISettingsProvider _settingsProvider;

        private bool _changesAvailable;

        public SettingsViewModel(GeneralSettingsViewModel generalSettingsViewModel,
            VideoSettingsViewModel videoSettingsViewModel,
            FiltersSettingsViewModel filtersSettingsViewModel,
            KeyboardMouseSettingsViewModel keyboardMouseSettingsViewModel,
            FileTypesSettingsViewModel fileTypesSettingsViewModel,
            ISettingsProvider settingsProvider)
        {
            _settingsProvider = settingsProvider;
            _generalSettingsViewModel = generalSettingsViewModel;
            _videoSettingsViewModel = videoSettingsViewModel;
            _filtersSettingsViewModel = filtersSettingsViewModel;
            _keyboardMouseSettingsViewModel = keyboardMouseSettingsViewModel;
            _fileTypesSettingsViewModel = fileTypesSettingsViewModel;

            _generalSettingsViewModel.PropertyChanged += OnAnyPropertyChanged;
            _videoSettingsViewModel.PropertyChanged += OnAnyPropertyChanged;
            _filtersSettingsViewModel.PropertyChanged += OnAnyPropertyChanged;
            _keyboardMouseSettingsViewModel.PropertyChanged += OnAnyPropertyChanged;
            _fileTypesSettingsViewModel.PropertyChanged += OnAnyPropertyChanged;
        }

        private void OnAnyPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            _changesAvailable = AnyChanges;
        }

        public GeneralSettingsViewModel GeneralSettingsViewModel
        {
            get { return _generalSettingsViewModel; }
        }

        public VideoSettingsViewModel VideoSettingsViewModel
        {
            get { return _videoSettingsViewModel; }
        }

        public FiltersSettingsViewModel FiltersSettingsViewModel
        {
            get { return _filtersSettingsViewModel; }
        }

        public KeyboardMouseSettingsViewModel KeyboardMouseSettingsViewModel
        {
            get { return _keyboardMouseSettingsViewModel; }
        }

        public FileTypesSettingsViewModel FileTypesSettingsViewModel
        {
            get { return _fileTypesSettingsViewModel; }
        }

        public ICommand OkCommand
        {
            get
            {
                if (_okCommand == null)
                {
                    _okCommand = new RelayCommand(
                        () =>
                        {
                            Persist();
                            Messenger.Default.Send<CommandMessage>(new CommandMessage(Command.SettingsWindowClose));
                        });
                }

                return _okCommand;
            }
        }

        public ICommand CancelCommand
        {
            get
            {
                if (_cancelCommand == null)
                {
                    _cancelCommand = new RelayCommand(
                        () =>
                        {
                            Messenger.Default.Send<CommandMessage>(new CommandMessage(Command.SettingsWindowClose));
                        });
                }

                return _cancelCommand;
            }
        }

        public ICommand ApplyCommand
        {
            get
            {
                if (_applyCommand == null)
                {
                    _applyCommand = new RelayCommand(
                        () =>
                        {
                            Persist();
                        },
                        () =>
                        {
                            return _changesAvailable;
                        });
                }

                return _applyCommand;
            }
        }

        private void Persist()
        {
            if (_changesAvailable)
            {
                _generalSettingsViewModel.Persist();
                _videoSettingsViewModel.Persist();
                _filtersSettingsViewModel.Persist();
                _keyboardMouseSettingsViewModel.Persist();
                _fileTypesSettingsViewModel.Persist();

            	_settingsProvider.Save();
                _changesAvailable = false;
            }
        }

        public bool AnyChanges
        {
            get
            {
                return _generalSettingsViewModel.AnyChanges || _videoSettingsViewModel.AnyChanges ||
                       _filtersSettingsViewModel.AnyChanges || _keyboardMouseSettingsViewModel.AnyChanges || 
                       _fileTypesSettingsViewModel.AnyChanges;
            }
        }
    }
}
