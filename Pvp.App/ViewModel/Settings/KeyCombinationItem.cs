using System;
using System.Linq;
using GalaSoft.MvvmLight;

namespace Pvp.App.ViewModel.Settings
{
    public class KeyCombinationItem : ViewModelBase
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
    }
}