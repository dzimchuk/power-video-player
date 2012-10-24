using System;
using System.Linq;
using GalaSoft.MvvmLight;

namespace Pvp.App.ViewModel.Settings
{
    public class KeyCombinationItem : ViewModelBase, IEquatable<KeyCombinationItem>
    {
        private KeyCombination _keyCombination;

        public KeyCombination KeyCombination
        {
            get { return _keyCombination; }
            set
            {
                if (Equals(value, _keyCombination)) return;
                _keyCombination = value;
                RaisePropertyChanged("KeyCombination");
            }
        }

        public string Key { get; set; }

        public bool Equals(KeyCombinationItem other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(_keyCombination, other._keyCombination) && string.Equals(Key, other.Key);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((KeyCombinationItem)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_keyCombination != null ? _keyCombination.GetHashCode() : 0) * 397) ^ (Key != null ? Key.GetHashCode() : 0);
            }
        }

        public static bool operator ==(KeyCombinationItem left, KeyCombinationItem right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(KeyCombinationItem left, KeyCombinationItem right)
        {
            return !Equals(left, right);
        }

        public KeyCombinationItem Clone()
        {
            return new KeyCombinationItem
                       {
                           Key = Key,
                           KeyCombination = KeyCombination
                       };
        }
    }
}