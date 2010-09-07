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
using System.Windows.Forms;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Globalization;
using Dzimchuk.MediaEngine.Core;
using Dzimchuk.AUI;
using Dzimchuk.Common;
using Dzimchuk.PVP.Util;

namespace Dzimchuk.PVP
{
    /// <summary>
    /// 
    /// </summary>
    public class MainFormSettings : MainFormControls
    {
        bool bStartFullscreen;
        bool bCenterWindow = true;

        MenuItemEx miControlbar, miPref, miExit, miAbout, miAppLang;
                        
        public MainFormSettings()
        {
            CreateAppMenu();
            PopulateContextMenu();
            HandleSystemTray();
            engine.InitSize += new EventHandler<InitSizeEventArgs>(engine_InitSize);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad (e);
            TopMost = bTopMost;
            if (bCenterWindow && WindowState == FormWindowState.Normal)
            {
                Rectangle bounds = DesktopBounds;
                Rectangle rect = Screen.FromControl(this).WorkingArea;
                bounds.X=bounds.Width<rect.Width ? rect.X + (rect.Width-bounds.Width)/2 : rect.X;
                bounds.Y=bounds.Height<rect.Height ? rect.Y + (rect.Height-bounds.Height)/2 : rect.Y;
                DesktopBounds = bounds;
            }
            if (bStartFullscreen)
                ToggleFullscreen();
        }

        protected override void SetMenuItemsText()
        {
            base.SetMenuItemsText();
            miApp.Text = Resources.Resources.mi_application;
            miControlbar.Text = Resources.Resources.mi_controlbar;
            miPref.Text = Resources.Resources.mi_preferences;
            miAbout.Text = Resources.Resources.mi_about;
            miExit.Text = Resources.Resources.mi_exit;

            miAppLang.Text = Resources.Resources.mi_app_lang;
            foreach (MenuItemEx item in miAppLang.MenuItems)
            {
                item.Text = Resources.Resources.ResourceManager.GetString("mi_app_lang_" + ((AppLang)item.Tag).name);
            }
        }

        private class AppLang
        {
            public string name;
            public string cultureName;
            public AppLang(string name, string cultureName)
            {
                this.name = name;
                this.cultureName = cultureName;
            }
        }

        #region CreateAppMenu
        void CreateAppMenu()
        {
            miApp = new MenuItemEx();
            miApp.Popup += new EventHandler(OnAppPopup);
            
            miControlbar = new MenuItemEx();
            miControlbar.Click += new EventHandler(OnSettingsControlbar);
            
            miPref = new MenuItemEx();
            miPref.Click += new EventHandler(OnSettingsPreferences);
            
            miAbout = new MenuItemEx();
            miAbout.Click += new EventHandler(OnHelpAbout);

            miAppLang = new MenuItemEx();
            miAppLang.Popup += new EventHandler(miAppLang_Popup);
            EventHandler eh = new EventHandler(OnMenuLangItem);
            MenuItemEx item = new MenuItemEx();
            item.Tag = new AppLang("default", "en-US");
            item.Click += eh;
            miAppLang.MenuItems.Add(item);
            item = new MenuItemEx();
            item.Tag = new AppLang("russian", "ru-RU");
            item.Click += eh;
            miAppLang.MenuItems.Add(item);
            
            miApp.MenuItems.AddRange(new MenuItem[] 
                { miControlbar, sep.CloneMenu(), miAppLang, miPref, miAbout } );

            miExit = new MenuItemEx();
            miExit.Click += new EventHandler(OnClose);

            htCommands.Add(SettingsForm.strKeysPref, miPref);
            htCommands.Add(SettingsForm.strKeysAbout, miAbout);
            htCommands.Add(SettingsForm.strKeysExit, miExit);
        }
        #endregion

        #region Populate context menu
        void PopulateContextMenu()
        {
            contextMenu.MenuItems.AddRange(new MenuItem[] { miFile, 
                    sep.CloneMenu(), miPlay, miPause, miStop, miRepeat, sep.CloneMenu(), 
                    miFullscreen, sep.CloneMenu(), miVideoSize, miAspectRatio, 
                    miRate, miVolume, sep.CloneMenu(), miApp, miExit} );
        }
        #endregion

        private void miAppLang_Popup(object sender, EventArgs e)
        {
            string cultName = Thread.CurrentThread.CurrentUICulture.Name;
            bool found = false;
            foreach (MenuItemEx item in miAppLang.MenuItems)
            {
                if (((AppLang)item.Tag).cultureName == cultName)
                {
                    item.Checked = true;
                    found = true;
                }
                else
                    item.Checked = false;
            }
            if (!found)
                miAppLang.MenuItems[0].Checked = true; // the 1st one is always the default one (en-US)
        }

        private void OnMenuLangItem(object sender, EventArgs e)
        {
            ChangeCurrentCulture(((AppLang)((MenuItemEx)sender).Tag).cultureName);
        }

        private void ChangeCurrentCulture(string cultureName)
        {
            if (cultureName != null && Thread.CurrentThread.CurrentUICulture.Name != cultureName)
            {
                CultureInfo ci = new CultureInfo(cultureName);
                Thread.CurrentThread.CurrentCulture = ci;
                Thread.CurrentThread.CurrentUICulture = ci;
                OnCultureChanged();
            }
        }

        private void OnHelpAbout(object sender, EventArgs e)
        {
            bOkToHideCursor = false;
            ShowCursor();
            Color inner, mid, outer;
            GetBorderColors(out inner, out mid, out outer);
            
            string version = Application.ProductVersion;
            int n = version.LastIndexOf('.');
            if (n != -1)
                version = version.Substring(0, n);
            AboutDialog dlg = new AboutDialog(this, AboutDialogBorder.Own, GetType(),
                "Dzimchuk.PVP.about.bmp", Resources.Resources.about_pvp,
                String.Format("{0} {1}\n{2}\n{3}", Resources.Resources.program_name, version, Resources.Resources.about_copyright, Resources.Resources.about_license),
                Color.White, Resources.Resources.ok);
            dlg.clrInner = inner;
            dlg.clrMid = mid;
            dlg.clrOuter = outer;
            dlg.TopMost = TopMost;
            ShowMyDialog(dlg);

            bOkToHideCursor = true;
        }
        
        private void OnAppPopup(object sender, EventArgs e)
        {
            miControlbar.Checked = bFullscreen ? controlbarHolder.Visible : controlbar.Visible;
            miControlbar.Enabled = Visible;
        }

        private void OnSettingsControlbar(object sender, EventArgs e)
        {
            if (bFullscreen)
                controlbarHolder.Visible = !miControlbar.Checked;
            else
                controlbar.Visible = !miControlbar.Checked;
        }

        private void OnSettingsPreferences(object sender, EventArgs e)
        {
            bOkToHideCursor = false;
            ShowCursor();
            
            SettingsForm dlg = new SettingsForm();
            dlg.TopMost = TopMost;
            dlg.Apply += new EventHandler(OnSettingsApply);

            dlg.AutoPlay = engine.AutoPlay;
            dlg.VideoRenderer = engine.PreferredVideoRenderer;
            dlg.ShowLogo = mediaWindowHost.ShowLogo;
            
            dlg.SystemTray = nicon.SystemTray;
            dlg.ShowTrayAlways = nicon.ShowTrayAlways;
            dlg.RememberVolume = bRememberVolume;
            dlg.StartFullscreen = bStartFullscreen;
            dlg.CenterWindow = bCenterWindow;
            dlg.AlwaysOnTop = bTopMost;
            dlg.ScreenshotsFolder = screenshotsFolder;
            if (!Visible)
                dlg.StartPosition = FormStartPosition.CenterScreen;

            using (FileAssociator fa = FileAssociator.GetFileAssociator(strDocTypePrefix, strProgName))
            {
                string[] types = dlg.FileTypes;
                Hashtable table = new Hashtable();
                foreach (string type in types)
                    table[type] = fa.IsAssociated(type);
                dlg.SelectedFileTypes = table;
            }

            dlg.KeysTable = htKeys;
            dlg.MouseWheelAction = wheelAction;

            dlg.UsePreferredFilters = engine.UsePreferredFilters;
            dlg.UsePreferredFilters4DVD = engine.UsePreferredFilters4DVD;
            
            if (ShowMyDialog(dlg) == DialogResult.OK)
                OnSettingsApply(dlg, EventArgs.Empty);
            if (dlg.FileTypesChanged)
                FileAssociator.NotifyShell();
            if (dlg.RestartTriggered)
            {
                MessageBox.Show(String.Format(Resources.Resources.systray_warning_format, "\n"), 
                    Resources.Resources.program_name, MessageBoxButtons.OK, 
                    MessageBoxIcon.Exclamation);
                Close();
            }

            bOkToHideCursor = true;
        }

        protected override void LoadSettings(PropertyBag props)
        {
            base.LoadSettings(props);
            nicon.SystemTray = props.Get<int>("systray_mode", 0); // 0 - taskbar only
            nicon.ShowTrayAlways = props.Get<bool>("systray_always", false);

            bStartFullscreen = props.Get<bool>("start_fullscreen", false);
            bCenterWindow = props.Get<bool>("center_window", true);
            bTopMost = props.Get<bool>("top_most_window", false);

            engine.AutoPlay = props.Get<bool>("auto_play", true);

            int nRenderer;
            bool bRendererSet = props.TryGetValue<int>("preferred_renderer", out nRenderer);
            if (!bRendererSet) 
            {
                nRenderer = (int)MediaEngineServiceProvider.RecommendedRenderer;
            }
            int[] values = (int[])Enum.GetValues(typeof(Renderer));
            engine.PreferredVideoRenderer = (nRenderer >= 0 && nRenderer <= values[values.Length-1]) 
                ? (Renderer) nRenderer : Renderer.VR;

            mediaWindowHost.ShowLogo = props.Get<bool>("show_logo", true);
            engine.Repeat = props.Get<bool>("repeat_on", false);
            engine.UsePreferredFilters = props.Get<bool>("use_preferred_filters", false);
            engine.UsePreferredFilters4DVD = props.Get<bool>("use_preferred_filters_4dvd", false);

            controlbar.Visible = props.Get<bool>("controlbar_on", true);

            ChangeCurrentCulture(props.Get<string>("ui_language", null));

            MediaTypeManager.GetInstance().Load(props.Get);
        }

        protected override void SaveSettings(PropertyBag props)
        {
            base.SaveSettings(props);
            props.Add("systray_mode", nicon.SystemTray);
            props.Add("systray_always", nicon.ShowTrayAlways);

            props.Add("start_fullscreen", bStartFullscreen);
            props.Add("center_window", bCenterWindow);
            props.Add("top_most_window", bTopMost);

            props.Add("auto_play", engine.AutoPlay);
            props.Add("preferred_renderer", (int)engine.PreferredVideoRenderer);
            props.Add("show_logo", mediaWindowHost.ShowLogo);
            props.Add("repeat_on", engine.Repeat);
            props.Add("use_preferred_filters", engine.UsePreferredFilters);
            props.Add("use_preferred_filters_4dvd", engine.UsePreferredFilters4DVD);

            props.Add("controlbar_on", bInit ? controlbar.Visible : true);

            props.Add("ui_language", Thread.CurrentThread.CurrentUICulture.Name);

            MediaTypeManager.GetInstance().Save(props.Add);
        }
        
        private void OnSettingsApply(object sender, EventArgs e)
        {
            SettingsForm dlg = (SettingsForm) sender;

            htKeys = dlg.KeysTable;
            wheelAction = dlg.MouseWheelAction;
            
            engine.AutoPlay = dlg.AutoPlay;
            engine.PreferredVideoRenderer = dlg.VideoRenderer;
            mediaWindowHost.ShowLogo = dlg.ShowLogo;
            engine.UsePreferredFilters = dlg.UsePreferredFilters;
            engine.UsePreferredFilters4DVD = dlg.UsePreferredFilters4DVD;

            string[] astrTypes = MediaTypeManager.GetInstance().TypeNames;
            foreach(string type in astrTypes)
                MediaTypeManager.GetInstance().SetTypesClsid(type, dlg.GetTypeClsid(type));

            screenshotsFolder = dlg.ScreenshotsFolder;
            bRememberVolume = dlg.RememberVolume;
            bStartFullscreen = dlg.StartFullscreen;
            bCenterWindow = dlg.CenterWindow;
            bTopMost = dlg.AlwaysOnTop;
            if (!bFullscreen)
                TopMost = bTopMost;

            nicon.SystemTray = dlg.SystemTray;
            nicon.ShowTrayAlways = dlg.ShowTrayAlways;
            if (!dlg.RestartTriggered)
                HandleSystemTray();

            LoadSaveSettings(false);
            if (dlg.FileTypesChanged)
                AssociateFiles(dlg);
        }

        void HandleSystemTray()
        {
            switch(nicon.SystemTray)
            {
                case 0:
                    nicon.Visible = false;
                    break;
                case 1:
                    if (WindowState != FormWindowState.Minimized)
                        nicon.Visible = nicon.ShowTrayAlways;
                    break;
                case 2:
                    nicon.Visible = true;
                    ShowInTaskbar = false;
                    break;
            }
        }

        void AssociateFiles(SettingsForm dlg)
        {
            using (FileAssociator fa = FileAssociator.GetFileAssociator(strDocTypePrefix, strProgName))
            {
                Hashtable table = dlg.SelectedFileTypes;
                foreach (DictionaryEntry entry in table)
                    fa.Associate(entry.Key.ToString(), (bool)entry.Value);
            }
        }

        private void engine_InitSize(object sender, InitSizeEventArgs e)
        {
            VideoSize size = engine.GetVideoSize();
            int div = 1;
            if (size == VideoSize.SIZE50)
            {
                size = VideoSize.SIZE100;
                div = 2;
            }
            if (WindowState != FormWindowState.Maximized && !bFullscreen
                && size != VideoSize.SIZE_FREE && e.NewVideSize.cy != 0 && e.NewVideSize.cx != 0)
            {
                Rectangle bounds = DesktopBounds;
                Size client = mediaWindowHost.ClientSize;

                int nSize = (int)size;
                int hor = ((int)(e.NewVideSize.cx * nSize / div)) - client.Width;
                int vert = ((int)(e.NewVideSize.cy * nSize / div)) - client.Height;

                bounds.Width += hor;
                bounds.Height += vert;

                if (bCenterWindow)
                {
                    Rectangle rect = Screen.FromControl(this).WorkingArea;
                    bounds.X = bounds.Width < rect.Width ? rect.X + (rect.Width - bounds.Width) / 2 : rect.X;
                    bounds.Y = bounds.Height < rect.Height ? rect.Y + (rect.Height - bounds.Height) / 2 : rect.Y;
                }

                DesktopBounds = bounds;
            }
        }
    }
}
