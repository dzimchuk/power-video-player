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
using Dzimchuk.Native;

namespace Dzimchuk.AUI
{
	/// <summary>
	/// Summary description for TrackBar.
	/// </summary>
	public class TrackBar : System.Windows.Forms.UserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		bool bStretched;
		double dCurrentPosition = 0.0;
		double dRange = 1.0;

		Bitmap bmSlider;
		Bitmap bmBack;
		bool bMouseOnSlider;
		bool bTracking;
		int nCurrentPosition; // used for painting
		int nOffset;

		public new event EventHandler Scroll;

		public TrackBar(bool bStretched)
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();
			Size = new Size(100, 25);
			this.bStretched = bStretched;
			SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | 
				ControlStyles.DoubleBuffer, true);
			ResizeRedraw = true;
		}

		public double Range
		{
			get { return dRange; }
		}

		public double CurrentPostion
		{
			get { return dCurrentPosition; }
		}

		public Bitmap Slider
		{
			get { return bmSlider; }
			set
			{
				if (bmSlider != null)
					bmSlider.Dispose();
				bmSlider = value;
			}
		}

		public Bitmap Background
		{
			get { return bmBack; }
			set
			{
				if (bmBack != null)
					bmBack.Dispose();
				bmBack = value;
			}
		}

		public bool Selectable
		{
			get { return CanSelect; }
			set { SetStyle(ControlStyles.Selectable, value); }
		}

		public void UpdateTrackBar(double d_pos, double d_range)
		{
			UpdateTrackBar(d_pos, d_range, true);
		}
		
		public void UpdateTrackBar(double d_pos, double d_range, bool bRepaint)
		{
			if (d_pos < 0.0 || d_range < 0.0)
				return;
			
			dCurrentPosition=d_pos;
			dRange=(d_range==0.0) ? 1.0 : d_range;

			Rectangle rectSlider = new Rectangle();
			rectSlider.X=(int)((dCurrentPosition/dRange) * (ClientRectangle.Width-bmSlider.Width/2));
			rectSlider.Y=0;
			rectSlider.Width=bmSlider.Width/2;
			rectSlider.Height=bmSlider.Height;

			bMouseOnSlider = rectSlider.Contains(PointToClient(MousePosition));
			if (bRepaint)
				Invalidate();
		}

		protected override void OnPaint(PaintEventArgs e)
		{
		//	base.OnPaint (e);
			if (bmSlider != null && bmBack != null)
			{
				Graphics g = e.Graphics;
				Rectangle rect = ClientRectangle;
				nCurrentPosition = (int) ((dCurrentPosition/dRange) * (rect.Width-bmSlider.Width/2));
				if (bStretched)
				{
					if (nCurrentPosition!=0)
					{
						g.DrawImage(bmBack, new Rectangle(0, 0, nCurrentPosition, bmBack.Height), 
							bmBack.Width/2, 0, bmBack.Width/2-1, bmBack.Height, GraphicsUnit.Pixel);
					}
					g.DrawImage(bmBack, new Rectangle(nCurrentPosition+bmSlider.Width/2, 0, 
						rect.Width-nCurrentPosition-bmSlider.Width/2, bmBack.Height), 
						0, 0, bmBack.Width/2-1, bmBack.Height, GraphicsUnit.Pixel);
				}
				else
				{
					if (nCurrentPosition!=0)
					{
						g.DrawImage(bmBack, new Rectangle(0, 0, nCurrentPosition, bmBack.Height), 
							bmBack.Width/2, 0, nCurrentPosition, bmBack.Height, GraphicsUnit.Pixel);
					}
					g.DrawImage(bmBack, new Rectangle(nCurrentPosition+bmSlider.Width/2, 0, 
						rect.Width-nCurrentPosition-bmSlider.Width/2, bmBack.Height), 
						nCurrentPosition+bmSlider.Width/2, 0, bmBack.Width/2-nCurrentPosition-bmSlider.Width/2, bmBack.Height, GraphicsUnit.Pixel);
				}

				int x=(bMouseOnSlider || bTracking) ? bmSlider.Width/2 : 0;
				g.DrawImage(bmSlider, new Rectangle(nCurrentPosition, 0, bmSlider.Width/2, bmSlider.Height), 
					x, 0, bmSlider.Width/2, bmSlider.Height, GraphicsUnit.Pixel);
			}
		}

		protected override void OnPaintBackground(PaintEventArgs pevent)
		{
		//	base.OnPaintBackground (pevent);
		}

		protected virtual void OnScroll()
		{
			if (Scroll != null)
				Scroll(this, EventArgs.Empty);
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown (e);
			if (e.Button == MouseButtons.Left && bmSlider != null)
			{
				Rectangle rectSlider = new Rectangle();
				rectSlider.X=nCurrentPosition;
				rectSlider.Y=0;
				rectSlider.Width=bmSlider.Width/2;
				rectSlider.Height=bmSlider.Height;

				Point point = new Point(e.X, e.Y);
				if (rectSlider.Contains(point))
				{
					bTracking = true;
					nOffset = point.X - nCurrentPosition;
				}
				else
				{
					bTracking = true;
					if (point.X>bmSlider.Width/4 && point.X<Width-bmSlider.Width/4)
					{
						nCurrentPosition=point.X-bmSlider.Width/4;
					}
					else
					{
						nCurrentPosition=point.X<=bmSlider.Width/4 ? point.X : point.X-bmSlider.Width/2;
					}
			
					double range=Width-bmSlider.Width/2;
					double pos=nCurrentPosition;
					dCurrentPosition=dRange*(pos/range);
				//	int nPos= (int) dCurrentPosition;
				//	dCurrentPosition=nPos;
					dCurrentPosition=Math.Round(dCurrentPosition);
		
					Invalidate();
					OnScroll();

					nOffset = point.X-nCurrentPosition; 
				}
			}
			
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove (e);
			Rectangle rectSlider = new Rectangle();
			rectSlider.X=nCurrentPosition;
			rectSlider.Y=0;
			rectSlider.Width=bmSlider.Width/2;
			rectSlider.Height=bmSlider.Height;

			Point point = new Point(e.X, e.Y);
			bMouseOnSlider=rectSlider.Contains(point);

			if (bTracking) // user is dragging the slider
			{
				int n=point.X-nOffset;
				if (n<0)
					n=0;

				nCurrentPosition= n+bmSlider.Width/2 > Width ? Width-bmSlider.Width/2 : n;
		
				double range=Width-bmSlider.Width/2;
				double pos=nCurrentPosition;
				dCurrentPosition=dRange*(pos/range);
			//	int nPos= (int) dCurrentPosition;
			//	dCurrentPosition=nPos;
				dCurrentPosition=Math.Round(dCurrentPosition);

				Invalidate();
				OnScroll();
			}
			else
				Invalidate();
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave (e);
			bMouseOnSlider = false;
			Invalidate();
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
