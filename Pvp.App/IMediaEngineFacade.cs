using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pvp.Core.MediaEngine.GraphBuilders;
using Pvp.Core.MediaEngine;
using Pvp.Core.MediaEngine.Description;
using Pvp.Core.DirectShow;

namespace Pvp.App
{
    public interface IMediaEngineFacade
    {
        #region Events

        /// <summary>
        /// Indicates that there is at least one unrendered stream.
        /// Details on failed streams are provided as a list of StreamInfo objects.
        /// 
        /// Caution: the event is raised on a thread pool thread,
        /// application is responsible to synchronize with the UI thread if needed.
        /// 
        /// Raising it on another thread is needed so that BuildGraph could continue.
        /// </summary>
        event FailedStreamsHandler FailedStreamsAvailable;

        /// <summary>
        /// Something bad just happened.
        /// Indicates that a user should be notified. The message is localized.
        /// </summary>
        event EventHandler<ErrorOccuredEventArgs> ErrorOccured;

        /// <summary>
        /// Indicates that an application's (context) menu may need to be updated.
        /// Occurs when playing DVD's.
        /// </summary>
        event EventHandler ModifyMenu;

        /// <summary>
        /// Occurs when a DVD playback enters another parental zone (level).
        /// The new level is available as part of the message (UserDecisionEventArgs.Message).
        /// 
        /// An application (user) should decide whether to continue or not by setting
        /// UserDecisionEventArgs.Accept.
        /// </summary>
        event EventHandler<UserDecisionEventArgs> DvdParentalChange;

        /// <summary>
        /// Occurs when DVD rendering has finished but not all streams have been rendered 
        /// (partial success).
        /// Details are available in UserDecisionEventArgs.Message.
        /// 
        /// An application (user) should decide whether to continue or not by setting
        /// UserDecisionEventArgs.Accept.
        /// </summary>
        event EventHandler<UserDecisionEventArgs> PartialSuccess;

        /// <summary>
        /// Occurs when something has changed in the engine's state that an application may be
        /// interested in. No details are provided but it's good to update your user control
        /// states to correspond to the actual state of the engine.
        /// </summary>
        event EventHandler UpdateSuggested;

        #endregion


        #region General properties (preferences)

        bool AutoPlay { get; set; }
        bool Repeat { get; set; }
        Renderer PreferredVideoRenderer { get; set; }

        #endregion


        #region Playback properties and methods

        int AudioStreams { get; }
        int CurrentAudioStream { get; set; }
        int FilterCount { get; }
        GraphState GraphState { get; }
        bool IsGraphSeekable { get; }
        bool IsEVRCurrentlyInUse { get; }
        MediaInfo MediaInfo { get; }
        SourceType SourceType { get; }

        string GetAudioStreamName(int nStream);
        string GetFilterName(int nFilterNum);

        double Rate { get; set; }

        double Volume { get; set; }
        bool IsMuted { get; set; }

        TimeSpan CurrentPosition { get; set; }
        TimeSpan Duration { get; }

        /// <summary>
        /// Gets a snapshot of the current image that is being shown.
        /// It is recommended to wrap the call to this method in a try/catch block.
        /// A pointer provided to IImageCreator.CreateImage method will be released upon return
        /// so a caller should make sure there is no dependency on the pointer.
        /// </summary>
        /// <param name="imageCreator">A platform/technology specific image creator.</param>
        void GetCurrentImage(IImageCreator imageCreator);

        #endregion


        #region Playback control methods

        bool BuildGraph(string source, MediaSourceType mediaSourceType);
        bool PauseGraph();
        void ResetGraph();
        bool ResumeGraph();
        bool StopGraph();

        #endregion


        #region DVD specific properties and methods

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
        void GoTo(int ulTitle, int ulChapter);
        bool IsAudioStreamEnabled(int ulStreamNum);
        bool IsResumeDVDEnabled();
        bool IsSubpictureEnabled();
        bool IsSubpictureStreamEnabled(int ulStreamNum);
        bool ResumeDVD();
        void ReturnFromSubmenu();
        void SetMenuLang(int nLang);
        void ShowMenu(DVD_MENU_ID menuID);

        #endregion


        #region Others

        bool DisplayFilterPropPage(IntPtr hParent, int nFilterNum, bool bDisplay);
        bool DisplayFilterPropPage(IntPtr hParent, string strFilter, bool bDisplay);
        void OnCultureChanged();

        #endregion
    }
}
