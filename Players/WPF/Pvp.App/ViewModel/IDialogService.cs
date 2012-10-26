using System;
using System.Linq;

namespace Pvp.App.ViewModel
{
    public interface IDialogService
    {
        void DisplayMessage(string message);
        void DisplayWarning(string message);
        void DisplayError(string message);
        bool DisplayYesNoDialog(string message);

        void DisplaySettingsDialog();
        KeyCombination ShowEnterKeyWindow();
        void DisplayMediaInformationWindow();
        void DisplayFailedStreamsWindow();
        void DisplayAboutAppWindow();
    }
}