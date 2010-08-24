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
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using Dzimchuk.MediaEngine.Core;
using Dzimchuk.DirectShow;
using Dzimchuk.PVP.Resources;

namespace Dzimchuk.PVP
{
    /// <summary>
    /// Summary description for InfoDialog.
    /// </summary>
    public class InfoDialog : System.Windows.Forms.Form
    {
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.ListView list;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public InfoDialog()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();
        }

        public InfoDialog(MediaWindow engine) : this()
        {
            MediaInfo info = engine.MediaInfo;
            if (info != null)
            {			
                InsertListItem(Resources.Resources.infodialog_media_source, info.source);
                InsertListItem(Resources.Resources.infodialog_type_format, info.GetStreamSubType());
                
                string s = String.Empty;
                if (info.StreamSubType != MediaSubType.DVD)
                {
                    long duration = engine.GetDuration();
                    if (duration != 0)
                    {
                        long second;
                        long minute;
                        long h;
                        long remain;
        
                        second = duration/MediaWindow.ONE_SECOND;
                        remain = second%3600;
                        h=second/3600;
                        minute=remain/60;
                        second = remain%60;

                        InsertListItem(Resources.Resources.infodialog_duration, String.Format("{0:d2}:{1:d2}:{2:d2}", h, minute, second));
                    }
                }
                else
                {
                    InsertListItem(String.Empty, String.Empty);
                    DVD_DOMAIN domain;
                    if (engine.GetCurrentDomain(out domain))
                    {
                        switch(domain)
                        {
                            case DVD_DOMAIN.DVD_DOMAIN_FirstPlay:
                                s=Resources.Resources.infodialog_dvddomain_FirstPlay;
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

                        InsertListItem(Resources.Resources.infodialog_Current_Domain, s);
                        if (domain==DVD_DOMAIN.DVD_DOMAIN_Title)
                        {
                            InsertListItem(Resources.Resources.infodialog_Current_Title, engine.CurrentTitle.ToString());
                            InsertListItem(Resources.Resources.infodialog_Current_Chapter, engine.CurrentChapter.ToString());
                            long duration=engine.GetDuration();
                            if (duration != 0)
                            {
                                long second;
                                long minute;
                                long h;
                                long remain;
                                        
                                second = duration/MediaWindow.ONE_SECOND;
                                remain = second%3600;
                                h=second/3600;
                                minute=remain/60;
                                second = remain%60;
                                                
                                InsertListItem(Resources.Resources.infodialog_Title_Duration, String.Format("{0:d2}:{1:d2}:{2:d2}", h, minute, second));
                            }
                        }
                    }
                }
            
                StreamInfo pStreamInfo;
                double d;
                int count = info.NumberOfStreams;
                for (int i=0; i<count; i++)
                {
                    InsertListItem(String.Empty, String.Empty);
        
                    pStreamInfo = info.GetStreamInfo(i);
                    InsertListItem(String.Format(Resources.Resources.infofialog_Stream_format, i+1), pStreamInfo.GetMajorType());

                    if ((pStreamInfo.Flags & StreamInfoFlags.SI_FOURCC)!=0) 
                        InsertListItem(Resources.Resources.infodialog_Format_Type, pStreamInfo.GetVideoSubType());
                    
                    if ((pStreamInfo.Flags & StreamInfoFlags.SI_DVDCOMPRESSION)!=0) 
                        InsertListItem(Resources.Resources.infodialog_Video_Format, pStreamInfo.GetDVDCompressionType());
                            
                    if ((pStreamInfo.Flags & StreamInfoFlags.SI_DVDFRAMEHEIGHT)!=0) 
                        InsertListItem(Resources.Resources.infodialog_TV_System, String.Format(Resources.Resources.infodialog_tv_system_value_format, 
                            pStreamInfo.ulFrameHeight, pStreamInfo.ulFrameRate));
                            
                    if ((pStreamInfo.Flags & StreamInfoFlags.SI_RECT)!=0) 
                        InsertListItem(Resources.Resources.infodialog_Video_Size, String.Format("{0} x {1}", 
                            pStreamInfo.rcSrc.right, pStreamInfo.rcSrc.bottom));
                    
                    if ((pStreamInfo.Flags & StreamInfoFlags.SI_ASPECTRATIO)!=0) 
                        InsertListItem(Resources.Resources.infodialog_Aspect_Ratio, String.Format("{0} : {1}", 
                            pStreamInfo.dwPictAspectRatioX, pStreamInfo.dwPictAspectRatioY));
                    
                    if ((pStreamInfo.Flags & StreamInfoFlags.SI_FRAMERATE)!=0) 
                    {
                        d=MediaWindow.ONE_SECOND;
                        double dTimePerFrame=(double) pStreamInfo.AvgTimePerFrame;
                        d /=dTimePerFrame;
                        InsertListItem(Resources.Resources.infodialog_Frame_Rate, 
                            String.Format(Resources.Resources.infodialog_framerate_value_format, d));
                    }

            //		if ((pStreamInfo.Flags & StreamInfoFlags.SI_DVDFRAMERATE)!=0)
            //			InsertListItem("Frame Rate", pStreamInfo.ulFrameRate);
                            
                    if ((pStreamInfo.Flags & StreamInfoFlags.SI_INTERLACEMODE)!=0) 
                        InsertListItem(Resources.Resources.infodialog_Interlace_Mode, pStreamInfo.GetInterlaceMode());
                    
                    if ((pStreamInfo.Flags & StreamInfoFlags.SI_WAVEFORMAT)!=0) 
                        InsertListItem(Resources.Resources.infodialog_Format_Type, pStreamInfo.GetWaveFormat());
                    
                    if ((pStreamInfo.Flags & StreamInfoFlags.SI_DVDAUDIOFORMAT)!=0) 
                        InsertListItem(Resources.Resources.infodialog_Format_Type, pStreamInfo.GetDVDAudioFormat());
                            
                    if ((pStreamInfo.Flags & StreamInfoFlags.SI_DVDAUDIOSTREAMNAME)!=0) 
                        InsertListItem(Resources.Resources.infodialog_Language, pStreamInfo.strDVDAudioStreamName);
                            
                    s = String.Empty;
                    string s1;
                    if ((pStreamInfo.Flags & StreamInfoFlags.SI_SAMPLERATE)!=0) 
                        s = String.Format(Resources.Resources.infodialog_samplerate_value_format, pStreamInfo.nSamplesPerSec);
                    
                    if ((pStreamInfo.Flags & StreamInfoFlags.SI_DVDFREQUENCY)!=0) 
                        s = String.Format(Resources.Resources.infodialog_dvd_frequency_value_format, pStreamInfo.dwFrequency);
                
                    if ((pStreamInfo.Flags & StreamInfoFlags.SI_WAVECHANNELS)!=0) 
                    {
                        if (pStreamInfo.nChannels == 1)
                            s1 = String.Format(Resources.Resources.infodialog_channel_format, pStreamInfo.nChannels);
                        else
                            s1 = String.Format(Resources.Resources.infodialog_channels_format, pStreamInfo.nChannels);
                        s+=s1;
                    }

                    if ((pStreamInfo.Flags & StreamInfoFlags.SI_BITSPERSAMPLE) != 0 
                        && pStreamInfo.wBitsPerSample != 0) 
                    {
                        s1 = String.Format(Resources.Resources.infodialog_bits_per_sample_format, pStreamInfo.wBitsPerSample);
                        s+=s1;
                    }

                    if (s.Length != 0)
                        InsertListItem(Resources.Resources.infodialog_Format, s);

                    if ((pStreamInfo.Flags & StreamInfoFlags.SI_AUDIOBITRATE)!=0) 
                        InsertListItem(Resources.Resources.infodialog_Bit_Rate, 
                            String.Format(Resources.Resources.infodialog_bitrate_value_format, 8*pStreamInfo.nAvgBytesPerSec/1000));
                    
                }
            }

            list.Columns[1].Width = -2;
        }

        void InsertListItem(string str1, string str2)
        {
            ListViewItem item = new ListViewItem(str1);
            item.SubItems.Add(str2);
            list.Items.Add(item);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose( bool disposing )
        {
            if( disposing )
            {
                if(components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose( disposing );
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InfoDialog));
            this.list = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
            this.btnOK = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // list
            // 
            this.list.AccessibleDescription = null;
            this.list.AccessibleName = null;
            resources.ApplyResources(this.list, "list");
            this.list.BackgroundImage = null;
            this.list.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.list.Font = null;
            this.list.GridLines = true;
            this.list.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.list.MultiSelect = false;
            this.list.Name = "list";
            this.list.UseCompatibleStateImageBehavior = false;
            this.list.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            resources.ApplyResources(this.columnHeader1, "columnHeader1");
            // 
            // columnHeader2
            // 
            resources.ApplyResources(this.columnHeader2, "columnHeader2");
            // 
            // btnOK
            // 
            this.btnOK.AccessibleDescription = null;
            this.btnOK.AccessibleName = null;
            resources.ApplyResources(this.btnOK, "btnOK");
            this.btnOK.BackgroundImage = null;
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnOK.Font = null;
            this.btnOK.Name = "btnOK";
            // 
            // InfoDialog
            // 
            this.AcceptButton = this.btnOK;
            this.AccessibleDescription = null;
            this.AccessibleName = null;
            resources.ApplyResources(this, "$this");
            this.BackgroundImage = null;
            this.CancelButton = this.btnOK;
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.list);
            this.Font = null;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = null;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "InfoDialog";
            this.ShowInTaskbar = false;
            this.ResumeLayout(false);

        }
        #endregion
    }
}
