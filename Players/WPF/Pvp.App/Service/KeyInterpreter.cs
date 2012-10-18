using System;
using System.Windows.Input;
using Pvp.App.ViewModel;
using Pvp.Core.Wpf;

namespace Pvp.App.Service
{
    internal class KeyInterpreter : IKeyInterpreter
    {
        public KeyCombination Interpret(EventArgs args)
        {
            KeyCombination result = null;

            var key = GetKey(args);
            if (key != Key.Return && key != Key.Enter)
            {
                if (key != Key.LeftCtrl && key != Key.RightCtrl &&
                    key != Key.LeftShift && key != Key.RightShift &&
                    key != Key.LeftAlt && key != Key.RightAlt &&
                    key != Key.LWin && key != Key.RWin)
                {
                    var modifiers = GetModifiers(args);

                    result = new KeyCombination(key, modifiers);
                }
                else
                {
                    result = new KeyCombination();
                }
            }

            return result;
        }

        private static Key GetKey(EventArgs args)
        {
            var keyArgs = args as KeyEventArgs;
            if (keyArgs != null)
            {
                return keyArgs.SystemKey == Key.None ? keyArgs.Key : keyArgs.SystemKey;
            }
            else
            {
                var mwKeyArgs = args as MWKeyEventArgs;
                return mwKeyArgs != null ? mwKeyArgs.Key : Key.None;
            }
        }

        private static ModifierKeys GetModifiers(EventArgs args)
        {
            var keyArgs = args as KeyEventArgs;
            if (keyArgs != null)
            {
                return keyArgs.KeyboardDevice.Modifiers;
            }
            else
            {
                var mwKeyArgs = args as MWKeyEventArgs;
                return mwKeyArgs != null ? mwKeyArgs.Modifiers : ModifierKeys.None;
            }
        }
    }
}