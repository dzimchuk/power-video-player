using System;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Pvp.App.Messaging;

namespace Pvp.App.ViewModel.Settings
{
    internal class EnterKeyViewModel : ViewModelBase
    {
        private readonly IKeyInterpreter _keyInterpreter;

        private ICommand _okCommand;
        private ICommand _keyDownCommand;

        private KeyCombination _selectedKeyCombination;

        public EnterKeyViewModel(IKeyInterpreter keyInterpreter)
        {
            _keyInterpreter = keyInterpreter;
            _selectedKeyCombination = new KeyCombination();
        }

        public KeyCombination SelectedKeyCombination
        {
            get { return _selectedKeyCombination; }
            set
            {
                if (Equals(value, _selectedKeyCombination)) return;
                _selectedKeyCombination = value;
                RaisePropertyChanged("SelectedKeyCombination");
            }
        }

        public ICommand OkCommand
        {
            get
            {
                if (_okCommand == null)
                {
                    _okCommand = new RelayCommand(() => Messenger.Default.Send(new CommandMessage(Command.EnterKeyWindowClose)));
                }

                return _okCommand;
            }
        }

        public ICommand KeyDownCommand
        {
            get
            {
                if (_keyDownCommand == null)
                {
                    _keyDownCommand = new RelayCommand<EventArgs>(args =>
                                                                      {
                                                                          var keyCombination = _keyInterpreter.Interpret(args);
                                                                          if (keyCombination != null)
                                                                          {
                                                                              SelectedKeyCombination = keyCombination;
                                                                          }
                                                                      });
                }

                return _keyDownCommand;
            }
        }
    }
}