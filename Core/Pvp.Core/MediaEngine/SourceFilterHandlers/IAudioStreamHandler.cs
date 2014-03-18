using System;
using Pvp.Core.DirectShow;

namespace Pvp.Core.MediaEngine.SourceFilterHandlers
{
    internal interface IAudioStreamHandler : IDisposable
    {
        void RenderAudio(IGraphBuilder pGraphBuilder);
        int AudioStreamsCount { get; }
        int CurrentAudioStream { get; set; }
        bool SetVolume(int volume);
        bool GetVolume(out int volume);
    }
}