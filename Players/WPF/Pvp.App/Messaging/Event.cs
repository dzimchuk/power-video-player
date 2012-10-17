using System;
using System.Linq;

namespace Pvp.App.Messaging
{
    internal enum Event
    {
        TitleBarDoubleClick,
        VideoAreaDoubleClick,
        MainWindowClosing,
        SessionEnding,
        MediaControlCreated,
        StateRefreshSuggested,
        DispatcherTimerTick,
        ContextMenuOpened,
        InitSize,
        MainWindowResizeSuggested,
        SettingsDialogActivated,
        SettingsDialogDeactivated,
        EditKeyCombination,
        KeyboardAction
    }
}
