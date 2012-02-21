using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using Dzimchuk.Pvp.App.Messaging;
using GalaSoft.MvvmLight.Messaging;

namespace Dzimchuk.Pvp.App.ViewModel
{
    internal class MainWindowViewModel : ViewModelBase
    {
        private bool _isFullScreen;
        private bool _isMaximized;
        private bool _isMinimized;

        private ICommand _minimizeCommand;
        private ICommand _maximizeCommand;
        private ICommand _closeCommand;

        public MainWindowViewModel()
        {
            Messenger.Default.Register<EventMessage>(this, MessageTokens.UI, OnUIEvent);
            Messenger.Default.Register<PropertyChangedMessageBase>(this, true, OnPropertyChanged);
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

        public bool IsMaximized
        {
            get { return _isMaximized; }
            set
            {
                if (value && IsFullScreen)
                    throw new InvalidOperationException("Window cannot be maximized while in full screen mode.");
                
                _isMaximized = value;
                RaisePropertyChanged("IsMaximized");
            }
        }

        public bool IsMinimized
        {
            get { return _isMinimized; }
            set
            {
                if (value && IsFullScreen)
                    throw new InvalidOperationException("Window cannot be minimized while in full screen mode.");

                _isMinimized = value;
                RaisePropertyChanged("IsMinimized");
            }
        }

        public ICommand MinimizeCommand
        {
            get
            {
                if (_minimizeCommand == null)
                {
                    _minimizeCommand = new RelayCommand
                        (
                            () =>
                            {
                                IsMinimized = !IsMinimized;
                            },
                            () =>
                            {
                                return true;
                            }
                        );
                }

                return _minimizeCommand;
            }
        }

        public ICommand MaximizeCommand
        {
            get
            {
                if (_maximizeCommand == null)
                {
                    _maximizeCommand = new RelayCommand
                        (
                            () =>
                            {
                                FlipMaximized();
                            },
                            () =>
                            {
                                return true;
                            }
                        );
                }

                return _maximizeCommand;
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
                                Messenger.Default.Send(new CommandMessage(Command.ApplicationClose));
                            },
                            () =>
                            {
                                return true;
                            }
                        );
                }

                return _closeCommand;
            }
        }

        private void OnUIEvent(EventMessage message)
        {
            if (message.Content == Event.TitleBarDoubleClick)
            {
                FlipMaximized();
            }
            else if (message.Content == Event.VideoAreaDoubleClick)
            {
                FlipFullScreen(true);
            }
        }

        private void FlipMaximized()
        {
            IsMaximized = !IsMaximized;
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
            }
        }
    }
}
