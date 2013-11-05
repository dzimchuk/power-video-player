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
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using Pvp.Core.MediaEngine;
using Pvp.Core.MediaEngine.Internal;
using Pvp.Core.Native;
using Dzimchuk.AUI;
using Pvp.Core.MediaEngine.Description;

namespace Pvp
{
    /// <summary>
    /// 
    /// </summary>
    public class MainFormFile : MainFormBase
    {
        protected MenuItemEx miFile, miOpen, miClose, miPlayDVD, miInfo;
        
        protected bool bPlayPending;
        protected string strFileName;
        protected MediaSourceType whatToPlay = MediaSourceType.File;
            
        public MainFormFile()
        {
            CreateFileMenu();
            mediaControl.FailedStreamsAvailable += new FailedStreamsHandler(engine_FailedStreamsAvailable);
        }
        
        protected override void SetMenuItemsText()
        {
            base.SetMenuItemsText();
            miFile.Text = Resources.Resources.mi_file;
            miOpen.Text = Resources.Resources.mi_file_open;
            miClose.Text = Resources.Resources.mi_file_close;
            miInfo.Text = Resources.Resources.mi_file_information;
            miPlayDVD.Text = Resources.Resources.mi_file_play_dvd;
        }

        #region CreateFileMenu
        void CreateFileMenu()
        {
            miFile = new MenuItemEx();
            miFile.Popup += new EventHandler(OnFilePopup);
            
            miOpen = new MenuItemEx();
            miOpen.Click += new EventHandler(OnFileOpen);
                    
            miClose = new MenuItemEx();
            miClose.Click += new EventHandler(OnFileClose);
                    
            miInfo = new MenuItemEx();
            miInfo.Click += new EventHandler(OnFileInfo);
                                                        
            miFile.MenuItems.AddRange(new MenuItem[] {miOpen, miClose});
            
            CreateCDRomMenu();

            miFile.MenuItems.AddRange(new MenuItem[] {sep.CloneMenu(), miInfo});
        }

        private void CreateCDRomMenu()
        {
            miPlayDVD = new MenuItemEx();
            miPlayDVD.Popup += new EventHandler(OnPlayDVDPopup);

            DriveInfo[] drives = DriveInfo.GetDrives();
            EventHandler eh = new EventHandler(OnPlayDVD);
            foreach (DriveInfo drive in drives)
            {
                try
                {
                    if (drive.DriveType == DriveType.CDRom)
                    {
                        MenuItemEx item = new MenuItemEx(drive.Name);
                        item.Tag = drive.Name;
                        item.Click += eh;
                        miPlayDVD.MenuItems.Add(item);
                    }
                }
                catch
                {
                }
            }
            if (miPlayDVD.MenuItems.Count != 0)
                miFile.MenuItems.AddRange(new MenuItem[] {sep.CloneMenu(), miPlayDVD});
        }
        #endregion
                
        private void OnPlayDVDPopup(object sender, EventArgs e)
        {
        /*	uint VolumeSerialNumber, MaximumComponentLength, FileSystemFlags;
            int nMode = NoCat.SetErrorMode(NoCat.SEM_FAILCRITICALERRORS);
            System.Text.StringBuilder builder = new System.Text.StringBuilder(Storage.MAX_PATH);
            foreach(MenuItemEx item in miPlayDVD.MenuItems)
            {
                string tag = (string)item.Tag;
                item.Enabled = 
                    Storage.GetVolumeInformation(tag, builder, builder.Capacity-tag.Length, 
                    out VolumeSerialNumber, out MaximumComponentLength, 
                    out FileSystemFlags, null, 0) != 0;
                builder.Insert(0, tag);
                item.Text = builder.ToString();
                builder.Remove(0, builder.Length);
            }
            NoCat.SetErrorMode(nMode); */

            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            foreach (MenuItemEx item in miPlayDVD.MenuItems)
            {
                item.Enabled = false;
                string tag = (string)item.Tag;
                builder.Append(tag);
                try
                {
                    DriveInfo drive = new DriveInfo(tag);
                    if (drive.IsReady)
                    {
                        item.Enabled = true;
                        builder.Append(drive.VolumeLabel);
                    }
                }
                catch
                { 
                }

                item.Text = builder.ToString();
                builder.Remove(0, builder.Length);
            }
        }

        protected virtual void OnPlayDVD(object sender, EventArgs e)
        {
            MenuItemEx item = (MenuItemEx)sender;
            string source = (string)item.Tag;
            source += "Video_ts";
            strFileName = source;
            whatToPlay = Pvp.Core.MediaEngine.MediaSourceType.Dvd;
            bPlayPending = true;
        }
        
        protected void OnFilePopup(object sender, EventArgs e)
        {
            miClose.Enabled = mediaControl.GraphState != GraphState.Reset;
            miInfo.Enabled = mediaControl.GraphState != GraphState.Reset;
        }

        bool bShowingFileDialog;
        bool bShowingDialog;
        Form CurrentDialog;
        protected DialogResult ShowMyDialog(Form dlg)
        {
            if (bShowingFileDialog)
                return DialogResult.Cancel;
            
            if (!bShowingDialog)
            {
                bShowingDialog = true;
                CurrentDialog = dlg;
                DialogResult result = dlg.ShowDialog(this);
                bShowingDialog = false;
                CurrentDialog = null;
                return result;
            }
            else
            {
                WindowsManagement.SetForegroundWindow(CurrentDialog.Handle);
                return DialogResult.Cancel;
            }
        }
        
        protected virtual void OnFileOpen(object sender, EventArgs e)
        {
            if (!bShowingFileDialog && !bShowingDialog)
            {
                bShowingFileDialog = true;
                
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.Filter = "Video Files (*.avi;*.divx;*.mpg;*.mpeg;*.asf;*.wmv;*.mov;*.qt;*.vob;*.dat;*.mkv;*.flv;*.mp4;*.3gp;*.3g2;*.m1v;*.m2v)|" +
                    "*.avi;*.divx;*.mpg;*.mpeg;*.asf;*.wmv;*.mov;*.qt;*.vob;*.dat;*.mkv;*.flv;*.mp4;*.3gp;*.3g2;*.m1v;*.m2v|All Files (*.*)|*.*";
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    strFileName = dlg.FileName;
                    whatToPlay = Pvp.Core.MediaEngine.MediaSourceType.File;
                    bPlayPending = true;
                }

                bShowingFileDialog = false;
            }
            else if (bShowingDialog)
                WindowsManagement.SetForegroundWindow(CurrentDialog.Handle);
        }
    
        protected virtual void OnFileClose(object sender, EventArgs e)
        {
            mediaControl.ResetGraph();
        }

        protected virtual void OnFileInfo(object sender, EventArgs e)
        {
            InfoDialog dlg = new InfoDialog(mediaControl);
            dlg.TopMost = TopMost;
            if (!Visible)
                dlg.StartPosition = FormStartPosition.CenterScreen;
            ShowMyDialog(dlg);
        }

        private void engine_FailedStreamsAvailable(IList<StreamInfo> streams)
        {
            Invoke(new MethodInvoker(delegate()
            {
                FailedPinsDialog dlg = new FailedPinsDialog(streams);
                dlg.TopMost = TopMost;
                if (!Visible)
                    dlg.StartPosition = FormStartPosition.CenterScreen;
                ShowMyDialog(dlg);
            }));
        }
    }
}
