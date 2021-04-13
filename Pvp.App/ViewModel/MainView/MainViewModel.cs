using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Pvp.App.Messaging;
using Pvp.App.ViewModel.Settings;
using Pvp.Core.MediaEngine;
using Pvp.Core.Wpf;

namespace Pvp.App.ViewModel.MainView
{
    internal class MainViewModel : ViewModelBase
    {
        private readonly IMediaEngineFacade _engine;
        private readonly ControlPanelViewModel _controlViewModel;

        private readonly IFileSelector _fileSelector;
        private readonly IDialogService _dialogService;
        private readonly ISettingsProvider _settingsProvider;
        private readonly IImageCreaterFactory _imageCreaterFactory;
        private readonly IDisplayService _displayService;
        private readonly IFailedStreamsContainer _failedStreamsContainer;
        private readonly ICursorManager _cursorManager;

        private ICommand _openCommand;
        private ICommand _closeCommand;
        private ICommand _infoCommand;
        private ICommand _fullScreenCommand;
        private ICommand _exitCommand;
        private ICommand _controlPanelVisibilityToggleCommand;

        private ICommand _settingsCommand;
        private ICommand _videoSizeCommand;
        private ICommand _aspectRatioCommand;
        private ICommand _aboutCommand;
        private ICommand _screenshotsCommand;
        private ICommand _playPauseCommand;

        private bool _isFullScreen;
        private bool _isRepeat;
        private bool _isMute;
        private bool _isControlPanelVisible;
        
        private bool _isInPlayingMode;

        private bool _showLogo;
        private bool _autoPlay;
        private bool _startFullScreen;

        private Tuple<double, double> _videoSize;
        private Dictionary<string, ICommand> _commandBag;

        private bool _isContextMenuOpen;
        private bool _fullScreenControlPanelOpened;

        private SupportedLanguage _language;

        public MainViewModel(IMediaEngineFacade engine,
            ControlPanelViewModel controlViewModel,
            IFileSelector fileSelector,
            IDialogService dialogService,
            ISettingsProvider settingsProvider, 
            IImageCreaterFactory imageCreaterFactory, 
            IDisplayService displayService, 
            IFailedStreamsContainer failedStreamsContainer, 
            ICursorManager cursorManager)
        {
            _engine = engine;
            _controlViewModel = controlViewModel;
            _fileSelector = fileSelector;
            _dialogService = dialogService;
            _settingsProvider = settingsProvider;
            _imageCreaterFactory = imageCreaterFactory;
            _displayService = displayService;
            _failedStreamsContainer = failedStreamsContainer;
            _cursorManager = cursorManager;

            _settingsProvider.SettingChanged += _settingsProvider_SettingChanged;

            ReadSettings();

            Messenger.Default.Register<PropertyChangedMessageBase>(this, true, OnPropertyChanged);
            Messenger.Default.Register<EventMessage>(this, true, OnEventMessage);
            Messenger.Default.Register<PlayNewFileMessage>(this, true, OnPlayNewFile);
            Messenger.Default.Register<PlayDiscMessage>(this, true, OnPlayDisc);

            PackUpCommandBag();
        }

        private void _settingsProvider_SettingChanged(object sender, SettingChangeEventArgs e)
        {
            if (e.SettingName.Equals("ShowLogo", StringComparison.InvariantCultureIgnoreCase))
            {
                ShowLogo = _settingsProvider.Get("ShowLogo", true);
            }
            else if (e.SettingName.Equals("AutoPlay", StringComparison.InvariantCultureIgnoreCase))
            {
                AutoPlay = _settingsProvider.Get("AutoPlay", true);
            }
        }

        private void ReadSettings()
        {
            _showLogo = _settingsProvider.Get("ShowLogo", true);
            _autoPlay = _settingsProvider.Get("AutoPlay", true);

            _isControlPanelVisible = !_settingsProvider.Get("ControlPanelVisible", true);
            FlipControlPanelVisibility();

            _startFullScreen = _settingsProvider.Get("StartFullScreen", false);

            Language = _settingsProvider.Get(SettingsConstants.Language, SupportedLanguage.English);
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
                            var filename = _fileSelector.SelectFile("Video Files (*.avi;*.divx;*.mpg;*.mpeg;*.asf;*.wmv;*.mov;*.qt;*.vob;*.dat;*.mkv;*.flv;*.mp4;*.3gp;*.3g2;*.m1v;*.m2v;*.m2ts;*.ifo)|" +
                                                                        "*.avi;*.divx;*.mpg;*.mpeg;*.asf;*.wmv;*.mov;*.qt;*.vob;*.dat;*.mkv;*.flv;*.mp4;*.3gp;*.3g2;*.m1v;*.m2v;*.m2ts;*.ifo|All Files (*.*)|*.*");
                            PlayFile(filename);
                        }
                        );
                }

                return _openCommand;
            }
        }

        private Renderer PreferredVideoRenderer
        {
            get { return _settingsProvider.Get("Renderer", MediaEngineServiceProvider.RecommendedRenderer); }
        }
  
        private void PlayFile(string filename)
        {
            if (!string.IsNullOrEmpty(filename))
            {
                _engine.PreferredVideoRenderer = PreferredVideoRenderer;

                _engine.BuildGraph(filename, MediaSourceType.File);
                UpdateState();
            }
        }

        private void PlayDvd(string driveName)
        {
            if (!string.IsNullOrEmpty(driveName))
            {
                _engine.PreferredVideoRenderer = PreferredVideoRenderer;

                string source = driveName + "Video_ts";
                _engine.BuildGraph(source, MediaSourceType.Dvd);
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
                                    _dialogService.DisplayMediaInformationWindow();
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
                if (_engine.VideoSize == value)
                    return;

                if (_videoSize != null)
                {
                    Messenger.Default.Send(new EventMessage(Event.MainWindowResizeSuggested,
                                                            new MainWindowResizeSuggestedEventArgs(value, _videoSize.Item1, _videoSize.Item2)));
                }

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

        private VideoSize _previousVideoSize;
        private void FlipFullScreen(bool sendNotification)
        {
            IsFullScreen = !IsFullScreen;
            if (sendNotification)
            {
                Messenger.Default.Send(new PropertyChangedMessage<bool>(this, !IsFullScreen, IsFullScreen, "IsFullScreen"));
            }

            if (IsFullScreen)
            {
                _previousVideoSize = _engine.VideoSize;
                _engine.VideoSize = VideoSize.SIZE_FREE;
            }
            else
            {
                _engine.VideoSize = _previousVideoSize;
            }
        }

        private void NotifyOnPlayingModeChanged()
        {
            _isInPlayingMode = _engine.GraphState != GraphState.Reset;
            Messenger.Default.Send(new PropertyChangedMessage<bool>(this, !_isInPlayingMode, _isInPlayingMode, "IsInPlayingMode"));
        }

        private void OnPlayNewFile(PlayNewFileMessage message)
        {
            PlayFile(message.Content);
        }

        private void OnPlayDisc(PlayDiscMessage message)
        {
            PlayDvd(message.Content);
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

                _engine.FailedStreamsAvailable += _engine_FailedStreamsAvailable;

                _engine.DvdParentalChange += OnUserDecisionNeeded;
                _engine.PartialSuccess += OnUserDecisionNeeded;

                _previousVideoSize = VideoSize;

                if (_startFullScreen && !IsFullScreen)
                {
                    FlipFullScreen(true);
                }
            }
            else if (message.Content == Event.DispatcherTimerTick)
            {
                if (_isInPlayingMode)
                {
                    Messenger.Default.Send(new EventMessage(Event.StateRefreshSuggested));

                    UpdateCursor();
                }
            }
            else if (message.Content == Event.ContextMenuOpened)
            {
                _isContextMenuOpen = true;
            }
            else if (message.Content == Event.ContextMenuClosed)
            {
                _isContextMenuOpen = false;
            }
            else if (message.Content == Event.FullScreenControlPanelOpened)
            {
                _fullScreenControlPanelOpened = true;
            }
            else if (message.Content == Event.FullScreenControlPanelClosed)
            {
                _fullScreenControlPanelOpened = false;
            }
            else if (message.Content == Event.InitSize)
            {
                var args = (InitSizeEventArgs)message.EventArgs;
                if (args.NewVideoSize.Width > 0.0 && args.NewVideoSize.Height > 0.0)
                {
                    _videoSize = new Tuple<double, double>(args.NewVideoSize.Width, args.NewVideoSize.Height);
                }
            }
            else if (message.Content == Event.KeyboardMouseAction)
            {
                var args = message.EventArgs as KeyboardMouseActionEventArgs;
                if (args != null)
                {
                    ExecuteCommand(args.Action);
                }
            }
            else if (message.Content == Event.MainWindowClosing)
            {
                _settingsProvider.Set("ControlPanelVisible", IsControlPanelVisible);
                _settingsProvider.Set(SettingsConstants.Language, Language);
            }
            else if (message.Content == Event.MouseMove)
            {
                if (!_cursorManager.IsCursorVisible)
                {
                    _cursorManager.ShowCursor();
                    _nCursorCount = 0;
                }
            }
        }

        private void _engine_FailedStreamsAvailable(IList<Core.MediaEngine.Description.StreamInfo> streams)
        {
            _failedStreamsContainer.SetFailedStreams(streams);
            _dialogService.DisplayFailedStreamsWindow();
            _failedStreamsContainer.Clear();
        }

        private int _nCursorCount;
        private void UpdateCursor()
        {
            if (++_nCursorCount > 4)
            {
                _nCursorCount = 0;

                if (_isInPlayingMode && IsFullScreen && !_isContextMenuOpen && !_fullScreenControlPanelOpened && _cursorManager.IsCursorVisible)
                {
                    _cursorManager.HideCursor();
                }
            }
        }

        private void OnUserDecisionNeeded(object sender, UserDecisionEventArgs e)
        {
            e.Accept = _dialogService.DisplayYesNoDialog(e.Message);
        }

        private void UpdateState()
        {
            if (_engine.GraphState == GraphState.Reset)
            {
                _videoSize = null;
                _displayService.AllowMonitorPowerdown();
            }
            else
            {
                _displayService.PreventMonitorPowerdown();
            }
            
            RaisePropertyChanged("PlayRateChangePossible");
            NotifyOnPlayingModeChanged();

            Messenger.Default.Send(new EventMessage(Event.StateRefreshSuggested));
        }
        
        public ICommand SettingsCommand
        {
            get
            {
                if (_settingsCommand == null)
                {
                    _settingsCommand = new RelayCommand(
                        () =>
                            {
                                Messenger.Default.Send(new EventMessage(Event.SettingsDialogActivated));
                                _dialogService.DisplaySettingsDialog();
                                Messenger.Default.Send(new EventMessage(Event.SettingsDialogDeactivated));
                            });
                }

                return _settingsCommand;
            }
        }

        public bool ShowLogo
        {
            get { return _showLogo; }
            set
            {
                if (value.Equals(_showLogo)) return;
                _showLogo = value;
                RaisePropertyChanged("ShowLogo");
            }
        }

        public bool AutoPlay
        {
            get { return _autoPlay; }
            set
            {
                if (value.Equals(_autoPlay)) return;
                _autoPlay = value;
                RaisePropertyChanged("AutoPlay");
            }
        }

        private void TakeScreenshot()
        {
            if (_engine.GraphState == GraphState.Reset)
                return;

            IImageCreator imageCreator = _imageCreaterFactory.GetNew();
            var goodToGo = false;
            try
            {
                _engine.GetCurrentImage(imageCreator);
                goodToGo = imageCreator.Created;
            }
            catch (Exception e)
            {
                TraceSink.GetTraceSink().TraceError(string.Format("Error creating a screenshot: {0}", e));
            }

            if (goodToGo)
            {
                Task.Factory.StartNew(SaveScreenshot, imageCreator).ContinueWith(prevTask =>
                                                                                {
                                                                                    var disposable = imageCreator as IDisposable;
                                                                                    if (disposable != null)
                                                                                        disposable.Dispose();
                                                                                });
            }
        }

        private readonly object _screenshotsSyncRoot = new object();
        private void SaveScreenshot(object state)
        {
            IImageCreator imageCreator = state as IImageCreator;
            if (imageCreator == null)
                return;

            try
            {
                lock (_screenshotsSyncRoot)
                {
                    imageCreator.Save(GetNewScreenshotName());
                }
            }
            catch (Exception e)
            {
                TraceSink.GetTraceSink().TraceError(string.Format("Error saving a screenshot: {0}", e));
            }
        }

        private const string SCREENSHOT_NAME_FORMAT = "pvp_screenshot_{0}.jpg";
        private readonly Regex _regexScrnshotName = new Regex(@"pvp_screenshot_(?<index>\d+).jpg");
        private string GetNewScreenshotName()
        {
            string dir = _settingsProvider.Get("ScreenshotsFolder", DefaultSettings.SreenshotsFolder);
            if (!Directory.Exists(dir))
                dir = DefaultSettings.SreenshotsFolder;
            string[] files = Directory.GetFiles(dir, "*.jpg", SearchOption.TopDirectoryOnly);
            int index = 0;
            foreach (string file in files)
            {
                Match m = _regexScrnshotName.Match(file);
                if (m.Success)
                {
                    int i;
                    if (Int32.TryParse(m.Groups["index"].Value, out i))
                    {
                        if (i >= index)
                            index = ++i;
                    }
                }
            }

            return Path.Combine(dir, string.Format(SCREENSHOT_NAME_FORMAT, index));
        }

        private void PackUpCommandBag()
        {
            _commandBag = new Dictionary<string, ICommand>();

            _commandBag.Add(CommandConstants.Open, OpenCommand);
            _commandBag.Add(CommandConstants.Close, CloseCommand);
            _commandBag.Add(CommandConstants.Info, InfoCommand);

            _commandBag.Add(CommandConstants.Play, _controlViewModel.PlayCommand);
            _commandBag.Add(CommandConstants.PlayPause, PlayPauseCommand);
            _commandBag.Add(CommandConstants.Stop, _controlViewModel.StopCommand);
            _commandBag.Add(CommandConstants.Repeat, _controlViewModel.RepeatCommand);
            _commandBag.Add(CommandConstants.FullScreen, FullScreenCommand);

            _commandBag.Add(CommandConstants.Back, _controlViewModel.BackwardCommand);
            _commandBag.Add(CommandConstants.Forth, _controlViewModel.ForwardCommand);

            _commandBag.Add(CommandConstants.VideoSize50, VideoSizeCommand);
            _commandBag.Add(CommandConstants.VideoSize100, VideoSizeCommand);
            _commandBag.Add(CommandConstants.VideoSize200, VideoSizeCommand);
            _commandBag.Add(CommandConstants.VideoSizeFree, VideoSizeCommand);

            _commandBag.Add(CommandConstants.AspectRatioOriginal, AspectRatioCommand);
            _commandBag.Add(CommandConstants.AspectRatio4X3, AspectRatioCommand);
            _commandBag.Add(CommandConstants.AspectRatio16X9, AspectRatioCommand);
            _commandBag.Add(CommandConstants.AspectRatio47X20, AspectRatioCommand);
            _commandBag.Add(CommandConstants.AspectRatio1X1, AspectRatioCommand);
            _commandBag.Add(CommandConstants.AspectRatio5X4, AspectRatioCommand);
            _commandBag.Add(CommandConstants.AspectRatio16X10, AspectRatioCommand);
            _commandBag.Add(CommandConstants.AspectRatioFree, AspectRatioCommand);

            _commandBag.Add(CommandConstants.VolumeUp, _controlViewModel.VolumeUpCommand);
            _commandBag.Add(CommandConstants.VolumeDown, _controlViewModel.VolumeDownCommand);
            _commandBag.Add(CommandConstants.Mute, _controlViewModel.MuteCommand);

            _commandBag.Add(CommandConstants.TakeScreenshot, ScreenshotsCommand);
            _commandBag.Add(CommandConstants.Settings, SettingsCommand);
            _commandBag.Add(CommandConstants.About, AboutCommand);
            _commandBag.Add(CommandConstants.Exit, ExitCommand);
        }

        private void ExecuteCommand(string commandName)
        {
            ICommand command;
            if (_commandBag.TryGetValue(commandName, out command))
            {
                if (command.CanExecute(commandName))
                {
                    command.Execute(commandName);
                }
            }
        }

        private ICommand PlayPauseCommand
        {
            get
            {
                if (_playPauseCommand == null)
                {
                    _playPauseCommand = new RelayCommand(
                        () =>
                            {
                                if (_controlViewModel.PlayCommand.CanExecute(null))
                                    _controlViewModel.PlayCommand.Execute(null);
                                else if (_controlViewModel.PauseCommand.CanExecute(null))
                                    _controlViewModel.PauseCommand.Execute(null);
                            },
                        () =>
                            {
                                return _controlViewModel.PlayCommand.CanExecute(null) || _controlViewModel.PauseCommand.CanExecute(null);
                            });
                }

                return _playPauseCommand;
            }
        }

        private ICommand VideoSizeCommand
        {
            get
            {
                if (_videoSizeCommand == null)
                {
                    _videoSizeCommand = new RelayCommand<string>(commandName =>
                    {
                        switch(commandName)
                        {
                            case CommandConstants.VideoSize50:
                                VideoSize = VideoSize.SIZE50;
                                break;
                            case CommandConstants.VideoSize100:
                                VideoSize = VideoSize.SIZE100;
                                break;
                            case CommandConstants.VideoSize200:
                                VideoSize = VideoSize.SIZE200;
                                break;
                            case CommandConstants.VideoSizeFree:
                                VideoSize = VideoSize.SIZE_FREE;
                                break;
                        }
                    });
                }

                return _videoSizeCommand;
            }
        }

        private ICommand AspectRatioCommand
        {
            get
            {
                if (_aspectRatioCommand == null)
                {
                    _aspectRatioCommand = new RelayCommand<string>(commandName =>
                    {
                        switch(commandName)
                        {
                            case CommandConstants.AspectRatioOriginal:
                                AspectRatio = AspectRatio.AR_ORIGINAL;
                                break;
                            case CommandConstants.AspectRatio4X3:
                                AspectRatio = AspectRatio.AR_4x3;
                                break;
                            case CommandConstants.AspectRatio16X9:
                                AspectRatio = AspectRatio.AR_16x9;
                                break;
                            case CommandConstants.AspectRatio47X20:
                                AspectRatio = AspectRatio.AR_47x20;
                                break;
                            case CommandConstants.AspectRatio1X1:
                                AspectRatio = AspectRatio.AR_1x1;
                                break;
                            case CommandConstants.AspectRatio5X4:
                                AspectRatio = AspectRatio.AR_5x4;
                                break;
                            case CommandConstants.AspectRatio16X10:
                                AspectRatio = AspectRatio.AR_16x10;
                                break;
                            case CommandConstants.AspectRatioFree:
                                AspectRatio = AspectRatio.AR_FREE;
                                break;
                        }
                    });
                }

                return _aspectRatioCommand;
            }
        }

        private ICommand ScreenshotsCommand
        {
            get 
            { 
                if (_screenshotsCommand == null)
                {
                    _screenshotsCommand = new RelayCommand(TakeScreenshot);
                }

                return _screenshotsCommand;
            }
        }

        public ICommand AboutCommand
        {
            get
            {
                if (_aboutCommand == null)
                {
                    _aboutCommand = new RelayCommand(() => _dialogService.DisplayAboutAppWindow());
                }

                return _aboutCommand;
            }
        }

        public SupportedLanguage Language
        {
            get { return _language; }
            set
            {
                _language = value;

                CultureInfo ci;
                switch(value)
                {
                    case SupportedLanguage.English:
                        ci = new CultureInfo("en-US");
                        break;
                    case SupportedLanguage.Russian:
                        ci = new CultureInfo("ru-RU");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(value.ToString());
                }

                Thread.CurrentThread.CurrentCulture = ci;
                Thread.CurrentThread.CurrentUICulture = ci;

                Messenger.Default.Send(new EventMessage(Event.CurrentCultureChanged));
                
                RaisePropertyChanged("Language");
            }
        }
    }
}
