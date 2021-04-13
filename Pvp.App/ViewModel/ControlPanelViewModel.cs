using System;
using System.Linq;
using GalaSoft.MvvmLight;
using Pvp.App.Messaging;
using Pvp.App.ViewModel.Settings;
using Pvp.Core.MediaEngine;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;

namespace Pvp.App.ViewModel
{
    internal class ControlPanelViewModel : ViewModelBase, IDurationProvider
    {
        private readonly IMediaEngineFacade _engine;
        private readonly ISettingsProvider _settingsProvider;

        private ICommand _playCommand;
        private ICommand _pauseCommand;
        private ICommand _stopCommand;
        private ICommand _forwardCommand;
        private ICommand _backwardCommand;
        private ICommand _toEndCommand;
        private ICommand _toBeginingCommand;
        private ICommand _repeatCommand;
        private ICommand _muteCommand;
        private ICommand _volumeUpCommand;
        private ICommand _volumeDownCommand;

        private bool _isFullScreen;
        private bool _isRepeat;
        private bool _isMute;

        private TimeSpan _duration;
        private TimeSpan _currentPosition;
        private readonly TimeSpan _seekStep = TimeSpan.FromSeconds(5);

        private bool _isControlPanelVisible;

        private double _volume = 0.8;
        private const double VolumeStep = 0.04;

        private bool _isInPlayingMode;
        private bool _engineReady;

        public ControlPanelViewModel(IMediaEngineFacade engine, ISettingsProvider settingsProvider)
        {
            _engine = engine;
            _settingsProvider = settingsProvider;

            Messenger.Default.Register<PropertyChangedMessageBase>(this, true, OnPropertyChanged);
            Messenger.Default.Register<EventMessage>(this, true, OnEventMessage);
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
                var booleanNotificationMessage = message as PropertyChangedMessage<bool>;

                if (booleanNotificationMessage != null)
                {
                    if (booleanNotificationMessage.PropertyName == "IsFullScreen")
                    {
                        IsFullScreen = booleanNotificationMessage.NewValue;
                    }
                    else if (booleanNotificationMessage.PropertyName == "IsControlPanelVisible")
                    {
                        IsControlPanelVisible = booleanNotificationMessage.NewValue;
                    }
                    else if (booleanNotificationMessage.PropertyName == "IsInPlayingMode")
                    {
                        IsInPlayingMode = booleanNotificationMessage.NewValue;
                    }
                }
            }
        }

        private void OnEventMessage(EventMessage message)
        {
            if (message.Content == Event.StateRefreshSuggested)
            {
                Duration = _engine.Duration;

                _updatingCurrentPosition = true;
                CurrentPosition = _engine.CurrentPosition;
                _updatingCurrentPosition = false;
            }
            else if (message.Content == Event.MediaControlCreated)
            {
                _engineReady = true;

                if (_settingsProvider.Get("RememberVolume", true))
                {
                    var volume = _settingsProvider.Get("Volume", _volume);
                    ChangeVolume(volume - _volume);

                    if (_settingsProvider.Get("IsMute", false))
                    {
                        ToggleMute();
                    }
                }
                else
                {
                    ChangeVolume(0);
                }
            }
            else if (message.Content == Event.MainWindowClosing)
            {
                if (_settingsProvider.Get("RememberVolume", true))
                {
                    _settingsProvider.Set("Volume", _volume);
                    _settingsProvider.Set("IsMute", _isMute);
                }
            }
        }

        public TimeSpan Duration
        {
            get { return _duration; }
            set
            {
                if (_duration != value)
                {
                    _duration = value;
                    RaisePropertyChanged("Duration");
                }
            }
        }

        private bool _updatingCurrentPosition;
        public TimeSpan CurrentPosition
        {
            get { return _currentPosition; }
            set
            {
                if (_currentPosition != value)
                {
                    _currentPosition = value;
                    if (_updatingCurrentPosition)
                    {
                        RaisePropertyChanged("CurrentPosition");
                    }
                    else
                    {
                        if (_engine.IsGraphSeekable)
                        {
                            _engine.CurrentPosition = value;
                        }
                    }
                }
            }
        }

        private bool _updatingVolume;
        public double Volume
        {
            get { return _volume; }
            set 
            { 
                _volume = value;
                _engine.Volume = value;

                if (_updatingVolume)
                {
                	RaisePropertyChanged("Volume");
                }
            }
        }

        public bool IsInPlayingMode
        {
            get { return _isInPlayingMode; }
            set 
            { 
                _isInPlayingMode = value;
                RaisePropertyChanged("IsInPlayingMode");
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
                                _engine.ResumeGraph();
                            },
                            () =>
                            {
                                GraphState state = _engine.GraphState;
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
                                _engine.PauseGraph();
                            },
                            () =>
                            {
                                return _engine.GraphState == GraphState.Running;
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
                                _engine.StopGraph();
                            },
                            () =>
                            {
                                GraphState state = _engine.GraphState;
                                return state == GraphState.Running || state == GraphState.Paused;
                            }
                        );
                }

                return _stopCommand;
            }
        }

        private void Seek(TimeSpan position)
        {
            if (_engine.IsGraphSeekable)
            {
                _engine.CurrentPosition = position;

                _updatingCurrentPosition = true;
                CurrentPosition = position;
                _updatingCurrentPosition = false;
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
                                var position = CurrentPosition.Add(_seekStep);
                                var duration = Duration;
                                if (position > duration)
                                    position = duration;

                                Seek(position);
                            },
                            () =>
                            {
                                return _engineReady && _engine.IsGraphSeekable;
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
                                var position = CurrentPosition.Subtract(_seekStep);
                                if (position < TimeSpan.Zero)
                                    position = TimeSpan.Zero;

                                Seek(position);
                            },
                            () =>
                            {
                                return _engineReady && _engine.IsGraphSeekable;
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
                                Seek(Duration.Subtract(TimeSpan.FromMilliseconds(200)));
                                _engine.PauseGraph();
                            },
                            () =>
                            {
                                return _engineReady && _engine.IsGraphSeekable;
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
                                Seek(TimeSpan.Zero);
                            },
                            () =>
                            {
                                return _engineReady && _engine.IsGraphSeekable;
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
                                var repeat = _engine.Repeat;
                                _engine.Repeat = !repeat;
                                IsRepeat = !repeat;
                                Messenger.Default.Send(new PropertyChangedMessage<bool>(this, !IsRepeat, IsRepeat, "IsRepeat"));
                            },
                            () =>
                            {
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
                                ToggleMute();
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

        private void ToggleMute()
        {
            _engine.IsMuted = !IsMute;

            IsMute = !IsMute;
            Messenger.Default.Send(new PropertyChangedMessage<bool>(this, !IsMute, IsMute, "IsMute"));
        }

        private void ChangeVolume(double delta)
        {
            var volume = Volume + delta;
            if (volume > 1.0)
                volume = 1.0;
            else if (volume < 0.0)
                volume = 0.0;

            _updatingVolume = true;
            Volume = volume;
            _updatingVolume = false;
        }

        public ICommand VolumeUpCommand
        {
            get
            {
                if (_volumeUpCommand == null)
                {
                    _volumeUpCommand = new RelayCommand
                        (
                            () =>
                            {
                                ChangeVolume(VolumeStep);
                            },
                            () =>
                            {
                                return _volume < 1.0;
                            }
                        );
                }

                return _volumeUpCommand;
            }
        }

        public ICommand VolumeDownCommand
        {
            get
            {
                if (_volumeDownCommand == null)
                {
                    _volumeDownCommand = new RelayCommand
                        (
                            () =>
                            {
                                ChangeVolume(-VolumeStep);
                            },
                            () =>
                            {
                                return _volume > 0.0;
                            }
                        );
                }

                return _volumeDownCommand;
            }
        }
    }
}
