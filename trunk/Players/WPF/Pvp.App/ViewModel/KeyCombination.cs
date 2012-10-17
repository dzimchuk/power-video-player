using System;
using System.Linq;
using System.Windows.Input;

namespace Pvp.App.ViewModel
{
    public class KeyCombination : IEquatable<KeyCombination>
    {
        public KeyCombination()
        {
            Key = Key.None;
            ModifierKeys = ModifierKeys.None;
        }

        public KeyCombination(Key key, ModifierKeys modifierKeys)
        {
            Key = key;
            ModifierKeys = modifierKeys;
        }

        public Key Key { get; private set; }
        public ModifierKeys ModifierKeys { get; private set; }

        public bool Equals(KeyCombination other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Key.Equals(other.Key) && ModifierKeys.Equals(other.ModifierKeys);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((KeyCombination)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Key.GetHashCode() * 397) ^ ModifierKeys.GetHashCode();
            }
        }

        public static bool operator ==(KeyCombination left, KeyCombination right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(KeyCombination left, KeyCombination right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            string modifiers = null;
            if (ModifierKeys != ModifierKeys.None)
            {
                var converter = new ModifierKeysConverter();
                modifiers = converter.ConvertToString(ModifierKeys);
            }

            string key = null;
            if (Key != Key.None)
            {
                var keyConverter = new KeyConverter();
                key = keyConverter.ConvertToString(Key);
            }

            return string.IsNullOrEmpty(key)
                       ? string.Empty
                       : (string.IsNullOrEmpty(modifiers) ? key : string.Format("{0}+{1}", modifiers, key));
        }

        public KeyCombination Clone()
        {
            return new KeyCombination(Key, ModifierKeys);
        }

        public bool IsEmpty
        {
            get { return Key == Key.None && ModifierKeys == ModifierKeys.None; }
        }
    }
}