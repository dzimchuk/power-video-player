using System;
using Pvp.Core.DirectShow;
using Pvp.Core.MediaEngine.FilterGraphs;
using Pvp.Core.MediaEngine.SourceFilterHandlers;

namespace Pvp.Core.MediaEngine
{
    internal static class SourceFilterHandlerFactory
    {
        public static ISourceFilterHandler AddSourceFilter(IGraphBuilder graphBuilder, string source)
        {
            IBaseFilter sourceFilter;

            var hr = graphBuilder.AddSourceFilter(source, "Source", out sourceFilter);
            hr.ThrowExceptionForHR(GraphBuilderError.SourceFilter);

            return new RegularSourceFilterHandler(sourceFilter);
        }
    }
}