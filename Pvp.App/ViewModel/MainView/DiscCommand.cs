using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Input;

namespace Pvp.App.ViewModel.MainView
{
    internal class DiscCommand : INotifyPropertyChanged
    {
        private readonly SynchronizationContext _context;

        public DiscCommand(SynchronizationContext context)
        {
            _context = context;
        }

        public IDriveInfo DriveInfo { get; set; }
        public ICommand Command { get; set; }

        private bool _isEnabled;
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set 
            {
                if (_context != null)
                {
                    _context.Post(state =>
                    {
                        SetIsEnabled(value);
                    }, null);
                }
                else
                {
                    SetIsEnabled(value);
                }
            }
        }
  
        private string _title;
        public string Title
        {
            get { return _title; }
            set 
            { 
                if (_context != null)
                {
                    _context.Post(state =>
                    {
                        SetTitle(value);
                    }, null);
                }
                else
                {
                	SetTitle(value);
                }
            }
        }

        private void SetTitle(string value)
        {
            _title = value;
            RaiseOnPropertyChanged("Title");
        }

        private void SetIsEnabled(bool value)
        {
            _isEnabled = value;
            RaiseOnPropertyChanged("IsEnabled");
        }
  
        public event PropertyChangedEventHandler PropertyChanged;

        private void RaiseOnPropertyChanged(string propertyName)
        {
        	if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}