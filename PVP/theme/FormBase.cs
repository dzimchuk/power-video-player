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
using System.IO;
using System.Xml;
using System.Reflection;
using Dzimchuk.Native;

namespace Dzimchuk.Theme
{
    /// <summary>
    /// Summary description for FormBase.
    /// </summary>
    public class FormBase : System.Windows.Forms.Form
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;
    
        private CaptionBar caption;
        private Border borderTop, borderBottom, borderLeft, borderRight;
        private bool bInitialized;
        private bool bBorder = true;
                                
        public FormBase()
        {
            SuspendLayout();
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();
                        
            FormBorderStyle = FormBorderStyle.None;
            MinimumSize = SystemInformation.MinimumWindowSize;
            MaximumSize = SystemInformation.WorkingArea.Size;
            bInitialized = true;
                        
            caption = new CaptionBar(this);
            caption.Close += new EventHandler(OnClose);
            caption.Maximize += new EventHandler(OnMaximize);
            caption.Minimize += new EventHandler(OnMinimize);
            borderTop = new Border(this, DockStyle.Top);
            borderBottom = new Border(this, DockStyle.Bottom);
            borderLeft = new Border(this, DockStyle.Left);
            borderRight = new Border(this, DockStyle.Right);

            ResumeLayout();
        }

        protected virtual void OnCultureChanged()
        {
            caption.UpdateToolTips();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad (e);
            AddSysMenu();
            SendEmBack();
            caption.SetMaxBtnToolTip();
        }

        protected void AddSysMenu()
        {
            WindowsManagement.SetWindowLong(Handle, WindowsManagement.GWL_STYLE, 
                WindowsManagement.GetWindowLong(Handle, WindowsManagement.GWL_STYLE)
                | WindowsManagement.WS_SYSMENU | WindowsManagement.WS_MINIMIZEBOX);
        }

        protected void SendEmBack()
        {
            caption.SendToBack();
            borderTop.SendToBack();
            borderBottom.SendToBack();
            borderLeft.SendToBack();
            borderRight.SendToBack();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize (e);
            if (WindowState == FormWindowState.Maximized)
                ShowBorder(false);
            else if (bBorder != borderTop.Visible)
            {
                ShowBorder(bBorder);
            }
        }

        private void ShowBorder(bool bShow)
        {
            SuspendLayout();
            borderTop.Visible = bShow;
            borderBottom.Visible = bShow;
            borderLeft.Visible = bShow;
            borderRight.Visible = bShow;
            if (bShow)
                SendEmBack();
            ResumeLayout();
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
            this.components = new System.ComponentModel.Container();
            this.Size = new System.Drawing.Size(300,300);
            this.Text = "FormBase";
        }
        #endregion

        public new FormBorderStyle FormBorderStyle
        {
            get { return base.FormBorderStyle; }
            set
            {
                if (!bInitialized)
                    base.FormBorderStyle = value;
            }
        }

        public new bool MinimizeBox
        {
            get { return base.MinimizeBox; }
            set
            {
                if (!bInitialized)
                    base.MinimizeBox = value;
            }
        }

        public new bool MaximizeBox
        {
            get { return base.MaximizeBox; }
            set
            {
                if (!bInitialized)
                    base.MaximizeBox = value;
            }
        }

        public new bool ControlBox
        {
            get { return base.ControlBox; }
            set
            {
                if (!bInitialized)
                    base.ControlBox = value;
            }
        }

        public CaptionBar CaptionBar
        {
            get { return caption; }
        }

        public bool Border
        {
            get { return bBorder; }
            set
            {
                bBorder = value;
                if (WindowState != FormWindowState.Maximized)
                    ShowBorder(value);
            }
        }

        public bool Sizable
        {
            get { return borderTop.bSizable; }
            set
            {
                borderTop.bSizable = value;
                borderBottom.bSizable = value;
                borderLeft.bSizable = value;
                borderRight.bSizable = value;
            }
        }
        
        public void SetBorderColors(Color inner, Color mid, Color outer)
        {
            borderTop.clrInner = borderBottom.clrInner = borderLeft.clrInner = 
                borderRight.clrInner = inner;
            borderTop.clrMid = borderBottom.clrMid = borderLeft.clrMid = 
                borderRight.clrMid = mid;
            borderTop.clrOuter = borderBottom.clrOuter = borderLeft.clrOuter = 
                borderRight.clrOuter = outer;
        }

        public void GetBorderColors(out Color inner, out Color mid, out Color outer)
        {
            inner = borderTop.clrInner;
            mid = borderTop.clrMid;
            outer = borderTop.clrOuter;
        }

        protected virtual void OnMaximize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
                WindowState = FormWindowState.Maximized;
            else
                WindowState = FormWindowState.Normal;
        }

        protected virtual void OnMinimize(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        protected virtual void OnClose(object sender, EventArgs e)
        {
            Close();
        }

        protected virtual bool LoadTheme(Stream xml, string strDefNamespace)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(xml);
                
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
                nsmgr.AddNamespace("pref", strDefNamespace);
                
                // Get and apply resources
                string strPath = "/pref:theme/pref:buttons/pref:button[@parent = \"mainform\"]";
                Type type = GetType();
                XmlNode node = doc.SelectSingleNode(strPath + "[pref:name = \"Minimize\"]", nsmgr);
                Bitmap bmMinimize = GetBitmap(node, type);
                node = doc.SelectSingleNode(strPath + "[pref:name = \"Maximize\"]", nsmgr);
                Bitmap bmMaximize = GetBitmap(node, type);
                node = doc.SelectSingleNode(strPath + "[pref:name = \"Close\"]", nsmgr);
                Bitmap bmClose = GetBitmap(node, type);
                node = doc.SelectSingleNode("/pref:theme/pref:backgrounds/pref:background[@parent = \"mainform\"][pref:name = \"caption\"]", nsmgr);
                Bitmap bmCaptionBack = GetBitmap(node, type);

                caption.Background = bmCaptionBack;
                caption.SetCaptionButtonsBitmaps(bmMinimize, bmMaximize, bmClose);

                // Apply colors
                strPath = "/pref:theme/pref:colors";
                node = doc.SelectSingleNode(strPath + "/pref:borders/pref:inner", nsmgr);
                Color clr = ColorTranslator.FromHtml(node.InnerText);
                borderTop.clrInner = clr;
                borderBottom.clrInner = clr;
                borderLeft.clrInner = clr;
                borderRight.clrInner = clr;

                node = doc.SelectSingleNode(strPath + "/pref:borders/pref:mid", nsmgr);
                clr = ColorTranslator.FromHtml(node.InnerText);
                borderTop.clrMid = clr;
                borderBottom.clrMid = clr;
                borderLeft.clrMid = clr;
                borderRight.clrMid = clr;

                node = doc.SelectSingleNode(strPath + "/pref:borders/pref:outer", nsmgr);
                clr = ColorTranslator.FromHtml(node.InnerText);
                borderTop.clrOuter = clr;
                borderBottom.clrOuter = clr;
                borderLeft.clrOuter = clr;
                borderRight.clrOuter = clr;
                
                node = doc.SelectSingleNode(strPath + "/pref:ActiveCaptionText", nsmgr);
                caption.ActiveCaptionText = ColorTranslator.FromHtml(node.InnerText);
                node = doc.SelectSingleNode(strPath + "/pref:InactiveCaptionText", nsmgr);
                caption.InactiveCaptionText = ColorTranslator.FromHtml(node.InnerText);
                
                caption.Invalidate(false);
                borderTop.Invalidate();
                borderBottom.Invalidate();
                borderLeft.Invalidate();
                borderRight.Invalidate();
                return true;
            }
            catch
            {
                return false;
            }
            
        }

        public static Bitmap GetBitmap(XmlNode node, Type type)
        {
            Stream stream = null;
            try
            {
                string strFile = node.ChildNodes[2].InnerText;
                if (strFile == String.Empty)
                    throw new Exception(Resources.Resources.err_file_not_defined);
            
                string strLoc = node.ChildNodes[1].InnerText;
                if (strLoc == "resource")
                    stream = Assembly.GetAssembly(type).GetManifestResourceStream(strFile);
                else if (strLoc == "file")
                {
                    stream = null;
                }
                else
                    throw new Exception(Resources.Resources.err_resource_loc_not_defined);

                return new Bitmap(stream);
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
        }

    }
}
