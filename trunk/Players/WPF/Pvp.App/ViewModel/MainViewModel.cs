using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using Pvp.Core.MediaEngine;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Pvp.App.Messaging;

namespace Pvp.App.ViewModel
{
    internal class MainViewModel : ViewModelBase
    {
        private readonly IMediaEngineFacade _engine;
        private readonly ControlPanelViewModel _controlViewModel;

        private readonly IFileSelector _fileSelector;
        private readonly IDialogService _dialogService;

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
        
        public MainViewModel(IMediaEngineFacade engine, 
                             ControlPanelViewModel controlViewModel,
                             IFileSelector fileSelector,
                             IDialogService dialogService)
        {
            _engine = engine;
            _controlViewModel = controlViewModel;
            _fileSelector = fileSelector;
            _dialogService = dialogService;

            Messenger.Default.Register<PropertyChangedMessageBase>(this, true, OnPropertyChanged);
            Messenger.Default.Register<EventMessage>(this, MessageTokens.App, true, OnEventMessage);

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
                                if (!string.IsNullOrEmpty(filename))
                                {
                                    // TODO set video renderer somewhere else
                                    _engine.PreferredVideoRenderer = MediaEngineServiceProvider.RecommendedRenderer;

                                    _engine.BuildGraph(filename, MediaSourceType.File);
                                }
                            }
                        );
                }

                return _openCommand;
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
            get
            {
                return ControlViewModel.PlayCommand;
            }
        }

        public ICommand PauseCommand
        {
            get
            {
                return ControlViewModel.PauseCommand;
            }
        }

        public ICommand StopCommand
        {
            get
            {
                return ControlViewModel.StopCommand;
            }
        }

        public ICommand RepeatCommand
        {
            get
            {
                return ControlViewModel.RepeatCommand;
            }
        }

        public ICommand MuteCommand
        {
            get
            {
                return ControlViewModel.MuteCommand;
            }
        }

        private void FlipFullScreen(bool sendNotification)
        {
            IsFullScreen = !IsFullScreen;
            if (sendNotification)
            {
                Messenger.Default.Send(new PropertyChangedMessage<bool>(this, !IsFullScreen, IsFullScreen, "IsFullScreen"));
            }
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
        }

        private void OnUserDecisionNeeded(object sender, UserDecisionEventArgs e)
        {
            e.Accept = _dialogService.DisplayYesNoDialog(e.Message);
        }
    }
}
