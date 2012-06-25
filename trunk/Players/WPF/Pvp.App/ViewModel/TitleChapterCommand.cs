using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace Pvp.App.ViewModel
{
    internal class TitleChapterCommand : INotifyPropertyChanged
    {
        private bool _isChecked;

        public TitleChapterCommand()
        {
        	SubItems = new List<TitleChapterCommand>();
        }

        public int Title { get; set; }
        public int Chapter { get; set; }
        public string DisplayName { get; set; }
        public ICommand Command { get; set; }

        public ICollection<TitleChapterCommand> SubItems { get; private set; }

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
