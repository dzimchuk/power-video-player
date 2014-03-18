using Pvp.Core.DirectShow;
using Pvp.Core.MediaEngine.SourceFilterHandlers;

namespace Pvp.Core.MediaEngine.StreamHandlers
{
    internal static class AudioStreamHandlerFactory
    {
        public static IAudioStreamHandler GetHandler(IBaseFilter splitter, bool disposeSplitter)
        {
            return new SimpleAudioStreamHandler(splitter, disposeSplitter);
        }
    }
}