using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using Dzimchuk.MediaEngine.Core;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;

namespace Dzimchuk.Pvp.App.ViewModel
{
    internal class ControlPanelViewModel : ViewModelBase
    {
        private readonly IMediaEngine _engine;

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

        public ControlPanelViewModel(IMediaEngine engine)
        {
            _engine = engine;
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
                                
                            },
                            () =>
                            {
                                return true;
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

                            },
                            () =>
                            {
                                return true;
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

                            },
                            () =>
                            {
                                return true;
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
