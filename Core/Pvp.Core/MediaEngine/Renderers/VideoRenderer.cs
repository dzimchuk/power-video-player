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

namespace Pvp.Core.MediaEngine.Renderers
{
    internal class VideoRenderer : WindowedRenderer
    {
        public VideoRenderer()
        {
            _renderer = Renderer.VR;
        }

        protected override void AddToGraph(IGraphBuilder pGraphBuilder, ThrowExceptionForHRPointer errorFunc)
        {
            // add the Video Renderer filter to the graph
            int hr = pGraphBuilder.AddFilter(BaseFilter, "Video Renderer");
            errorFunc(hr, GraphBuilderError.AddVideoRenderer);
        }

        protected override Guid RendererID
        {
            get { return Clsid.VideoRenderer; }
        }

        protected override void HandleInstantiationError(Exception e)
        {
            throw new FilterGraphBuilderException(GraphBuilderError.VideoRenderer, e);
        }

        protected override Guid IID_4DVDGraphInstantiation
        {
            get { return Guid.Empty; }
        }
    }
}
