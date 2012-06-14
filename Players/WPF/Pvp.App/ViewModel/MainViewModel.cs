using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Pvp.App.Messaging;
using Pvp.Core.MediaEngine;

namespace Pvp.App.ViewModel
{
    internal class MainViewModel : ViewModelBase
    {
        private readonly IMediaEngineFacade _engine;
        private readonly ControlPanelViewModel _controlViewModel;

        private readonly IFileSelector _fileSelector;
        private readonly IDialogService _dialogService;
        private readonly IWindowHandleProvider _windowHandleProvider;

        private ICommand _openCommand;
        private ICommand _closeCommand;
        private ICommand _infoCommand;
        private ICommand _fullScreenCommand;
        private ICommand _exitCommand;
        private ICommand _controlPanelVisibilityToggleCommand;

        private bool _isFullScreen;
        private bool _isRepeat;
        private bool _isMute;
        private bool _isControlPanelVisible;
        
        private bool _isInPlayingMode;

        public MainViewModel(IMediaEngineFacade engine,
            ControlPanelViewModel controlViewModel,
            IFileSelector fileSelector,
            IDialogService dialogService,
            IWindowHandleProvider windowHandleProvider)
        {
            _engine = engine;
            _controlViewModel = controlViewModel;
            _fileSelector = fileSelector;
            _dialogService = dialogService;
            _windowHandleProvider = windowHandleProvider;

            Messenger.Default.Register<PropertyChangedMessageBase>(this, true, OnPropertyChanged);
            Messenger.Default.Register<EventMessage>(this, true, OnEventMessage);
            Messenger.Default.Register<DragDropMessage>(this, true, OnDragDrop);

            FlipControlPanelVisibility(); // TODO: read it from settings
        }

        public ControlPanelViewModel ControlViewModel
        {
            get { return _controlViewModel; }
        }

        public bool IsFullScreen
        {
            get { return _isFullScreen; }
            set
            {
                _isFullScreen = value;
                RaisePropertyChanged("IsFullScreen");
            }
        }

        public bool IsRepeat
        {
            get { return _isRepeat; }
            set
            {
                _isRepeat = value;
                RaisePropertyChanged("IsRepeat");
            }
        }

        public bool IsMute
        {
            get { return _isMute; }
            set
            {
                _isMute = value;
                RaisePropertyChanged("IsMute");
            }
        }

        public bool IsControlPanelVisible
        {
            get { return _isControlPanelVisible; }
            set
            {
                _isControlPanelVisible = value;
                RaisePropertyChanged("IsControlPanelVisible");
            }
        }

        private void FlipControlPanelVisibility()
        {
            IsControlPanelVisible = !IsControlPanelVisible;
            Messenger.Default.Send(new PropertyChangedMessage<bool>(this, !IsControlPanelVisible, IsControlPanelVisible, "IsControlPanelVisible"));
        }

        public ICommand OpenCommand
        {
            get
            {
                if (_openCommand == null)
                {
                    _openCommand = new RelayCommand
                        (
                        () =>
                        {
                            var filename = _fileSelector.SelectFile("Video Files (*.avi;*.divx;*.mpg;*.mpeg;*.asf;*.wmv;*.mov;*.qt;*.vob;*.dat;*.mkv;*.flv;*.mp4;*.3gp;*.3g2;*.m1v;*.m2v)|" +
                                                                        "*.avi;*.divx;*.mpg;*.mpeg;*.asf;*.wmv;*.mov;*.qt;*.vob;*.dat;*.mkv;*.flv;*.mp4;*.3gp;*.3g2;*.m1v;*.m2v|All Files (*.*)|*.*");
                            PlayFile(filename);
                        }
                        );
                }

                return _openCommand;
            }
        }
  
        private void PlayFile(string filename)
        {
            if (!string.IsNullOrEmpty(filename))
            {
                // TODO set video renderer somewhere else
                _engine.PreferredVideoRenderer = MediaEngineServiceProvider.RecommendedRenderer;

                _engine.BuildGraph(filename, MediaSourceType.File);
                UpdateState();
            }
        }

        public ICommand CloseCommand
        {
            get
            {
                if (_closeCommand == null)
                {
                    _closeCommand = new RelayCommand
                        (
                            () =>
                            {
                                _engine.ResetGraph();
                                UpdateState();
                            },
                            () =>
                            {
                                return _engine.GraphState != GraphState.Reset;
                            }
                        );
                }

                return _closeCommand;
            }
        }

        public ICommand InfoCommand
        {
            get
            {
                if (_infoCommand == null)
                {
                    _infoCommand = new RelayCommand
                        (
                            () =>
                            {

                            },
                            () =>
                            {
                                return _engine.GraphState != GraphState.Reset;
                            }
                        );
                }

                return _infoCommand;
            }
        }

        public ICommand FullScreenCommand
        {
            get
            {
                if (_fullScreenCommand == null)
                {
                    _fullScreenCommand = new RelayCommand
                        (
                            () =>
                            {
                                FlipFullScreen(true);
                            }
                        );
                }

                return _fullScreenCommand;
            }
        }
        
        public ICommand ExitCommand
        {
            get
            {
                if (_exitCommand == null)
                {
                    _exitCommand = new RelayCommand
                        (
                            () =>
                            {
                                Messenger.Default.Send(new CommandMessage(Command.ApplicationClose));
                            }
                        );
                }

                return _exitCommand;
            }
        }

        public ICommand ControlPanelVisibilityToggleCommand
        {
            get
            {
                if (_controlPanelVisibilityToggleCommand == null)
                {
                    _controlPanelVisibilityToggleCommand = new RelayCommand
                        (
                            () =>
                            {
                                FlipControlPanelVisibility();
                            }
                        );
                }

                return _controlPanelVisibilityToggleCommand;
            }
        }

        public ICommand PlayCommand
        {
            get { return ControlViewModel.PlayCommand; }
        }

        public ICommand PauseCommand
        {
            get { return ControlViewModel.PauseCommand; }
        }

        public ICommand StopCommand
        {
            get { return ControlViewModel.StopCommand; }
        }

        public ICommand RepeatCommand
        {
            get { return ControlViewModel.RepeatCommand; }
        }

        public ICommand MuteCommand
        {
            get { return ControlViewModel.MuteCommand; }
        }

        public ICommand VolumeUpCommand
        {
            get { return ControlViewModel.VolumeUpCommand; }
        }

        public ICommand VolumeDownCommand
        {
            get { return ControlViewModel.VolumeDownCommand; }
        }

        public VideoSize VideoSize
        {
            get { return _engine.VideoSize; }
            set 
            { 
                _engine.VideoSize = value;
                RaisePropertyChanged("VideoSize");
            }
        }

        public AspectRatio AspectRatio
        {
            get { return _engine.AspectRatio; }
            set
            {
                _engine.AspectRatio = value;
                RaisePropertyChanged("AspectRatio");
            }
        }

        public double PlayRate
        {
            get { return _engine.Rate; }
            set
            {
                _engine.Rate = value;
                RaisePropertyChanged("PlayRate");
            }
        }

        public bool PlayRateChangePossible
        {
            get { return _engine.IsGraphSeekable; }
        }

        public string MenuItemName
        {
            get { return null; }
        }

        private void FlipFullScreen(bool sendNotification)
        {
            IsFullScreen = !IsFullScreen;
            if (sendNotification)
            {
                Messenger.Default.Send(new PropertyChangedMessage<bool>(this, !IsFullScreen, IsFullScreen, "IsFullScreen"));
            }
        }

        private void NotifyOnPlayingModeChanged()
        {
            _isInPlayingMode = _engine.GraphState != GraphState.Reset;
            Messenger.Default.Send(new PropertyChangedMessage<bool>(this, !_isInPlayingMode, _isInPlayingMode, "IsInPlayingMode"));
        }

        private void OnDragDrop(DragDropMessage message)
        {
            PlayFile(message.Content);
        }

        private void OnPropertyChanged(PropertyChangedMessageBase message)
        {
            if (message.Sender != this)
            {
                if (message.PropertyName == "IsFullScreen")
                {
                    FlipFullScreen(false);
                }
                else if (message.PropertyName == "IsRepeat")
                {
                    IsRepeat = !IsRepeat;
                }
                else if (message.PropertyName == "IsMute")
                {
                    IsMute = !IsMute;
                }
            }
        }

        private void OnEventMessage(EventMessage message)
        {
            if (message.Content == Event.MediaControlCreated)
            {
                _engine.ErrorOccured += delegate(object sender, ErrorOccuredEventArgs args)
                {
                    _dialogService.DisplayError(args.Message);
                };

                _engine.DvdParentalChange += OnUserDecisionNeeded;
                _engine.PartialSuccess += OnUserDecisionNeeded;
            }
            else if (message.Content == Event.DispatcherTimerTick)
            {
                if (_isInPlayingMode)
                {
                    Messenger.Default.Send(new EventMessage(Event.StateRefreshSuggested));
                }
            }
        }

        private void OnUserDecisionNeeded(object sender, UserDecisionEventArgs e)
        {
            e.Accept = _dialogService.DisplayYesNoDialog(e.Message);
        }

        private void UpdateState()
        {
            UpdateMenu();
            RaisePropertyChanged("PlayRateChangePossible");
            NotifyOnPlayingModeChanged();

            Messenger.Default.Send(new EventMessage(Event.StateRefreshSuggested));
        }

        private void UpdateMenu()
        {
            UpdateFiltersMenu();
        }

        internal class NumberedCommand
        {
            public int Number { get; set; }
            public string Title { get; set; }
            public ICommand Command { get; set; }
        }

        private readonly ObservableCollection<NumberedCommand> _filters = new ObservableCollection<NumberedCommand>();
        private void UpdateFiltersMenu()
        {
            if (_engine.GraphState == GraphState.Reset)
            {
                _filters.Clear();
                ShowFiltersMenu = false;
            }
            else
            {
                var last = _engine.FilterCount;
                if (last > 15)
                    last = 15;

                for (int i = 0; i < last; i++)
                {
                    _filters.Add(new NumberedCommand
                        {
                            Number = i,
                            Title = _engine.GetFilterName(i),
                            Command = new GenericRelayCommand<NumberedCommand>(
                                nc =>
                                {
                                    if (nc != null)
                                        _engine.DisplayFilterPropPage(_windowHandleProvider.Handle, nc.Number, true);
                                },
                                nc =>
                                {
                                    return nc != null ? _engine.DisplayFilterPropPage(_windowHandleProvider.Handle, nc.Number, false) : false;
                                })
                        });
                }

                ShowFiltersMenu = true;
            }
        }

        private bool _showFiltersMenu;
        public bool ShowFiltersMenu
        {
            get { return _showFiltersMenu; }
            set
            {
                _showFiltersMenu = value;
                RaisePropertyChanged("ShowFiltersMenu");
            }
        }

        public ObservableCollection<NumberedCommand> Filters
        {
            get { return _filters; }
        }
    }
}
