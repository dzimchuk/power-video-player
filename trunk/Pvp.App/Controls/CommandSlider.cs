using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;

namespace Dzimchuk.Pvp.App.Controls
{
    public class CommandSlider : Slider, ICommandSource
    {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command",
                                        typeof(ICommand),
                                        typeof(CommandSlider),
                                        new PropertyMetadata((ICommand)null,
                                        new PropertyChangedCallback(CommandChanged)));

        private static void CommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CommandSlider control = (CommandSlider)d;
            control.HookUpCommand((ICommand)e.OldValue, (ICommand)e.NewValue);
        }

        public ICommand Command
        {
            get
            {
                return (ICommand)GetValue(CommandProperty);
            }
            set
            {
                SetValue(CommandProperty, value);
            }
        }

        public object CommandParameter
        {
            get { return null; }
        }

        public IInputElement CommandTarget
        {
            get { return null; }
        }

        private void CanExecuteChanged(object sender, EventArgs args)
        {
            var command = Command;
            if (command != null)
            {
                IsEnabled = command.CanExecute(CommandParameter);
            }
        }

        private void HookUpCommand(ICommand oldCommand, ICommand newCommand)
        {
            if (oldCommand != null)
            {
                oldCommand.CanExecuteChanged -= CanExecuteChanged;
            }

            if (newCommand != null)
            {
                newCommand.CanExecuteChanged += CanExecuteChanged;
            }
        }

        protected override void OnValueChanged(double oldValue, double newValue)
        {
            base.OnValueChanged(oldValue, newValue);

            var command = Command;
            if (command != null)
            {
                command.Execute(CommandParameter);
            }
        }
    }
}
