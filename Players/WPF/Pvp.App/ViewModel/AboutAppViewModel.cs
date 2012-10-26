using System;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Pvp.App.Messaging;

namespace Pvp.App.ViewModel
{
    public class AboutAppViewModel : ViewModelBase
    {
        private ICommand _okCommand;

        public ICommand OkCommand
        {
            get
            {
                if (_okCommand == null)
                {
                    _okCommand = new RelayCommand(
                        () =>
                        {
                            Messenger.Default.Send<CommandMessage>(new CommandMessage(Command.AboutAppWindowClose));
                        });
                }

                return _okCommand;
            }
        }
    }
}