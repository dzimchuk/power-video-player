using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Pvp.Core.DirectShow;
using Pvp.Core.MediaEngine.SourceFilterHandlers;

namespace Pvp.Core.MediaEngine.StreamHandlers
{
    internal class SelectingAudioStreamHandler : IAudioStreamHandler
    {
        private IAMStreamSelect _streamSelect;

        // DirectSound Interfaces
        private IBaseFilter _directSoundBaseFilter;
        private IBasicAudio _basicAudio;

        // Audio streams stuff
        private readonly List<SelectableStream> _audioStreams;

        public SelectingAudioStreamHandler()
        {
            _audioStreams = new List<SelectableStream>();
        }

        public void Dispose()
        {
            if (_directSoundBaseFilter != null)
            {
                Marshal.FinalReleaseComObject(_directSoundBaseFilter);
                _directSoundBaseFilter = null;
            }

            _basicAudio = null;
        }

        public static bool CanHandle(IBaseFilter splitter)
        {
            var result = false;

            var pPin = DsUtils.GetPin(splitter, PinDirection.Output, new[] { MediaType.Audio });
            if (pPin != null)
            {
                Marshal.ReleaseComObject(pPin);

                var pStreamSelect = splitter as IAMStreamSelect;
                if (pStreamSelect != null)
                {
                    result = pStreamSelect.GetSelectableStreams().Any(s => s.MajorType == MediaType.Audio);
                }
            }

            return result;
        }

        public void RenderAudio(IGraphBuilder pGraphBuilder, IBaseFilter splitter)
        {
            var pPin = DsUtils.GetPin(splitter, PinDirection.Output, new[] { MediaType.Audio });
            if (pPin != null)
            {
                _streamSelect = splitter as IAMStreamSelect;
                if (_streamSelect != null && BuildSoundRenderer(pGraphBuilder))
                {
                    var pInputPin = DsUtils.GetPin(_directSoundBaseFilter, PinDirection.Input);
                    var hr = pGraphBuilder.Connect(pPin, pInputPin);
                    Marshal.ReleaseComObject(pInputPin);
                    if (hr == DsHlp.S_OK || hr == DsHlp.VFW_S_PARTIAL_RENDER)
                    {
                        _audioStreams.AddRange(_streamSelect.GetSelectableStreams().Where(s => s.MajorType == MediaType.Audio));
                    }
                    else
                    {
                        pGraphBuilder.RemoveFilter(_directSoundBaseFilter);
                        Marshal.FinalReleaseComObject(_directSoundBaseFilter);
                        _directSoundBaseFilter = null;
                        _basicAudio = null;
                    }
                }

                Marshal.ReleaseComObject(pPin);
            }
        }

        private bool BuildSoundRenderer(IGraphBuilder pGraphBuilder)
        {
            var result = false;
            var renderer = pGraphBuilder.AddSoundRenderer();

            if (renderer != null)
            {
                _directSoundBaseFilter = renderer.Item1;
                _basicAudio = renderer.Item2;

                result = true;
            }

            return result;
        }

        public int AudioStreamsCount
        {
            get { return _audioStreams.Count; }
        }

        public int CurrentAudioStream
        {
            get
            {
                var index = 0;

                for (var i = 0; i < _audioStreams.Count; i++)
                {
                    if (_audioStreams[i].Enabled)
                    {
                        index = i;
                        break;
                    }
                }

                return index;
            }
            set
            {
                if (value < 0 || value >= _audioStreams.Count)
                    return;

                var stream = _audioStreams.Skip(value).Take(1).Single();
                if (_streamSelect.SelectStream(stream.Index))
                {
                    stream.Enabled = true;
                    _audioStreams.Where(s => s != stream).ToList().ForEach(s => s.Enabled = false);
                }
            }
        }

        public bool SetVolume(int volume)
        {
            return _basicAudio.put_Volume(volume) == DsHlp.S_OK;
        }

        public bool GetVolume(out int volume)
        {
            return _basicAudio.get_Volume(out volume) == DsHlp.S_OK;
        }

        public string GetAudioStreamName(int nStream)
        {
            if (nStream < 0 || nStream >= _audioStreams.Count)
                return string.Empty;

            return _audioStreams[nStream].Name;
        }

        public void OnExternalStreamSelection()
        {
            _audioStreams.ForEach(s =>
                                  {
                                      s.Enabled = _streamSelect.IsStreamSelected(s.Index);
                                  });
        }

        public void EnumMediaTypes(IPin pin, AMMediaType pinMediaType, Action<AMMediaType> action)
        {
            _audioStreams.ForEach(s => _streamSelect.InspectStream(s.Index, (mt, name, enabled) => action(mt)));
        }
    }
}