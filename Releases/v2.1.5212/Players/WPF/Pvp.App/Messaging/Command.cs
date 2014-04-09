using System;
using System.Linq;

namespace Pvp.App.Messaging
{
    internal enum Command
    {
        ApplicationClose,
        SettingsWindowClose,
        EnterKeyWindowClose,
        MediaInformationWindowClose,
        FailedStreamsWindowClose,
        AboutAppWindowClose,
        ResizeMainWindow,
        SaveSettings
    }
}
