using Pvp.Core.DirectShow;
using Pvp.Core.MediaEngine.SourceFilterHandlers;

namespace Pvp.Core.MediaEngine.StreamHandlers
{
    internal static class SubpictureStreamHandlerFactory
    {
        public static ISubpictureStreamHandler GetHandler(IBaseFilter splitter)
        {
            ISubpictureStreamHandler handler = null;

            if (DirectVobSubSubpictureStreamHandler.CanHandle(splitter))
                handler = new DirectVobSubSubpictureStreamHandler();

            return handler;
        }    
    }
}