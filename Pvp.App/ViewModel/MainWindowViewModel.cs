using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Pvp.App.Messaging;
using Pvp.App.ViewModel.Settings;
using Pvp.Core.MediaEngine;
using Pvp.Core.Wpf;

namespace Pvp.App.ViewModel
{
    internal class MainWindowViewModel : ViewModelBase
    {
        private bool _isFullScreen;
        private bool _isMaximized;
        private bool _isMinimized;

        private bool _topMost;
        private bool _centerWindow;
        private readonly Dictionary<KeyCombination, string> _keys = new Dictionary<KeyCombination, string>();
        private MouseWheelAction _mouseWheelAction;

        private bool _settingsDialogActivated;

        private ICommand _minimizeCommand;
        private ICommand _maximizeCommand;
        private ICommand _closeCommand;
        private ICommand _keyDownCommand;
        private ICommand _mouseWheelCommand;

        private readonly ISettingsProvider _settingsProvider;
        private readonly IKeyInterpreter _keyInterpreter;
        private readonly IMouseWheelInterpreter _mouseWheelInterpreter;
        private readonly IMediaEngineFacade _engine;

        public MainWindowViewModel(ISettingsProvider settingsProvider, IMediaEngineFacade engine, IKeyInterpreter keyInterpreter, IMouseWheelInterpreter mouseWheelInterpreter)
        {
            _settingsProvider = settingsProvider;
            _engine = engine;
            _keyInterpreter = keyInterpreter;
            _mouseWheelInterpreter = mouseWheelInterpreter;
            _settingsProvider.SettingChanged += _settingsProvider_SettingChanged;

            ReadSettings();

            Messenger.Default.Register<EventMessage>(this, OnEvent);
            Messenger.Default.Register<PropertyChangedMessageBase>(this, true, OnPropertyChanged);
            Messenger.Default.Register<CommandMessage>(this, OnCommand);
        }

        private void OnCommand(CommandMessage command)
        {
            if (command.Content == Command.SaveSettings)
            {
                _settingsProvider.Save();
            }
        }

        private void _settingsProvider_SettingChanged(object sender, SettingChangeEventArgs e)
        {
            if (e.SettingName.Equals(SettingsConstants.TopMost, StringComparison.InvariantCultureIgnoreCase))
            {
                TopMost = _settingsProvider.Get(SettingsConstants.TopMost, DefaultSettings.TopMost);
            }
            else if (e.SettingName.Equals(SettingsConstants.CenterWindow, StringComparison.InvariantCultureIgnoreCase))
            {
                CenterWindow = _settingsProvider.Get(SettingsConstants.CenterWindow, DefaultSettings.CenterWindow);
            }
            else if (e.SettingName.Equals(SettingsConstants.KeyMap, StringComparison.InvariantCultureIgnoreCase))
            {
                ReadKeyCombinations();
            }
            else if (e.SettingName.Equals(SettingsConstants.MouseWheelAction, StringComparison.InvariantCultureIgnoreCase))
            {
                _mouseWheelAction = _settingsProvider.Get(SettingsConstants.MouseWheelAction, DefaultSettings.MouseWheekAction);
            }
        }

        private void ReadSettings()
        {
            _topMost = _settingsProvider.Get(SettingsConstants.TopMost, DefaultSettings.TopMost);
            _centerWindow = _settingsProvider.Get(SettingsConstants.CenterWindow, DefaultSettings.CenterWindow);
            ReadKeyCombinations();
            _mouseWheelAction = _settingsProvider.Get(SettingsConstants.MouseWheelAction, DefaultSettings.MouseWheekAction);
        }

        private void ReadKeyCombinations()
        {
            _keys.Clear();
            var keys = _settingsProvider.Get(SettingsConstants.KeyMap, DefaultSettings.KeyMap);
            foreach (var pair in keys)
            {
                var keyCombination = pair.Value;
                if (keyCombination != null && !keyCombination.IsEmpty && !_keys.ContainsKey(keyCombination))
                {
                    _keys.Add(keyCombination, pair.Key);
                }
            }
        }

        public bool TopMost
        {
            get { return _topMost; }
            set
            {
                _topMost = value;
                RaisePropertyChanged("TopMost");
            }
        }

        public bool CenterWindow
        {
            get { return _centerWindow; }
            set
            {
                if (value.Equals(_centerWindow)) return;
                _centerWindow = value;
                RaisePropertyChanged("CenterWindow");
            }
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

        private void OnEvent(EventMessage message)
        {
            if (message.Content == Event.TitleBarDoubleClick)
            {
                FlipMaximized();
            }
            else if (message.Content == Event.VideoAreaDoubleClick)
            {
                FlipFullScreen(true);
            }
            else if (message.Content == Event.InitSize)
            {
                var args = (InitSizeEventArgs)message.EventArgs;
                if (args.NewVideoSize.Width > 0.0 && args.NewVideoSize.Height > 0.0)
                {
                    RaiseResizeMainWindowEvent(_engine.VideoSize, args.NewVideoSize.Width, args.NewVideoSize.Height);
                }
            }
            else if (message.Content == Event.MainWindowResizeSuggested)
            {
                var args = (MainWindowResizeSuggestedEventArgs)message.EventArgs;
                RaiseResizeMainWindowEvent(args.VideoSize, args.Width, args.Height);
            }
            else if (message.Content == Event.SettingsDialogActivated)
            {
                _settingsDialogActivated = true;
            }
            else if (message.Content == Event.SettingsDialogDeactivated)
            {
                _settingsDialogActivated = false;
            }
        }

        private void RaiseResizeMainWindowEvent(VideoSize videoSize, double width, double height)
        {
            videoSize.RaiseResizeMainWindowEvent(new Tuple<double, double>(width, height), _centerWindow);
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

        //    TopMost = IsFullScreen || _settingsProvider.Get("TopMost", false);
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

        public ICommand KeyDownCommand
        {
            get
            {
                if (_keyDownCommand == null)
                {
                    _keyDownCommand = new RelayCommand<EventArgs>(args =>
                    {
                        if (!_settingsDialogActivated)
                        {
                            var keyCombination = _keyInterpreter.Interpret(args);
                            if (keyCombination != null)
                            {
                                string command;
                                if (_keys.TryGetValue(keyCombination, out command))
                                {
                                    Messenger.Default.Send(new EventMessage(Event.KeyboardMouseAction, new KeyboardMouseActionEventArgs(command)));
                                }
                            }
                        }
                    });
                }

                return _keyDownCommand;
            }
        }

        public ICommand MouseWheelCommand
        {
            get
            {
                if (_mouseWheelCommand == null)
                {
                    _mouseWheelCommand = new RelayCommand<EventArgs>(args =>
                                                                         {
                                                                             var delta = _mouseWheelInterpreter.Interpret(args);
                                                                             if (delta != 0)
                                                                             {
                                                                                 string command;
                                                                                 switch(_mouseWheelAction)
                                                                                 {
                                                                                     case MouseWheelAction.Volume:
                                                                                         command = delta > 0
                                                                                                        ? CommandConstants.VolumeUp
                                                                                                        : CommandConstants.VolumeDown;
                                                                                         break;
                                                                                     case MouseWheelAction.Seek:
                                                                                         command = delta > 0 ? CommandConstants.Forth : CommandConstants.Back;
                                                                                         break;
                                                                                     default:
                                                                                         throw new ArgumentOutOfRangeException();
                                                                                 }

                                                                                 Messenger.Default.Send(new EventMessage(Event.KeyboardMouseAction, new KeyboardMouseActionEventArgs(command)));
                                                                             }
                                                                         });
                }

                return _mouseWheelCommand;
            }
        }
    }
}
