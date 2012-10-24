using System;
using System.Linq;

namespace Pvp.App.Messaging
{
    internal enum Command
    {
        ApplicationClose,
        SettingsWindowClose,
        EnterKeyWindowClose,
        ResizeMainWindow,
        SaveSettings
    }
}
