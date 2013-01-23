using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;

namespace Pvp.App.Util
{
    /// <summary>
    /// This is a workaround class for a bug in ContextMenu when CommandParameter is not being passed to CanExecute handler.
    /// See:
    /// http://stackoverflow.com/questions/3027224/icommand-canexecute-being-passed-null-even-though-commandparameter-is-set
    /// https://connect.microsoft.com/VisualStudio/feedback/details/504976/command-canexecute-still-not-requeried-after-commandparameter-change
    /// </summary>
    public static class BoundCommand
    {
        public static object GetParameter(DependencyObject obj)
        {
            return obj.GetValue(ParameterProperty);
        }

        public static void SetParameter(DependencyObject obj, object value)
        {
            obj.SetValue(ParameterProperty, value);
        }

        public static readonly DependencyProperty ParameterProperty = 
            DependencyProperty.RegisterAttached("Parameter", 
                                                typeof(object), 
                                                typeof(BoundCommand), 
                                                new UIPropertyMetadata(null, ParameterChanged));

        private static void ParameterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var commandSource = d as ICommandSource;
            if (commandSource == null)
            {
                return;
            }

            if (commandSource is ButtonBase)
            {
                ((ButtonBase)commandSource).CommandParameter = e.NewValue;
            }
            else if (commandSource is MenuItem)
            {
                ((MenuItem)commandSource).CommandParameter = e.NewValue;
            }
            else
            {
                return;
            }
            
            var command = commandSource.Command as ICanExecuteChangedRaiser;
            if (command != null)
            {
                command.RaiseCanExecuteChanged();
            }
        }
    }
    
    /// <summary>
    /// This will help abstract away command implementation.
    /// When using MVVMLight we just need to derive from RelayCommand<T>, it already has a method with the same signature.
    /// </summary>
    public interface ICanExecuteChangedRaiser
    {
        void RaiseCanExecuteChanged();
    }
}
