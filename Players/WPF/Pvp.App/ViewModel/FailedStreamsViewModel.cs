using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Pvp.App.Messaging;
using Pvp.Core.MediaEngine;
using Pvp.Core.MediaEngine.Description;

namespace Pvp.App.ViewModel
{
    public class FailedStreamsViewModel : ViewModelBase
    {
        private readonly IFailedStreamsContainer _failedStreamsContainer;
        private readonly List<MediaInfoItem> _infoList;

        private ICommand _okCommand;

        public FailedStreamsViewModel(IFailedStreamsContainer failedStreamsContainer)
        {
            _failedStreamsContainer = failedStreamsContainer;
            _infoList = new List<MediaInfoItem>();

            Load();
        }

        private void Load()
        {
            var streams = _failedStreamsContainer.GetFailedStreams().ToArray();

            int count = streams.Count();
            for (int i = 0; i < count; i++)
            {
                StreamInfo pStreamInfo = streams[i];
                AddInfoItem(String.Format(Resources.Resources.infodialog_Stream_format, i + 1), pStreamInfo.GetMajorType());

                if ((pStreamInfo.Flags & StreamInfoFlags.SI_FOURCC) != 0)
                    AddInfoItem(Resources.Resources.infodialog_Format_Type, pStreamInfo.GetVideoSubType());

                if ((pStreamInfo.Flags & StreamInfoFlags.SI_DVDCOMPRESSION) != 0)
                    AddInfoItem(Resources.Resources.infodialog_Video_Format, pStreamInfo.GetDVDCompressionType());

                if ((pStreamInfo.Flags & StreamInfoFlags.SI_DVDFRAMEHEIGHT) != 0)
                    AddInfoItem(Resources.Resources.infodialog_TV_System, String.Format(Resources.Resources.infodialog_tv_system_value_format,
                                                                                        pStreamInfo.ulFrameHeight, pStreamInfo.ulFrameRate));

                if ((pStreamInfo.Flags & StreamInfoFlags.SI_RECT) != 0)
                    AddInfoItem(Resources.Resources.infodialog_Video_Size, String.Format("{0} x {1}",
                                                                                         pStreamInfo.rcSrc.right, pStreamInfo.rcSrc.bottom));

                if ((pStreamInfo.Flags & StreamInfoFlags.SI_ASPECTRATIO) != 0)
                    AddInfoItem(Resources.Resources.infodialog_Aspect_Ratio, String.Format("{0} : {1}",
                                                                                           pStreamInfo.dwPictAspectRatioX, pStreamInfo.dwPictAspectRatioY));

                if ((pStreamInfo.Flags & StreamInfoFlags.SI_FRAMERATE) != 0)
                {
                    double d = CoreDefinitions.ONE_SECOND;
                    double dTimePerFrame = pStreamInfo.AvgTimePerFrame;
                    d /= dTimePerFrame;
                    AddInfoItem(Resources.Resources.infodialog_Frame_Rate,
                                String.Format(Resources.Resources.infodialog_framerate_value_format, d));
                }

                //		if ((pStreamInfo.Flags & StreamInfoFlags.SI_DVDFRAMERATE)!=0)
                //			AddInfoItem("Frame Rate", pStreamInfo.ulFrameRate);

                if ((pStreamInfo.Flags & StreamInfoFlags.SI_INTERLACEMODE) != 0)
                    AddInfoItem(Resources.Resources.infodialog_Interlace_Mode, pStreamInfo.GetInterlaceMode());

                if ((pStreamInfo.Flags & StreamInfoFlags.SI_WAVEFORMAT) != 0)
                    AddInfoItem(Resources.Resources.infodialog_Format_Type, pStreamInfo.GetWaveFormat());

                if ((pStreamInfo.Flags & StreamInfoFlags.SI_DVDAUDIOFORMAT) != 0)
                    AddInfoItem(Resources.Resources.infodialog_Format_Type, pStreamInfo.GetDVDAudioFormat());

                if ((pStreamInfo.Flags & StreamInfoFlags.SI_DVDAUDIOSTREAMNAME) != 0)
                    AddInfoItem(Resources.Resources.infodialog_Language, pStreamInfo.strDVDAudioStreamName);

                string s = String.Empty;
                string s1;
                if ((pStreamInfo.Flags & StreamInfoFlags.SI_SAMPLERATE) != 0)
                    s = String.Format(Resources.Resources.infodialog_samplerate_value_format, pStreamInfo.nSamplesPerSec);

                if ((pStreamInfo.Flags & StreamInfoFlags.SI_DVDFREQUENCY) != 0)
                    s = String.Format(Resources.Resources.infodialog_dvd_frequency_value_format, pStreamInfo.dwFrequency);

                if ((pStreamInfo.Flags & StreamInfoFlags.SI_WAVECHANNELS) != 0)
                {
                    if (pStreamInfo.nChannels == 1)
                        s1 = String.Format(Resources.Resources.infodialog_channel_format, pStreamInfo.nChannels);
                    else
                        s1 = String.Format(Resources.Resources.infodialog_channels_format, pStreamInfo.nChannels);
                    s += s1;
                }

                if ((pStreamInfo.Flags & StreamInfoFlags.SI_BITSPERSAMPLE) != 0
                    && pStreamInfo.wBitsPerSample != 0)
                {
                    s1 = String.Format(Resources.Resources.infodialog_bits_per_sample_format, pStreamInfo.wBitsPerSample);
                    s += s1;
                }

                if (s.Length != 0)
                    AddInfoItem(Resources.Resources.infodialog_Format, s);

                if ((pStreamInfo.Flags & StreamInfoFlags.SI_AUDIOBITRATE) != 0)
                    AddInfoItem(Resources.Resources.infodialog_Bit_Rate,
                                String.Format(Resources.Resources.infodialog_bitrate_value_format, 8 * pStreamInfo.nAvgBytesPerSec / 1000));

                AddInfoItem(String.Empty, String.Empty);
            }
        }

        private void AddInfoItem(string name, string value)
        {
            _infoList.Add(new MediaInfoItem(name, value));
        }

        public IEnumerable<MediaInfoItem> FailedStreams
        {
            get { return _infoList; }
        }

        public ICommand OkCommand
        {
            get
            {
                if (_okCommand == null)
                {
                    _okCommand = new RelayCommand(
                        () => { Messenger.Default.Send(new CommandMessage(Command.FailedStreamsWindowClose)); });
                }

                return _okCommand;
            }
        }
    }
}