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

namespace Dzimchuk.AUI
{
	/// <summary>
	/// VS.NET like status bar.
	/// </summary>
	public class NetStatusBar : StatusBar
	{
		public NetStatusBar()
		{
			Height = 20;
			// After setting double buffering the base class stops doing painting. 
			// Instead it calls OnPaint and OnEraseBackground (they are not called
			// when double buffering isn't used!).
			// OnDrawItem is not called so we should call it (or any other function)
			// from OnPaint.
			SetStyle(ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint | 
				ControlStyles.UserPaint, true);
		}
		
		protected override void OnPaint(PaintEventArgs e)
		{
		//	base.OnPaint (e);
			if (ShowPanels && Panels.Count > 0)
			{
				using (Brush brush = new SolidBrush(ForeColor))
				{
					DrawPanels(e.Graphics, brush);
				}
			}
			else if (Text.Length > 0)
			{
				StringFormat strfmt = new StringFormat();
				strfmt.LineAlignment = StringAlignment.Center;
				strfmt.Alignment = StringAlignment.Near;
				strfmt.FormatFlags = StringFormatFlags.NoWrap;
				using (Brush brush = new SolidBrush(ForeColor))
				{
					e.Graphics.DrawString(Text, Font, brush, ClientRectangle, strfmt);
				}
			}
			
			if (SizingGrip)
				ControlPaint.DrawSizeGrip(e.Graphics, SystemColors.Control, 
					Width-14, Height-13, 13, 13);
		}

		protected virtual void DrawPanels(Graphics g, Brush brush)
		{
			int x = 0;
			int y = 2;
			int height = Height - y;
			
			StringFormat strfmt = new StringFormat();
			strfmt.LineAlignment = StringAlignment.Center;
			strfmt.FormatFlags = StringFormatFlags.NoWrap;
			foreach (StatusBarPanel panel in Panels)
			{
				if(panel.Alignment == HorizontalAlignment.Center)
					strfmt.Alignment = StringAlignment.Center;
				else if(panel.Alignment == HorizontalAlignment.Left)
					strfmt.Alignment = StringAlignment.Near;
				else
					strfmt.Alignment = StringAlignment.Far;

				Rectangle rect = new Rectangle(x, y, panel.Width, height);
				rect.Width -= 3;
				g.DrawString(panel.Text, Font, brush, rect, strfmt);
				rect.Height -= 1;
				g.DrawRectangle(SystemPens.ControlDark, rect);
				x += panel.Width;
			}
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize (e);
			if (Parent != null)
				SizingGrip = ((Form) Parent).WindowState == FormWindowState.Normal;
		}
				
	}

	public class NetStatusBarPanel : StatusBarPanel
	{
		public NetStatusBarPanel()
		{
			Style = StatusBarPanelStyle.OwnerDraw;
			BorderStyle = StatusBarPanelBorderStyle.None;
		}
	}
}
