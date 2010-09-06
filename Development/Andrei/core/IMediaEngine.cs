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
using Dzimchuk.DirectShow;
using Dzimchuk.MediaEngine.Core.Description;
using Dzimchuk.MediaEngine.Core.GraphBuilders;

namespace Dzimchuk.MediaEngine.Core
{
    public interface IMediaEngine
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
        /// Occurs when new video has been rendered by BuildGraph and indicates that
        /// the application should resize to accomodate the new video frame.
        /// </summary>
        event EventHandler<InitSizeEventArgs> InitSize;

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
        /// Occurs when the MediaEngine has completed recalculating the new 
        /// position and size of the video destination rectangle (i.e. the Media Window).
        /// 
        /// It occurs whenever ResizeNormal is called, for example, video size or 
        /// aspect ratio is changed, or when the media window host was resized (in the 
        /// latter case the application should notify the engine by calling OnMediaWindowHostResized).
        /// 
        /// The listener is responsible to draw the border around the destination
        /// rectangle (Media Window). The new destination rectangle's postion and
        /// size is provided with EventArgs.
        /// 
        /// The destination rectangle itself will be painted by DirectShow renderers.
        /// </summary>
        event EventHandler<DestinationRectangleChangedEventArgs> DestinationRectangleChanged;

        /// <summary>
        /// Occurs when something has changed in the engine's state that an application may be
        /// interested in. No details are provided but it's good to update your user control
        /// states to correspond to the actual state of the engine.
        /// </summary>
        event EventHandler Update;

        #endregion

        
        #region General properties (preferences)

        bool     AutoPlay { get; set; }
        bool     Repeat { get; set; }
        Renderer PreferredVideoRenderer { get; set; }
        bool     UsePreferredFilters { get; set; }
        bool     UsePreferredFilters4DVD { get; set; }

        #endregion


        #region Playback properties and methods

        AspectRatio AspectRatio { get; set; }
        int         AudioStreams { get; }
        int         CurrentAudioStream { get; set; }
        int         FilterCount { get; }
        GraphState  GraphState { get; }
        bool        IsGraphSeekable { get; }
        bool        IsEVRCurrentlyInUse { get; }
        MediaInfo   MediaInfo { get; }
        SourceType  SourceType { get; }

        string GetAudioStreamName(int nStream);
        long   GetCurrentPosition();
        long   GetDuration();
        string GetFilterName(int nFilterNum);
        double GetRate();
        bool   GetVolume(out int volume);
        void   SetCurrentPosition(long time);
        void   SetRate(double dRate);
        bool   SetVolume(int volume);

        VideoSize GetVideoSize();
        void SetVideoSize(VideoSize size);
        void SetVideoSize(VideoSize size, bool bInitSize);

        #endregion


        #region Playback control methods

        bool BuildGraph(IMediaWindow mediaWindow, string source, WhatToPlay CurrentlyPlaying);
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
        bool   IsResumeDVDEnabled();
        bool   IsSubpictureEnabled();
        bool   IsSubpictureStreamEnabled(int ulStreamNum);
        bool   ResumeDVD();
        void   ReturnFromSubmenu();
        void   SetMenuLang(int nLang);
        void   ShowMenu(DVD_MENU_ID menuID);

        void ActivateSelectedDVDMenuButton();
        void SelectDVDMenuButtonUp();
        void SelectDVDMenuButtonDown();
        void SelectDVDMenuButtonLeft();
        void SelectDVDMenuButtonRight();

        #endregion


        #region Others

        bool DisplayFilterPropPage(IntPtr hParent, int nFilterNum, bool bDisplay);
        bool DisplayFilterPropPage(IntPtr hParent, string strFilter, bool bDisplay);
        void OnCultureChanged();

        /// <summary>
        /// Application should call this method whenever the media window host window
        /// has been resized so that the engine could perform necessary recalculations.
        /// </summary>
        void OnMediaWindowHostResized();

        #endregion
    }
}
