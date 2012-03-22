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
using Dzimchuk.AUI;
using System.IO;
using System.Xml;
using System.Drawing.Drawing2D;
using System.Collections;
using System.Reflection;

namespace Pvp.Theme
{
    /// <summary>
    /// 
    /// </summary>
    public class ControlBar : Control
    {
        public ButtonEx btnPlay, btnPause, btnStop, btnForward, btnBackward, btnToEnd, 
            btnToBegining, btnRepeat, btnMute;

        public Dzimchuk.AUI.TrackBar seekbar, volumebar;
        public DClock dclock;

        Bitmap background_left;
        Bitmap background_mid;
        Bitmap background_right;

        ToolTip tip;

        const string strPlay = "Play";
        const string strPause = "Pause";
        const string strStop = "Stop";
        const string strForward = "Forward";
        const string strBackward = "Backward";
        const string strToEnd = "To the end";
        const string strToBegining = "To the begining";
        const string strRepeat = "Repeat";
        const string strMute = "Mute";
        const string strVolume = "Volume";
        const string strSeekbar = "Seekbar";

        public const int VOLUME_RANGE = 5000;
        int nNormalWidth;
        
        public ControlBar(Form parent) : this()
        {
            Parent = parent;
        }
        
        public ControlBar()
        {
            SuspendLayout();
            Dock = DockStyle.Bottom;

            btnPlay = new ButtonEx(ButtonExStyle.Bitmap);
            btnPause = new ButtonEx(ButtonExStyle.Bitmap);
            btnStop = new ButtonEx(ButtonExStyle.Bitmap);
            btnForward = new ButtonEx(ButtonExStyle.Bitmap);
            btnBackward = new ButtonEx(ButtonExStyle.Bitmap);
            btnToEnd = new ButtonEx(ButtonExStyle.Bitmap);
            btnToBegining = new ButtonEx(ButtonExStyle.Bitmap);
            btnRepeat = new ButtonEx(ButtonExStyle.Bitmap);
            btnMute = new ButtonEx(ButtonExStyle.Bitmap);
            btnRepeat.Sticky = true;
            btnMute.Sticky = true;

            seekbar = new Dzimchuk.AUI.TrackBar(true);
            volumebar = new Dzimchuk.AUI.TrackBar(false);

            dclock = new DClock();
            
            Controls.AddRange(new Control[] { btnPlay, btnPause, btnStop,
                                            btnForward, btnBackward, btnToEnd, 
                                            btnToBegining, btnRepeat, btnMute,
                                            seekbar, volumebar, dclock } );
            
            tip = new ToolTip();
            UpdateToolTips();
            
            MouseEventHandler eh = new MouseEventHandler(OnCtrlMouseMove);
            foreach(Control c in Controls)
            {
                c.MouseMove += eh;
                c.MouseDown += eh;
                if (c is ButtonEx)
                    ((ButtonEx)c).Selectable = false;
                else if (c is Dzimchuk.AUI.TrackBar)
                    ((Dzimchuk.AUI.TrackBar)c).Selectable = false;
            }
            
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | 
                ControlStyles.DoubleBuffer, true);
            SetStyle(ControlStyles.Selectable, false);
            ResizeRedraw = true;

            ResumeLayout();
        }

        public void UpdateToolTips()
        {
            tip.SetToolTip(btnPlay, Resources.Resources.controlbar_play);
            tip.SetToolTip(btnPause, Resources.Resources.controlbar_pause);
            tip.SetToolTip(btnStop, Resources.Resources.controlbar_stop);
            tip.SetToolTip(btnForward, Resources.Resources.controlbar_forward);
            tip.SetToolTip(btnBackward, Resources.Resources.controlbar_backward);
            tip.SetToolTip(btnToEnd, Resources.Resources.controlbar_to_end);
            tip.SetToolTip(btnToBegining, Resources.Resources.controlbar_to_begining);
            tip.SetToolTip(btnRepeat, Resources.Resources.controlbar_repeat);
            tip.SetToolTip(btnMute, Resources.Resources.controlbar_mute);
            tip.SetToolTip(seekbar, Resources.Resources.controlbar_seekbar);
            tip.SetToolTip(volumebar, Resources.Resources.controlbar_volume);
        }

        void OnCtrlMouseMove(object sender, MouseEventArgs e)
        {
            if (!(sender is DClock))
            Cursor.Current = Cursors.Hand;
        }
                
        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            if (background_left != null && background_right != null && background_mid != null)
            {
                Graphics g = pevent.Graphics;
                g.DrawImage(background_left, 0, 0, background_left.Width, Height);
                g.DrawImage(background_right, Width-background_right.Width, 0, background_right.Width, Height);
                int cx = Width - background_right.Width - background_left.Width;
                if (cx > 0)
                    g.DrawImage(background_mid, new Rectangle(background_left.Width, 0, cx, Height), 
                        0, 0, background_mid.Width-1, background_mid.Height, GraphicsUnit.Pixel);
            }
            else
                base.OnPaintBackground (pevent);
        }

        public bool LoadTheme(Stream xml, string strDefNamespace)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(xml);

                XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
                nsmgr.AddNamespace("pref", strDefNamespace);
                
                // Get the normal width of the controlbar
                XmlNode node = doc.SelectSingleNode("pref:theme/pref:misc/pref:normalsize", nsmgr);
                int d = Int32.Parse(node.Attributes["digits"].Value);
                string strValue = node.Attributes["value"].Value;
                nNormalWidth = Int32.Parse(strValue.Substring(0, d), 
                    System.Globalization.NumberStyles.HexNumber) - 6;
                
                // Get the background images for the controlbar
                string strPath = "/pref:theme/pref:backgrounds/pref:background[@parent = \"controlbar\"]";
                Type type = Parent.GetType();
                node = doc.SelectSingleNode(strPath + "[pref:name = \"left\"]", nsmgr);
                background_left = FormBase.GetBitmap(node, type);
                node = doc.SelectSingleNode(strPath + "[pref:name = \"right\"]", nsmgr);
                background_right = FormBase.GetBitmap(node, type);
                node = doc.SelectSingleNode(strPath + "[pref:name = \"mid\"]", nsmgr);
                background_mid = FormBase.GetBitmap(node, type);
                
                Height = background_left.Height;

                // apply buttons' bitmaps and locations
                strPath = "/pref:theme/pref:buttons/pref:button[@parent = \"controlbar\"][pref:name = \"";
                node = doc.SelectSingleNode(strPath + strPlay + "\"]", nsmgr);
                btnPlay.Bitmap = FormBase.GetBitmap(node, type);
                btnPlay.Location = GetPosition(node, nsmgr);
                SetControlRegion(btnPlay, node, nsmgr);

                node = doc.SelectSingleNode(strPath + strPause + "\"]", nsmgr);
                btnPause.Bitmap = FormBase.GetBitmap(node, type);
                btnPause.Location = GetPosition(node, nsmgr);
                SetControlRegion(btnPause, node, nsmgr);

                node = doc.SelectSingleNode(strPath + strStop + "\"]", nsmgr);
                btnStop.Bitmap = FormBase.GetBitmap(node, type);
                btnStop.Location = GetPosition(node, nsmgr);
                SetControlRegion(btnStop, node, nsmgr);

                node = doc.SelectSingleNode(strPath + strForward + "\"]", nsmgr);
                btnForward.Bitmap = FormBase.GetBitmap(node, type);
                btnForward.Location = GetPosition(node, nsmgr);
                SetControlRegion(btnForward, node, nsmgr);

                node = doc.SelectSingleNode(strPath + strBackward + "\"]", nsmgr);
                btnBackward.Bitmap = FormBase.GetBitmap(node, type);
                btnBackward.Location = GetPosition(node, nsmgr);
                SetControlRegion(btnBackward, node, nsmgr);

                node = doc.SelectSingleNode(strPath + strToEnd + "\"]", nsmgr);
                btnToEnd.Bitmap = FormBase.GetBitmap(node, type);
                btnToEnd.Location = GetPosition(node, nsmgr);
                SetControlRegion(btnToEnd, node, nsmgr);

                node = doc.SelectSingleNode(strPath + strToBegining + "\"]", nsmgr);
                btnToBegining.Bitmap = FormBase.GetBitmap(node, type);
                btnToBegining.Location = GetPosition(node, nsmgr);
                SetControlRegion(btnToBegining, node, nsmgr);

                node = doc.SelectSingleNode(strPath + strRepeat + "\"]", nsmgr);
                btnRepeat.Bitmap = FormBase.GetBitmap(node, type);
                btnRepeat.Location = GetPosition(node, nsmgr);
                SetControlRegion(btnRepeat, node, nsmgr);

                node = doc.SelectSingleNode(strPath + strMute + "\"]", nsmgr);
                btnMute.Bitmap = FormBase.GetBitmap(node, type);
                btnMute.Location = GetPosition(node, nsmgr);
                SetControlRegion(btnMute, node, nsmgr);

                // apply trackbars' bitmaps and locations
                strPath = "/pref:theme/pref:trackbars/pref:trackbar[@parent = \"controlbar\"][pref:name = \"";
                node = doc.SelectSingleNode(strPath + strSeekbar + "\"]", nsmgr);
                seekbar.Background = GetTrackbarBitmap(node, type, false);
                seekbar.Slider = GetTrackbarBitmap(node, type, true);
                seekbar.Location = GetPosition(node, nsmgr);
                Size size = GetSize(node, nsmgr);
                size.Width += (Width-nNormalWidth);
                seekbar.Size = size;
                AnchorControl(seekbar, node, nsmgr);

                node = doc.SelectSingleNode(strPath + strVolume + "\"]", nsmgr);
                volumebar.Background = GetTrackbarBitmap(node, type, false);
                volumebar.Slider = GetTrackbarBitmap(node, type, true);
                Point point = GetPosition(node, nsmgr);
                point.X += (Width-nNormalWidth);
                volumebar.Location = point;
                volumebar.Size = GetSize(node, nsmgr);
                AnchorControl(volumebar, node, nsmgr);
                volumebar.UpdateTrackBar(0, VOLUME_RANGE, false);

                // "skin" the dclock
                strPath = "/pref:theme/pref:backgrounds/pref:background[@parent = \"dclock\"]";
                node = doc.SelectSingleNode(strPath, nsmgr);
                dclock.BackgroundBitmap = FormBase.GetBitmap(node, type);
                strPath = "/pref:theme/pref:dclock";
                node = doc.SelectSingleNode(strPath, nsmgr);
                point = GetPosition(node, nsmgr);
                point.X += (Width-nNormalWidth);
                dclock.Location = point;
                dclock.Size = GetSize(node, nsmgr);
                AnchorControl(dclock, node, nsmgr);
                node = node.SelectSingleNode("pref:Font", nsmgr);
                dclock.Font = new Font(node.InnerText, Font.SizeInPoints);
                dclock.Brush = new SolidBrush(ColorTranslator.FromHtml(node.Attributes["color"].Value));
                dclock.AdjustFont();
                            
                return true;
            }
            catch
            {
                return false;
            }
            
        }

        Point GetPosition(XmlNode node, XmlNamespaceManager nsmgr)
        {
            XmlNode n = node.SelectSingleNode("pref:position", nsmgr);
            if (n == null)
            {
                n = node.SelectSingleNode("pref:pos", nsmgr);
                if (n == null)
                    n = node.SelectSingleNode("pref:Pos", nsmgr);
            }
                        
            int d = Int32.Parse(n.Attributes["digits"].Value);
            string strValue = n.Attributes["value"].Value;
            int a = Int32.Parse(strValue.Substring(0, d), 
                    System.Globalization.NumberStyles.HexNumber);
            int b = Int32.Parse(strValue.Substring(d, d), 
                    System.Globalization.NumberStyles.HexNumber);
            return new Point(a, b);	
        }

        Size GetSize(XmlNode node, XmlNamespaceManager nsmgr)
        {
            XmlNode n = node.SelectSingleNode("pref:size", nsmgr);
            if (n == null)
                n = node.SelectSingleNode("pref:Size", nsmgr);
                        
            int d = Int32.Parse(n.Attributes["digits"].Value);
            string strValue = n.Attributes["value"].Value;
            int a = Int32.Parse(strValue.Substring(0, d), 
                System.Globalization.NumberStyles.HexNumber);
            int b = Int32.Parse(strValue.Substring(d, d), 
                System.Globalization.NumberStyles.HexNumber);
        
            return new Size(a, b);	
        }

        GraphicsPath GetPath(XmlNode node, XmlNamespaceManager nsmgr)
        {
            ArrayList points = new ArrayList();
            ArrayList bytes = new ArrayList();

            XmlNodeList nodes = node.SelectNodes("pref:path/pref:points/pref:point", nsmgr);
            foreach(XmlNode n in nodes)
                points.Add(new PointF(Single.Parse(n.Attributes["x"].Value)/Single.Parse(n.Attributes["divx"].Value), 
                    Single.Parse(n.Attributes["y"].Value)/Single.Parse(n.Attributes["divy"].Value)));

            XmlNode n1 = node.SelectSingleNode("pref:path/pref:bytes", nsmgr);
            
            if (n1 != null)
            {
                int d = Int32.Parse(n1.Attributes["digits"].Value);
                string strValue = n1.Attributes["value"].Value;
                for (int i = 0; i < strValue.Length; i += d)
                    bytes.Add(Byte.Parse(strValue.Substring(i, d)));
            }

            if (points.Count == bytes.Count && points.Count != 0 && bytes.Count != 0)
                return new GraphicsPath((PointF[]) points.ToArray(typeof(PointF)), (byte[]) bytes.ToArray(typeof(byte)));
        
            return null;
        }

        void AnchorControl(Control ctrl, XmlNode node, XmlNamespaceManager nsmgr)
        {
            XmlNode n = node.SelectSingleNode("pref:anchor", nsmgr);
            if (n==null)
            {
                n = node.SelectSingleNode("pref:tanchor", nsmgr);
                if (n==null)
                    n = node.SelectSingleNode("pref:Anchor", nsmgr);
            }
            if (n != null)
            {
                AnchorStyles styles = AnchorStyles.None;
                if (n.Attributes["left"].Value != "0")
                    styles |= AnchorStyles.Left;
                if (n.Attributes["top"].Value != "0")
                    styles |= AnchorStyles.Top;
                if (n.Attributes["right"].Value != "0")
                    styles |= AnchorStyles.Right;
                if (n.Attributes["bottom"].Value != "0")
                    styles |= AnchorStyles.Bottom;
                ctrl.Anchor = styles;
            }
        }

        void SetControlRegion(Control ctrl, XmlNode node, XmlNamespaceManager nsmgr)
        {
            GraphicsPath path = GetPath(node, nsmgr);
            if (path != null)
                ctrl.Region = new Region(path);
            AnchorControl(ctrl, node, nsmgr);
            if ((ctrl.Anchor & AnchorStyles.Right) != 0)
            {
                Point point = ctrl.Location;
                point.X += (Width-nNormalWidth);
                ctrl.Location = point;
            }
        }

        public static Bitmap GetTrackbarBitmap(XmlNode node, Type type, bool bSlider)
        {
            Stream stream = null;
            try
            {
                int n = bSlider ? 3 : 2;
                string strFile = node.ChildNodes[n].InnerText;
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
