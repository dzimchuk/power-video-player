using System;
using Pvp.Core.DirectShow;
using Pvp.Core.Native;

namespace Pvp.Core.MediaEngine
{
    internal interface IDvdFilterGraph
    {
        int AnglesAvailable { get; }
        int CurrentAngle { get; set; }
        int CurrentChapter { get; }
        int CurrentSubpictureStream { get; set; }
        int CurrentTitle { get; }
        bool IsMenuOn { get; }
        int MenuLangCount { get; }
        int NumberOfSubpictureStreams { get; }
        int NumberOfTitles { get; }
        VALID_UOP_FLAG UOPS { get; }
        bool EnableSubpicture(bool bEnable);
        bool GetCurrentDomain(out DVD_DOMAIN pDomain);
        string GetMenuLangName(int nLang);
        int GetNumChapters(int ulTitle);
        string GetSubpictureStreamName(int nStream);
        bool GoTo(int ulTitle, int ulChapter);
        bool IsAudioStreamEnabled(int ulStreamNum);
        bool IsResumeDvdEnabled();
        bool IsSubpictureEnabled();
        bool IsSubpictureStreamEnabled(int ulStreamNum);
        bool ResumeDvd();
        void ReturnFromSubmenu();
        void SetMenuLang(int nLang);
        void ShowMenu(DVD_MENU_ID menuId);
        void ActivateSelectedDvdMenuButton();
        void SelectDvdMenuButton(DVD_RELATIVE_BUTTON relativeButton);
        void ActivateDvdMenuButtonAtPosition(GDI.POINT point);
        void SelectDvdMenuButtonAtPosition(GDI.POINT point);
        event EventHandler ModifyMenu;
        event EventHandler DiscEjected;
        event EventHandler InitSize;
        event EventHandler<UserDecisionEventArgs> DvdParentalChange;
    }
}