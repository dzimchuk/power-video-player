using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using Dzimchuk.MediaEngine.Core;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Dzimchuk.Pvp.App.Messaging;

namespace Dzimchuk.Pvp.App.ViewModel
{
    internal class MainViewModel : ViewModelBase
    {
        private readonly IMediaEngine _engine;
        private readonly ControlPanelViewModel _controlViewModel;

        private ICommand _fullScreenCommand;
        private ICommand _exitCommand;
        private ICommand _controlPanelVisibilityToggleCommand;

        private bool _isFullScreen;
        private bool _isRepeat;
        private bool _isMute;
        private bool _isControlPanelVisible;
        
        public MainViewModel(IMediaEngine engine, ControlPanelViewModel controlViewModel)
        {
            _engine = engine;
            _controlViewModel = controlViewModel;

            Messenger.Default.Register<PropertyChangedMessageBase>(this, true, OnPropertyChanged);

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
    }
}
