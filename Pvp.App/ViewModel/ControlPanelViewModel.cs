using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using Pvp.Core.MediaEngine;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;

namespace Pvp.App.ViewModel
{
    internal class ControlPanelViewModel : ViewModelBase
    {
        private readonly IMediaEngineProvider _engineProvider;

        private ICommand _playCommand;
        private ICommand _pauseCommand;
        private ICommand _stopCommand;
        private ICommand _forwardCommand;
        private ICommand _backwardCommand;
        private ICommand _toEndCommand;
        private ICommand _toBeginingCommand;
        private ICommand _repeatCommand;
        private ICommand _muteCommand;
        private ICommand _volumeCommand;
        private ICommand _seekCommand;

        private bool _isFullScreen;
        private bool _isRepeat;
        private bool _isMute;
        private TimeSpan _duration;
        private TimeSpan _currentPosition;
        private bool _isControlPanelVisible;

        public ControlPanelViewModel(IMediaEngineProvider engineProvider)
        {
            _engineProvider = engineProvider;

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

        private void OnPropertyChanged(PropertyChangedMessageBase message)
        {
            if (message.Sender != this)
            {
                if (message.PropertyName == "IsFullScreen")
                {
                    IsFullScreen = !IsFullScreen;
                }
                else if (message.PropertyName == "IsControlPanelVisible")
                {
                    IsControlPanelVisible = !IsControlPanelVisible;
                }
            }
        }

        public TimeSpan Duration
        {
            get { return _duration; }
            set
            {
                _duration = value;
                RaisePropertyChanged("Duration");
            }
        }

        public TimeSpan CurrentPosition
        {
            get { return _currentPosition; }
            set
            {
                _currentPosition = value;
                RaisePropertyChanged("CurrentPosition");
            }
        }

        public ICommand PlayCommand
        {
            get
            {
                if (_playCommand == null)
                {
                    _playCommand = new RelayCommand
                        (
                            () =>
                            {
                                _engineProvider.MediaEngine.ResumeGraph();
                            },
                            () =>
                            {
                                GraphState state = _engineProvider.MediaEngine.GraphState;
                                return state != GraphState.Running && state != GraphState.Reset;
                            }
                        );
                }

                return _playCommand;
            }
        }

        public ICommand PauseCommand
        {
            get
            {
                if (_pauseCommand == null)
                {
                    _pauseCommand = new RelayCommand
                        (
                            () =>
                            {
                                _engineProvider.MediaEngine.PauseGraph();
                            },
                            () =>
                            {
                                GraphState state = _engineProvider.MediaEngine.GraphState;
                                return state == GraphState.Running;
                            }
                        );
                }

                return _pauseCommand;
            }
        }

        public ICommand StopCommand
        {
            get
            {
                if (_stopCommand == null)
                {
                    _stopCommand = new RelayCommand
                        (
                            () =>
                            {
                                _engineProvider.MediaEngine.StopGraph();
                            },
                            () =>
                            {
                                GraphState state = _engineProvider.MediaEngine.GraphState;
                                return state == GraphState.Running || state == GraphState.Paused;
                            }
                        );
                }

                return _stopCommand;
            }
        }

        public ICommand ForwardCommand
        {
            get
            {
                if (_forwardCommand == null)
                {
                    _forwardCommand = new RelayCommand
                        (
                            () =>
                            {

                            },
                            () =>
                            {
                                return true;
                            }
                        );
                }

                return _forwardCommand;
            }
        }

        public ICommand BackwardCommand
        {
            get
            {
                if (_backwardCommand == null)
                {
                    _backwardCommand = new RelayCommand
                        (
                            () =>
                            {

                            },
                            () =>
                            {
                                return true;
                            }
                        );
                }

                return _backwardCommand;
            }
        }

        public ICommand ToEndCommand
        {
            get
            {
                if (_toEndCommand == null)
                {
                    _toEndCommand = new RelayCommand
                        (
                            () =>
                            {

                            },
                            () =>
                            {
                                return true;
                            }
                        );
                }

                return _toEndCommand;
            }
        }

        public ICommand ToBeginingCommand
        {
            get
            {
                if (_toBeginingCommand == null)
                {
                    _toBeginingCommand = new RelayCommand
                        (
                            () =>
                            {

                            },
                            () =>
                            {
                                return true;
                            }
                        );
                }

                return _toBeginingCommand;
            }
        }

        public ICommand RepeatCommand
        {
            get
            {
                if (_repeatCommand == null)
                {
                    _repeatCommand = new RelayCommand
                        (
                            () =>
                            {
                                var repeat = _engineProvider.MediaEngine.Repeat;
                                _engineProvider.MediaEngine.Repeat = !repeat;
                                IsRepeat = !repeat;
                                Messenger.Default.Send(new PropertyChangedMessage<bool>(this, !IsRepeat, IsRepeat, "IsRepeat"));
                            },
                            () =>
                            {
                                GraphState state = _engineProvider.MediaEngine.GraphState;
                                return true;
                            }
                        );
                }

                return _repeatCommand;
            }
        }

        public ICommand MuteCommand
        {
            get
            {
                if (_muteCommand == null)
                {
                    _muteCommand = new RelayCommand
                        (
                            () =>
                            {
                                IsMute = !IsMute;
                                Messenger.Default.Send(new PropertyChangedMessage<bool>(this, !IsMute, IsMute, "IsMute"));
                            },
                            () =>
                            {
                                return true;
                            }
                        );
                }

                return _muteCommand;
            }
        }

        public ICommand VolumeCommand
        {
            get
            {
                if (_volumeCommand == null)
                {
                    _volumeCommand = new RelayCommand
                        (
                            () =>
                            {

                            },
                            () =>
                            {
                                return true;
                            }
                        );
                }

                return _volumeCommand;
            }
        }

        public ICommand SeekCommand
        {
            get
            {
                if (_seekCommand == null)
                {
                    _seekCommand = new RelayCommand
                        (
                            () =>
                            {

                            },
                            () =>
                            {
                                return true;
                            }
                        );
                }

                return _seekCommand;
            }
        }
    }
}
