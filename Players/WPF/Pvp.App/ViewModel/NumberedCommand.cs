using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;

namespace Pvp.App.ViewModel
{
    internal class NumberedCommand : INotifyPropertyChanged
    {
        private bool _isChecked;

        public int Number { get; set; }
        public string Title { get; set; }
        public ICommand Command { get; set; }

        public bool IsChecked
        {
            get { return _isChecked; }
            set 
            { 
                _isChecked = value;
                RaiseOnPropertyChanged("IsChecked");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaiseOnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}