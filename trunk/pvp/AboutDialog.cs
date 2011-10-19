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
using System.Reflection;
using Dzimchuk.Native;

namespace Dzimchuk.Common
{
    public enum AboutDialogBorder
    {
        None,
        Standard, 
        Own
    }
    
    /// <summary>
    /// Summary description for AboutDialog.
    /// </summary>
    public class AboutDialog : System.Windows.Forms.Form
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;
        
        public Color clrInner = Color.FromArgb(212, 208, 200);
        public Color clrMid = Color.FromArgb(172, 168, 153);
        public Color clrOuter = Color.FromArgb(113, 111, 100);
        
        const int btnWidth = 60;
        const int btnHeight = 13*7/4;
        Form MainForm;
        Bitmap background;
        AboutDialogBorder Border;
        bool bTracking;
        Size szTopLeft;
        SizeF sizefOrigAutoScaleDimensions = new SizeF(6F, 13F);

        public AboutDialog(Form parent, 
                            AboutDialogBorder Border, 
                            Type type, 
                            string strResourceImage, 
                            string strCaptionText, 
                            string strText,
                            Color clrText,
                            string strOk)
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();
            
            Text = strCaptionText;
            MainForm = parent;

            this.Border = Border;
            if (Border != AboutDialogBorder.Standard)
                FormBorderStyle = FormBorderStyle.None;
            
            Button btn = new Button();
            btn.Parent = this;
            btn.Text = strOk != null ? strOk : "OK";
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.MouseOverBackColor = Color.Transparent;
            btn.FlatAppearance.MouseDownBackColor = Color.Transparent;
            btn.BackColor = Color.Transparent;
            btn.ForeColor = clrText;
            btn.Size = new Size(btnWidth, btnHeight);
            btn.DialogResult = DialogResult.OK;
            
            AcceptButton = btn;
            CancelButton = btn;

            Label label = new Label();
            label.Parent = this;
            label.Text = strText;
            label.BackColor = Color.Transparent;
            label.ForeColor = clrText;
                                    
            try
            {
                background = new Bitmap(Assembly.GetAssembly(type).GetManifestResourceStream(strResourceImage));
                int n = Border == AboutDialogBorder.Own ? 6 : 0;
                ClientSize = new Size(background.Width+n, background.Height+n);
                StartPosition = MainForm.Visible ? FormStartPosition.Manual : 
                    FormStartPosition.CenterScreen;

                btn.Location = new Point(ClientSize.Width-(int)(btnWidth*1.3), 
                                            ClientSize.Height-btnHeight*2);
                label.Location = new Point(btnWidth / 3, ClientSize.Height - (int)(btnHeight * 2.2));
                label.Size = new Size(btn.Location.X-label.Location.X, 
                                        ClientSize.Height-label.Location.Y);

                AutoScaleDimensions = sizefOrigAutoScaleDimensions;
                PerformAutoScale();
            }
            catch
            {
                background = null;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad (e);
            if (background==null)
                Close();

            if (MainForm.Visible)
            {
                int xPos = ClientSize.Width < MainForm.ClientSize.Width ? 
                    MainForm.Location.X+(MainForm.ClientSize.Width-ClientSize.Width)/2 : 
                    MainForm.Location.X+SystemInformation.FrameBorderSize.Width+
                    SystemInformation.CaptionButtonSize.Width;

                int yPos = ClientSize.Height < MainForm.ClientSize.Height ? 
                    MainForm.Location.Y+(MainForm.ClientSize.Height-ClientSize.Height)/2 : 
                    MainForm.Location.Y+SystemInformation.FrameBorderSize.Height+
                    SystemInformation.CaptionButtonSize.Height;

                Location = new Point(xPos, yPos);
            }
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            if (background != null)
            {
                Graphics g = pevent.Graphics;
                Size size = ClientSize;
                int n = Border == AboutDialogBorder.Own ? 6 : 0;
                g.DrawImage(background, n/2, n/2, size.Width-n, size.Height-n);

                if (Border == AboutDialogBorder.Own)
                {
                    using (Pen penInner = new Pen(clrInner))
                    {
                        using (Pen penMid = new Pen(clrMid))
                        {
                            using (Pen penOuter = new Pen(clrOuter))
                            {
                                g.DrawRectangle(penOuter, 0, 0, size.Width-1, size.Height-1);
                            }
                            g.DrawRectangle(penMid, 1, 1, size.Width-3, size.Height-3);
                        }
                        g.DrawRectangle(penInner, 2, 2, size.Width-5, size.Height-5);
                    }
                }
            }
            else
                base.OnPaintBackground(pevent);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown (e);
            if (e.Button == MouseButtons.Left && FormBorderStyle == FormBorderStyle.None)
            {
                Rectangle rect = DesktopBounds;
                Point pt = PointToScreen(new Point(e.X, e.Y));
                szTopLeft = new Size(pt.X - rect.Left, pt.Y - rect.Top);

                bTracking = true;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove (e);
            if (bTracking)
            {
                Point pt = PointToScreen(new Point(e.X, e.Y));
                pt -= szTopLeft;
                DesktopBounds = new Rectangle(pt, DesktopBounds.Size);
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == (int)WindowsMessages.WM_CAPTURECHANGED)
                bTracking = false;

            base.WndProc (ref m);
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
            this.SuspendLayout();
            // 
            // AboutDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 266);
            this.ControlBox = false;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutDialog";
            this.ShowInTaskbar = false;
            this.ResumeLayout(false);

        }
        #endregion
    }
}
