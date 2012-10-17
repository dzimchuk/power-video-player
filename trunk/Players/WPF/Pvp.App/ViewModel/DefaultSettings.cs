using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace Pvp.App.ViewModel
{
    internal static class DefaultSettings
    {
        public static bool TopMost
        {
            get { return false; }
        }

        public static bool CenterWindow
        {
            get { return true; }
        }

        public static string SreenshotsFolder
        {
            get { return Environment.GetFolderPath(Environment.SpecialFolder.Desktop); }
        }

        public static MouseWheelAction MouseWheekAction
        {
            get { return MouseWheelAction.Volume; }
        }

        public static Dictionary<string, KeyCombination> KeyMap
        {
            get
            {
                return new Dictionary<string, KeyCombination>
                           {
                               { CommandConstants.Open, new KeyCombination(Key.O, ModifierKeys.Control) },
                               { CommandConstants.Close, new KeyCombination(Key.X, ModifierKeys.Control) },
                               { CommandConstants.Info, new KeyCombination(Key.I, ModifierKeys.None) },
                               { CommandConstants.Play, new KeyCombination(Key.P, ModifierKeys.None) },
                               { CommandConstants.PlayPause, new KeyCombination(Key.Space, ModifierKeys.None) },
                               { CommandConstants.Stop, new KeyCombination(Key.S, ModifierKeys.None) },
                               { CommandConstants.Repeat, new KeyCombination(Key.R, ModifierKeys.None) },
                               { CommandConstants.FullScreen, new KeyCombination(Key.F, ModifierKeys.None) },
                               { CommandConstants.Forth, new KeyCombination(Key.Right, ModifierKeys.None) },
                               { CommandConstants.Back, new KeyCombination(Key.Left, ModifierKeys.None) },
                               { CommandConstants.VideoSize50, new KeyCombination(Key.D1, ModifierKeys.None) },
                               { CommandConstants.VideoSize100, new KeyCombination(Key.D2, ModifierKeys.None) },
                               { CommandConstants.VideoSize200, new KeyCombination(Key.D3, ModifierKeys.None) },
                               { CommandConstants.VideoSizeFree, new KeyCombination(Key.D4, ModifierKeys.None) },
                               { CommandConstants.AspectRatioOriginal, new KeyCombination(Key.D1, ModifierKeys.Alt) },
                               { CommandConstants.AspectRatio4X3, new KeyCombination(Key.D2, ModifierKeys.Alt) },
                               { CommandConstants.AspectRatio16X9, new KeyCombination(Key.D3, ModifierKeys.Alt) },
                               { CommandConstants.AspectRatio47X20, new KeyCombination(Key.D4, ModifierKeys.Alt) },
                               { CommandConstants.AspectRatio1X1, new KeyCombination(Key.D5, ModifierKeys.Alt) },
                               { CommandConstants.AspectRatio5X4, new KeyCombination(Key.D6, ModifierKeys.Alt) },
                               { CommandConstants.AspectRatio16X10, new KeyCombination(Key.D7, ModifierKeys.Alt) },
                               { CommandConstants.AspectRatioFree, new KeyCombination(Key.D8, ModifierKeys.Alt) },
                               { CommandConstants.VolumeUp, new KeyCombination(Key.Up, ModifierKeys.None) },
                               { CommandConstants.VolumeDown, new KeyCombination(Key.Down, ModifierKeys.None) },
                               { CommandConstants.Mute, new KeyCombination(Key.M, ModifierKeys.None) },
                               { CommandConstants.TakeScreenshot, new KeyCombination(Key.S, ModifierKeys.Control) },
                               { CommandConstants.Settings, new KeyCombination(Key.F2, ModifierKeys.None) },
                               { CommandConstants.About, new KeyCombination(Key.A, ModifierKeys.None) },
                               { CommandConstants.Exit, new KeyCombination(Key.Escape, ModifierKeys.None) }
                           };
            }
        }
    }
}