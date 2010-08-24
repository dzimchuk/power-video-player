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
using System.Windows.Forms;
using Dzimchuk.AUI;
using Dzimchuk.Native;

namespace Dzimchuk.Theme
{
    /// <summary>
    /// 
    /// </summary>
    public class CaptionBar : System.Windows.Forms.Panel
    {
        public event EventHandler Minimize;
        public event EventHandler Maximize;
        public event EventHandler Close;
        
        public bool bHandOverButton = true;
        
        Form parent;
        bool bTracking;
        Size szTopLeft;
        Bitmap background;
        Bitmap caption;
        Icon icon;
        Color clrActiveCaption = Color.White;
        Color clrInactiveCaption = Color.White;

        ButtonEx btnMin, btnMax, btnClose;
        ToolTip tip;
        
        public CaptionBar(Form parent)
        {
            Parent = parent;
            this.parent = parent;
            
            SuspendLayout();
            
            Height = 26;
            Dock = DockStyle.Top;
            Font = new Font("Tahoma", 0.125f, FontStyle.Bold, GraphicsUnit.Inch);
            EventHandler eh = new EventHandler(OnUpdate);
            parent.Activated += eh;
            parent.Deactivate += eh;
            parent.TextChanged += eh;
            parent.Load += new EventHandler(parent_Load);

            Size size = new Size(16, 16);
            EventHandler ehBtn = new EventHandler(OnCaptionBtn);
            MouseEventHandler ehBtn1 = new MouseEventHandler(OnBtnMouseMove);
            btnMin = new ButtonEx(ButtonExStyle.Bitmap);
            btnMax = new ButtonEx(ButtonExStyle.Bitmap);
            btnClose = new ButtonEx(ButtonExStyle.Bitmap);
            btnMin.Size = btnMax.Size = btnClose.Size = size;
            btnMin.Anchor = btnMax.Anchor = btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnMin.TabStop = btnMax.TabStop = btnClose.TabStop = false;
            btnMin.Selectable = btnMax.Selectable = btnClose.Selectable = false;
            btnMin.Click += ehBtn;
            btnMax.Click += ehBtn;
            btnClose.Click += ehBtn;
            btnMin.MouseMove += ehBtn1;
            btnMax.MouseMove += ehBtn1;
            btnClose.MouseMove += ehBtn1;
            btnMin.MouseDown += ehBtn1;
            btnMax.MouseDown += ehBtn1;
            btnClose.MouseDown += ehBtn1;
            SetCaptionButtons(true, true, true);
            Controls.AddRange(new Control[] { btnMin, btnMax, btnClose } );

            tip = new ToolTip();
            UpdateToolTips();
            
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | 
                ControlStyles.DoubleBuffer, true);
            ResizeRedraw = true;

            ResumeLayout();
        }

        internal void SetMaxBtnToolTip()
        {
            tip.SetToolTip(btnMax, parent.WindowState == FormWindowState.Maximized ? 
                Resources.Resources.captionbar_restore : Resources.Resources.captionbar_maximize);
        }

        internal void UpdateToolTips()
        {
            tip.SetToolTip(btnMin, Resources.Resources.captionbar_minimize);
            SetMaxBtnToolTip();
            tip.SetToolTip(btnClose, Resources.Resources.captionbar_close);
        }

        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {
            base.OnPaint (e);
            Rectangle rect = ClientRectangle;
            
            if (icon != null && Height >= 16)
            {
                e.Graphics.DrawIcon(icon, new Rectangle(2, (Height-16)/2, 16, 16));
                rect = new Rectangle(rect.Left+20, rect.Top, rect.Width-20, rect.Height);
            }
            
            if (caption != null)
            {

            }
            else
            {
                StringFormat strfmt = new StringFormat(StringFormatFlags.NoWrap);
                strfmt.LineAlignment = StringAlignment.Center;
                strfmt.Alignment = StringAlignment.Near;
                
                Color clr = Form.ActiveForm == parent ? clrActiveCaption : clrInactiveCaption;
                using (Brush brush = new SolidBrush(clr))
                {
                    e.Graphics.DrawString(Parent.Text, Font, brush, rect, strfmt);
                }
            }

        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            if (background != null)
            {
                pevent.Graphics.DrawImage(background, new Rectangle(0, 0, Width, Height), 
                    0, 0, background.Width-1, background.Height, GraphicsUnit.Pixel);
            }
            else
            {
                Color clr = Form.ActiveForm == parent ? SystemColors.ActiveCaption : SystemColors.InactiveCaption;
                pevent.Graphics.Clear(clr);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown (e);
            if (e.Button == MouseButtons.Left)
            {
                Rectangle rect = parent.DesktopBounds;
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
                parent.DesktopBounds = new Rectangle(pt, parent.DesktopBounds.Size);
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == (int)WindowsMessages.WM_CAPTURECHANGED)
                bTracking = false;

            base.WndProc (ref m);
        }

        private void OnUpdate(object sender, EventArgs e)
        {
            Invalidate();
        }

        private void parent_Load(object sender, EventArgs e)
        {
            if (parent.Icon != null)
                icon = new Icon(parent.Icon, 16, 16);
        }

        public Bitmap Background
        {
            get { return background; }
            set
            {
                if (background != null)
                    background.Dispose();
                background = value;
                if (value != null)
                    Height = value.Height; 
            }
        }

        public Bitmap Caption
        {
            get { return caption; }
            set
            {
                if (caption != null)
                    caption.Dispose();
                caption = value;
            }
        }

        public Icon Icon
        {
            get { return icon; }
            set
            {
                if (icon != null)
                    icon.Dispose();
                icon = new Icon(value, 16, 16);
            }
        }

        public Color ActiveCaptionText
        {
            get { return clrActiveCaption; }
            set { clrActiveCaption = value; }
        }

        public Color InactiveCaptionText
        {
            get { return clrInactiveCaption; }
            set { clrInactiveCaption = value; }
        }

        public void SetCaptionButtons(bool bMin, bool bMax, bool bClose)
        {
            btnClose.Visible = btnMax.Visible = btnMin.Visible = false;
            int x = ClientRectangle.Width - 2;
            
            if (bClose)
            {
                x -= btnClose.Width;
                btnClose.Location = new Point(x, (Height-btnClose.Height)/2);
                btnClose.Visible = true;
                x -= 2;
            }

            if (bMax)
            {
                x -= btnMax.Width;
                btnMax.Location = new Point(x, (Height-btnMax.Height)/2);
                btnMax.Visible = true;
                x -= 2;
            }

            if (bMin)
            {
                x -= btnMin.Width;
                btnMin.Location = new Point(x, (Height-btnMin.Height)/2);
                btnMin.Visible = true;
                x -= 2;
            }
        }

        public void SetCaptionButtonsBitmaps(Bitmap min, Bitmap max, Bitmap close)
        {
            btnMin.Bitmap = min;
            btnMax.Bitmap = max;
            btnClose.Bitmap = close;
            SetCaptionButtons(min != null, max != null, close != null);
        }

        void OnCaptionBtn(object sender, EventArgs e)
        {
            ButtonEx btn = (ButtonEx) sender;
            if (btn == btnMin && Minimize != null)
                Minimize(this, EventArgs.Empty);
            else if (btn == btnMax && Maximize != null)
            {
                Maximize(this, EventArgs.Empty);
                SetMaxBtnToolTip();
            }
            else if (btn == btnClose && Close != null)
                Close(this, EventArgs.Empty);
        }

        protected override void OnDoubleClick(EventArgs e)
        {
            base.OnDoubleClick (e);
            if (Maximize != null)
            {
                Maximize(this, EventArgs.Empty);
                SetMaxBtnToolTip();
            }
        }

        void OnBtnMouseMove(object sender, MouseEventArgs e)
        {
            if (bHandOverButton)
                Cursor.Current = Cursors.Hand;
        }
    }
}
