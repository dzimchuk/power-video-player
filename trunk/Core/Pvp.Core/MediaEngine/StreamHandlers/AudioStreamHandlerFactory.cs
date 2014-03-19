using Pvp.Core.DirectShow;
using Pvp.Core.MediaEngine.SourceFilterHandlers;

namespace Pvp.Core.MediaEngine.StreamHandlers
{
    internal static class AudioStreamHandlerFactory
    {
        public static IAudioStreamHandler GetHandler(IBaseFilter splitter)
        {
            IAudioStreamHandler handler = null;

            if (SelectingAudioStreamHandler.CanHandle(splitter))
                handler = new SelectingAudioStreamHandler();
            else if (SimpleAudioStreamHandler.CanHandle(splitter))
                handler = new SimpleAudioStreamHandler();

            return handler;
        }
    }
}