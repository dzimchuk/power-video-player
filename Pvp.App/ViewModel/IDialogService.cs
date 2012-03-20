using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pvp.App.ViewModel
{
    public interface IDialogService
    {
        void DisplayMessage(string message);
        void DisplayWarning(string message);
        void DisplayError(string message);
        bool DisplayYesNoDialog(string message);
    }
}
