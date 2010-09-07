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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Globalization;
using Dzimchuk.MediaEngine.Core;
using Dzimchuk.Theme;
using Dzimchuk.DirectShow;
using Dzimchuk.Native;
using Dzimchuk.AUI;
using System.Text.RegularExpressions;

namespace Dzimchuk.PVP
{
    /// <summary>
    /// 
    /// </summary>
    public class MainFormControls : MainFormFile
    {
        private const int DEFAULT_VOLUME = -1000;
        
        protected MouseWheelAction wheelAction = MouseWheelAction.Volume;
        protected bool bRememberVolume = true;
        protected string screenshotsFolder = DefaultScreenshotsFolder;
        
        int nVolume = DEFAULT_VOLUME;
        bool bMute;     
        
        bool bCanChangeVolume;
        bool bNeedUpdate;
        protected bool bOkToHideCursor = true;
        bool bCursorVisible = true;
        int nCursorCount;
        int nControlbarCount;

        bool bContextMenu;
        bool bModifyMenuPending;
        
        const int VOLUME_STEP = 200;
        const int WHEEL_DELTA = 120;

        protected MenuItemEx miApp, miPlay, miPause, miStop, miRepeat, miFullscreen, 
            miVideoSize, miAspectRatio, miRate, miVolume;
        private MenuItemEx miVideoSize50, miVideoSize100, miVideoSize200, miVideoSizeFree, 
            miAspectRatioOriginal, miAspectRatioFree, 
            miVolumeUp, miVolumeDown, miVolumeMute;
        // dynamic menus
        private MenuItemEx miFilters, miStreams, goto_menu, angles_menu, menus_menu, miTitle, 
            miRoot, miSubpicture, miAudio, miAngle, miChapter, lang_menu, streams_menu;
        private MenuItemEx miCloseMenu;

        protected Hashtable htKeys;
        protected Hashtable htCommands;

        private static string DefaultScreenshotsFolder
        {
            get { return Environment.GetFolderPath(Environment.SpecialFolder.Desktop); }
        }
                    
        public MainFormControls()
        {
            htCommands = new Hashtable(40);
            htCommands.Add(SettingsForm.strKeysFileOpen, miOpen);
            htCommands.Add(SettingsForm.strKeysFileClose, miClose);
            htCommands.Add(SettingsForm.strKeysFileInfo, miInfo);
            htCommands.Add(SettingsForm.strKeysBack, new MenuItemEx("Back", new EventHandler(OnBack)));
            htCommands.Add(SettingsForm.strKeysForth, new MenuItemEx("Forth", new EventHandler(OnForth)));
            htCommands.Add(SettingsForm.strKeysScreenshot, new MenuItemEx("Screenshot", new EventHandler(OnTakeScreenshot)));
            
            CreateControlsMenu();
            NormalizeShortcuts();

            mediaWindowHost.MW_DoubleClick += new EventHandler(OnFullscreen);
            mediaWindowHost.MW_ContextMenu += new ContextMenuHandler(OnContextMenu);
            mediaWindowHost.MW_Click += new EventHandler(engine_MW_Click);
            engine.Update += new EventHandler(engine_Update);
            engine.ModifyMenu += new EventHandler(engine_ModifyMenu);
            
            mediaWindowHost.MW_MouseMove += new EventHandler(engine_MW_MouseMove);
            mediaWindowHost.MW_MouseLeave += new EventHandler(engine_MW_MouseLeave);
                        
            nicon.BeforeShowMenu += new EventHandler(OnBeforeShowMenu);
            nicon.AfterShowMenu += new EventHandler(OnAfterShowMenu);
            
            controlbar.volumebar.Scroll += new EventHandler(OnScrollVolumeBar);
            controlbar.seekbar.Scroll += new EventHandler(OnScrollSeekBar);
                        
            controlbar.btnPlay.Click += new EventHandler(OnPlay);
            controlbar.btnPause.Click += new EventHandler(OnPause);
            controlbar.btnStop.Click += new EventHandler(OnStop);
            controlbar.btnBackward.Click += new EventHandler(OnBack);
            controlbar.btnForward.Click += new EventHandler(OnForth);
            controlbar.btnToBegining.Click += new EventHandler(OnToBegining);
            controlbar.btnToEnd.Click += new EventHandler(OnToEnd);
            controlbar.btnRepeat.Click += new EventHandler(OnRepeat);
            controlbar.btnMute.Click += new EventHandler(OnMute);

            controlbarHolder.KeyDown += new KeyEventHandler(controlbarHolder_KeyDown);
            controlbarHolder.MouseWheel += new MouseEventHandler(controlbarHolder_MouseWheel);
            
            UpdateButtons();
                                    
            Timer timer = new Timer();
            timer.Interval = 500;
            timer.Tick += new EventHandler(OnTimer);
            timer.Start();

            // timer for simulating keyboard activity
            Timer timer2 = new Timer();
            timer2.Interval = 30 * 1000;
            timer2.Tick += new EventHandler(timer2_Tick);
            timer2.Start();
        }

        private const string SHIFT_KEY = "+";
        private bool bIgnoreNextShift = false;
        private void timer2_Tick(object sender, EventArgs e)
        {
            if (ContainsFocus && engine.GraphState != GraphState.Stopped &&
                engine.GraphState != GraphState.Reset)
            {
                bIgnoreNextShift = true;
                SendKeys.Send(SHIFT_KEY);
            }
        }

        protected override void SetMenuItemsText()
        {
            base.SetMenuItemsText();
            miPlay.Text = Resources.Resources.mi_play;
            miPause.Text = Resources.Resources.mi_pause;
            miStop.Text = Resources.Resources.mi_stop;
            miRepeat.Text = Resources.Resources.mi_repeat;
            miFullscreen.Text = Resources.Resources.mi_fullscreen;

            CultureInfo ci = new CultureInfo(System.Threading.Thread.CurrentThread.CurrentCulture.Name);
            NumberFormatInfo nfi = ci.NumberFormat;
            nfi.PercentDecimalDigits = 0;
            miVideoSize.Text = Resources.Resources.mi_video_size;
            miVideoSize50.Text = 0.5.ToString("P", nfi);
            miVideoSize100.Text = 1.ToString("P", nfi);
            miVideoSize200.Text = 2.ToString("P", nfi);
            miVideoSizeFree.Text = Resources.Resources.mi_video_size_free;

            miAspectRatio.Text = Resources.Resources.mi_aspect_ratio;
            miAspectRatioOriginal.Text = Resources.Resources.mi_aspect_ratio_original;
            miAspectRatioFree.Text = Resources.Resources.mi_aspect_ratio_free;
            miRate.Text = Resources.Resources.mi_playrate;
            miVolume.Text = Resources.Resources.mi_volume;
            miVolumeUp.Text = Resources.Resources.mi_volume_up;
            miVolumeDown.Text = Resources.Resources.mi_volume_down;
            miVolumeMute.Text = Resources.Resources.mi_volume_mute;

            // recreate dynamic menus
            engine_ModifyMenu(this, EventArgs.Empty);
        }

        #region CreateControlsMenu
        void CreateControlsMenu()
        {
            contextMenu.Popup += new EventHandler(OnContextPopup);
            
            miPlay = new MenuItemEx();
            miPlay.Click += new EventHandler(OnPlay);
            
            miPause = new MenuItemEx();
            miPause.Click += new EventHandler(OnPause);
            
            miStop = new MenuItemEx();
            miStop.Click += new EventHandler(OnStop);

            miRepeat = new MenuItemEx();
            miRepeat.Click += new EventHandler(OnRepeat);

            miFullscreen = new MenuItemEx();
            miFullscreen.Click += new EventHandler(OnFullscreen);

            htCommands.Add(SettingsForm.strKeysPlay, miPlay);
            htCommands.Add(SettingsForm.strKeysPause, new MenuItemEx("Pause2", new EventHandler(OnPause2)));
            htCommands.Add(SettingsForm.strKeysStop, miStop);
            htCommands.Add(SettingsForm.strKeysRepeat, miRepeat);
            htCommands.Add(SettingsForm.strKeysFullscreen, miFullscreen);
            
            miVideoSize = new MenuItemEx();
            miVideoSize.Popup += new EventHandler(OnVideoSizePopup);

            miVideoSize50 = new MenuItemEx();
            miVideoSize50.Click += new EventHandler(OnVideoSize50);

            miVideoSize100 = new MenuItemEx();
            miVideoSize100.Click += new EventHandler(OnVideoSize100);

            miVideoSize200 = new MenuItemEx();
            miVideoSize200.Click += new EventHandler(OnVideoSize200);

            miVideoSizeFree = new MenuItemEx();
            miVideoSizeFree.Click += new EventHandler(OnVideoSizeFree);

            miVideoSize.MenuItems.AddRange(new MenuItem[] { miVideoSize50, miVideoSize100, miVideoSize200, 
                                                            sep.CloneMenu(), miVideoSizeFree });

            htCommands.Add(SettingsForm.strKeys50, miVideoSize50);
            htCommands.Add(SettingsForm.strKeys100, miVideoSize100);
            htCommands.Add(SettingsForm.strKeys200, miVideoSize200);
            htCommands.Add(SettingsForm.strKeysFree, miVideoSizeFree);

            MenuItemEx cmd, cmd1, cmd2, cmd3;
            
            miAspectRatio = new MenuItemEx();
            miAspectRatio.Popup += new EventHandler(OnAspectRatioPopup);
            EventHandler eh = new EventHandler(OnAspectRatio);
            miAspectRatioOriginal = new MenuItemEx();
            miAspectRatioOriginal.Click += eh;
            miAspectRatioOriginal.Tag = AspectRatio.AR_ORIGINAL;
            
            cmd1 = new MenuItemEx(SettingsForm.strKeysAspect16_9);
            cmd1.Click += eh;
            cmd1.Tag = AspectRatio.AR_16x9;
            
            cmd2 = new MenuItemEx(SettingsForm.strKeysAspect4_3);
            cmd2.Click += eh;
            cmd2.Tag = AspectRatio.AR_4x3;
            
            cmd3 = new MenuItemEx(SettingsForm.strKeysAspect47_20);
            cmd3.Click += eh;
            cmd3.Tag = AspectRatio.AR_47x20;
            
            miAspectRatioFree = new MenuItemEx();
            miAspectRatioFree.Click += eh;
            miAspectRatioFree.Tag = AspectRatio.AR_FREE;

            miAspectRatio.MenuItems.AddRange(new MenuItem[] { miAspectRatioOriginal, sep.CloneMenu(), 
                                                    cmd2, cmd1, cmd3, sep.CloneMenu(), miAspectRatioFree });

            htCommands.Add(SettingsForm.strKeysAspectOriginal, miAspectRatioOriginal);
            htCommands.Add(SettingsForm.strKeysAspect16_9, cmd1);
            htCommands.Add(SettingsForm.strKeysAspect4_3, cmd2);
            htCommands.Add(SettingsForm.strKeysAspect47_20, cmd3);
            htCommands.Add(SettingsForm.strKeysAspectFree, miAspectRatioFree);
            
            miRate = new MenuItemEx();
            miRate.Popup += new EventHandler(OnRatePopup);

            double[] rates = {0.50, 0.75, 1.00, 1.25, 1.50, 2.00};
            eh = new EventHandler(OnPlayrate);
            CultureInfo ci = new CultureInfo(System.Threading.Thread.CurrentThread.CurrentCulture.Name);
            NumberFormatInfo nfi = ci.NumberFormat;
            nfi.PercentDecimalDigits = 0;
            for (int i = 0; i < rates.Length; i++)
            {
                cmd = new MenuItemEx(rates[i].ToString("P", nfi));
                cmd.Click += eh;
                cmd.Tag = rates[i];
                miRate.MenuItems.Add(cmd);
            }

            miVolume = new MenuItemEx();
            miVolume.Popup += new EventHandler(OnVolumePopup);

            miVolumeUp = new MenuItemEx();
            miVolumeUp.Click += new EventHandler(OnVolumeUp);

            miVolumeDown = new MenuItemEx();
            miVolumeDown.Click += new EventHandler(OnVolumeDown);

            miVolumeMute = new MenuItemEx();
            miVolumeMute.Click += new EventHandler(OnMute);

            miVolume.MenuItems.AddRange(new MenuItem[] { miVolumeUp, miVolumeDown, miVolumeMute });

            htCommands.Add(SettingsForm.strKeysUp, miVolumeUp);
            htCommands.Add(SettingsForm.strKeysDown, miVolumeDown);
            htCommands.Add(SettingsForm.strKeysMute, miVolumeMute);
        }
        #endregion

        private void NormalizeShortcuts()
        {
            if (htKeys == null)
                htKeys = SettingsForm.htDefault;
        }

        protected override void LoadSettings(PropertyBag props)
        {
            base.LoadSettings(props);
            nVolume = props.Get<int>("volume_level", DEFAULT_VOLUME);
            bMute = props.Get<bool>("mute_on", false);
            bRememberVolume = props.Get<bool>("remember_volume", true); ;
            if (!bRememberVolume)
            {
                nVolume = -1000;
                bMute = false;
            }
            wheelAction = props.Get<MouseWheelAction>("wheel_action", MouseWheelAction.Volume);

            screenshotsFolder = props.Get("screenshots_folder", DefaultScreenshotsFolder);

            htKeys = props.Get<Hashtable>("keys_definition", null);
        }

        protected override void SaveSettings(PropertyBag props)
        {
            base.SaveSettings(props);
            props.Add<int>("volume_level", nVolume);
            props.Add<bool>("mute_on", bMute);
            props.Add<bool>("remember_volume", bRememberVolume);
            props.Add<MouseWheelAction>("wheel_action", wheelAction);
            props.Add("screenshots_folder", screenshotsFolder);

            props.Add("keys_definition", htKeys);
        }

        protected void PlayIt(string source, WhatToPlay CurrentlyPlaying)
        {
            bNeedUpdate = false;
            controlbar.dclock.ResetDClock();
            controlbar.seekbar.Enabled = false;

            if (mediaWindowHost.BuildGraph(source, CurrentlyPlaying))
            {
                int volume;
                if (engine.GetVolume(out volume))
                {
                    bCanChangeVolume = true;
                    engine.SetVolume(bMute ? -10000 : nVolume);
                }
                bNeedUpdate = true;
            }

            engine_ModifyMenu(engine, EventArgs.Empty); 
            UpdateButtons();
        }

        protected override void OnFileInfo(object sender, EventArgs e)
        {
            bOkToHideCursor = false;
            ShowCursor();
            base.OnFileInfo (sender, e);
            bOkToHideCursor = true;
        }

        protected override void OnPlayDVD(object sender, EventArgs e)
        {
            bOkToHideCursor = false;
            ShowCursor();
            base.OnPlayDVD (sender, e);
            bOkToHideCursor = true;
        }

        protected override void OnFileOpen(object sender, EventArgs e)
        {
            bOkToHideCursor = false;
            ShowCursor();
            base.OnFileOpen (sender, e);
            bOkToHideCursor = true;
        }

        protected override void OnFileClose(object sender, EventArgs e)
        {
            base.OnFileClose (sender, e);
            bNeedUpdate = false;
            bCanChangeVolume = false;
            controlbar.dclock.ResetDClock();

            engine_ModifyMenu(engine, EventArgs.Empty);
            UpdateButtons();
        }
        
        private void OnContextPopup(object sender, EventArgs e)
        {
            GraphState state = engine.GraphState;

            miPlay.Enabled = state != GraphState.Running && 
                state != GraphState.Reset;
            miPause.Enabled = state == GraphState.Running;
            miStop.Enabled = state == GraphState.Running || 
                state == GraphState.Paused;
            miRepeat.Checked = engine.Repeat;
            miFullscreen.Checked = bFullscreen;

            if (miCloseMenu != null && contextMenu.MenuItems.Contains(miCloseMenu))
                miCloseMenu.Enabled = engine.IsResumeDVDEnabled();
        }

        private void OnPlay(object sender, EventArgs e)
        {
            engine.ResumeGraph();
            UpdateButtons();
        }

        private void OnPause(object sender, EventArgs e)
        {
            engine.PauseGraph();
            UpdateButtons();
        }

        private void OnPause2(object sender, EventArgs e)
        {
            switch(engine.GraphState)
            {
                case GraphState.Running:
                    engine.PauseGraph(); 
                    break;
                case GraphState.Paused:
                    engine.ResumeGraph();
                    break;
            }
            UpdateButtons();
        }

        private void OnStop(object sender, EventArgs e)
        {
            engine.StopGraph();
            ResetSeekBar();
            UpdateButtons();
        }

        private void OnRepeat(object sender, EventArgs e)
        {
            engine.Repeat=!engine.Repeat;
            UpdateButtons();
        }

        private void OnFullscreen(object sender, EventArgs e)
        {
            ToggleFullscreen();
        }

        void UpdateButtons()
        {
            GraphState state = engine.GraphState;
            bool bSeekable = engine.IsGraphSeekable;
            
            controlbar.btnPlay.Enabled = state != GraphState.Running && 
                state != GraphState.Reset;
            controlbar.btnPause.Enabled = state == GraphState.Running;
            controlbar.btnStop.Enabled = state == GraphState.Running || 
                state == GraphState.Paused;
            controlbar.btnBackward.Enabled = bSeekable;
            controlbar.btnForward.Enabled = bSeekable;
            controlbar.btnToBegining.Enabled = bSeekable;
            controlbar.btnToEnd.Enabled = bSeekable;
            controlbar.btnRepeat.Pressed = engine.Repeat;
            controlbar.btnMute.Pressed = bMute;

            controlbar.volumebar.UpdateTrackBar(nVolume+ControlBar.VOLUME_RANGE, ControlBar.VOLUME_RANGE);
            controlbar.seekbar.Enabled = bSeekable;
            if (!bSeekable)
                ResetSeekBar();
        }

        void ResetSeekBar()
        {
            controlbar.seekbar.UpdateTrackBar(0, controlbar.seekbar.Range);
        }

        private void OnScrollVolumeBar(object sender, EventArgs e)
        {
            nVolume = (int)controlbar.volumebar.CurrentPostion - ControlBar.VOLUME_RANGE;
            if (!bMute && bCanChangeVolume)
                engine.SetVolume(nVolume);
        }

        private void OnScrollSeekBar(object sender, EventArgs e)
        {
            long duration=engine.GetDuration();
            double currentTime=controlbar.seekbar.CurrentPostion;
            long time=(long) currentTime*CoreDefinitions.ONE_SECOND;
            engine.SetCurrentPosition(time);
        }

        private void OnVolumePopup(object sender, EventArgs e)
        {
            MenuItemEx popup = (MenuItemEx)sender;
            popup.MenuItems[0].Enabled = nVolume<0;
            popup.MenuItems[1].Enabled = nVolume > -ControlBar.VOLUME_RANGE;
            popup.MenuItems[2].Checked = bMute;
        }

        private void OnVolumeUp(object sender, EventArgs e)
        {
            nVolume+=VOLUME_STEP;
            if (nVolume>0)
                nVolume=0;
    
            UpdateButtons();
            if (!bMute && bCanChangeVolume)
                engine.SetVolume(nVolume);
        }

        private void OnVolumeDown(object sender, EventArgs e)
        {
            nVolume-=VOLUME_STEP;
            if (nVolume < -ControlBar.VOLUME_RANGE)
                nVolume = -ControlBar.VOLUME_RANGE;
            
            UpdateButtons();
            if (!bMute && bCanChangeVolume)
                engine.SetVolume(nVolume);
        }

        private void OnMute(object sender, EventArgs e)
        {
            if (bMute)
            {
                bMute=false;
                engine.SetVolume(nVolume);
            }
            else
            {
                bMute=true;
                engine.SetVolume(-10000);
            }
            UpdateButtons();
        }

        private void OnBack(object sender, EventArgs e)
        {
            if (engine.IsGraphSeekable)
            {
                long time=engine.GetCurrentPosition();
                if (time > 5 * CoreDefinitions.ONE_SECOND)
                    time -= 5 * CoreDefinitions.ONE_SECOND;
                else
                    time=0;
                engine.SetCurrentPosition(time);
            }
        }

        private void OnForth(object sender, EventArgs e)
        {
            if (engine.IsGraphSeekable)
            {
                long time=engine.GetCurrentPosition();
                long duration=engine.GetDuration();
                if (duration - time > 5 * CoreDefinitions.ONE_SECOND)
                    time += 5 * CoreDefinitions.ONE_SECOND;
                else
                    time=duration;
                engine.SetCurrentPosition(time);
            }
        }

        private void OnToBegining(object sender, EventArgs e)
        {
            engine.SetCurrentPosition(0);
        }

        private void OnToEnd(object sender, EventArgs e)
        {
            long duration = engine.GetDuration();
            engine.SetCurrentPosition(duration-2500000);
            engine.PauseGraph();
            UpdateButtons();
        }

        private void OnTimer(object sender, EventArgs e)
        {
            if (bNeedUpdate)
            {
                double dCurrentTime, dDuration;
                string strTime;
                long duration=engine.GetDuration();
                if (duration!=0)
                {
                    long time = engine.GetCurrentPosition();
                    long second, _second;
                    long minute, _minute;
                    long h, _h;
                    long remain;

                    _second = time / CoreDefinitions.ONE_SECOND;
                    dCurrentTime = (double) _second;
                    remain = _second%3600;
                    _h=_second/3600;
                    _minute=remain/60;
                    _second = remain%60;

                    second = duration / CoreDefinitions.ONE_SECOND;
                    dDuration = (double) second;
                    remain = second%3600;
                    h=second/3600;
                    minute=remain/60;
                    second = remain%60;
            
                    strTime = String.Format("{0:d2}:{1:d2}:{2:d2} / {3:d2}:{4:d2}:{5:d2}", _h, _minute, _second, h, minute, second);
                }
                else
                {
                    dCurrentTime=dDuration=0.0;
                    strTime = String.Empty;
                }
                controlbar.dclock.Text = strTime;
                
                if (engine.IsGraphSeekable)
                    controlbar.seekbar.UpdateTrackBar(dCurrentTime, dDuration);

                if (bCursorVisible && ++nCursorCount > 4)
                {
                    nCursorCount = 0;
                    if (bOkToHideCursor && bFullscreen && 
                        !controlbarHolder.Visible)
                    {
                        Cursor.Hide();
                        bCursorVisible = false;
                    }
                }
            }

            if (bFullscreen && controlbarHolder.Visible)
            {
                if (++nControlbarCount > 3)
                {
                    nControlbarCount = 0;
                    Point point = PointToClient(Cursor.Position);
                    if (point.Y < Height-controlbarHolder.Height)
                        controlbarHolder.Visible = false;
                }
            }

            if (ActiveControl != null)
                ActiveControl = null;
        }

        protected void ShowCursor()
        {
            if (!bCursorVisible)
            {
                Cursor.Show();
                nCursorCount = 0;
                bCursorVisible = true;
            }
        }
        
        private void engine_MW_MouseMove(object sender, EventArgs e)
        {
            ShowCursor();
            if (bFullscreen && !controlbarHolder.Visible)
            {
                Point point = PointToClient(Cursor.Position);
                if (point.Y > Height-controlbarHolder.Height)
                {
                    nControlbarCount = 0;
                    controlbarHolder.Visible = true;
                }
            }
        }

        private void engine_MW_MouseLeave(object sender, EventArgs e)
        {
            ShowCursor();
        }

        private void OnVideoSizePopup(object sender, EventArgs e)
        {
            MenuItemEx popup = (MenuItemEx)sender;
            VideoSize size = engine.GetVideoSize();
            popup.MenuItems[0].Checked = size == VideoSize.SIZE50;
            popup.MenuItems[1].Checked = size == VideoSize.SIZE100;
            popup.MenuItems[2].Checked = size == VideoSize.SIZE200;
            popup.MenuItems[4].Checked = size == VideoSize.SIZE_FREE;
        }

        private void OnVideoSize50(object sender, EventArgs e)
        {
            nicon.Restore();
            engine.SetVideoSize(VideoSize.SIZE50);
        }

        private void OnVideoSize100(object sender, EventArgs e)
        {
            nicon.Restore();
            engine.SetVideoSize(VideoSize.SIZE100);
        }

        private void OnVideoSize200(object sender, EventArgs e)
        {
            nicon.Restore();
            engine.SetVideoSize(VideoSize.SIZE200);
        }

        private void OnVideoSizeFree(object sender, EventArgs e)
        {
            nicon.Restore();
            engine.SetVideoSize(VideoSize.SIZE_FREE);
        }

        private void OnAspectRatioPopup(object sender, EventArgs e)
        {
            MenuItemEx popup = (MenuItemEx)sender;
            AspectRatio ratio = engine.AspectRatio;
            bool bEnabled = /*engine.SourceType != SourceType.DVD*/true; // we will always control aspect ratio
            foreach(MenuItemEx item in popup.MenuItems)
            {
                if (item.Tag != null)
                {
                    item.Enabled = bEnabled;
                    item.Checked = (AspectRatio)item.Tag == ratio;
                }
            }
        }

        private void OnAspectRatio(object sender, EventArgs e)
        {
            engine.AspectRatio = (AspectRatio)((MenuItemEx)sender).Tag;
        }

        private void OnRatePopup(object sender, EventArgs e)
        {
            MenuItemEx popup = (MenuItemEx)sender;
            double rate = engine.GetRate();
            foreach(MenuItemEx item in popup.MenuItems)
            {
                item.Enabled = engine.IsGraphSeekable;
                item.Checked = rate == (double)item.Tag;
            }
        }

        private void OnPlayrate(object sender, EventArgs e)
        {
            engine.SetRate((double)((MenuItemEx)sender).Tag);
        }

        private void OnContextMenu(Point ptScreen)
        {
            bOkToHideCursor = false;			
            ShowCursor();
            
            OnBeforeShowMenu(null, null);
            contextMenu.Show(this, PointToClient(ptScreen));
            OnAfterShowMenu(null, null);
            
            bOkToHideCursor = true;
        }

        private void OnBeforeShowMenu(object sender, EventArgs e)
        {
            bContextMenu = true;
        }

        private void OnAfterShowMenu(object sender, EventArgs e)
        {
            bContextMenu = false;
            if (bModifyMenuPending)
            {
                engine_ModifyMenu(engine, EventArgs.Empty);
                bModifyMenuPending = false;
            }
        }

        private void engine_Update(object sender, EventArgs e)
        {
            UpdateButtons();
        }

        private void engine_MW_Click(object sender, EventArgs e)
        {
            ShowCursor();
        }

        #region Dynamic menus
        private void CreateFiltersMenu(Menu popup, int nPos, bool bAppend)
        {
            if (engine.FilterCount == 0)
                return;
            miFilters = new MenuItemEx(Resources.Resources.mi_filters);
            miFilters.Popup += new EventHandler(OnFiltersPopup);
            int nLast = engine.FilterCount <= 15 ? engine.FilterCount : 15;
            EventHandler eh = new EventHandler(OnFilterClick);
            for (int i=0; i<nLast; i++)
            {
                MenuItemEx item = new MenuItemEx(engine.GetFilterName(i));
                item.Click += eh;
                item.Tag = i;
                miFilters.MenuItems.Add(item);
            }
    
            if (bAppend)
                popup.MenuItems.Add(miFilters);
            else
                popup.MenuItems.Add(nPos, miFilters);
        }

        private void OnFiltersPopup(object sender, EventArgs e)
        {
            MenuItemEx popup = (MenuItemEx)sender;
            foreach(MenuItemEx item in popup.MenuItems)
                item.Enabled = engine.DisplayFilterPropPage(Handle, (int)item.Tag, false);
        }

        private void OnFilterClick(object sender, EventArgs e)
        {
            bOkToHideCursor = false;
            MenuItemEx item = (MenuItemEx)sender;
            engine.DisplayFilterPropPage(Handle, (int)item.Tag, true);
            bOkToHideCursor = true;
        }

        private void CreateAudioStreamsMenu(Menu popup, int nPos, bool bAppend)
        {
            int nStreams=engine.AudioStreams;
            if (nStreams == 0)
                return;
            miStreams = new MenuItemEx(Resources.Resources.mi_audio_streams);
            miStreams.Popup += new EventHandler(OnAudioStreamsPopup);
            int nLast = nStreams <= 8 ? nStreams : 8;
            EventHandler eh = new EventHandler(OnAudioStream);
            for (int i=0; i<nLast; i++)
            {
                MenuItemEx item = new MenuItemEx(engine.GetAudioStreamName(i));
                item.Click += eh;
                item.Tag = i;
                miStreams.MenuItems.Add(item);
            }

            if (bAppend)
                popup.MenuItems.Add(miStreams);
            else
                popup.MenuItems.Add(nPos, miStreams);
        }

        private void OnAudioStreamsPopup(object sender, EventArgs e)
        {
            MenuItemEx popup = (MenuItemEx)sender;
            foreach(MenuItemEx item in popup.MenuItems)
            {
                if (engine.SourceType == SourceType.DVD)
                {
                    bool bEnable=((engine.IsAudioStreamEnabled((int)item.Tag)) && 
                        (engine.UOPS & VALID_UOP_FLAG.UOP_FLAG_Select_Audio_Stream)==0);
                    item.Enabled = bEnable;
                }
                
                item.Checked = engine.CurrentAudioStream == (int)item.Tag;
            }
        }

        private void OnAudioStream(object sender, EventArgs e)
        {
            engine.CurrentAudioStream = (int)((MenuItemEx)sender).Tag;
        }

        private void engine_ModifyMenu(object sender, EventArgs e)
        {
            if (bContextMenu)
            {
                bModifyMenuPending = true;
                return;
            }
            
            // clearing the context menu
            int iVolume = contextMenu.MenuItems.IndexOf(miVolume);
            int iApp = contextMenu.MenuItems.IndexOf(miApp);
            iApp-=2;
            for (int i=iApp; i>iVolume; i--)
            {
                contextMenu.MenuItems.RemoveAt(i);
            }
                
            if (engine.FilterCount == 0)
                return;

            iVolume++;
                
            CreateDVDMenusMenu(contextMenu, iVolume, false);
            CreateFiltersMenu(contextMenu, iVolume, false);	
            CreateDVDMenusLangMenu(contextMenu, iVolume, false);
            CreateAnglesMenu(contextMenu, iVolume, false);
            CreateSubpictureMenu(contextMenu, iVolume, false);
            CreateAudioStreamsMenu(contextMenu, iVolume, false);
            CreateGoToMenu(contextMenu, iVolume, false);

            contextMenu.MenuItems.Add(iVolume, sep.CloneMenu());
        }

        private struct TitleChapter
        {
            public int title;
            public int chapter;

            public TitleChapter(int title, int chapter)
            {
                this.title = title;
                this.chapter = chapter;
            }
        }
        
        private void CreateGoToMenu(Menu popup, int nPos, bool bAppend)
        {
            int ulTitles=engine.NumberOfTitles;
            if (ulTitles == 0)
                return;

            goto_menu = new MenuItemEx(Resources.Resources.mi_goto_menu);
            goto_menu.Popup += new EventHandler(OnGoToPopup);
                        
            EventHandler eh = new EventHandler(OnGoTo);
            int ch_count;
            if (ulTitles==1)
            {
                ch_count=0;
                for (int ch=1; ch<=engine.GetNumChapters(1); ch++)
                {
                    MenuItemEx item = new MenuItemEx(String.Format(Resources.Resources.mi_chapter_format, ch));
                    item.Click += eh;
                    item.Tag = new TitleChapter(1, ch);
                    if (ch_count==20)
                    {
                        item.BarBreak = true;
                        goto_menu.MenuItems.Add(item);
                        ch_count=1;
                    }
                    else
                    {
                        goto_menu.MenuItems.Add(item);
                        ch_count++;
                    }
                }
            }
            else
            {
                EventHandler eh1 = new EventHandler(OnTitlePopup);
                int t_count=0;
                for (int i=1; i<=ulTitles; i++)
                {
                    MenuItemEx ptitle_menu = new MenuItemEx(String.Format(Resources.Resources.mi_title_format, i));
                    ptitle_menu.Popup += eh1;
                    ptitle_menu.Tag = i;
                    ch_count=0;
                    for (int ch=1; ch<=engine.GetNumChapters(i); ch++)
                    {
                        MenuItemEx item = new MenuItemEx(String.Format(Resources.Resources.mi_chapter_format, ch));
                        item.Click += eh;
                        item.Tag = new TitleChapter(i, ch);
                        if (ch_count==20)
                        {
                            item.BarBreak = true;
                            ptitle_menu.MenuItems.Add(item);
                            ch_count=1;
                        }
                        else
                        {
                            ptitle_menu.MenuItems.Add(item);
                            ch_count++;
                        }
                        
                    }
                    
                    if (t_count==20)
                    {
                        ptitle_menu.BarBreak = true;
                        goto_menu.MenuItems.Add(ptitle_menu);
                        t_count=1;
                    }
                    else
                    {
                        goto_menu.MenuItems.Add(ptitle_menu);
                        t_count++;
                    }

                }
            }
    
            if (bAppend)
                popup.MenuItems.Add(goto_menu);
            else
                popup.MenuItems.Add(nPos, goto_menu);
        }

        private void OnGoToPopup(object sender, EventArgs e)
        {
            bool bEnable=(engine.UOPS & VALID_UOP_FLAG.UOP_FLAG_Play_Chapter) == 0;
            MenuItemEx popup = (MenuItemEx)sender;
            int title = engine.CurrentTitle;
            int chapter = engine.CurrentChapter;
            foreach(MenuItemEx item in popup.MenuItems)
            {
                if (item.IsParent)
                {
                    int nChecked = title == (int)item.Tag ? WindowsManagement.MF_CHECKED : 
                        WindowsManagement.MF_UNCHECKED;
                    WindowsManagement.CheckMenuItem(popup.Handle, item.ID, 
                        WindowsManagement.MF_BYCOMMAND | nChecked);
                }
                else
                {
                    TitleChapter tc = (TitleChapter)item.Tag;
                    item.Checked = tc.title == title && tc.chapter == chapter;
                }

                item.Enabled = bEnable;
            }
        }

        private void OnTitlePopup(object sender, EventArgs e)
        {
            MenuItemEx popup = (MenuItemEx)sender;
            int title = engine.CurrentTitle;
            int chapter = engine.CurrentChapter;
            foreach(MenuItemEx item in popup.MenuItems)
            {
                TitleChapter tc = (TitleChapter)item.Tag;
                item.Checked = tc.title == title && tc.chapter == chapter;
            }
        }

        private void OnGoTo(object sender, EventArgs e)
        {
            TitleChapter tc = (TitleChapter)((MenuItemEx)sender).Tag;
            engine.GoTo(tc.title, tc.chapter);
        }

        private void CreateAnglesMenu(Menu popup, int nPos, bool bAppend)
        {
            int ulAngles=engine.AnglesAvailable;
            if (ulAngles < 2)
                return;

            angles_menu = new MenuItemEx(Resources.Resources.mi_angles);
            angles_menu.Popup += new EventHandler(OnAnglesPopup);
            EventHandler eh = new EventHandler(OnAngles);
            for (int i=0; i<ulAngles; i++)
            {
                MenuItemEx item = new MenuItemEx(String.Format(Resources.Resources.mi_angle_format, i+1));
                item.Click += eh;
                item.Tag = i+1;
                angles_menu.MenuItems.Add(item);
            }

            if (bAppend)
                popup.MenuItems.Add(angles_menu);
            else
                popup.MenuItems.Add(nPos, angles_menu);
        }

        private void OnAnglesPopup(object sender, EventArgs e)
        {
            bool bEnable = (engine.UOPS & VALID_UOP_FLAG.UOP_FLAG_Select_Angle) == 0;
            MenuItemEx popup = (MenuItemEx)sender;
            foreach(MenuItemEx item in popup.MenuItems)
            {
                item.Checked = (int)item.Tag == engine.CurrentAngle;
                item.Enabled = bEnable;
            }
        }

        private void OnAngles(object sender, EventArgs e)
        {
            engine.CurrentAngle = (int)((MenuItemEx)sender).Tag;
        }

        private void CreateDVDMenusMenu(Menu popup, int nPos, bool bAppend)
        {
            if (engine.SourceType != SourceType.DVD)
                return;
            menus_menu = new MenuItemEx(Resources.Resources.mi_select_menu);
            menus_menu.Popup += new EventHandler(OnDVDMenusPopup);

            EventHandler eh = new EventHandler(OnDVDMenu);
            miTitle = new MenuItemEx(Resources.Resources.mi_title_menu);
            miTitle.Click += eh;
            miTitle.Tag = DVD_MENU_ID.DVD_MENU_Title;
            miRoot = new MenuItemEx(Resources.Resources.mi_root_menu);
            miRoot.Click += eh;
            miRoot.Tag = DVD_MENU_ID.DVD_MENU_Root;
            miSubpicture = new MenuItemEx(Resources.Resources.mi_subpicture_menu);
            miSubpicture.Click += eh;
            miSubpicture.Tag = DVD_MENU_ID.DVD_MENU_Subpicture;
            miAudio = new MenuItemEx(Resources.Resources.mi_audio_menu);
            miAudio.Click += eh;
            miAudio.Tag = DVD_MENU_ID.DVD_MENU_Audio;
            miAngle = new MenuItemEx(Resources.Resources.mi_angle_menu);
            miAngle.Click += eh;
            miAngle.Tag = DVD_MENU_ID.DVD_MENU_Angle;
            miChapter = new MenuItemEx(Resources.Resources.mi_chapter_menu);
            miChapter.Click += eh;
            miChapter.Tag = DVD_MENU_ID.DVD_MENU_Chapter;

            menus_menu.MenuItems.AddRange(new MenuItem[] {miTitle, miRoot, miSubpicture, 
                miAudio, miAngle, miChapter});

            miCloseMenu = new MenuItemEx(Resources.Resources.mi_close_menu_resume);
            miCloseMenu.Click += new EventHandler(OnCloseMenu);
            if (bAppend)
            {
                popup.MenuItems.Add(sep.CloneMenu());
                popup.MenuItems.Add(menus_menu);
                popup.MenuItems.Add(miCloseMenu);
            }
            else
            {
                popup.MenuItems.Add(nPos, miCloseMenu);
                popup.MenuItems.Add(nPos, menus_menu);
                popup.MenuItems.Add(nPos, sep.CloneMenu());
            }
        }

        private void OnDVDMenusPopup(object sender, EventArgs e)
        {
            VALID_UOP_FLAG UOPS = engine.UOPS;
            MenuItemEx popup = (MenuItemEx)sender;
            foreach(MenuItemEx item in popup.MenuItems)
            {
                DVD_MENU_ID id = (DVD_MENU_ID)item.Tag;
                switch(id)
                {
                    case DVD_MENU_ID.DVD_MENU_Title:
                        item.Enabled = (UOPS & VALID_UOP_FLAG.UOP_FLAG_ShowMenu_Title)==0;
                        break;
                    case DVD_MENU_ID.DVD_MENU_Root:
                        item.Enabled = (UOPS & VALID_UOP_FLAG.UOP_FLAG_ShowMenu_Root)==0;
                        break;
                    case DVD_MENU_ID.DVD_MENU_Subpicture:
                        item.Enabled = (UOPS & VALID_UOP_FLAG.UOP_FLAG_ShowMenu_SubPic)==0;
                        break;
                    case DVD_MENU_ID.DVD_MENU_Audio:
                        item.Enabled = (UOPS & VALID_UOP_FLAG.UOP_FLAG_ShowMenu_Audio)==0;
                        break;
                    case DVD_MENU_ID.DVD_MENU_Angle:
                        item.Enabled = (UOPS & VALID_UOP_FLAG.UOP_FLAG_ShowMenu_Angle)==0;
                        break;
                    case DVD_MENU_ID.DVD_MENU_Chapter:
                        item.Enabled = (UOPS & VALID_UOP_FLAG.UOP_FLAG_ShowMenu_Chapter)==0;
                        break;
                }
            }
        }

        private void OnDVDMenu(object sender, EventArgs e)
        {
            engine.ShowMenu((DVD_MENU_ID)((MenuItemEx)sender).Tag);
        }

        private void OnCloseMenu(object sender, EventArgs e)
        {
            engine.ResumeDVD();
        }

        private void CreateDVDMenusLangMenu(Menu popup, int nPos, bool bAppend)
        {
            int nLang=engine.MenuLangCount;
            if (nLang == 0)
                return;

            lang_menu = new MenuItemEx(Resources.Resources.mi_menu_languages);
            lang_menu.Popup += new EventHandler(OnMenuLangPopup);
            
            EventHandler eh = new EventHandler(OnMenuLang);
            int nLast = nLang <= 10 ? nLang : 10;
            for (int i=0; i<nLast; i++)
            {
                MenuItemEx item = new MenuItemEx(engine.GetMenuLangName(i));
                item.Click += eh;
                item.Tag = i;
                lang_menu.MenuItems.Add(item);
            }

            if (bAppend)
                popup.MenuItems.Add(lang_menu);
            else
                popup.MenuItems.Add(nPos, lang_menu);
        }

        private void OnMenuLangPopup(object sender, EventArgs e)
        {
            MenuItemEx popup = (MenuItemEx)sender;
            foreach(MenuItemEx item in popup.MenuItems)
                item.Enabled = engine.MenuLangCount > 1;
        }

        private void OnMenuLang(object sender, EventArgs e)
        {
            engine.SetMenuLang((int)((MenuItemEx)sender).Tag);
        }

        private void CreateSubpictureMenu(Menu popup, int nPos, bool bAppend)
        {
            int nStreams=engine.NumberOfSubpictureStreams;
            if (nStreams == 0)
                return;

            streams_menu = new MenuItemEx(Resources.Resources.mi_subpictures);
            streams_menu.Popup += new EventHandler(OnSubtitlesPopup);
            
            MenuItemEx item = new MenuItemEx(Resources.Resources.mi_display_subpictures);
            item.Click += new EventHandler(OnDisplaySubtitles);
            item.Tag = -1;
            
            streams_menu.MenuItems.AddRange(new MenuItem[] {item, sep.CloneMenu()});

            EventHandler eh = new EventHandler(OnSubpictureStream);
            int s_count=0;
            for (int i=0; i<nStreams; i++)
            {
                item = new MenuItemEx(engine.GetSubpictureStreamName(i));
                item.Click += eh;
                item.Tag = i;
                if (s_count==16)
                {
                    item.BarBreak = true;
                    streams_menu.MenuItems.Add(item);
                    s_count=1;
                }
                else
                {
                    streams_menu.MenuItems.Add(item);
                    s_count++;
                }
            }

            if (bAppend)
                popup.MenuItems.Add(streams_menu);
            else
                popup.MenuItems.Add(nPos, streams_menu);
        }

        private void OnSubtitlesPopup(object sender, EventArgs e)
        {
            VALID_UOP_FLAG UOPS = engine.UOPS;
            MenuItemEx popup = (MenuItemEx)sender;
            foreach(MenuItemEx item in popup.MenuItems)
                if (item.Tag != null)
                {
                    if ((int)item.Tag == -1)
                    {
                        item.Checked = engine.IsSubpictureEnabled();
                        item.Enabled = (UOPS & VALID_UOP_FLAG.UOP_FLAG_Select_SubPic_Stream)==0;
                    }
                    else
                    {
                        int tag = (int)item.Tag;
                        bool bEnable = (UOPS & VALID_UOP_FLAG.UOP_FLAG_Select_SubPic_Stream)==0 && 
                            engine.IsSubpictureStreamEnabled(tag);
                        item.Checked = tag == engine.CurrentSubpictureStream;
                        item.Enabled = bEnable;
                    }
                }
        }

        private void OnDisplaySubtitles(object sender, EventArgs e)
        {
            engine.EnableSubpicture(!engine.IsSubpictureEnabled());
        }

        private void OnSubpictureStream(object sender, EventArgs e)
        {
            engine.CurrentSubpictureStream = (int)((MenuItemEx)sender).Tag;
        }

        #endregion

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown (e);
            if (e.Shift && bIgnoreNextShift)
            {
                bIgnoreNextShift = false;
                return;
            }
            
            DVD_DOMAIN domain;
            engine.GetCurrentDomain(out domain);
            if (domain == DVD_DOMAIN.DVD_DOMAIN_VideoTitleSetMenu && 
                (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down || 
                e.KeyCode == Keys.Right || e.KeyCode == Keys.Left))
                return;
            if (htKeys.ContainsKey(e.KeyData))
            {
                string command = (string)htKeys[e.KeyData];
                if (htCommands.ContainsKey(command))
                    ((MenuItemEx)htCommands[command]).PerformClickEx();
            }
        }

        private void controlbarHolder_KeyDown(object sender, KeyEventArgs e)
        {
            OnKeyDown(e);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel (e);
            if (wheelAction == MouseWheelAction.Volume)
            {
                nVolume+=e.Delta*VOLUME_STEP/WHEEL_DELTA;
                if (nVolume < -ControlBar.VOLUME_RANGE)
                    nVolume = -ControlBar.VOLUME_RANGE;
                else if (nVolume>0)
                    nVolume=0;
            
                UpdateButtons();
                if (!bMute && bCanChangeVolume)
                    engine.SetVolume(nVolume);
            }
            else if (e.Delta>0)
                OnForth(null, EventArgs.Empty);
            else if (e.Delta<0)
                OnBack(null, EventArgs.Empty);
        }

        private void controlbarHolder_MouseWheel(object sender, MouseEventArgs e)
        {
            OnMouseWheel(e);
        }

        private void OnTakeScreenshot(object sender, EventArgs e)
        {
            if (engine.GraphState == GraphState.Reset)
                return;

            ImageCreator imageCreator = new ImageCreator();
            try
            {
                engine.GetCurrentImage(imageCreator);
                if (imageCreator.Created)
                {
                    System.Threading.ThreadPool.QueueUserWorkItem(SaveScreenshot, imageCreator);
                }
            }
            catch (Exception ex)
            {
                Trace.GetTrace().TraceError("Error creating a screenshot: " + ex.Message);
            }
        }

        private object _syncRoot = new object();
        private void SaveScreenshot(object state)
        {
            ImageCreator imageCreator = state as ImageCreator;
            if (imageCreator == null)
                return;

            try
            {
                lock (_syncRoot)
                {
                    imageCreator.Save(GetNewScreenshotName());
                }
            }
            catch(Exception e)
            {
                Trace.GetTrace().TraceError("Error saving a screenshot: " + e.Message);
            }
            finally
            {
                imageCreator.Dispose();
            }
        }

        private const string SCREENSHOT_NAME_FORMAT = "pvp_screenshot_{0}.jpg";
        private Regex regexScrnshotName = new Regex(@"pvp_screenshot_(?<index>\d+).jpg");
        private string GetNewScreenshotName()
        {
            string dir = screenshotsFolder;
            if (!Directory.Exists(dir))
                dir = DefaultScreenshotsFolder;
            string[] files = Directory.GetFiles(dir, "*.jpg", SearchOption.TopDirectoryOnly);
            int index = 0;
            foreach (string file in files)
            {
                Match m = regexScrnshotName.Match(file);
                if (m.Success)
                {
                    int i;
                    if (Int32.TryParse(m.Groups["index"].Value, out i))
                    {
                        if (i >= index)
                            index = ++i;
                    }
                }
            }

            return Path.Combine(dir, string.Format(SCREENSHOT_NAME_FORMAT, index));
        }
    }
}
