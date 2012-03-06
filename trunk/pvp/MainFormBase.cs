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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.IO.IsolatedStorage;
using System.Security;
using System.Security.Permissions;
using Dzimchuk.AUI;
using Dzimchuk.Theme;
using Dzimchuk.MediaEngine.Core;
using Dzimchuk.Native;
using System.Xml;
//using System.Xml.Schema;
using System.Reflection;

namespace Dzimchuk.PVP
{
    public class MainFormBase : FormBase
    {
        private System.ComponentModel.IContainer components = null;

        const string strConfig = "config.bin"; 
        protected const string strProgName = "Power Video Player";
        protected const string strDocTypePrefix = "PVP.AssocFile";
        protected ControlBar controlbar = new ControlBar();
        protected Form controlbarHolder = new Form();
        protected NotifyIconEx nicon = new NotifyIconEx();
        protected MediaWindowHost mediaWindowHost = new MediaWindowHost();
        protected ContextMenu contextMenu = new ContextMenu();
        protected MenuItemEx sep = new MenuItemEx("-");

        protected IMediaEngine engine;
                        
    //	bool bXsdOk;
        protected bool bInit = true;
    //	const string strXSD = "Dzimchuk.PVP.theme.default.xsd";
        private const string strDefaultTheme = "Dzimchuk.PVP.theme.default.xml";
        private string strCurTheme = strDefaultTheme;

        private Rectangle rectNormal;
            
        protected bool bTopMost;
        protected bool bFullscreen;
        private bool bControlbar;
        private int nControlbarChildIndex;
        private FormWindowState formWindowState;
        private VideoSize nVideoSize;
                            
        public MainFormBase()
        {
            // This call is required by the Windows Form Designer.
            InitializeComponent();
            SetUpTrace();
            
            rectNormal = DesktopBounds;
            StartPosition = FormStartPosition.Manual;
            Text = strProgName;
        
            nicon.Text = strProgName;
            nicon.HandleMinMax = this;
            nicon.ContextMenu = contextMenu;
            try
            {
                Icon icon = new Icon(GetType(), "pvp.ico");
                Icon = icon;
                nicon.Icon = new Icon(icon, 16, 16);
            }
            catch
            {
            }
            
            mediaWindowHost.Parent = this;
            mediaWindowHost.Dock = DockStyle.Fill;
            Bitmap logo = null;
            try
            {
                logo = new Bitmap(GetType(), "logo.bmp");
                mediaWindowHost.Logo = logo;
            }
            catch
            {
            }
            finally
            {
                if (logo != null)
                    logo.Dispose();
            }

            engine = mediaWindowHost.MediaEngine;
            engine.DvdParentalChange += OnUserDecisionNeeded;
            engine.PartialSuccess += OnUserDecisionNeeded;

            engine.ErrorOccured += delegate(object sender, ErrorOccuredEventArgs args)
            {
                MessageBox.Show(args.Message, strProgName, MessageBoxButtons.OK, MessageBoxIcon.Stop);
            };
            
            controlbar.Parent = this;

            controlbarHolder.Owner = this;
            controlbarHolder.ShowInTaskbar = false;
            controlbarHolder.FormBorderStyle = FormBorderStyle.None;

            LoadSaveSettings(true);
            LoadTheme(strCurTheme);
        }

        private void OnUserDecisionNeeded(object sender, UserDecisionEventArgs e)
        {
            e.Accept = (DialogResult.Yes == MessageBox.Show(e.Message, strProgName,
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question));
        }

        private const string logFileName = "pvplog.txt";
        private const string logDirName = "log";
        private const string traceSwitchName = "PVPTraceSwitch";
        private void SetUpTrace()
        {
            Trace.Listeners.Clear();
            System.Diagnostics.TraceSwitch traceSwitch = new System.Diagnostics.TraceSwitch(traceSwitchName, traceSwitchName);
            Trace.GetTrace().TraceSwitch = traceSwitch;

            IsolatedStorageFilePermission perm =
                new IsolatedStorageFilePermission(PermissionState.Unrestricted);
            perm.UsageAllowed = IsolatedStorageContainment.AssemblyIsolationByUser;
            if (!SecurityManager.IsGranted(perm))
                return;

            IsolatedStorageFileStream stream = null;
            try
            {
                IsolatedStorageFile storage =
                IsolatedStorageFile.GetUserStoreForAssembly();

                string[] astr = storage.GetDirectoryNames(logDirName);
                if (astr.Length == 0)
                    storage.CreateDirectory(logDirName);

                string logFile = Path.Combine(logDirName, logFileName);
                stream = new IsolatedStorageFileStream(logFile, FileMode.Append, FileAccess.Write, 
                    FileShare.Read, storage);
                if (stream.Length > 1024 * 1024 * 10)
                {
                    stream.Close();
                    stream = null;
                    storage.DeleteFile(logFile);
                    stream = new IsolatedStorageFileStream(logFile, FileMode.Append, FileAccess.Write,
                        FileShare.Read, storage);
                }

                Trace.Listeners.Add(new System.Diagnostics.TextWriterTraceListener(stream));
                stream = null;
            }
            catch
            {
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
        }

        protected virtual void SetMenuItemsText()
        {
        }

        protected override void OnCultureChanged()
        {
            base.OnCultureChanged();
            controlbar.UpdateToolTips();
            SetMenuItemsText();
            engine.OnCultureChanged();
        }

        protected override void OnLoad(EventArgs e)
        {
            if (!bInit)
                Close();
            base.OnLoad (e);
            SetMenuItemsText();
            Trace.GetTrace().TraceInformation("PVP started.");
        }

        protected override void OnClosed(EventArgs e)
        {
            engine.ResetGraph();
            if (bFullscreen)
                ToggleFullscreen();
            base.OnClosed (e);
            LoadSaveSettings(false);
            Trace.GetTrace().TraceInformation("PVP closed.");
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize (e);
            if (WindowState == FormWindowState.Normal && !bFullscreen)
                rectNormal = DesktopBounds;
        }

        protected override void OnMove(EventArgs e)
        {
            base.OnMove (e);
            if (WindowState == FormWindowState.Normal && !bFullscreen)
                rectNormal = DesktopBounds;
        }

        #region LoadSaveSettings
        protected void LoadSaveSettings(bool bLoad)
        {
            IsolatedStorageFilePermission perm =
                new IsolatedStorageFilePermission(PermissionState.Unrestricted);
            perm.UsageAllowed = IsolatedStorageContainment.AssemblyIsolationByUser;
            if (!SecurityManager.IsGranted(perm))
            {
                MessageBox.Show("User settings won't be saved.", strProgName, MessageBoxButtons.OK, 
                    MessageBoxIcon.Warning);
                return;
            }

            IsolatedStorageFileStream stream = null;
            try
            {
                IsolatedStorageFile storage =
                    IsolatedStorageFile.GetUserStoreForAssembly();
                if (bLoad)
                {
                    string[] astr = storage.GetFileNames(strConfig);
                    if (astr.Length > 0)
                    {
                        stream = new IsolatedStorageFileStream(strConfig, FileMode.Open, 
                            FileAccess.Read, FileShare.Read, storage);

                        PropertyBag props = new PropertyBag(stream);
                        LoadSettings(props);
                    }
                    else
                    {
                        // default settings
                        engine.AutoPlay = true;
                        mediaWindowHost.ShowLogo = true;
                        engine.PreferredVideoRenderer = MediaEngineServiceProvider.RecommendedRenderer;
                    }
                }
                else
                {
                    PropertyBag props = new PropertyBag();
                    SaveSettings(props);
                    
                    stream = new IsolatedStorageFileStream(strConfig, FileMode.Create, FileAccess.Write, storage);
                    props.Save(stream);
                }
            }
            catch
            {
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
        }

        protected virtual void LoadSettings(PropertyBag props)
        {
            int x = props.Get<int>("pos_x", 0);
            int y = props.Get<int>("pos_y", 0);
            int cx = props.Get<int>("pos_cx", 0);
            int cy = props.Get<int>("pos_cy", 0);

            rectNormal = new Rectangle(x, y, cx, cy);

            // the desktop size might have changed so we should make 
            // the necessary adjustments
            Rectangle rectDesk = SystemInformation.WorkingArea;
            rectNormal.Width = Math.Min(rectNormal.Width, rectDesk.Width);
            rectNormal.Height = Math.Min(rectNormal.Height, rectDesk.Height);
            rectNormal.X -= Math.Max(rectNormal.Right-rectDesk.Right, 0);
            rectNormal.Y -= Math.Max(rectNormal.Bottom-rectDesk.Bottom, 0);

            DesktopBounds = rectNormal;

            int state = props.Get<int>("window_state", (int)FormWindowState.Normal);
            WindowState = Enum.IsDefined(typeof(FormWindowState), state) ? 
                (FormWindowState)state : FormWindowState.Normal;

            strCurTheme = props.Get<string>("current_theme", strDefaultTheme);
        }

        protected virtual void SaveSettings(PropertyBag props)
        {
            props.Add<int>("pos_x", rectNormal.X);
            props.Add<int>("pos_y", rectNormal.Y);
            props.Add<int>("pos_cx", rectNormal.Width);
            props.Add<int>("pos_cy", rectNormal.Height);
            props.Add<int>("window_state", (int)WindowState);

            props.Add<string>("current_theme", strCurTheme);
        }
        #endregion

        #region Theme
        protected void LoadTheme(string theme)
        {
            Stream xml = null;
            try
            {
                if (CheckThemeXml(theme))
                {
                    xml = GetThemeStream(theme);
                    if (xml == null)
                        throw new Exception(); // just in case
                    if (LoadTheme(xml, strProgName))
                        strCurTheme = theme;
                    else if (theme != strDefaultTheme && CheckThemeXml(strDefaultTheme))
                    {
                        // the specified NOT default theme failed to load so we
                        // try to load the default theme (after checking it)
                        xml.Close();
                        xml = Assembly.GetAssembly(GetType()).GetManifestResourceStream(strDefaultTheme);
                        if (!LoadTheme(xml, strProgName))
                            throw new Exception();
                        strCurTheme = strDefaultTheme;
                    }
                    else throw new Exception(); // deafault theme failed to load
                }
                else if (theme != strDefaultTheme && CheckThemeXml(strDefaultTheme))
                {
                    // the specified NOT default theme failed XSD checking so we
                    // try to load the default theme (after checking it)
                    xml = Assembly.GetAssembly(GetType()).GetManifestResourceStream(strDefaultTheme);
                    if (!LoadTheme(xml, strProgName))
                        throw new Exception();
                    strCurTheme = strDefaultTheme;
                }
                else throw new Exception(); // default theme didn't pass XSD checking
            }
            catch
            {
                bInit = false;
            }
            finally
            {
                if (xml != null)
                    xml.Close();
            }
        }

        Stream GetThemeStream(string theme)
        {
            //	Stream stream;
            if(theme != strDefaultTheme)
            {

            }
                
            return Assembly.GetAssembly(GetType()).GetManifestResourceStream(strDefaultTheme);
        }

        bool CheckThemeXml(string theme)
        {
        /*	XmlTextReader xsdReader = null;
            XmlValidatingReader reader = null;
            Stream xml = null;
            try
            {
                xsdReader = new XmlTextReader(Assembly.GetAssembly(GetType()).GetManifestResourceStream(strXSD));
                
                xml = GetThemeStream(theme);
                if (xml == null)
                    throw new Exception(); // just in case
                XmlTextReader nvr = new XmlTextReader(xml);
                reader = new XmlValidatingReader(nvr);
                reader.Schemas.Add(strProgName, xsdReader);
                reader.ValidationEventHandler += new ValidationEventHandler(OnValidationError);
                
                bXsdOk = true;
                while (reader.Read());
                return bXsdOk;
            }
            catch
            {
                return false;
            }
            finally
            {
                if (xsdReader != null)
                    xsdReader.Close();
                if (reader != null)
                    reader.Close();
            }*/return true;
        }

    /*	private void OnValidationError(object sender, ValidationEventArgs e)
        {
            bXsdOk = false;
        }*/

        protected override bool LoadTheme(Stream xml, string strDefNamespace)
        {
            if (base.LoadTheme (xml, strDefNamespace))
            {
                xml.Position = 0;
                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(xml);

                    XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
                    nsmgr.AddNamespace("pref", strDefNamespace);
                        
                    XmlNode node = doc.SelectSingleNode("pref:theme/pref:misc/pref:minsize", nsmgr);
                    int d = Int32.Parse(node.Attributes["digits"].Value);
                    string strValue = node.Attributes["value"].Value;
                    int a = Int32.Parse(strValue.Substring(0, d), 
                        System.Globalization.NumberStyles.HexNumber);
                    int b = Int32.Parse(strValue.Substring(d, d), 
                        System.Globalization.NumberStyles.HexNumber);

                    MinimumSize = new Size(a, b);

                    node = doc.SelectSingleNode("pref:theme/pref:misc/pref:normalsize", nsmgr);
                    d = Int32.Parse(node.Attributes["digits"].Value);
                    strValue = node.Attributes["value"].Value;
                    a = Int32.Parse(strValue.Substring(0, d), 
                        System.Globalization.NumberStyles.HexNumber);
                    b = Int32.Parse(strValue.Substring(d, d), 
                        System.Globalization.NumberStyles.HexNumber);

                    Size = new Size(a, b);
                                                            
                    xml.Position = 0;	
                    return controlbar.LoadTheme(xml, strDefNamespace);
                }
                catch
                {
                    return false;
                }
                
            }

            return false;
        }
        #endregion

        #region Fullscreen
        public virtual void ToggleFullscreen()
        {
            if (!bFullscreen)
                FullscreenOn();
            else
                FullscreenOff();
        }

        void FullscreenOn()
        {
            nicon.Restore();
            bFullscreen = true;
                        
            bControlbar = controlbar.Visible;
            nControlbarChildIndex = Controls.GetChildIndex(controlbar);
            controlbar.Parent = controlbarHolder;
            controlbar.Visible = true;
                    
            CaptionBar.Visible = false;
            Border = false;
            TopMost = true;

            nVideoSize = engine.GetVideoSize();
            engine.SetVideoSize(VideoSize.SIZE_FREE, false);

            Rectangle rect = Screen.FromControl(this).Bounds;
            MaximumSize = rect.Size;
            formWindowState = WindowState;
            WindowState = FormWindowState.Normal;
                    
            Bounds = rect;
            
            controlbarHolder.StartPosition = FormStartPosition.Manual;
            controlbarHolder.Bounds = new Rectangle(rect.Left, rect.Bottom-controlbar.Height, 
                rect.Width, controlbar.Height);
            controlbarHolder.TopMost = true;

        //	Activate(); // We will handle KeyDown and MouseWheel of the controlbarHolder
                        // in MainFormControls so we don't need to activate ourselves
        }

        void FullscreenOff()
        {
            engine.SetVideoSize(nVideoSize, false);
            TopMost = bTopMost;
            MaximumSize = SystemInformation.WorkingArea.Size;
            DesktopBounds = rectNormal;
            WindowState = formWindowState;
            
            CaptionBar.Visible = true;
            Border = true;
            
            controlbarHolder.Visible = false;
            controlbar.Visible = bControlbar;
            controlbar.Parent = this;
            Controls.SetChildIndex(controlbar, nControlbarChildIndex);
                                    
            bFullscreen = false;
        }
        #endregion
                        
        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose( bool disposing )
        {
            if( disposing )
            {
                if (components != null) 
                {
                    components.Dispose();
                }
            }
            base.Dispose( disposing );
        }

        #region Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
        }
        #endregion

    }
}

