using System;
using System.Linq;

namespace Pvp.App.Messaging
{
    internal enum Event
    {
        TitleBarDoubleClick,
        VideoAreaDoubleClick,
        MouseMove,
        MainWindowClosing,
        SessionEnding,
        MediaControlCreated,
        StateRefreshSuggested,
        DispatcherTimerTick,
        ContextMenuOpened,
        ContextMenuClosed,
        InitSize,
        MainWindowResizeSuggested,
        SettingsDialogActivated,
        SettingsDialogDeactivated,
        KeyboardMouseAction,
        FullScreenControlPanelOpened,
        FullScreenControlPanelClosed,
        CurrentCultureChanged
    }
}
