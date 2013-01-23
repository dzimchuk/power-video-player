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
using Pvp.Core.Native;

namespace Pvp.Theme
{
	/// <summary>
	/// 
	/// </summary>
	public class Border : System.Windows.Forms.Control
	{
		bool bTracking;
		Size szTopLeft, szBottomRight;
		MousePos pos;
		const int gap = 10;
		Form parent;
		
		public Color clrInner = Color.FromArgb(212, 208, 200);
		public Color clrMid = Color.FromArgb(172, 168, 153);
		public Color clrOuter = Color.FromArgb(113, 111, 100);
		public bool bSizable = true;
		
		public Border(Form parent, DockStyle ds)
		{
			Parent = parent;
			this.parent = parent;
			Dock = ds;
			if (ds == DockStyle.Top || ds == DockStyle.Bottom)
				Height = 3;
			else
				Width = 3;

			SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | 
				ControlStyles.DoubleBuffer, true);
			SetStyle(ControlStyles.Selectable, false);
			ResizeRedraw = true;
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			using (Pen penInner = new Pen(clrInner))
			{
				using (Pen penMid = new Pen(clrMid))
				{
					using (Pen penOuter = new Pen(clrOuter))
					{
						Graphics g = e.Graphics;
						switch(Dock)
						{
							case DockStyle.Top:
								g.DrawLine(penOuter, 0, 0, Size.Width-1, 0);
								g.DrawLine(penMid, 0, 1, Size.Width-1, 1);
								g.DrawLine(penInner, 0, 2, Size.Width-1, 2);
								break;
							case DockStyle.Bottom:
								g.DrawLine(penOuter, 0, 2, Size.Width-1, 2);
								g.DrawLine(penMid, 0, 1, Size.Width-1, 1);
								g.DrawLine(penInner, 0, 0, Size.Width-1, 0);
								break;
							case DockStyle.Left:
								g.DrawLines(penOuter, new Point[]
									{	new Point(2, Size.Height-1), 
										new Point(0, Size.Height-1), 
										new Point(0, 0), 
										new Point(2, 0) } );
								g.DrawLines(penMid, new Point[]
									{	new Point(2, Size.Height-2), 
										new Point(1, Size.Height-2), 
										new Point(1, 1), 
										new Point(2, 1) } );
								g.DrawLine(penInner, 2, 2, 2, Size.Height-3);
								break;
							case DockStyle.Right:
								g.DrawLines(penOuter, new Point[]
									{	new Point(0, Size.Height-1), 
										new Point(2, Size.Height-1), 
										new Point(2, 0), 
										new Point(0, 0) } );
								g.DrawLines(penMid, new Point[]
									{	new Point(0, Size.Height-2), 
										new Point(1, Size.Height-2), 
										new Point(1, 1), 
										new Point(0, 1) } );
								g.DrawLine(penInner, 0, 2, 0, Size.Height-3);
								break;
						}
					}
				}
			}
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown (e);
			if (bSizable)
				SetCursor(e);
			if (e.Button == MouseButtons.Left)
			{
				Rectangle rect = parent.DesktopBounds;
				Point pt = PointToScreen(new Point(e.X, e.Y));
				szTopLeft = new Size(pt.X - rect.Left, pt.Y - rect.Top);
				szBottomRight = new Size(rect.Right - pt.X, rect.Bottom - pt.Y);
				switch(Dock)
				{
					case DockStyle.Top:
						if (e.X >= Size.Width-gap)
							pos = MousePos.TopRight;
						else if (e.X <= gap)
							pos = MousePos.TopLeft;
						else
							pos = MousePos.Top;
						break;
					case DockStyle.Bottom:
						if (e.X >= Size.Width-gap)
							pos = MousePos.BottomRight;
						else if (e.X <= gap)
							pos = MousePos.BottomLeft;
						else
							pos = MousePos.Bottom;
						break;
					case DockStyle.Left:
						if (e.Y >= Size.Height-gap)
							pos = MousePos.BottomLeft;
						else if (e.Y <= gap)
							pos = MousePos.TopLeft;
						else
							pos = MousePos.Left;
						break;
					case DockStyle.Right:
						if (e.Y >= Size.Height-gap)
							pos = MousePos.BottomRight;
						else if (e.Y <= gap)
							pos = MousePos.TopRight;
						else
							pos = MousePos.Right;
						break;
					default:
						pos = MousePos.None;
						break;
				}

				bTracking = true;
			}
			else
				Capture = false;
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove (e);
			if (bTracking)
			{
				Rectangle rect = parent.DesktopBounds;
				Point pt = PointToScreen(new Point(e.X, e.Y));
				switch(pos)
				{
					case MousePos.Top:
						rect = Rectangle.FromLTRB(rect.Left, pt.Y-szTopLeft.Height, rect.Right, rect.Bottom);
						break;
					case MousePos.Bottom:
						rect = Rectangle.FromLTRB(rect.Left, rect.Top, rect.Right, pt.Y+szBottomRight.Height);
						break;
					case MousePos.Left:
						rect = Rectangle.FromLTRB(pt.X-szTopLeft.Width, rect.Top, rect.Right, rect.Bottom);
						break;
					case MousePos.Right:
						rect = Rectangle.FromLTRB(rect.Left, rect.Top, pt.X+szBottomRight.Width, rect.Bottom);
						break;
					case MousePos.TopLeft:
						rect = Rectangle.FromLTRB(pt.X-szTopLeft.Width, pt.Y-szTopLeft.Height, rect.Right, rect.Bottom);
						break;
					case MousePos.TopRight:
						rect = Rectangle.FromLTRB(rect.Left, pt.Y-szTopLeft.Height, pt.X+szBottomRight.Width, rect.Bottom);
						break;
					case MousePos.BottomLeft:
						rect = Rectangle.FromLTRB(pt.X-szTopLeft.Width, rect.Top, rect.Right, pt.Y+szBottomRight.Height);
						break;
					case MousePos.BottomRight:
						rect = Rectangle.FromLTRB(rect.Left, rect.Top, pt.X+szBottomRight.Width, pt.Y+szBottomRight.Height);
						break;
				}

				parent.DesktopBounds = rect;
				try
				{
					foreach(Control control in parent.Controls)
						control.Update();
				}
				catch
				{
					parent.Close();
				}
			}
			else if (bSizable)
				SetCursor(e);
		}

		void SetCursor(MouseEventArgs e)
		{
			switch(Dock)
			{
				case DockStyle.Top:
					if (e.X >= Size.Width-gap)
						Cursor.Current = Cursors.SizeNESW;
					else if (e.X <= gap)
						Cursor.Current = Cursors.SizeNWSE;
					else
						Cursor.Current = Cursors.SizeNS;
					break;
				case DockStyle.Bottom:
					if (e.X >= Size.Width-gap)
						Cursor.Current = Cursors.SizeNWSE;
					else if (e.X <= gap)
						Cursor.Current = Cursors.SizeNESW;
					else
						Cursor.Current = Cursors.SizeNS;
					break;
				case DockStyle.Left:
					if (e.Y >= Size.Height-gap)
						Cursor.Current = Cursors.SizeNESW;
					else if (e.Y <= gap)
						Cursor.Current = Cursors.SizeNWSE;
					else
						Cursor.Current = Cursors.SizeWE;
					break;
				case DockStyle.Right:
					if (e.Y >= Size.Height-gap)
						Cursor.Current = Cursors.SizeNWSE;
					else if (e.Y <= gap)
						Cursor.Current = Cursors.SizeNESW;
					else
						Cursor.Current = Cursors.SizeWE;
					break;
			}

		}

		protected override void WndProc(ref Message m)
		{
			if (m.Msg == (int)WindowsMessages.WM_CAPTURECHANGED)
				bTracking = false;
			
			base.WndProc (ref m);
		}

	}

	enum MousePos
	{
		Top, 
		Bottom, 
		Left, 
		Right, 
		TopLeft, 
		TopRight,
		BottomLeft,
		BottomRight,
		None
	}
}
