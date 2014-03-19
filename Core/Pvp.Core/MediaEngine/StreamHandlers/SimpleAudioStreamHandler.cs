using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Pvp.Core.DirectShow;
using Pvp.Core.MediaEngine.SourceFilterHandlers;

namespace Pvp.Core.MediaEngine.StreamHandlers
{
    internal class SimpleAudioStreamHandler : IAudioStreamHandler
    {
        // DirectSound Interfaces
        private readonly List<IBaseFilter> _directSoundBaseFilters;
        private readonly List<IBasicAudio> _basicAudioInterfaces;

        // Audio streams stuff
        private int _audioStreamsCount;
        private int _currentAudioStream;

        public SimpleAudioStreamHandler()
        {
            _directSoundBaseFilters = new List<IBaseFilter>();
            _basicAudioInterfaces = new List<IBasicAudio>();
        }

        public void Dispose()
        {
            _basicAudioInterfaces.Clear();
            _directSoundBaseFilters.ForEach(f => Marshal.FinalReleaseComObject(f));
            _directSoundBaseFilters.Clear();
        }

        public static bool CanHandle(IBaseFilter splitter)
        {
            var result = false;

            var pPin = DsUtils.GetPin(splitter, PinDirection.Output, new[] { MediaType.Audio });
            if (pPin != null)
            {
                Marshal.ReleaseComObject(pPin);
                result = true;
            }

            return result;
        }

        public void RenderAudio(IGraphBuilder pGraphBuilder, IBaseFilter splitter)
        {
            IPin pPin;

            var nSkip = 0;
            while ((pPin = DsUtils.GetPin(splitter, PinDirection.Output, false, nSkip)) != null)
            {
                if (DsUtils.IsMediaTypeSupported(pPin, MediaType.Audio) == 0)
                {
                    // this unconnected pin supports audio type!
                    // let's render it!
                    if (BuildSoundRenderer(pGraphBuilder))
                    {
                        var pInputPin = DsUtils.GetPin(_directSoundBaseFilters.Last(), PinDirection.Input);
                        var hr = pGraphBuilder.Connect(pPin, pInputPin);
                        Marshal.ReleaseComObject(pInputPin);
                        if (hr == DsHlp.S_OK || hr == DsHlp.VFW_S_PARTIAL_RENDER)
                        {
                            if (_directSoundBaseFilters.Count == 8)
                            {
                                Marshal.ReleaseComObject(pPin);
                                break; // out of while cycle
                            }

                        }
                        else
                        {
                            var pBaseFilter = _directSoundBaseFilters.Last();
                            pGraphBuilder.RemoveFilter(pBaseFilter);
                            Marshal.ReleaseComObject(pBaseFilter);

                            _basicAudioInterfaces.RemoveAt(_basicAudioInterfaces.Count - 1);
                            _directSoundBaseFilters.RemoveAt(_directSoundBaseFilters.Count - 1);

                            nSkip++;
                        }
                    }
                    else
                    {
                        // could not create/add DirectSound filter
                        Marshal.ReleaseComObject(pPin);
                        break; // out of while cycle
                    }
                }
                else
                    nSkip++;
                Marshal.ReleaseComObject(pPin);
            } // end of while

            _currentAudioStream = 0;
            _audioStreamsCount = _basicAudioInterfaces.Count;
            const int lVolume = -10000;
            for (var i = 1; i < _audioStreamsCount; i++)
            {
                _basicAudioInterfaces[i].put_Volume(lVolume);
            }
        }

        private bool BuildSoundRenderer(IGraphBuilder pGraphBuilder)
        {
            var result = false;
            var renderer = pGraphBuilder.AddSoundRenderer();

            if (renderer != null)
            {
                _directSoundBaseFilters.Add(renderer.Item1);
                _basicAudioInterfaces.Add(renderer.Item2);

                result = true;
            }

            return result;
        }

        public int AudioStreamsCount
        {
            get { return _audioStreamsCount; }
        }

        public int CurrentAudioStream
        {
            get { return _currentAudioStream; }
            set
            {
                if (_basicAudioInterfaces.Count == 0)
                    return;

                if (value < 0 || value >= _basicAudioInterfaces.Count)
                    return;

                int lVolume;
                GetVolume(out lVolume);

                const int lMute = -10000;
                for (int i = 0; i < _audioStreamsCount; i++)
                {
                    _basicAudioInterfaces[i].put_Volume(lMute);
                }

                _currentAudioStream = value;
                SetVolume(lVolume);
            }
        }

        public bool SetVolume(int volume)
        {
            if (_basicAudioInterfaces.Count == 0)
                return false;

            var basicAudio = _basicAudioInterfaces[_currentAudioStream];
            return basicAudio.put_Volume(volume) == DsHlp.S_OK;
        }

        public bool GetVolume(out int volume)
        {
            volume = 0;

            if (_basicAudioInterfaces.Count == 0)
                return false;

            var basicAudio = _basicAudioInterfaces[_currentAudioStream];
            return basicAudio.get_Volume(out volume) == DsHlp.S_OK;
        }

        public string GetAudioStreamName(int nStream)
        {
            return string.Format(Resources.Resources.mw_stream_format, nStream + 1);
        }

        public void OnExternalStreamSelection()
        {
        }
    }
}