using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace Dzimchuk.Pvp.App.Controls
{
    [TemplatePart(Name = "PART_Track", Type = typeof(Track))]
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
            get 
            {
                var max = Maximum;
                if (max != 0.0)
                {
                    return Value / max;
                }
                return 0.0; 
            }
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

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var track = Template.FindName("PART_Track", this) as Track;
            if (track != null)
            {
                track.Thumb.MouseEnter += new MouseEventHandler(thumb_MouseEnter);
            }
        }

        private void thumb_MouseEnter(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && e.MouseDevice.Captured == null)
            {
                // the left button is pressed on mouse enter
                // but the mouse isn't captured, so the thumb
                // must have been moved under the mouse in response
                // to a click on the track.
                //
                // Generate a MouseLeftButtonDown event.
                
                MouseButtonEventArgs args = new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, MouseButton.Left);

                args.RoutedEvent = MouseLeftButtonDownEvent;

                (sender as Thumb).RaiseEvent(args);
            }
        }
    }
}
