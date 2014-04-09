using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using Pvp.App.Messaging;
using Pvp.Core.MediaEngine;

namespace Pvp.App.ViewModel.MainView
{
    internal abstract class MenuViewModelBase : ViewModelBase
    {
        private readonly IMediaEngineFacade _engine;

        protected MenuViewModelBase(IMediaEngineFacade engine)
        {
            _engine = engine;

            Messenger.Default.Register<PropertyChangedMessageBase>(this, true, OnPropertyChanged);
            Messenger.Default.Register<EventMessage>(this, true, OnEventMessage);
        }

        private void OnPropertyChanged(PropertyChangedMessageBase message)
        {
            if (message.PropertyName == "IsInPlayingMode")
            {
                IsInPlayingMode = _engine.GraphState != GraphState.Reset;
                UpdateMenu();
            }
        }

        private void OnEventMessage(EventMessage message)
        {
            if (message.Content == Event.MediaControlCreated)
            {
                _engine.ModifyMenu += (sender, args) => UpdateMenu();
            }
            else if (message.Content == Event.DispatcherTimerTick)
            {
                if (IsInPlayingMode)
                {
                    UpdateMenuCheckedStatus();
                }
            }
        }

        protected bool IsInPlayingMode { get; private set; }

        protected abstract void UpdateMenuCheckedStatus();
        protected abstract void UpdateMenu();
    }
}