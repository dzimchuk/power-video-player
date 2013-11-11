using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Pvp.App.Messaging;
using Pvp.App.ViewModel.Settings;
using Pvp.Core.DirectShow;
using Pvp.Core.MediaEngine;
using Pvp.Core.Wpf;

namespace Pvp.App.ViewModel
{
    internal class MainViewModel : ViewModelBase
    {
        private readonly IMediaEngineFacade _engine;
        private readonly ControlPanelViewModel _controlViewModel;

        private readonly IFileSelector _fileSelector;
        private readonly IDialogService _dialogService;
        private readonly IWindowHandleProvider _windowHandleProvider;
        private readonly IDriveService _driveService;
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

        private bool _mediaControlCreated;

        public MainViewModel(IMediaEngineFacade engine,
            ControlPanelViewModel controlViewModel,
            IFileSelector fileSelector,
            IDialogService dialogService,
            IWindowHandleProvider windowHandleProvider,
            IDriveService driveService,
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
            _windowHandleProvider = windowHandleProvider;
            _driveService = driveService;
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

            PopulateCDRomMenu();
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
                _mediaControlCreated = true;

                _engine.ErrorOccured += delegate(object sender, ErrorOccuredEventArgs args)
                {
                    _dialogService.DisplayError(args.Message);
                };

                _engine.FailedStreamsAvailable += _engine_FailedStreamsAvailable;

                _engine.DvdParentalChange += OnUserDecisionNeeded;
                _engine.PartialSuccess += OnUserDecisionNeeded;
                _engine.ModifyMenu += _engine_ModifyMenu;

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

                    UpdateMenusCheckedStatus();
                    UpdateCursor();
                }
            }
            else if (message.Content == Event.ContextMenuOpened)
            {
                _isContextMenuOpen = true;
            	ScanCDRomDrives();
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
  
        private void UpdateMenusCheckedStatus()
        {
            var dvdAngle = _engine.CurrentAngle;
            foreach (var item in _dvdAngles)
            {
                item.IsChecked = item.Number == dvdAngle;
            }

            var subpictureStream = _engine.CurrentSubpictureStream;
            foreach (var item in _dvdSubpictureStreams)
            {
                bool toBeChecked = false;
                if (item.Number == -1)
                {
                    toBeChecked = _engine.IsSubpictureEnabled();
                }
                else
                {
                    toBeChecked = item.Number == subpictureStream;
                }

                item.IsChecked = toBeChecked;
            }

            var audioStream = _engine.CurrentAudioStream;
            foreach (var item in _audioStreams)
            {
                item.IsChecked = item.Number == audioStream;
            }

            var title = _engine.CurrentTitle;
            var chapter = _engine.CurrentChapter;
            foreach (var item in _dvdChapters)
            {
            	CheckTitleChaperMenuItem(item, title, chapter);
            }
        }

        private void CheckTitleChaperMenuItem(TitleChapterCommand item, int currentTitle, int currentChapter)
        {
            bool toBeChecked = item.SubItems.Any() ? item.Title == currentTitle : (item.Title == currentTitle && item.Chapter == currentChapter);
            item.IsChecked = toBeChecked;

            foreach (var subItem in item.SubItems)
            {
                CheckTitleChaperMenuItem(subItem, currentTitle, currentChapter);
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
            
            UpdateMenu();
            RaisePropertyChanged("PlayRateChangePossible");
            NotifyOnPlayingModeChanged();

            Messenger.Default.Send(new EventMessage(Event.StateRefreshSuggested));
        }
        
        private readonly ObservableCollection<CDRomCommand> _cdRomMenuItems = new ObservableCollection<CDRomCommand>();
        public ObservableCollection<CDRomCommand> CDRomMenuItems
        {
            get { return _cdRomMenuItems; }
        }

        private void PopulateCDRomMenu()
        {
            var command = new RelayCommand<CDRomCommand>(
                d =>
                {
                    if (d != null)
                    {
                    	PlayDvd(d.DriveInfo.Name);
                    }
                },
                d =>
                {
                    return d != null ? d.IsEnabled : false;
                });

        	var drives = _driveService.GetAvailableCDRomDrives();
            foreach (var drive in drives)
            {
                _cdRomMenuItems.Add(new CDRomCommand(SynchronizationContext.Current)
                {
                    DriveInfo = drive,
                    Title = drive.Name,
                    Command = command
                });
            }
        }

        private void ScanCDRomDrives()
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                var builder = new StringBuilder();

                foreach (var item in _cdRomMenuItems)
                {
                    item.IsEnabled = false;
                    builder.Append(item.DriveInfo.Name);
                    try
                    {
                        if (item.DriveInfo.IsReady)
                        {
                        	builder.Append(item.DriveInfo.VolumeLabel);

                            item.Title = builder.ToString();
                            item.IsEnabled = true;
                        }
                    }
                    catch {}

                    item.Title = builder.ToString();
                    builder.Remove(0, builder.Length);
                }
            });
        }

        private void UpdateMenu()
        {
            UpdateFiltersMenu();
            UpdateDvdMenu();
            UpdateDvdMenuLanguagesMenu();
            UpdateDvdAnglesMenu();
            UpdateDvdSubpictureStreamsMenu();
            UpdateAudioStreamsMenu();
            UpdateDvdChaptersMenu();
        }

        private void _engine_ModifyMenu(object sender, EventArgs e)
        {
            UpdateMenu();
        }
                
        private void UpdateFiltersMenu()
        {
            _filters.Clear();

            if (_engine.GraphState == GraphState.Reset)
            {
                FiltersMenuVisible = false;
            }
            else
            {
                var last = _engine.FilterCount;
                if (last > 15)
                    last = 15;

                var command = new GenericRelayCommand<NumberedCommand>(
                    nc =>
                    {
                        if (nc != null)
                            _engine.DisplayFilterPropPage(_windowHandleProvider.Handle, nc.Number, true);
                    },
                    nc =>
                    {
                        return nc != null ? _engine.DisplayFilterPropPage(_windowHandleProvider.Handle, nc.Number, false) : false;
                    });

                for (int i = 0; i < last; i++)
                {
                    _filters.Add(new NumberedCommand { Number = i, Title = _engine.GetFilterName(i), Command = command });
                }

                FiltersMenuVisible = true;
            }
        }

        private bool _filtersMenuVisible;
        public bool FiltersMenuVisible
        {
            get { return _filtersMenuVisible; }
            set
            {
                _filtersMenuVisible = value;
                RaisePropertyChanged("FiltersMenuVisible");
            }
        }

        private readonly ObservableCollection<NumberedCommand> _filters = new ObservableCollection<NumberedCommand>();
        public ObservableCollection<NumberedCommand> Filters
        {
            get { return _filters; }
        }

        private bool _dvdMenuVisible;
        public bool DvdMenuVisible
        {
            get { return _dvdMenuVisible; }
            set
            {
                _dvdMenuVisible = value;
                RaisePropertyChanged("DvdMenuVisible");
            }
        }

        private readonly ObservableCollection<NumberedCommand> _dvdMenuItems = new ObservableCollection<NumberedCommand>();
        public ObservableCollection<NumberedCommand> DvdMenuItems
        {
            get { return _dvdMenuItems; }
        }

        private void UpdateDvdMenu()
        {
        	_dvdMenuItems.Clear();
            DvdMenuVisible = false;

            if (_engine.SourceType == SourceType.Dvd)
            {
                var command = new GenericRelayCommand<NumberedCommand>(
                    nc =>
                    {
                        if (nc != null)
                        {
                            _engine.ShowMenu((DVD_MENU_ID)nc.Number);
                        }
                    },
                    nc =>
                    {
                        var enabled = false;

                        if (nc != null)
                        {
                            VALID_UOP_FLAG uops = _engine.UOPS;
                            DVD_MENU_ID id = (DVD_MENU_ID)nc.Number;
                            switch (id)
                            {
                                case DVD_MENU_ID.DVD_MENU_Title:
                                    enabled = (uops & VALID_UOP_FLAG.UOP_FLAG_ShowMenu_Title) == 0;
                                    break;
                                case DVD_MENU_ID.DVD_MENU_Root:
                                    enabled = (uops & VALID_UOP_FLAG.UOP_FLAG_ShowMenu_Root) == 0;
                                    break;
                                case DVD_MENU_ID.DVD_MENU_Subpicture:
                                    enabled = (uops & VALID_UOP_FLAG.UOP_FLAG_ShowMenu_SubPic) == 0;
                                    break;
                                case DVD_MENU_ID.DVD_MENU_Audio:
                                    enabled = (uops & VALID_UOP_FLAG.UOP_FLAG_ShowMenu_Audio) == 0;
                                    break;
                                case DVD_MENU_ID.DVD_MENU_Angle:
                                    enabled = (uops & VALID_UOP_FLAG.UOP_FLAG_ShowMenu_Angle) == 0;
                                    break;
                                case DVD_MENU_ID.DVD_MENU_Chapter:
                                    enabled = (uops & VALID_UOP_FLAG.UOP_FLAG_ShowMenu_Chapter) == 0;
                                    break;
                            }
                        }

                        return enabled;
                    });

                _dvdMenuItems.Add(new NumberedCommand
                {
                    Title = Resources.Resources.mi_title_menu,
                    Command = command,
                    Number = (int)DVD_MENU_ID.DVD_MENU_Title
                });

                _dvdMenuItems.Add(new NumberedCommand
                {
                    Title = Resources.Resources.mi_root_menu,
                    Command = command,
                    Number = (int)DVD_MENU_ID.DVD_MENU_Root
                });

                _dvdMenuItems.Add(new NumberedCommand
                {
                    Title = Resources.Resources.mi_subpicture_menu,
                    Command = command,
                    Number = (int)DVD_MENU_ID.DVD_MENU_Subpicture
                });

                _dvdMenuItems.Add(new NumberedCommand
                {
                    Title = Resources.Resources.mi_audio_menu,
                    Command = command,
                    Number = (int)DVD_MENU_ID.DVD_MENU_Audio
                });

                _dvdMenuItems.Add(new NumberedCommand
                {
                    Title = Resources.Resources.mi_angle_menu,
                    Command = command,
                    Number = (int)DVD_MENU_ID.DVD_MENU_Angle
                });

                _dvdMenuItems.Add(new NumberedCommand
                {
                    Title = Resources.Resources.mi_chapter_menu,
                    Command = command,
                    Number = (int)DVD_MENU_ID.DVD_MENU_Chapter
                });

                DvdMenuVisible = true;
            }
        }

        private ICommand _dvdResumeCommand;
        public ICommand DvdResumeCommand
        {
            get
            {
                if (_dvdResumeCommand == null)
                {
                    _dvdResumeCommand = new RelayCommand(
                        () =>
                        {
                            _engine.ResumeDVD();
                        },
                        () =>
                        {
                            return _engine.IsResumeDVDEnabled();
                        });
                }

                return _dvdResumeCommand;
            }
        }

        private void UpdateDvdMenuLanguagesMenu()
        {
            _dvdMenuLanguages.Clear();
            DvdMenuLanguagesMenuVisible = false;

            if (_engine.GraphState != GraphState.Reset)
            {
                var nLang = _engine.MenuLangCount;

                if (nLang > 0)
                {
                    if (nLang > 10)
                        nLang = 10;

                    var command = new GenericRelayCommand<NumberedCommand>(
                        nc =>
                        {
                            if (nc != null)
                                _engine.SetMenuLang(nc.Number);
                        },
                        nc =>
                        {
                            return nc != null ? _engine.MenuLangCount > 1 : false;
                        });

                    for (int i = 0; i < nLang; i++)
                    {
                        _dvdMenuLanguages.Add(new NumberedCommand { Number = i, Title = _engine.GetMenuLangName(i), Command = command });
                    }

                    DvdMenuLanguagesMenuVisible = true;
                }
            }
        }

        private bool _dvdMenuLanguagesMenuVisible;
        public bool DvdMenuLanguagesMenuVisible
        {
            get { return _dvdMenuLanguagesMenuVisible; }
            set
            {
                _dvdMenuLanguagesMenuVisible = value;
                RaisePropertyChanged("DvdMenuLanguagesMenuVisible");
            }
        }

        private readonly ObservableCollection<NumberedCommand> _dvdMenuLanguages = new ObservableCollection<NumberedCommand>();
        public ObservableCollection<NumberedCommand> DvdMenuLanguages
        {
            get { return _dvdMenuLanguages; }
        }

        private void UpdateDvdAnglesMenu()
        {
            _dvdAngles.Clear();
            DvdAnglesMenuVisible = false;

            if (_engine.GraphState != GraphState.Reset)
            {
                int ulAngles = _engine.AnglesAvailable;

                if (ulAngles > 1)
                {
                    var command = new GenericRelayCommand<NumberedCommand>(
                        nc =>
                        {
                            if (nc != null)
                                _engine.CurrentAngle = nc.Number;
                        },
                        nc =>
                        {
                            return nc != null ? (_engine.UOPS & VALID_UOP_FLAG.UOP_FLAG_Select_Angle) == 0 : false;
                        });

                    for (int i = 0; i < ulAngles; i++)
                    {
                        _dvdAngles.Add(new NumberedCommand { Number = i + 1, Title = string.Format(Resources.Resources.mi_angle_format, i + 1), Command = command });
                    }

                    DvdAnglesMenuVisible = true;
                }
            }
        }

        private bool _dvdAnglesMenuVisible;
        public bool DvdAnglesMenuVisible
        {
            get { return _dvdAnglesMenuVisible; }
            set
            {
                _dvdAnglesMenuVisible = value;
                RaisePropertyChanged("DvdAnglesMenuVisible");
            }
        }

        private readonly ObservableCollection<NumberedCommand> _dvdAngles = new ObservableCollection<NumberedCommand>();
        public ObservableCollection<NumberedCommand> DvdAngles
        {
            get { return _dvdAngles; }
        }

        private void UpdateDvdSubpictureStreamsMenu()
        {
            _dvdSubpictureStreams.Clear();
            DvdSubpictureStreamsMenuVisible = false;

            if (_engine.GraphState != GraphState.Reset)
            {
                int nStreams = _engine.NumberOfSubpictureStreams;

                if (nStreams > 0)
                {
                    var command = new GenericRelayCommand<NumberedCommand>(
                        nc =>
                        {
                            if (nc != null)
                            {
                                if (nc.Number == -1)
                                {
                                    _engine.EnableSubpicture(!_engine.IsSubpictureEnabled());
                                }
                                else
                                {
                                    _engine.CurrentSubpictureStream = nc.Number;
                                }
                            }
                        },
                        nc =>
                        {
                            var enabled = false;
                            if (nc != null)
                            {
                                if (nc.Number == -1)
                                {
                                    enabled = (_engine.UOPS & VALID_UOP_FLAG.UOP_FLAG_Select_SubPic_Stream) == 0;
                                }
                                else
                                {
                                    enabled = (_engine.UOPS & VALID_UOP_FLAG.UOP_FLAG_Select_SubPic_Stream) == 0 &&
                                        _engine.IsSubpictureStreamEnabled(nc.Number);
                                }
                            }

                            return enabled;
                        });

                    _dvdSubpictureStreams.Add(new NumberedCommand { Number = -1, Title = Resources.Resources.mi_display_subpictures, Command = command });
                    for (int i = 0; i < nStreams; i++)
                    {
                        _dvdSubpictureStreams.Add(new NumberedCommand { Number = i, Title = _engine.GetSubpictureStreamName(i), Command = command });
                    }

                    DvdSubpictureStreamsMenuVisible = true;
                }
            }
        }

        private bool _dvdSubpictureStreamsMenuVisible;
        public bool DvdSubpictureStreamsMenuVisible
        {
            get { return _dvdSubpictureStreamsMenuVisible; }
            set
            {
                _dvdSubpictureStreamsMenuVisible = value;
                RaisePropertyChanged("DvdSubpictureStreamsMenuVisible");
            }
        }

        private readonly ObservableCollection<NumberedCommand> _dvdSubpictureStreams = new ObservableCollection<NumberedCommand>();
        public ObservableCollection<NumberedCommand> DvdSubpictureStreams
        {
            get { return _dvdSubpictureStreams; }
        }

        private void UpdateAudioStreamsMenu()
        {
            _audioStreams.Clear();
            AudioStreamsMenuVisible = false;

            if (_engine.GraphState != GraphState.Reset)
            {
                int nStreams = _engine.AudioStreams;

                if (nStreams > 0)
                {
                    var command = new GenericRelayCommand<NumberedCommand>(
                        nc =>
                        {
                            if (nc != null)
                                _engine.CurrentAudioStream = nc.Number;
                        },
                        nc =>
                            {
                                return nc != null &&
                                       (_engine.SourceType != SourceType.Dvd ||
                                        ((_engine.IsAudioStreamEnabled(nc.Number)) && (_engine.UOPS & VALID_UOP_FLAG.UOP_FLAG_Select_Audio_Stream) == 0));
                            });

                    if (nStreams > 8)
                        nStreams = 8;

                    for (int i = 0; i < nStreams; i++)
                    {
                        _audioStreams.Add(new NumberedCommand { Number = i, Title = _engine.GetAudioStreamName(i), Command = command });
                    }

                    AudioStreamsMenuVisible = true;
                }
            }
        }

        private bool _audioStreamsMenuVisible;
        public bool AudioStreamsMenuVisible
        {
            get { return _audioStreamsMenuVisible; }
            set
            {
                _audioStreamsMenuVisible = value;
                RaisePropertyChanged("AudioStreamsMenuVisible");
            }
        }

        private readonly ObservableCollection<NumberedCommand> _audioStreams = new ObservableCollection<NumberedCommand>();
        public ObservableCollection<NumberedCommand> AudioStreams
        {
            get { return _audioStreams; }
        }

        private void UpdateDvdChaptersMenu()
        {
            _dvdChapters.Clear();
            DvdChaptersMenuVisible = false;

            if (_engine.GraphState != GraphState.Reset)
            {
                int ulTitles = _engine.NumberOfTitles;

                if (ulTitles > 0)
                {
                    var command = new GenericRelayCommand<TitleChapterCommand>(
                        tc =>
                        {
                            if (tc != null)
                                _engine.GoTo(tc.Title, tc.Chapter);
                        },
                        tc =>
                        {
                            return tc != null ?
                                    (_engine.UOPS & VALID_UOP_FLAG.UOP_FLAG_Play_Chapter) == 0
                                    :
                                    false;
                        });

                    if (ulTitles == 1)
                    {
                    	int nChapters = _engine.GetNumChapters(1);
                        for (int i = 1; i <= nChapters; i++)
                        {
                            _dvdChapters.Add(new TitleChapterCommand
                            {
                                Title = 1,
                                Chapter = i,
                                Command = command,
                                DisplayName = string.Format(Resources.Resources.mi_chapter_format, i)
                            });
                        }
                    }
                    else
                    {
                        for (int title = 1; title <= ulTitles; title++)
                        {
                            var titleItem = new TitleChapterCommand { Title = title, DisplayName = string.Format(Resources.Resources.mi_title_format, title) };
                            
                            int nChapters = _engine.GetNumChapters(title);
                            for (int i = 1; i <= nChapters; i++)
                            {
                                titleItem.SubItems.Add(new TitleChapterCommand
                                {
                                    Title = title,
                                    Chapter = i,
                                    Command = command,
                                    DisplayName = string.Format(Resources.Resources.mi_chapter_format, i)
                                });
                            }

                            _dvdChapters.Add(titleItem);
                        }
                    }

                    DvdChaptersMenuVisible = true;
                }
            }
        }

        private bool _dvdChaptersMenuVisible;
        public bool DvdChaptersMenuVisible
        {
            get { return _dvdChaptersMenuVisible; }
            set
            {
                _dvdChaptersMenuVisible = value;
                RaisePropertyChanged("DvdChaptersMenuVisible");
            }
        }

        private readonly ObservableCollection<TitleChapterCommand> _dvdChapters = new ObservableCollection<TitleChapterCommand>();
        
        public ObservableCollection<TitleChapterCommand> DvdChapters
        {
            get { return _dvdChapters; }
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

                CultureInfo ci = null;
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
