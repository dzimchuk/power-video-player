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
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

namespace Pvp.Theme
{
	/// <summary>
	/// Summary description for DClock.
	/// </summary>
	public class DClock : System.Windows.Forms.UserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		Bitmap background;
		Brush brush;
		StringFormat strfmt;
		const string strZeros = "00:00:00 / 00:00:00";

		public DClock()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer | 
				ControlStyles.UserPaint, true);
			SetStyle(ControlStyles.Selectable, false);

			strfmt = new StringFormat();
			strfmt.Alignment = strfmt.LineAlignment = StringAlignment.Center;

			ResetDClock();
		}

		public Bitmap BackgroundBitmap
		{
			get { return background; }
			set
			{
				if (background != null)
					background.Dispose();
				background = value;
			}
		}

		public Brush Brush
		{
			get { return brush; }
			set
			{
				if (brush != null)
					brush.Dispose();
				brush = value;
			}
		}

		public void ResetDClock()
		{
			Text = strZeros;
		}

		protected override void OnTextChanged(EventArgs e)
		{
			base.OnTextChanged (e);
			Invalidate();
		}
		
		public void AdjustFont()
		{
			Graphics g = CreateGraphics();
			SizeF sizef = g.MeasureString(strZeros, Font);
			float fscale = Math.Min(Width/sizef.Width, Height/sizef.Height);
			Font font = Font;
			Font = new Font(font.FontFamily, fscale*font.SizeInPoints);
			font.Dispose();
			g.Dispose();
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			if (Text != String.Empty)
				e.Graphics.DrawString(Text, Font, brush, ClientRectangle, strfmt);
		}

		protected override void OnPaintBackground(PaintEventArgs pevent)
		{
			if (background != null)
				pevent.Graphics.DrawImage(background, new Rectangle(0, 0, Width, Height), 
					0, 0, background.Width-1, background.Height, GraphicsUnit.Pixel);
			else
				base.OnPaintBackground (pevent);
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

		#region Component Designer generated code
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
