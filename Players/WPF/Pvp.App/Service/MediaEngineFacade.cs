using System;
using System.Linq;
using Pvp.Core.MediaEngine;
using Pvp.Core.Wpf;
using Pvp.Core.MediaEngine.Description;
using Pvp.Core.DirectShow;

namespace Pvp.App.Service
{
    internal class MediaEngineFacade : IMediaEngineFacade, IMediaControlAcceptor
    {
        private static readonly MediaEngineFacade _instance;

        static MediaEngineFacade()
        {
            _instance = new MediaEngineFacade();
        }

        public static MediaEngineFacade Instance
        {
            get { return _instance; }
        }

        private MediaControl _mediaControl;

        MediaControl IMediaControlAcceptor.MediaControl
        {
            set { _mediaControl = value; }
        }

        #region Events

        public event FailedStreamsHandler FailedStreamsAvailable
        {
            add { _mediaControl.FailedStreamsAvailable += value; }
            remove { _mediaControl.FailedStreamsAvailable -= value; }
        }

        public event EventHandler<ErrorOccuredEventArgs> ErrorOccured
        {
            add { _mediaControl.ErrorOccured += value; }
            remove { _mediaControl.ErrorOccured -= value; }
        }

        public event EventHandler ModifyMenu
        {
            add { _mediaControl.ModifyMenu += value; }
            remove { _mediaControl.ModifyMenu -= value; }
        }

        public event EventHandler<UserDecisionEventArgs> DvdParentalChange
        {
            add { _mediaControl.DvdParentalChange += value; }
            remove { _mediaControl.DvdParentalChange -= value; }
        }

        public event EventHandler<UserDecisionEventArgs> PartialSuccess
        {
            add { _mediaControl.PartialSuccess += value; }
            remove { _mediaControl.PartialSuccess -= value; }
        }

        public event EventHandler UpdateSuggested
        {
            add { _mediaControl.UpdateSuggested += value; }
            remove { _mediaControl.UpdateSuggested -= value; }
        }

        #endregion

        #region General properties (preferences)

        public bool AutoPlay
        {
            get { return _mediaControl.AutoPlay; }
            set { _mediaControl.AutoPlay = value; }
        }

        public bool Repeat
        {
            get { return _mediaControl.Repeat; }
            set { _mediaControl.Repeat = value; }
        }

        public Renderer PreferredVideoRenderer
        {
            get { return _mediaControl.PreferredVideoRenderer; }
            set { _mediaControl.PreferredVideoRenderer = value; }
        }

        public VideoSize VideoSize
        {
            get { return _mediaControl.VideoSize; }
            set { _mediaControl.VideoSize = value; }
        }

        public AspectRatio AspectRatio
        {
            get { return _mediaControl.AspectRatio; }
            set { _mediaControl.AspectRatio = value; }
        }

        #endregion

        #region Playback properties and methods

        public int AudioStreams
        {
            get { return _mediaControl.AudioStreams; }
        }

        public int CurrentAudioStream
        {
            get { return _mediaControl.CurrentAudioStream; }
            set { _mediaControl.CurrentAudioStream = value; }
        }

        public int FilterCount
        {
            get { return _mediaControl.FilterCount; }
        }

        public GraphState GraphState
        {
            get { return _mediaControl.GraphState; }
        }

        public bool IsGraphSeekable
        {
            get { return _mediaControl.IsGraphSeekable; }
        }

        public MediaInfo MediaInfo
        {
            get { return _mediaControl.MediaInfo; }
        }

        public SourceType SourceType
        {
            get { return _mediaControl.SourceType; }
        }

        public string GetAudioStreamName(int nStream)
        {
            return _mediaControl.GetAudioStreamName(nStream);
        }

        public string GetFilterName(int nFilterNum)
        {
            return _mediaControl.GetFilterName(nFilterNum);
        }

        public double Rate
        {
            get { return _mediaControl.Rate; }
            set { _mediaControl.Rate = value; }
        }

        public double Volume
        {
            get { return _mediaControl.Volume; }
            set { _mediaControl.Volume = value; }
        }

        public bool IsMuted
        {
            get { return _mediaControl.IsMuted; }
            set { _mediaControl.IsMuted = value; }
        }

        public TimeSpan CurrentPosition
        {
            get { return _mediaControl.CurrentPosition; }
            set { _mediaControl.CurrentPosition = value; }
        }

        public TimeSpan Duration
        {
            get { return _mediaControl.Duration; }
        }

        public void GetCurrentImage(IImageCreator imageCreator)
        {
            _mediaControl.GetCurrentImage(imageCreator);
        }

        #endregion

        #region Playback control methods

        public bool BuildGraph(string source, MediaSourceType mediaSourceType)
        {
            return _mediaControl.BuildGraph(source, mediaSourceType);
        }

        public bool PauseGraph()
        {
            return _mediaControl.PauseGraph();
        }

        public void ResetGraph()
        {
            _mediaControl.ResetGraph();
        }

        public bool ResumeGraph()
        {
            return _mediaControl.ResumeGraph();
        }

        public bool StopGraph()
        {
            return _mediaControl.StopGraph();
        }

        #endregion

        #region DVD specific properties and methods

        public int AnglesAvailable
        {
            get { return _mediaControl.AnglesAvailable; }
        }

        public int CurrentAngle
        {
            get { return _mediaControl.CurrentAngle; }
            set { _mediaControl.CurrentAngle = value; }
        }

        public int CurrentChapter
        {
            get { return _mediaControl.CurrentChapter; }
        }

        public int CurrentSubpictureStream
        {
            get { return _mediaControl.CurrentSubpictureStream; }
            set { _mediaControl.CurrentSubpictureStream = value; }
        }

        public int CurrentTitle
        {
            get { return _mediaControl.CurrentTitle; }
        }

        public bool IsMenuOn
        {
            get { return _mediaControl.IsMenuOn; }
        }

        public int MenuLangCount
        {
            get { return _mediaControl.MenuLangCount; }
        }

        public int NumberOfSubpictureStreams
        {
            get { return _mediaControl.NumberOfSubpictureStreams; }
        }

        public int NumberOfTitles
        {
            get { return _mediaControl.NumberOfTitles; }
        }

        public VALID_UOP_FLAG UOPS
        {
            get { return _mediaControl.UOPS; }
        }

        public bool EnableSubpicture(bool bEnable)
        {
            return _mediaControl.EnableSubpicture(bEnable);
        }

        public bool GetCurrentDomain(out DVD_DOMAIN pDomain)
        {
            return _mediaControl.GetCurrentDomain(out pDomain);
        }

        public string GetMenuLangName(int nLang)
        {
            return _mediaControl.GetMenuLangName(nLang);
        }

        public int GetNumChapters(int ulTitle)
        {
            return _mediaControl.GetNumChapters(ulTitle);
        }

        public string GetSubpictureStreamName(int nStream)
        {
            return _mediaControl.GetSubpictureStreamName(nStream);
        }

        public void GoTo(int ulTitle, int ulChapter)
        {
            _mediaControl.GoTo(ulTitle, ulChapter);
        }

        public bool IsAudioStreamEnabled(int ulStreamNum)
        {
            return _mediaControl.IsAudioStreamEnabled(ulStreamNum);
        }

        public bool IsResumeDVDEnabled()
        {
            return _mediaControl.IsResumeDVDEnabled();
        }

        public bool IsSubpictureEnabled()
        {
            return _mediaControl.IsSubpictureEnabled();
        }

        public bool IsSubpictureStreamEnabled(int ulStreamNum)
        {
            return _mediaControl.IsSubpictureStreamEnabled(ulStreamNum);
        }

        public bool ResumeDVD()
        {
            return _mediaControl.ResumeDVD();
        }

        public void ReturnFromSubmenu()
        {
            _mediaControl.ReturnFromSubmenu();
        }

        public void SetMenuLang(int nLang)
        {
            _mediaControl.SetMenuLang(nLang);
        }

        public void ShowMenu(DVD_MENU_ID menuID)
        {
            _mediaControl.ShowMenu(menuID);
        }

        #endregion

        #region Others

        public bool DisplayFilterPropPage(IntPtr hParent, int nFilterNum, bool bDisplay)
        {
            return _mediaControl.DisplayFilterPropPage(hParent, nFilterNum, bDisplay);
        }

        public bool DisplayFilterPropPage(IntPtr hParent, string strFilter, bool bDisplay)
        {
            return _mediaControl.DisplayFilterPropPage(hParent, strFilter, bDisplay);
        }

        #endregion
    }
}
