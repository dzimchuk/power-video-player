/* ****************************************************************************
 *
 * Copyright (c) Andrei Dzimchuk. All rights reserved.
 *
 * This software is subject to the Microsoft Public License (Ms-PL). 
 * A copy of the license can be found in the license.htm file included 
 * in this distribution.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using Pvp.Core.DirectShow;
using Pvp.Core.MediaEngine.Description;
using Pvp.Core.MediaEngine.Internal;
using Pvp.Core.Native;

namespace Pvp.Core.MediaEngine
{
    /// <summary>
    /// Specifies a public contract of the core media engine.
    /// </summary>
    public interface IMediaEngine
    {
        #region Events

        /// <summary>
        /// Indicates that there is at least one unrendered stream.
        /// Details on failed streams are provided as a list of StreamInfo objects.
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
        /// Occurs when new video has been rendered by BuildGraph and indicates that
        /// the application should resize to accomodate the new video frame. In this case
        /// CoreInitSizeEventArgs.Initial will be true.
        /// 
        /// This event is also raised when entering a new DVD domain and it indicates that 
        /// the video size and aspect ratio might have changes. In this case CoreInitSizeEventArgs.Initial
        /// will be false.
        /// </summary>
        event EventHandler<CoreInitSizeEventArgs> InitSize;

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

        /// <summary>
        /// Occures when the engine disposes of the media window, that is, it should no longer be used.
        /// This event is raised when the graph is reset.
        /// 
        /// Application might be interested in this event if it wants to display something like a logo inside a
        /// media window host's frame when the video is not rendered.
        /// </summary>
        event EventHandler MediaWindowDisposed;

        #endregion
        

        #region General properties (preferences)

        bool     AutoPlay { get; set; }
        bool     Repeat { get; set; }
        Renderer PreferredVideoRenderer { get; set; }

        #endregion


        #region Playback properties and methods

        int         AudioStreamsCount { get; }
        int         CurrentAudioStream { get; set; }
        int         FilterCount { get; }
        GraphState  GraphState { get; }
        bool        IsGraphSeekable { get; }
        MediaInfo   MediaInfo { get; }
        SourceType  SourceType { get; }

        string GetAudioStreamName(int nStream);
        string GetFilterName(int nFilterNum);

        [Obsolete("Use Rate property instead.")]
        double GetRate();
        [Obsolete("Use Rate property instead.")]
        void SetRate(double dRate);

        [Obsolete("Use Volume property instead.")]
        bool   GetVolume(out int volume);
        [Obsolete("Use Volume property instead.")]
        bool SetVolume(int volume);
        [Obsolete("Use CurrentPosition property instead.")]
        void   SetCurrentPosition(long time);
        [Obsolete("Use CurrentPosition property instead.")]
        long GetCurrentPosition();
        [Obsolete("Use Duration property instead.")]
        long GetDuration();

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
                        
        bool   EnableSubpicture(bool bEnable);
        bool   GetCurrentDomain(out DVD_DOMAIN pDomain);
        string GetMenuLangName(int nLang);
        int    GetNumChapters(int ulTitle);
        string GetSubpictureStreamName(int nStream);
        void   GoTo(int ulTitle, int ulChapter);
        bool   IsAudioStreamEnabled(int ulStreamNum);
        bool   IsResumeDvdEnabled();
        bool   IsSubpictureEnabled();
        bool   IsSubpictureStreamEnabled(int ulStreamNum);
        bool   ResumeDvd();
        void   ReturnFromSubmenu();
        void   SetMenuLang(int nLang);
        void   ShowMenu(DVD_MENU_ID menuId);

        void ActivateSelectedDvdMenuButton();
        void SelectDvdMenuButtonUp();
        void SelectDvdMenuButtonDown();
        void SelectDvdMenuButtonLeft();
        void SelectDvdMenuButtonRight();
        /// <summary>
        /// Activates the menu button under the mouse pointer position (if there is a button).
        /// Call it when DVD menu is on (check IsMenuOn) and the user clicks anywhere on the video area.
        /// </summary>
        /// <param name="point">Point on the client window area (that is, relative to the upper left of the client area).</param>
        void ActivateDvdMenuButtonAtPosition(GDI.POINT point);
        /// <summary>
        /// Highlights the menu button under the mouse pointer position (if there is a button).
        /// Call it when DVD menu is on (check IsMenuOn) and the user moves the mouse over the video area.
        /// </summary>
        /// <param name="point">Point on the client window area (that is, relative to the upper left of the client area).</param>
        void SelectDvdMenuButtonAtPosition(GDI.POINT point);

        #endregion


        #region Others

        bool DisplayFilterPropPage(IntPtr hParent, int nFilterNum, bool bDisplay);
        bool DisplayFilterPropPage(IntPtr hParent, string strFilter, bool bDisplay);

        /// <summary>
        /// Sets a destination video rectangle relative to media window.
        /// Applications should call this method when the media window is resized.
        /// </summary>
        /// <param name="rcDest">Destination video rectangle relative to media window.</param>
        void SetVideoPosition(GDI.RECT rcDest);

        #endregion
    }
}
