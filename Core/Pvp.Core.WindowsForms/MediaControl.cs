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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pvp.Core.MediaEngine.GraphBuilders;
using Pvp.Core.MediaEngine;
using Pvp.Core.MediaEngine.Description;
using Pvp.Core.DirectShow;

namespace Pvp.Core.WindowsForms
{
    /// <summary>
    /// A control that plays video and DVD through DirectShow.
    /// </summary>
    public class MediaControl : MediaWindowHost
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
        public event FailedStreamsHandler FailedStreamsAvailable
        {
            add { MediaEngine.FailedStreamsAvailable += value; }
            remove { MediaEngine.FailedStreamsAvailable -= value; }
        }

        /// <summary>
        /// Something bad just happened.
        /// Indicates that a user should be notified. The message is localized.
        /// </summary>
        public event EventHandler<ErrorOccuredEventArgs> ErrorOccured
        {
            add { MediaEngine.ErrorOccured += value; }
            remove { MediaEngine.ErrorOccured -= value; }
        }

        /// <summary>
        /// Indicates that an application's (context) menu may need to be updated.
        /// Occurs when playing DVD's.
        /// </summary>
        public event EventHandler ModifyMenu
        {
            add { MediaEngine.ModifyMenu += value; }
            remove { MediaEngine.ModifyMenu -= value; }
        }

        /// <summary>
        /// Occurs when a DVD playback enters another parental zone (level).
        /// The new level is available as part of the message (UserDecisionEventArgs.Message).
        /// 
        /// An application (user) should decide whether to continue or not by setting
        /// UserDecisionEventArgs.Accept.
        /// </summary>
        public event EventHandler<UserDecisionEventArgs> DvdParentalChange
        {
            add { MediaEngine.DvdParentalChange += value; }
            remove { MediaEngine.DvdParentalChange -= value; }
        }

        /// <summary>
        /// Occurs when DVD rendering has finished but not all streams have been rendered 
        /// (partial success).
        /// Details are available in UserDecisionEventArgs.Message.
        /// 
        /// An application (user) should decide whether to continue or not by setting
        /// UserDecisionEventArgs.Accept.
        /// </summary>
        public event EventHandler<UserDecisionEventArgs> PartialSuccess
        {
            add { MediaEngine.PartialSuccess += value; }
            remove { MediaEngine.PartialSuccess -= value; }
        }

        /// <summary>
        /// Occurs when something has changed in the engine's state that an application may be
        /// interested in. No details are provided but it's good to update your user control
        /// states to correspond to the actual state of the engine.
        /// </summary>
        public event EventHandler UpdateSuggested
        {
            add { MediaEngine.UpdateSuggested += value; }
            remove { MediaEngine.UpdateSuggested -= value; }
        }

        #endregion

        #region General properties (preferences)

        public bool AutoPlay 
        {
            get { return MediaEngine.AutoPlay; }
            set { MediaEngine.AutoPlay = value; } 
        }

        public bool Repeat
        {
            get { return MediaEngine.Repeat; }
            set { MediaEngine.Repeat = value; }
        }

        public Renderer PreferredVideoRenderer
        {
            get { return MediaEngine.PreferredVideoRenderer; }
            set { MediaEngine.PreferredVideoRenderer = value; }
        }

        #endregion

        #region Playback properties and methods

        public int AudioStreams 
        {
            get { return MediaEngine.AudioStreams; }
        }

        public int CurrentAudioStream 
        {
            get { return MediaEngine.CurrentAudioStream; }
            set { MediaEngine.CurrentAudioStream = value; }
        }

        public int FilterCount 
        {
            get { return MediaEngine.FilterCount; }
        }

        public GraphState GraphState 
        {
            get { return MediaEngine.GraphState; }
        }

        public bool IsGraphSeekable 
        {
            get { return MediaEngine.IsGraphSeekable; }
        }

        public bool IsEVRCurrentlyInUse 
        {
            get { return MediaEngine.IsEVRCurrentlyInUse; }
        }

        public MediaInfo MediaInfo 
        {
            get { return MediaEngine.MediaInfo; }
        }

        public SourceType SourceType 
        {
            get { return MediaEngine.SourceType; } 
        }

        public string GetAudioStreamName(int nStream)
        {
            return MediaEngine.GetAudioStreamName(nStream);
        }

        public string GetFilterName(int nFilterNum)
        {
            return MediaEngine.GetFilterName(nFilterNum);
        }

        public double Rate
        {
            get { return MediaEngine.Rate; }
            set { MediaEngine.Rate = value; }
        }

        [Obsolete("Use Volume property instead.")]
        public bool GetVolume(out int volume)
        {
            return MediaEngine.GetVolume(out volume);
        }

        [Obsolete("Use Volume property instead.")]
        public bool SetVolume(int volume)
        {
            return MediaEngine.SetVolume(volume);
        }

        [Obsolete("Use CurrentPosition property instead.")]
        public void SetCurrentPosition(long time)
        {
            MediaEngine.SetCurrentPosition(time);
        }

        [Obsolete("Use CurrentPosition property instead.")]
        public long GetCurrentPosition()
        {
            return MediaEngine.GetCurrentPosition();
        }

        [Obsolete("Use Duration property instead.")]
        public long GetDuration()
        {
            return MediaEngine.GetDuration();
        }

        public double Volume 
        {
            get { return MediaEngine.Volume; }
            set { MediaEngine.Volume = value; } 
        }

        public bool IsMuted 
        {
            get { return MediaEngine.IsMuted; }
            set { MediaEngine.IsMuted = value; }
        }

        public TimeSpan CurrentPosition 
        {
            get { return MediaEngine.CurrentPosition; }
            set { MediaEngine.CurrentPosition = value; }
        }

        public TimeSpan Duration 
        {
            get { return MediaEngine.Duration; }
        }

        /// <summary>
        /// Gets a snapshot of the current image that is being shown.
        /// It is recommended to wrap the call to this method in a try/catch block.
        /// A pointer provided to IImageCreator.CreateImage method will be released upon return
        /// so a caller should make sure there is no dependency on the pointer.
        /// </summary>
        /// <param name="imageCreator">A platform/technology specific image creator.</param>
        public void GetCurrentImage(IImageCreator imageCreator)
        {
            MediaEngine.GetCurrentImage(imageCreator);
        }

        #endregion

        #region Playback control methods

        public bool BuildGraph(string source, MediaSourceType mediaSourceType)
        {
            return MediaEngine.BuildGraph(source, mediaSourceType);
        }

        public bool PauseGraph()
        {
            return MediaEngine.PauseGraph();
        }

        public void ResetGraph()
        {
            MediaEngine.ResetGraph();
        }

        public bool ResumeGraph()
        {
            return MediaEngine.ResumeGraph();
        }

        public bool StopGraph()
        {
            return MediaEngine.StopGraph();
        }

        #endregion

        #region DVD specific properties and methods

        public int AnglesAvailable 
        {
            get { return MediaEngine.AnglesAvailable; } 
        }

        public int CurrentAngle 
        {
            get { return MediaEngine.CurrentAngle; }
            set { MediaEngine.CurrentAngle = value; }
        }

        public int CurrentChapter 
        {
            get { return MediaEngine.CurrentChapter; }
        }

        public int CurrentSubpictureStream 
        {
            get { return MediaEngine.CurrentSubpictureStream; }
            set { MediaEngine.CurrentSubpictureStream = value; }
        }

        public int CurrentTitle 
        {
            get { return MediaEngine.CurrentTitle; }
        }

        public bool IsMenuOn 
        {
            get { return MediaEngine.IsMenuOn; }
        }

        public int MenuLangCount 
        {
            get { return MediaEngine.MenuLangCount; }
        }

        public int NumberOfSubpictureStreams 
        {
            get { return MediaEngine.NumberOfSubpictureStreams; }
        }

        public int NumberOfTitles 
        {
            get { return MediaEngine.NumberOfTitles; }
        }

        public VALID_UOP_FLAG UOPS 
        {
            get { return MediaEngine.UOPS; }
        }

        public bool EnableSubpicture(bool bEnable)
        {
            return MediaEngine.EnableSubpicture(bEnable);
        }

        public bool GetCurrentDomain(out DVD_DOMAIN pDomain)
        {
            return MediaEngine.GetCurrentDomain(out pDomain);
        }

        public string GetMenuLangName(int nLang)
        {
            return MediaEngine.GetMenuLangName(nLang);
        }

        public int GetNumChapters(int ulTitle)
        {
            return MediaEngine.GetNumChapters(ulTitle);
        }

        public string GetSubpictureStreamName(int nStream)
        {
            return MediaEngine.GetSubpictureStreamName(nStream);
        }

        public void GoTo(int ulTitle, int ulChapter)
        {
            MediaEngine.GoTo(ulTitle, ulChapter);
        }

        public bool IsAudioStreamEnabled(int ulStreamNum)
        {
            return MediaEngine.IsAudioStreamEnabled(ulStreamNum);
        }

        public bool IsResumeDVDEnabled()
        {
            return MediaEngine.IsResumeDVDEnabled();
        }

        public bool IsSubpictureEnabled()
        {
            return MediaEngine.IsSubpictureEnabled();
        }

        public bool IsSubpictureStreamEnabled(int ulStreamNum)
        {
            return MediaEngine.IsSubpictureStreamEnabled(ulStreamNum);
        }

        public bool ResumeDVD()
        {
            return MediaEngine.ResumeDVD();
        }

        public void ReturnFromSubmenu()
        {
            MediaEngine.ReturnFromSubmenu();
        }

        public void SetMenuLang(int nLang)
        {
            MediaEngine.SetMenuLang(nLang);
        }

        public void ShowMenu(DVD_MENU_ID menuID)
        {
            MediaEngine.ShowMenu(menuID);
        }

        #endregion

        #region Others

        public bool DisplayFilterPropPage(IntPtr hParent, int nFilterNum, bool bDisplay)
        {
            return MediaEngine.DisplayFilterPropPage(hParent, nFilterNum, bDisplay);
        }

        public bool DisplayFilterPropPage(IntPtr hParent, string strFilter, bool bDisplay)
        {
            return MediaEngine.DisplayFilterPropPage(hParent, strFilter, bDisplay);
        }

        public void OnCultureChanged()
        {
            MediaEngine.OnCultureChanged();
        }

        #endregion
    }
}
