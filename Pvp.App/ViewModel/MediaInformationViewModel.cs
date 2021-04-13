using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Pvp.App.Messaging;
using Pvp.Core.DirectShow;
using Pvp.Core.MediaEngine;
using Pvp.Core.MediaEngine.Description;

namespace Pvp.App.ViewModel
{
    public class MediaInformationViewModel : ViewModelBase
    {
        private readonly IMediaEngineFacade _engine;
        private readonly List<MediaInfoItem> _infoList;

        private ICommand _okCommand;

        public MediaInformationViewModel(IMediaEngineFacade engine)
        {
            _engine = engine;
            _infoList = new List<MediaInfoItem>();

            Load();
        }

        private void Load()
        {
            var info = _engine.MediaInfo;
            if (info != null)
            {
                AddInfoItem(Resources.Resources.infodialog_media_source, info.source);
                AddInfoItem(Resources.Resources.infodialog_type_format, info.GetStreamSubType());

                string s = String.Empty;
                if (info.StreamSubType != MediaSubType.DVD)
                {
                    var duration = _engine.Duration;
                    if (duration != TimeSpan.Zero)
                    {
                        AddInfoItem(Resources.Resources.infodialog_duration,
                                    string.Format("{0:d2}:{1:d2}:{2:d2}", duration.Hours, duration.Minutes, duration.Seconds));
                    }
                }
                else
                {
                    AddInfoItem(String.Empty, String.Empty);
                    DVD_DOMAIN domain;
                    if (_engine.GetCurrentDomain(out domain))
                    {
                        switch (domain)
                        {
                            case DVD_DOMAIN.DVD_DOMAIN_FirstPlay:
                                s = Resources.Resources.infodialog_dvddomain_FirstPlay;
                                break;
                            case DVD_DOMAIN.DVD_DOMAIN_VideoManagerMenu:
                                s = Resources.Resources.infodialog_dvddomain_VideoManagerMenu;
                                break;
                            case DVD_DOMAIN.DVD_DOMAIN_VideoTitleSetMenu:
                                s = Resources.Resources.infodialog_dvddomain_VideoTitleSetMenu;
                                break;
                            case DVD_DOMAIN.DVD_DOMAIN_Title:
                                s = Resources.Resources.infodialog_dvddomain_Title;
                                break;
                            case DVD_DOMAIN.DVD_DOMAIN_Stop:
                                s = Resources.Resources.infodialog_dvddomain_Stop;
                                break;
                        }

                        AddInfoItem(Resources.Resources.infodialog_Current_Domain, s);
                        if (domain == DVD_DOMAIN.DVD_DOMAIN_Title)
                        {
                            AddInfoItem(Resources.Resources.infodialog_Current_Title, _engine.CurrentTitle.ToString());
                            AddInfoItem(Resources.Resources.infodialog_Current_Chapter, _engine.CurrentChapter.ToString());
                            var duration = _engine.Duration;
                            if (duration != TimeSpan.Zero)
                            {
                                AddInfoItem(Resources.Resources.infodialog_Title_Duration,
                                            string.Format("{0:d2}:{1:d2}:{2:d2}", duration.Hours, duration.Minutes, duration.Seconds));
                            }
                        }
                    }
                }

                double d;
                int count = info.NumberOfStreams;
                for (int i = 0; i < count; i++)
                {
                    AddInfoItem(String.Empty, String.Empty);

                    var pStreamInfo = info.GetStreamInfo(i);
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
                        d = CoreDefinitions.ONE_SECOND;
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

                    s = String.Empty;
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
                }
            }
        }

        private void AddInfoItem(string name, string value)
        {
            _infoList.Add(new MediaInfoItem(name, value));
        }

        public IEnumerable<MediaInfoItem> MediaInfo
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
                        () =>
                        {
                            Messenger.Default.Send<CommandMessage>(new CommandMessage(Command.MediaInformationWindowClose));
                        });
                }

                return _okCommand;
            }
        }
    }
}