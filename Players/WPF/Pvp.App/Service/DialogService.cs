using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pvp.App.ViewModel;
using System.Windows;

namespace Pvp.App.Service
{
    internal class DialogService : IDialogService
    {
        public void DisplayMessage(string message)
        {
            MessageBox.Show(message, Resources.Resources.program_name, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void DisplayWarning(string message)
        {
            MessageBox.Show(message, Resources.Resources.program_name, MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }

        public void DisplayError(string message)
        {
            MessageBox.Show(message, Resources.Resources.program_name, MessageBoxButton.OK, MessageBoxImage.Stop);
        }

        public bool DisplayYesNoDialog(string message)
        {
            return MessageBox.Show(message, Resources.Resources.program_name, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
        }
    }
}
