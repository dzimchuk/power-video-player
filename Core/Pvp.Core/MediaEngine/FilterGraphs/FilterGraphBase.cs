using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Pvp.Core.DirectShow;
using Pvp.Core.MediaEngine.Description;
using Pvp.Core.Native;

namespace Pvp.Core.MediaEngine.FilterGraphs
{
    internal abstract class FilterGraphBase : IFilterGraph
    {
        public static readonly uint UWM_GRAPH_NOTIFY = WindowsManagement.RegisterWindowMessage("GraphNotify-{D4D312B2-EF7F-4d35-BD66-58214F3B90B4}");

        private bool _disposed;

        private int _dwRegister;
        private bool _addedToRot;

        private readonly MediaInfo _info;
        private IEnumerable<string> _filterNames;

        // Video Renderer
        private IRenderer _renderer;

        // Filter Graph interfaces
        private IFilterGraph2 _filterGraph2;
        private IMediaSeeking _mediaSeeking;
        private IMediaEventEx _mediaEventEx;
        private IMediaControl _mediaControl;
        private IGraphBuilder _graphBuilder;
        private IBasicAudio _basicAudio;

        protected FilterGraphBase()
        {
            _info = new MediaInfo();

            GraphState = GraphState.Stopped; // or Reset? but it was set to Stopped when graph was rendered
        }

        ~FilterGraphBase()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    CloseInterfaces();
                }

                // unmanaged cleanup
                if (_addedToRot)
                {
                    DsUtils.RemoveGraphFromRot(ref _dwRegister);
                }
            }

            _disposed = true;
        }

        protected virtual void CloseInterfaces()
        {
            if (_mediaControl != null)
            {
                _mediaControl.Stop();
                _mediaControl = null;
            }

            // CALLBACK handle
            if (_mediaEventEx != null)
            {
                _mediaEventEx.SetNotifyWindow(IntPtr.Zero, 0, IntPtr.Zero);
                _mediaEventEx = null;
            }

            if (_renderer != null)
            {
                try
                {
                    _renderer.Close();
                }
                catch (Exception e)
                {
                    // TODO: lot it
                }
            }

            // GRAPH interfaces
            _filterGraph2 = null;
            _basicAudio = null;
            _mediaSeeking = null;

            if (_graphBuilder != null)
            {
                while (Marshal.ReleaseComObject(_graphBuilder) > 0) { }
                _graphBuilder = null;
            }
        }

        public event EventHandler<string> GraphError;
        public event EventHandler PlayBackComplete;
        public event EventHandler ErrorAbort;
        public event FailedStreamsHandler FailedStreamsAvailable;

        protected virtual void OnFailedStreamsAvailable(IList<StreamInfo> streams)
        {
            var handler = Interlocked.CompareExchange(ref FailedStreamsAvailable, null, null);
            if (handler != null) handler(streams);
        }

        protected virtual void OnGraphError(string message)
        {
            message.Raise(this, ref GraphError);
        }

        protected virtual void OnPlayBackComplete()
        {
            EventArgs.Empty.Raise(this, ref PlayBackComplete);
        }

        protected virtual void OnErrorAbort()
        {
            EventArgs.Empty.Raise(this, ref ErrorAbort);
        }

        public abstract void BuildUp(FilterGraphBuilderParameters parameters);
        public abstract SourceType SourceType { get; }

        public GraphState GraphState { get; protected set; }

        public MediaInfo MediaInfo
        {
            get { return _info; }
        }

        public IEnumerable<string> FilterNames
        {
            get 
            {
                if (_graphBuilder == null)
                {
                    throw new Exception("Filter graph has not been built.");
                }

                IEnumerable<string> filters;
                if (DsHlp.FAILED(DsUtils.EnumFilters(_graphBuilder, out filters)))
                {
                    filters = new string[] { };
                }

                return filters;
            }
        }

        public void AddToRot()
        {
            _addedToRot = DsUtils.AddToRot(_graphBuilder, out _dwRegister);
        }

        public IRenderer Renderer
        {
            get { return _renderer; }
            protected set
            {
                if (_renderer != null)
                {
                    _renderer.Close();
                }
                _renderer = value;
            }
        }

        protected IFilterGraph2 FilterGraph2
        {
            get { return _filterGraph2; }
        }

        protected IMediaSeeking MediaSeeking
        {
            get { return _mediaSeeking; }
        }

        protected IMediaEventEx MediaEventEx
        {
            get { return _mediaEventEx; }
        }

        protected IMediaControl MediaControl
        {
            get { return _mediaControl; }
        }

        protected IGraphBuilder GraphBuilder
        {
            get { return _graphBuilder; }
        }

        protected IBasicAudio BasicAudio
        {
            get { return _basicAudio; }
        }

        protected void AddToMediaInfo(string source)
        {
            _info.source = source;
        }

        protected void AddToMediaInfo(Guid subType)
        {
            _info.StreamSubType = subType;
        }

        protected void AddToMediaInfo(StreamInfo streamInfo)
        {
            _info.streams.Add(streamInfo);
        }

        protected void ClearStreamsInfo()
        {
            _info.streams.Clear();
        }

        protected void InitializeGraphBuilder(Func<IGraphBuilder> initFunc)
        {
            if (_graphBuilder != null)
            {
                throw new Exception("GraphBuilder has already been initialized.");
            }

            _graphBuilder = initFunc();

            if (_graphBuilder == null)
            {
                throw new Exception("GraphBuilder has not been initialized.");
            }
        }

        protected void InitializeMediaEventEx(IntPtr hMediaWindow)
        {
            try
            {
                _mediaEventEx = (IMediaEventEx)_graphBuilder;
            }
            catch (Exception e)
            {
                throw new FilterGraphBuilderException(GraphBuilderError.NecessaryInterfaces, e);
            }

            // set graph state window callback
            _mediaEventEx.SetNotifyWindow(hMediaWindow, (int)UWM_GRAPH_NOTIFY, IntPtr.Zero);
        }

        protected void InitializeMediaControl()
        {
            try
            {
                _mediaControl = (IMediaControl)_graphBuilder;
            }
            catch (Exception e)
            {
                throw new FilterGraphBuilderException(GraphBuilderError.NecessaryInterfaces, e);
            }
        }

        protected void InitializeBasicAudio()
        {
            try
            {
                _basicAudio = (IBasicAudio)_graphBuilder;
            }
            catch (Exception e)
            {
                throw new FilterGraphBuilderException(GraphBuilderError.NecessaryInterfaces, e);
            }
        }

        protected void InitializeMediaSeeking()
        {
            try
            {
                _mediaSeeking = (IMediaSeeking)_graphBuilder;
            }
            catch (Exception e)
            {
                throw new FilterGraphBuilderException(GraphBuilderError.NecessaryInterfaces, e);
            }
        }

        protected void InitializeFilterGraph2()
        {
            try
            {
                _filterGraph2 = (IFilterGraph2)_graphBuilder;
            }
            catch (Exception e)
            {
                throw new FilterGraphBuilderException(GraphBuilderError.NecessaryInterfaces, e);
            }
        }

        public int FilterCount
        {
            get
            {
                if (_graphBuilder == null)
                {
                    return 0;
                }

                if (_filterNames == null)
                {
                    DsUtils.EnumFilters(_graphBuilder, out _filterNames);
                }

                return _filterNames.Count();
            }
        }

        public string GetFilterName(int nFilterNum)
        {
            var result = string.Empty;

            if (_graphBuilder != null && _filterNames == null)
            {
                DsUtils.EnumFilters(_graphBuilder, out _filterNames);
            }

            if (_filterNames != null)
            {
                var filterNames = _filterNames as string[] ?? _filterNames.ToArray();
                if (_filterNames != null && nFilterNum < filterNames.Count())
                {
                    result = filterNames[nFilterNum];
                }
            }

            return result;
        }

        public abstract bool IsGraphSeekable { get; }

        public abstract long Duration { get; }

        public abstract double Rate { get; }

        public virtual bool PauseGraph()
        {
            if (_mediaControl == null)
            {
                return false;
            }

            var hr = _mediaControl.Pause();
            if (hr == DsHlp.S_OK)
            {
                GraphState = GraphState.Paused;
                return true;
            }
            
            if (hr == DsHlp.S_FALSE)
            {
                if (UpdateGraphState())
                    return GraphState == GraphState.Paused;

                _mediaControl.Stop();
            }

            GraphState = GraphState.Stopped; // Pause() failed
            return false;
        }

        public virtual bool ResumeGraph()
        {
            if (_mediaControl == null)
            {
                return false;
            }

            if (DsHlp.SUCCEEDED(_mediaControl.Run()))
            {
                GraphState = GraphState.Running;
                return true; // ok, we're running
            }
            
            if (UpdateGraphState())
            {
                GraphState = GraphState.Running;
                return true; // ok, we're running
            }
            
            return false;
        }

        public virtual bool StopGraph()
        {
            if (_mediaControl == null)
            {
                return false;
            }

            PauseGraph();
            SetCurrentPosition(0);
            
            _mediaControl.Stop();
            GraphState = GraphState.Stopped;

            return true;
        }

        public abstract long GetCurrentPosition();

        public abstract void SetCurrentPosition(long time);

        public abstract void SetRate(double rate);

        //This function will return FALSE if the state is RESET or unidentified!!!!
        //The application must determine whether to stop the graph.
        private bool UpdateGraphState()
        {
            if (_mediaControl == null)
            {
                return false;
            }

            FilterState fs;
            var hr = _mediaControl.GetState(2000, out fs);
            if (hr == DsHlp.S_OK)
            {
                switch (fs)
                {
                    case FilterState.State_Stopped:
                        GraphState = GraphState.Stopped;
                        break;
                    case FilterState.State_Paused:
                        GraphState = GraphState.Paused;
                        break;
                    case FilterState.State_Running:
                        GraphState = GraphState.Running;
                        break;
                }

                return true;
            }

            if (hr == DsHlp.VFW_S_CANT_CUE)
            {
                GraphState = GraphState.Paused;
                return true;
            }
            
            if (hr == DsHlp.VFW_S_STATE_INTERMEDIATE) //Don't know what the state is so just stay at the old one
                return true;
            
            return false;
        }

        public abstract int AudioStreamsCount { get; }
        public abstract int CurrentAudioStream { get; set; }

        public abstract GDI.RECT SourceRect { get; }
        public abstract double AspectRatio { get; }

        public abstract string GetAudioStreamName(int nStream);

        public abstract bool SetVolume(int volume);
        public abstract bool GetVolume(out int volume);

        public bool DisplayFilterPropPage(IntPtr hParent, string strFilter, bool bDisplay)
        {
            if (_graphBuilder == null)
                return false;

            IBaseFilter pFilter;
            _graphBuilder.FindFilterByName(strFilter, out pFilter);
            if (pFilter == null)
                return false;

            var bRet = false;
            var pProp = pFilter as ISpecifyPropertyPages;
            if (pProp != null)
            {
                bRet = true;
                if (bDisplay)
                {
                    // Show the page. 
                    CAUUID caGuid;
                    pProp.GetPages(out caGuid);

                    object pFilterUnk = pFilter;
                    DsUtils.OleCreatePropertyFrame(
                        hParent,                // Parent window
                        0, 0,                   // Reserved
                        strFilter,				// Caption for the dialog box
                        1,                      // Number of objects (just the filter)
                        ref pFilterUnk,			// Array of object pointers. 
                        caGuid.cElems,          // Number of property pages
                        caGuid.pElems,          // Array of property page CLSIDs
                        0,                      // Locale identifier
                        0, IntPtr.Zero          // Reserved
                        );

                    // Clean up.
                    Marshal.FreeCoTaskMem(caGuid.pElems);
                }

                //    Marshal.ReleaseComObject(pProp);
            }

            //    Marshal.ReleaseComObject(pFilter);
            return bRet;
        }

        public void HandleGraphEvent()
        {
            if (_mediaEventEx == null)
            {
                return;
            }

            int evCode, lParam1, lParam2;

            while (DsHlp.SUCCEEDED(_mediaEventEx.GetEvent(out evCode, out lParam1, out lParam2, 0)))
            {
                HandleGraphEvent(evCode, lParam1, lParam2);

                _mediaEventEx.FreeEventParams(evCode, lParam1, lParam2);
            }
        }

        protected virtual bool HandleGraphEvent(int evCode, int lParam1, int lParam2)
        {
            var handled = false;

            switch (evCode)
            {
                case (int)DsEvCode.Complete:
                    OnPlayBackComplete();
                    handled = true;
                    break;
                case (int)DsEvCode.ErrorAbort:
                    OnErrorAbort();
                    handled = true;
                    break;
            }

            return handled;
        }
    }
}