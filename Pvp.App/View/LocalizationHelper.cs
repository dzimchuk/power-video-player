using System;
using System.ComponentModel;
using System.Linq;
using GalaSoft.MvvmLight.Messaging;
using Pvp.App.Messaging;

namespace Pvp.App.View
{
    internal class LocalizationHelper : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public LocalizationHelper()
        {
            Messenger.Default.Register<EventMessage>(this, OnEvent);
        }

        private void OnEvent(EventMessage message)
        {
            if (message.Content == Event.CurrentCultureChanged)
            {
                OnPropertyChanged("LS");
            }
        }

        public string LS
        {
            get { return string.Empty; }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}