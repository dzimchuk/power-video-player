using System;
using System.Windows.Input;
using Pvp.App.ViewModel;

namespace Pvp.App.Service
{
    internal class KeyInterpreter : IKeyInterpreter
    {
        public KeyCombination Interpret(EventArgs args)
        {
            KeyCombination result = null;

            var keyArgs = args as KeyEventArgs;

            if (keyArgs != null)
            {
                var key = GetKey(keyArgs);
                if (!keyArgs.IsRepeat && key != Key.Return && key != Key.Enter)
                {
                    if (key != Key.LeftCtrl && key != Key.RightCtrl &&
                        key != Key.LeftShift && key != Key.RightShift &&
                        key != Key.LeftAlt && key != Key.RightAlt &&
                        key != Key.LWin && key != Key.RWin)
                    {
                        ModifierKeys modifiers = ModifierKeys.None;
                        if ((keyArgs.KeyboardDevice.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                            modifiers |= ModifierKeys.Control;
                        if ((keyArgs.KeyboardDevice.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                            modifiers |= ModifierKeys.Shift;
                        if ((keyArgs.KeyboardDevice.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
                            modifiers |= ModifierKeys.Alt;
                        if ((keyArgs.KeyboardDevice.Modifiers & ModifierKeys.Windows) == ModifierKeys.Windows)
                            modifiers |= ModifierKeys.Windows;

                        result = new KeyCombination(key, modifiers);
                    }
                    else
                    {
                        result = new KeyCombination();
                    }
                }
            }

            return result;
        }

        private Key GetKey(KeyEventArgs keyArgs)
        {
            return keyArgs.SystemKey == Key.None ? keyArgs.Key : keyArgs.SystemKey;
        }
    }
}