using System;
using System.Linq;
using GalaSoft.MvvmLight;

namespace Pvp.App.ViewModel.Settings
{
    public class FileTypeItem : ViewModelBase
    {
        private bool _selected;

        public bool Selected
        {
            get { return _selected; }
            set
            {
                if (value.Equals(_selected)) return;
                _selected = value;
                RaisePropertyChanged("Selected");
            }
        }

        public string Extension { get; set; }
    }
}