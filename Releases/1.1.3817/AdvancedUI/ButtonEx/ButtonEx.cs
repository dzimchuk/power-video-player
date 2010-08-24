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
using Dzimchuk.Native;

namespace Dzimchuk.AUI
{
	/// <summary>
	/// 
	/// </summary>
	public class ButtonEx : System.Windows.Forms.Button
	{
		ButtonExStyle style;
		Bitmap bitmap; // 0 - normal, 1 - hover, 2 - pressed, 3 - disabled
		bool bTracking;
		bool bSticky;
		bool bPressed;
				
		public ButtonEx() : this(ButtonExStyle.Normal)
		{	
		}
		
		public ButtonEx(ButtonExStyle style)
		{
			this.style = style;
		}

		public ButtonExStyle Style
		{
			get { return style; }
			set { style = value; }
		}

		public bool Sticky
		{
			get { return bSticky; }
			set { bSticky = value; }
		}

		public bool Pressed
		{
			get { return bPressed; }
			set
			{
				Sticky = true;
				bPressed = value;
				Invalidate();
			}
		}

		public Bitmap Bitmap
		{
			get { return bitmap; }
			set
			{
				if (value == null && style != ButtonExStyle.Normal)
					style = ButtonExStyle.Normal;
				if (bitmap != null)
					bitmap.Dispose();
				bitmap = value;
				if (value != null)
					Size = new Size(value.Width/4, value.Height);
			}
		}

		public bool Selectable
		{
			get { return CanSelect; }
			set { SetStyle(ControlStyles.Selectable, value); }
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			Point pt = PointToClient(MousePosition);
			if (style == ButtonExStyle.Normal || bitmap == null)
			{
				base.OnPaint (e);
				return;
			}
			else if (!Enabled)
			{
				e.Graphics.DrawImage(bitmap, new Rectangle(0, 0, Width, Height), 
					bitmap.Width*3/4, 0, bitmap.Width/4, bitmap.Height, GraphicsUnit.Pixel);
			}
			else if (Region != null && GDI.PtInRegion(Region.GetHrgn(e.Graphics), pt.X, pt.Y) != 0)
			{
				int n = bTracking || (bSticky && bPressed) ? 2 : 1;
				e.Graphics.DrawImage(bitmap, new Rectangle(0, 0, Width, Height), 
					bitmap.Width*n/4, 0, bitmap.Width/4, bitmap.Height, GraphicsUnit.Pixel);
			}
			else if (Region == null && ClientRectangle.Contains(pt))
			{
				int n = bTracking || (bSticky && bPressed) ? 2 : 1;
				e.Graphics.DrawImage(bitmap, new Rectangle(0, 0, Width, Height), 
					bitmap.Width*n/4, 0, bitmap.Width/4, bitmap.Height, GraphicsUnit.Pixel);
			}
			else if (bSticky && bPressed)
			{
				e.Graphics.DrawImage(bitmap, new Rectangle(0, 0, Width, Height), 
					bitmap.Width/2, 0, bitmap.Width/4, bitmap.Height, GraphicsUnit.Pixel);
			}
			else
			{
				e.Graphics.DrawImage(bitmap, new Rectangle(0, 0, Width, Height), 
					0, 0, bitmap.Width/4, bitmap.Height, GraphicsUnit.Pixel);
			}

			if (style == ButtonExStyle.BitmapAndText)
			{
				StringFormat strfmt = new StringFormat(StringFormatFlags.NoWrap);
				switch(TextAlign)
				{
					case ContentAlignment.BottomCenter:
						strfmt.Alignment = StringAlignment.Center;
						strfmt.LineAlignment = StringAlignment.Far;
						break;
					case ContentAlignment.BottomLeft:
						strfmt.Alignment = StringAlignment.Near;
						strfmt.LineAlignment = StringAlignment.Far;
						break;
					case ContentAlignment.BottomRight:
						strfmt.Alignment = StringAlignment.Far;
						strfmt.LineAlignment = StringAlignment.Far;
						break;
					case ContentAlignment.MiddleCenter:
						strfmt.Alignment = StringAlignment.Center;
						strfmt.LineAlignment = StringAlignment.Center;
						break;
					case ContentAlignment.MiddleLeft:
						strfmt.Alignment = StringAlignment.Near;
						strfmt.LineAlignment = StringAlignment.Center;
						break;
					case ContentAlignment.MiddleRight:
						strfmt.Alignment = StringAlignment.Far;
						strfmt.LineAlignment = StringAlignment.Center;
						break;
					case ContentAlignment.TopCenter:
						strfmt.Alignment = StringAlignment.Center;
						strfmt.LineAlignment = StringAlignment.Near;
						break;
					case ContentAlignment.TopLeft:
						strfmt.Alignment = StringAlignment.Near;
						strfmt.LineAlignment = StringAlignment.Near;
						break;
					case ContentAlignment.TopRight:
						strfmt.Alignment = StringAlignment.Far;
						strfmt.LineAlignment = StringAlignment.Near;
						break;
				}

				using (Brush brush = new SolidBrush(ForeColor))
				{
					e.Graphics.DrawString(Text, Font, brush, ClientRectangle, strfmt);
				}
			}
		}

		protected override void OnMouseEnter(EventArgs e)
		{
			base.OnMouseEnter (e);
			if (style != ButtonExStyle.Normal)
				Invalidate();
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave (e);
			if (style != ButtonExStyle.Normal)
				Invalidate();
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown (e);
			if (style != ButtonExStyle.Normal && e.Button == MouseButtons.Left)
				bTracking = true;
			else if (style != ButtonExStyle.Normal)
				Capture = false;
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp (e);
			if (style != ButtonExStyle.Normal && e.Button == MouseButtons.Left)
			{
				bTracking = false;
				Invalidate();
			}
		}

		protected override void OnClick(EventArgs e)
		{
			bPressed = !bPressed;
			base.OnClick (e);
		}

	}

	public enum ButtonExStyle
	{
		Normal,
		Bitmap,
		BitmapAndText
	}
}
