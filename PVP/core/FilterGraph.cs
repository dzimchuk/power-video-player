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
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Dzimchuk.Native;
using Dzimchuk.DirectShow;
using Dzimchuk.MediaEngine.Core.Render;

namespace Dzimchuk.MediaEngine.Core
{
    public enum Renderer
    {
        VR,
        VMR_Windowless,
        VMR_Windowed,
        VMR9_Windowless,
        VMR9_Windowed,
        EVR
    }

    public enum WhatToPlay
    {
        PLAYING_FILE,
        PLAYING_DVD
    }

    public enum GraphState
    {
        Running,
        Paused,
        Stopped,
        Reset
    }

    public enum SourceType
    {
        Unknown,
        Basic,		// avi, mpeg...
        Asf,		// asf, wmv, wma
        DVD,		// DVD disc
        Mkv,        // matroska
        Flv
    }

    public enum AspectRatio
    {
        AR_ORIGINAL,
        AR_16x9,
        AR_4x3,
        AR_47x20,
        AR_FREE
    }

    internal enum Error
    {
        Unknown,
        FilterGraphManager,
        SourceFilter,
        NecessaryInterfaces,
        VideoRenderer,
        AddVideoRenderer,
        AddVMR9,
        ConfigureVMR9,
        AddVMR,
        ConfigureVMR,
        CantPlayFile,
        CantRenderFile,
        DirectSoundFilter,
        AddDirectSoundFilter,
        DvdGraphBuilder, 
        CantPlayDisc,
        NoVideoDimension,
        AddEVR,
        ConfigureEVR
    }
    
    /// <summary>
    /// 
    /// </summary>
    internal class FilterGraph : IDisposable
    {
        bool bDisposed;

        public static readonly uint UWM_GRAPH_NOTIFY = WindowsManagement.RegisterWindowMessage("GraphNotify-{D4D312B2-EF7F-4d35-BD66-58214F3B90B4}");
        
        public Error error = Error.Unknown;
        private static Dictionary<Error, string> errorMessages;
        
        public MediaInfo info;
        public double dAspectRatio;
        public GDI.RECT rcSrc;
        public bool bSeekable;
        public long rtDuration;    //the number of 100 nanoseconds units (by default)
        public long rtCurrentTime; //to get it in seconds you should divide 'em by 10000000
        public double dRate;
        public GraphState GraphState;
        public int ARHeight;
        public int ARWidth;
        public SourceType SourceType;
        public Guid RecommnedSourceFilterId;
        public ArrayList aFilters; // array of the names of the filters in the graph
        public int dwRegister;
        public bool bAddedToRot;
    
        // Audio streams stuff
        public int nAudioStreams;
        public int nCurrentAudioStream;

        // DVD related variables
        public int ulNumTitles;
        public ArrayList arrayNumChapters; // number of chapters in each title
        public ArrayList arrayAudioStream;
        public ArrayList arrayMenuLang;
        public ArrayList arrayMenuLangLCID;
        public ArrayList arraySubpictureStream;
        public bool bPanscanPermitted;
        public bool bLetterboxPermitted;
        public bool bLine21Field1InGOP;
        public bool bLine21Field2InGOP;
        public int ulAnglesAvailable = 1;
        public int ulCurrentAngle = 1;

        public bool bShowMenuCalledFromTitle;
        public VALID_UOP_FLAG UOPS;
        public DVD_DOMAIN CurDomain = DVD_DOMAIN.DVD_DOMAIN_Stop;
        public int ulCurChapter;            // track the current chapter number
        public int ulCurTitle;              // track the current title number
        public DVD_HMSF_TIMECODE CurTime;   // track the current playback time
        public bool bMenuOn;                // we are in a menu
        public bool bStillOn;               // used to track if there is a still frame on or not
        public bool bDVDAudioRendered = true;
        public bool bDVDSubpictureRendered = true;
        
        // Video Renderer
        public IRenderer pRenderer;
    
        // Filter Graph interfaces
        public IFilterGraph2 pFilterGraph2;
        public IMediaSeeking pMediaSeeking;
        public IMediaEventEx pMediaEventEx;
        public IMediaControl pMediaControl;
        public IGraphBuilder pGraphBuilder;
        public IBasicAudio pBasicAudio;

        // DirectSound Interfaces
        public ArrayList arrayDSBaseFilter;
        public ArrayList arrayBasicAudio;

        // Source filter interfaces
        public IBaseFilter pSource;		// every filter exposes this interface,
                                        // we need it for a source filter
        // Splitter filter
        public IBaseFilter pSplitterFilter;
    
        // IPin
        public IPin pSourceOutPin;

        // DVD interfaces
        public IDvdGraphBuilder pDVDGraphBuilder;
        public IDvdInfo2 pDvdInfo2;
        public IDvdControl2 pDvdControl2;
        public IAMLine21Decoder pAMLine21Decoder;

        public Renderer RendererInUse
        {
            get { return pRenderer != null ? pRenderer.Renderer : Renderer.VR; }
        }
        
        public FilterGraph()
        {
            info = new MediaInfo();
            dAspectRatio = 1.0;
            dRate = 1.00;
            GraphState = GraphState.Reset;
            SourceType = SourceType.Unknown;
            RecommnedSourceFilterId = Guid.Empty;
            aFilters = new ArrayList();

            arrayDSBaseFilter = new ArrayList();
            arrayBasicAudio = new ArrayList();

            arrayNumChapters = new ArrayList();
            arrayAudioStream = new ArrayList();
            arrayMenuLang = new ArrayList();
            arrayMenuLangLCID = new ArrayList();
            arraySubpictureStream = new ArrayList();
        }

        ~FilterGraph()
        {
            Dispose(false);
        }

        static FilterGraph()
        {
            errorMessages = new Dictionary<Error, string>();
            ReadErrorMessages();
        }

        public static string GetErrorText(Error error)
        {
            return errorMessages[error];
        }

        public static void ReadErrorMessages()
        {
            errorMessages[Error.Unknown] = Resources.Resources.error;
            errorMessages[Error.FilterGraphManager] = Resources.Resources.error_cant_create_fgm;
            errorMessages[Error.SourceFilter] = Resources.Resources.error_no_source_filter;
            errorMessages[Error.NecessaryInterfaces] = Resources.Resources.error_cant_retrieve_all_interfaces;
            errorMessages[Error.VideoRenderer] = Resources.Resources.error_cant_create_vr;
            errorMessages[Error.AddVideoRenderer] = Resources.Resources.error_cant_add_vr;
            errorMessages[Error.AddVMR9] = Resources.Resources.error_cant_add_vmr9;
            errorMessages[Error.ConfigureVMR9] = Resources.Resources.error_cant_configure_vmr9;
            errorMessages[Error.AddVMR] = Resources.Resources.error_cant_add_vmr;
            errorMessages[Error.ConfigureVMR] = Resources.Resources.error_cant_configure_vmr;
            errorMessages[Error.CantPlayFile] = Resources.Resources.error_cant_play_file;
            errorMessages[Error.CantRenderFile] = Resources.Resources.error_cant_render_file;
            errorMessages[Error.DirectSoundFilter] = Resources.Resources.error_cant_create_ds;
            errorMessages[Error.AddDirectSoundFilter] = Resources.Resources.error_cant_add_ds;
            errorMessages[Error.DvdGraphBuilder] = Resources.Resources.error_cant_create_dvd_builder;
            errorMessages[Error.CantPlayDisc] = Resources.Resources.error_cant_play_disc;
            errorMessages[Error.NoVideoDimension] = Resources.Resources.error_cant_get_video_size; 
            errorMessages[Error.AddEVR] = Resources.Resources.error_cant_add_evr;
            errorMessages[Error.ConfigureEVR] = Resources.Resources.error_cant_configure_evr;
        }

        #region IDisposable Members

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        #endregion

        protected virtual void Dispose(bool disposing)
        {
            if (!bDisposed)
            {
                // unmanaged cleanup
                if (bAddedToRot)
                    DsUtils.RemoveGraphFromRot(ref dwRegister);
                                    
                if (disposing)
                { // call Dispose(disposing) on child managed components
                    CloseInterfaces();
                }
            }
            bDisposed = true;
        }

        private void CloseInterfaces()
        {
            if (pMediaControl != null)
            {
                pMediaControl.Stop();
                pMediaControl = null;
            }
                
            // CALLBACK handle
            if (pMediaEventEx != null)
            {
                pMediaEventEx.SetNotifyWindow(IntPtr.Zero, 0, IntPtr.Zero);
                pMediaEventEx = null;
            }

            if (pRenderer != null)
                pRenderer.Close();
                
            // GRAPH interfaces
            pFilterGraph2 = null;
            pBasicAudio = null;
            pMediaSeeking =null;
                
            // IPin
            if (pSourceOutPin != null)
            {
                while(Marshal.ReleaseComObject(pSourceOutPin) > 0) {}
                pSourceOutPin = null;
            }

            if (pSource != null)
            {
                while(Marshal.ReleaseComObject(pSource) > 0) {}
                if (pSplitterFilter == pSource)
                    pSplitterFilter = null;
                pSource = null;
            }

            if (pSplitterFilter != null)
            {
                while(Marshal.ReleaseComObject(pSplitterFilter) > 0) {}
                pSplitterFilter = null;
            }

            arrayBasicAudio.Clear();
            IEnumerator ie = arrayDSBaseFilter.GetEnumerator();
            while(ie.MoveNext())
            {
                IBaseFilter pBaseFilter = (IBaseFilter)ie.Current;
                while(Marshal.ReleaseComObject(pBaseFilter) > 0) {}
            }
            arrayDSBaseFilter.Clear();

            if (pGraphBuilder != null)
            {
                while(Marshal.ReleaseComObject(pGraphBuilder) > 0) {}
                pGraphBuilder = null;
            }

            // DVD interfaces:
            if (pDVDGraphBuilder != null)
            {
                while(Marshal.ReleaseComObject(pDVDGraphBuilder) > 0) {}
                pDVDGraphBuilder = null;
            }

            if (pAMLine21Decoder != null)
            {
                while(Marshal.ReleaseComObject(pAMLine21Decoder) > 0) {}
                pAMLine21Decoder = null;
            }
        
            if (pDvdControl2 != null)
            {
                Marshal.ReleaseComObject(pDvdControl2);
                pDvdControl2 = null;
            }

            if (pDvdInfo2 != null)
            {
                Marshal.ReleaseComObject(pDvdInfo2);
                pDvdInfo2 = null;
            }
        }
    }
}
