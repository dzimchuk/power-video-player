﻿/* ****************************************************************************
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
using System.Text;
using System.Runtime.InteropServices;
using Dzimchuk.DirectShow;
using Dzimchuk.Native;

namespace Dzimchuk.MediaEngine.Core.Render
{
    internal abstract class RendererBase : IRenderer, IDisposable
    {
        protected Renderer _renderer;
        protected IBaseFilter pBaseFilter;
        private bool _disposed = false;
        private bool _ready = false;
        protected IGraphBuilder pGraphBuilder;
        protected IntPtr hMediaWindow;
        
        #region IRenderer Members

        public Renderer Renderer
        {
            get { return _renderer; }
        }

        public void Close()
        {
            Dispose();
        }

        public void RemoveFromGraph()
        {
            pGraphBuilder.RemoveFilter(pBaseFilter);
            Close();
        }

        public IPin GetInputPin()
        {
            IPin pPin = null;
            if (pBaseFilter != null)
            {
                // try unconnected pins first
                pPin = DsUtils.GetPin(pBaseFilter, PinDirection.Input);
                if (pPin == null)
                {
                    // let's try connected pins
                    if ((pPin = DsUtils.GetPin(pBaseFilter, PinDirection.Input, true)) != null)
                    {
                        DsUtils.Disconnect(pGraphBuilder, pPin);
                    }
                }
            }
            return pPin;
        }

        private bool GetPins(out IPin rendererInputPin, out IPin decoderOutPin)
        {
            bool bRet = false;
            rendererInputPin = decoderOutPin = null;
            if (pBaseFilter != null && (rendererInputPin = DsUtils.GetPin(pBaseFilter, PinDirection.Input, true)) != null)
            {
                int hr = rendererInputPin.ConnectedTo(out decoderOutPin);
                if (hr == DsHlp.S_OK)
                {
                    DsUtils.Disconnect(pGraphBuilder, rendererInputPin);
                    bRet = true;
                }
                else
                {
                    Marshal.ReleaseComObject(rendererInputPin);
                    rendererInputPin = null;
                }
            }
            return bRet;
        }

        public abstract void SetVideoPosition(ref GDI.RECT rcSrc, ref GDI.RECT rcDest);
        public abstract void GetNativeVideoSize(out int width, out int height, out int ARWidth, out int ARHeight);

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        #endregion

        ~RendererBase()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            lock (this)
            {
                if (!_disposed)
                {
                    if (disposing) // release managed components if any
                    {
                        CloseInterfaces();
                    }

                    // release unmanaged components if any
                    _disposed = true;
                }
            }
        }

        public static IRenderer AddRenderer(IGraphBuilder pGraphBuilder,
                                            Renderer preferredVideoRenderer,
                                            ThrowExceptionForHRPointer errorFunc,
                                            IntPtr hMediaWindow)
        {
            return AddRenderer(pGraphBuilder, preferredVideoRenderer, errorFunc, hMediaWindow, true);
        }
        
        public static IRenderer AddRenderer(IGraphBuilder pGraphBuilder, 
                                            Renderer preferredVideoRenderer, 
                                            ThrowExceptionForHRPointer errorFunc, 
                                            IntPtr hMediaWindow,
                                            bool fallBackOnVR)
        {
            RendererBase renderer = GetRenderer(preferredVideoRenderer);
            try
            {
                bool bOk = renderer.InstantiateRenderer();
                if (!bOk)
                {
                    if (fallBackOnVR)
                    {
                        // try default renderer
                        renderer.Close();
                        renderer = GetRenderer(Renderer.VR);
                        renderer.InstantiateRenderer(); // it will throw FilterGraphBuilderException if it can't be instantiated
                    }
                    else
                        throw new FilterGraphBuilderException(Error.VideoRenderer);
                }

                renderer.AddToGraph(pGraphBuilder, errorFunc);

                // Windowed renderers should be initialized (parent window is set, etc) _after_ the renderer is connected because a video window is created then
                // On the contrary windowless renderes _must_ be initialized _before_ they are connected
                if (!renderer.IsDelayedInitialize)
                    renderer.Initialize(pGraphBuilder, hMediaWindow);

                renderer.pGraphBuilder = pGraphBuilder;
                renderer.hMediaWindow = hMediaWindow;
                renderer._ready = true;
                return renderer;
            }
            finally
            {
                if (!renderer._ready)
                    renderer.Close();
            }
        }

        public static IRenderer AddRenderer(IDvdGraphBuilder pDVDGraphBuilder,
                                            IGraphBuilder pGraphBuilder,
                                            Renderer preferredVideoRenderer,
                                            IntPtr hMediaWindow)
        {
            // this methods will instruct DVDGraphBuilder to instantiate and use the preferred renderer; it will return a wrapper if it was successful
            // it will return null if:
            // a) preferredVideoRenderer is VR -> we can't instruct DVDGraphBuilder to create one so the program will have a possibility to fall back on the default one
            // b) there were errors instantiating the preferred renderer; in this case the program should fall back on the default renderer by calling GetExistingRenderer
            //    after the graph is built
            // it will throw an exception (originating from renderers' Initialize method) if initialization fails (similar behavior when adding renderers to non-DVD graphs)
            RendererBase renderer = null;
            try
            {
                if (preferredVideoRenderer != Renderer.VR)
                {
                    renderer = GetRenderer(preferredVideoRenderer);
                    if (renderer.Instantiate(pDVDGraphBuilder, pGraphBuilder))
                    {
                        if (!renderer.IsDelayedInitialize)
                            renderer.Initialize(pGraphBuilder, hMediaWindow);

                        renderer.pGraphBuilder = pGraphBuilder;
                        renderer.hMediaWindow = hMediaWindow;
                        renderer._ready = true;
                        return renderer;
                    }
                }

                return null;
            }
            finally
            {
                if (renderer != null && !renderer._ready)
                    renderer.Close();
            }
        }
        
        private static RendererBase TryGetUnknownRenderer(IGraphBuilder pGraphBuilder)
        {
            // this is the last resort
            RendererBase renderer = null;

            IEnumFilters pEnum = null;
            IBaseFilter pFilter;
            int cFetched;
            
            int hr = pGraphBuilder.EnumFilters(out pEnum);
            if (DsHlp.SUCCEEDED(hr))
            {
                bool bFound = false;
                while (!bFound && pEnum.Next(1, out pFilter, out cFetched) == DsHlp.S_OK)
                {
                    IPin pPin = null;
                    // there should be no output pins
                    if ((pPin = DsUtils.GetPin(pFilter, PinDirection.Output, false)) != null)
                    {
                        // there is an unconnected output pin, this is not a renderer
                        Marshal.ReleaseComObject(pPin);
                    }
                    else if ((pPin = DsUtils.GetPin(pFilter, PinDirection.Output, true)) != null)
                    {
                        // there is a connected output pin, this is not a renderer
                        Marshal.ReleaseComObject(pPin);
                    }
                    else
                    {
                        // let's check the input pins: there must be at least one connected of type 'video'
                        int nSkip = 0;
                        while ((pPin = DsUtils.GetPin(pFilter, PinDirection.Input, true, nSkip)) != null)
                        {
                            if (DsUtils.IsMediaTypeSupported(pPin, MediaType.Video) == 0)
                            {
                                // there is connected input pin of type 'video'; this looks like a renderer
                                Marshal.ReleaseComObject(pPin);
                                renderer = GetRenderer(Renderer.VR); // let's just default it VR
                                renderer.pBaseFilter = pFilter;
                                bFound = true;
                                break;
                            }
                            else
                            {
                                nSkip++;
                                Marshal.ReleaseComObject(pPin);
                            }
                        }
                    }
                    
                    if (!bFound)
                        Marshal.ReleaseComObject(pFilter);
                }

                Marshal.ReleaseComObject(pEnum);
            }

            return renderer;
        }

        public static IRenderer GetExistingRenderer(IGraphBuilder pGraphBuilder, IntPtr hMediaWindow)
        {
            // this method is to be called to create a wrapper for a renderer that is already in the graph (added by intelligent connect)
            // at the momemnt it is assumed that this is a Windowed Renderer (compatability assumption)
            RendererBase renderer = null;
            try
            {
                IEnumFilters pEnum = null;
                IBaseFilter pFilter;
                int cFetched;

                int hr = pGraphBuilder.EnumFilters(out pEnum);
                if (DsHlp.SUCCEEDED(hr))
                {
                    while (pEnum.Next(1, out pFilter, out cFetched) == DsHlp.S_OK)
                    {
                        Guid clsid;
                        if (pFilter.GetClassID(out clsid) == DsHlp.S_OK)
                        {
                            renderer = GetRenderer(clsid);
                            if (renderer != null)
                            {
                                renderer.pBaseFilter = pFilter;
                                break;
                            }
                            else
                                Marshal.ReleaseComObject(pFilter);
                        }
                        else
                            Marshal.ReleaseComObject(pFilter);
                    }

                    Marshal.ReleaseComObject(pEnum);
                }

                if (renderer == null)
                    renderer = TryGetUnknownRenderer(pGraphBuilder); // last resort

                if (renderer == null)
                    throw new FilterGraphBuilderException(Error.VideoRenderer); // we've tried hard enough, there is no point to continue

                if (!renderer.IsDelayedInitialize)
                    renderer.Initialize(pGraphBuilder, hMediaWindow);

                renderer.pGraphBuilder = pGraphBuilder;
                renderer.hMediaWindow = hMediaWindow;
                renderer._ready = true;
                return renderer;
            }
            finally
            {
                if (renderer != null && !renderer._ready)
                    renderer.Close();
            }
        }

        public static IRenderer SubstituteRenderer(IGraphBuilder pGraphBuilder, IntPtr hMediaWindow, Renderer desiredRenderer)
        {
            RendererBase existingRenderer = (RendererBase)GetExistingRenderer(pGraphBuilder, hMediaWindow);
            // existingRenderer is either VR or VMR windowed (default mode for VMR); VMR9 and EVR are never default renderers
            // VR and VMRWindowed need delayed initialization or it's ok to reconnect them before we call GetNativeVideoSize

            // if desiredRenderer fails we will return the existing one
            IRenderer renderer = existingRenderer;
            if (existingRenderer.Renderer != desiredRenderer && desiredRenderer != Renderer.VR) // substitution with VR doesn't work well at least on Vista 64bit
            {
                RendererBase newRenderer = null;
                try
                {
                    newRenderer = (RendererBase)AddRenderer(pGraphBuilder,
                                                            desiredRenderer,
                                                            delegate(int hrCode, Error error) { },
                                                            hMediaWindow,
                                                            false);
                    IPin existingRendererInput, decoderOut;
                    if (existingRenderer.GetPins(out existingRendererInput, out decoderOut))
                    {
                        IPin newRendererInput = newRenderer.GetInputPin();
                        if (newRendererInput != null)
                        {
                            int hr = pGraphBuilder.Connect(decoderOut, newRendererInput);
                            if (DsHlp.SUCCEEDED(hr))
                            {
                                renderer = newRenderer;
                                newRenderer = null; // so that we don't close it (see finally)

                                Marshal.ReleaseComObject(existingRendererInput);
                                existingRendererInput = null;
                                existingRenderer.RemoveFromGraph();
                            }
                            else
                            {
                                hr = pGraphBuilder.Connect(decoderOut, existingRendererInput);
                            }
                            Marshal.ReleaseComObject(newRendererInput);
                        }

                        if (existingRendererInput != null)
                            Marshal.ReleaseComObject(existingRendererInput);
                        Marshal.ReleaseComObject(decoderOut);
                    }
                }
                catch // VR throws it if it can't be instantiated; also renderers will throw an expection if they can't be added to the graph
                {     // we just ignore it here and return existingRenderer
                }
                finally
                {
                    if (newRenderer != null)
                        newRenderer.Close();
                }
            }
            return renderer;
        }

        protected abstract void AddToGraph(IGraphBuilder pGraphBuilder, ThrowExceptionForHRPointer errorFunc);
        protected abstract void Initialize(IGraphBuilder pGraphBuilder, IntPtr hMediaWindow);
        protected abstract Guid RendererID { get; }
        protected abstract void HandleInstantiationError(Exception e);
        protected abstract bool IsDelayedInitialize { get; }
        protected abstract Guid IID_4DVDGraphInstantiation { get; }

        protected virtual void CloseInterfaces()
        {
            if (pBaseFilter != null)
            {
                while (Marshal.ReleaseComObject(pBaseFilter) > 0) { }
                pBaseFilter = null;
            }

            pGraphBuilder = null;
            hMediaWindow = IntPtr.Zero;
        }

        private void GetBaseFilter(IGraphBuilder pGraphBuilder)
        {
            IEnumFilters pEnum = null;
            IBaseFilter pFilter;
            int cFetched;

            int hr = pGraphBuilder.EnumFilters(out pEnum);
            if (DsHlp.SUCCEEDED(hr))
            {
                while (pEnum.Next(1, out pFilter, out cFetched) == DsHlp.S_OK)
                {
                    Guid clsid;
                    if (pFilter.GetClassID(out clsid) == DsHlp.S_OK && clsid == RendererID)
                    {
                        pBaseFilter = pFilter;
                        break;
                    }
                    else
                        Marshal.ReleaseComObject(pFilter);
                }

                Marshal.ReleaseComObject(pEnum);
            }
        }

        protected virtual bool Instantiate(IDvdGraphBuilder pDvdGraphBuilder, IGraphBuilder pGraphBuilder)
        {
            Guid IID = IID_4DVDGraphInstantiation;
            bool bRet = false;
            object o;
            int hr = pDvdGraphBuilder.GetDvdInterface(ref IID, out o);
            if (hr == DsHlp.S_OK)
            {
                Marshal.FinalReleaseComObject(o); // pDvdGraphBuilder has some connection to the renderer already so it will be used when rendering the graph
                GetBaseFilter(pGraphBuilder);
                bRet = pBaseFilter != null;
            }
            return bRet;
        }

        protected virtual bool InstantiateRenderer()
        {
            object comobj = null;
            try
            {
                Type type = Type.GetTypeFromCLSID(RendererID, true);
                comobj = Activator.CreateInstance(type);
                pBaseFilter = (IBaseFilter)comobj;
                return true;
            }
            catch (Exception e)
            {
                if (comobj != null)
                    while (Marshal.ReleaseComObject(comobj) > 0) { }
                HandleInstantiationError(e);
                return false;
            }
        }

        private static RendererBase GetRenderer(Renderer preferredVideoRenderer)
        {
            // switch for now
            RendererBase renderer;
            switch (preferredVideoRenderer)
            {
                case Renderer.VMR_Windowed:
                    renderer = new VMRWindowed();
                    break;
                case Renderer.VMR_Windowless:
                    renderer = new VMRWindowless();
                    break;
                case Renderer.VMR9_Windowed:
                    renderer = new VMR9Windowed();
                    break;
                case Renderer.VMR9_Windowless:
                    renderer = new VMR9Windowless();
                    break;
                case Renderer.EVR:
                    renderer = new EVR();
                    break;
                default:
                    renderer = new VideoRenderer();
                    break;
            }
            return renderer;
        }

        public static RendererBase GetRenderer(Guid ClsId)
        {
            RendererBase renderer = null;

            if (ClsId == Clsid.VideoRenderer)
                renderer = new VideoRenderer();
            else if (ClsId == Clsid.VideoMixingRenderer)
                renderer = new VMRWindowed(); // by default VMR operates in windowed mode with a single video stream, also called compatibility mode
            else if (ClsId == Clsid.VideoMixingRenderer9)
                renderer = new VMR9Windowed(); // VMR-9 defaults to windowed mode with four input pins
            else if (ClsId == Clsid.EnhancedVideoRenderer)
                renderer = new EVR(); // EVR is windowless with 1 input pin
            
            return renderer;
        }
    }
}
