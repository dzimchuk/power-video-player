using System;
using System.Windows.Forms;
using System.Drawing;

namespace Dzimchuk.AUI
{
	/// <summary>
	/// 
	/// </summary>
	public class ToolBarDockHolder : UserControl
	{
		AllowedBorders allowedBorders = AllowedBorders.All;
		Control control;
		Point preferredDockedLocation = new Point(0,0);
		ToolBarDockArea preferredDockedArea;
		Form form = new Form();
		string toolbarTitle = string.Empty;
		DockStyle style = DockStyle.Top;
		Panel panel;
		ToolBarManager dockManager = null;
		
		public ToolBarDockHolder(ToolBarManager dm, Control c, DockStyle style)
		{
			SetStyle(ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint | 
				ControlStyles.UserPaint, true);
			BackColor = SystemColors.ControlLight;

			panel = new Panel();
			panel.Parent = this;
			panel.Dock = DockStyle.Fill;

			panel.Controls.Add(c);
			DockManager = dm;
			if(style == DockStyle.Left) 
			{
				preferredDockedArea = dm.Left;
			} 
			else if(style == DockStyle.Right) 
			{
				preferredDockedArea = dm.Right;
			}
			else if(style == DockStyle.Bottom) 
			{
				preferredDockedArea = dm.Bottom;
			} 
			else
			{
				preferredDockedArea = dm.Top;
			}
			
			control = c;
			FloatForm.Visible = false;
			FloatForm.FormBorderStyle = FormBorderStyle.None;
			FloatForm.MaximizeBox = false;
			FloatForm.MinimizeBox = false;
			FloatForm.ShowInTaskbar = false;
			FloatForm.ClientSize = new Size(10,10);
			// Added by mav
			DockManager.MainForm.AddOwnedForm(FloatForm); 
			DockStyle = style; // this will call create()
			ToolbarTitle = c.Text;
		}

		#region Public Properties
		public AllowedBorders AllowedBorders
		{
			get {  return allowedBorders; }
			set { allowedBorders = value; }    
		}

		public Control Control { get { return control; } }

		public Point PreferredDockedLocation 
		{
			get { return preferredDockedLocation; }
			set { preferredDockedLocation = value; }
		}

		public ToolBarDockArea PreferredDockedArea 
		{
			get { return preferredDockedArea; }
			set { preferredDockedArea = value; }
		}

		public Form FloatForm 
		{
			get { return form; }
		}
		
		public string ToolbarTitle
		{
			get {  return toolbarTitle; }
			set
			{ 
				if (toolbarTitle != value)
				{
					toolbarTitle = value;
					TitleTextChanged();
				}

			}    
		}
	
		public DockStyle DockStyle 
		{
			get { return style; }
			set 
			{
				style = value;
				Create();
			}
		}

		public ToolBarManager DockManager 
		{
			get { return dockManager; }
			set { dockManager = value; }
		}
		#endregion

		public bool IsAllowed(DockStyle dock)
		{
			switch (dock)
			{
				case DockStyle.Fill:
					return false;
				case DockStyle.Top:
					return (allowedBorders & AllowedBorders.Top) == AllowedBorders.Top;
				case DockStyle.Left:
					return (allowedBorders & AllowedBorders.Left) == AllowedBorders.Left;
				case DockStyle.Bottom:
					return (allowedBorders & AllowedBorders.Bottom) == AllowedBorders.Bottom;
				case DockStyle.Right:
					return (allowedBorders & AllowedBorders.Right) == AllowedBorders.Right;
				case DockStyle.None:
					return true;
			}
			return false;
		}

		public bool CanDrag(Point p) 
		{
			if(DockStyle == DockStyle.None) 
			{
				return p.Y < 16 && p.X < Width-16;
			}
			else 
			{
				if(DockStyle != DockStyle.Right && DockStyle != DockStyle.Left)
					return p.X < 8 && ClientRectangle.Contains(p);
				return p.Y < 8 && ClientRectangle.Contains(p);
			}
		}

		private void TitleTextChanged() 
		{
			if(FloatForm.Visible)
				Invalidate(false);
		}

		private void Create() 
		{
			Size sz = new Size(0,0);
			if(typeof(System.Windows.Forms.ToolBar).IsAssignableFrom(control.GetType())) 
			{
				ToolBar tb = (ToolBar) control;
				int w = 0;
				int h = 0;
				if(DockStyle != DockStyle.Right && DockStyle != DockStyle.Left) 
				{
					control.Dock = DockStyle.Top;
					foreach(System.Windows.Forms.ToolBarButton but in tb.Buttons)
						w += but.Rectangle.Width;
					h = tb.ButtonSize.Height;
					sz = new Size(w, h);
				}
				else
				{
					control.Dock = DockStyle.Left;
					foreach(System.Windows.Forms.ToolBarButton but in tb.Buttons)
						// Added by mav
						if(but.Style == ToolBarButtonStyle.Separator) 
							h += 2*but.Rectangle.Width;
						else 
							h += but.Rectangle.Height; 
					w = tb.ButtonSize.Width + 2;
					sz = new Size(w, h);
				}
			} 
			else 
			{
				sz = control.Size;
				control.Dock = DockStyle.Fill;	
			}

			DockPadding.All = 0;
			if(DockStyle == DockStyle.None) 
			{
				DockPadding.Left = 2;
				DockPadding.Bottom = 2;
				DockPadding.Right = 2;
				DockPadding.Top = 15;
				sz = new Size(sz.Width+4, sz.Height+18);
			}
			else if(DockStyle != DockStyle.Right && DockStyle != DockStyle.Left) 
			{
				DockPadding.Left = 8;
				sz = new Size(sz.Width+8, sz.Height);
			}
			else
			{
				DockPadding.Top = 8;
				sz = new Size(sz.Width, sz.Height+8);
			}
			
			Size = sz;
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint (e);
			Graphics g = e.Graphics;
			if(DockStyle == DockStyle.None) 
			{
				g.FillRectangle(SystemBrushes.ControlDark, ClientRectangle);
				// Added by mav
				DrawString(g, ToolbarTitle, new Rectangle(0,0, this.Width - 16, 14), SystemBrushes.ControlText);
				Rectangle closeRect = new Rectangle(this.Width-15, 2, 10, 10);
				using (Pen pen = new Pen(SystemColors.ControlText))
				{
					DrawCloseButton(g, closeRect, pen);
					if(closeRect.Contains(PointToClient(MousePosition)))
						g.DrawRectangle(pen, closeRect);
				}
				Rectangle r = ClientRectangle;
				r.Width--;
				r.Height--;
				g.DrawRectangle(SystemPens.ControlDarkDark, r);
			}
			else
			{
				g.FillRectangle(SystemBrushes.ControlLight, ClientRectangle);
				int off = 2;
				using (Pen pen = new Pen(SystemColors.ControlDark))
				{
					if(DockStyle != DockStyle.Right && DockStyle != DockStyle.Left) 
					{
						for(int i=3; i<this.Size.Height-3; i+=off) 
							g.DrawLine(pen, new Point(off, i), new Point(off+off, i));
					} 
					else 
					{
						for(int i=3; i<this.Size.Width-3; i+=off) 
							g.DrawLine(pen, new Point(i, off), new Point(i, off+off));
					}
				}
			}
		}

		static int _mininumStrSize = 0;
		private void DrawString(Graphics g, string s, Rectangle area, Brush brush) 
		{
			if(_mininumStrSize == 0) 
			{
				_mininumStrSize = (int) g.MeasureString("....", Font).Width;
			}
			if(area.Width < _mininumStrSize) 
				return;
			StringFormat drawFormat = new StringFormat(StringFormatFlags.NoWrap | StringFormatFlags.FitBlackBox);
			drawFormat.Trimming = StringTrimming.EllipsisCharacter;
			SizeF ss = g.MeasureString(s, Font);
			if(ss.Height < area.Height) 
			{
				int offset = (int)(area.Height - ss.Height)/2;
				area.Y += offset;
				area.Height -= offset;
			}
			g.DrawString(s, Font, brush, area, drawFormat);
		}

		private void DrawCloseButton(Graphics g, Rectangle cross, Pen pen) 
		{
			cross.Inflate(-2, -2);

			g.DrawLine(pen, cross.X, cross.Y, cross.Right, cross.Bottom);
			g.DrawLine(pen, cross.X+1, cross.Y, cross.Right, cross.Bottom-1);
			g.DrawLine(pen, cross.X, cross.Y+1, cross.Right-1, cross.Bottom);
			g.DrawLine(pen, cross.Right, cross.Y, cross.Left, cross.Bottom);
			g.DrawLine(pen, cross.Right-1, cross.Y, cross.Left, cross.Bottom-1);
			g.DrawLine(pen, cross.Right, cross.Y+1, cross.Left+1, cross.Bottom);
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp (e);
			if(e.Button == MouseButtons.Right && CanDrag(new Point(e.X, e.Y))) 
			{				
				DockManager.ShowContextMenu(PointToScreen(new Point(e.X, e.Y)));
			} 
			// Floating Form Close Button Clicked
			if(e.Button == MouseButtons.Left 
				&& DockStyle == DockStyle.None
				&& e.Y < 16 && e.X > Width-16)
			{
				FloatForm.Visible = false;
			}
		}

		protected override void OnMouseEnter(EventArgs e)
		{
			base.OnMouseEnter (e);
			if(DockStyle != DockStyle.None && CanDrag(PointToClient(MousePosition)))
				Cursor = Cursors.SizeAll;
			else
				Cursor = Cursors.Default;		
			Invalidate(false);
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove (e);
			if(DockStyle != DockStyle.None && CanDrag(new Point(e.X, e.Y)))
				Cursor = Cursors.SizeAll;
			else
				Cursor = Cursors.Default;
			Invalidate(false);
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave (e);
			Cursor = Cursors.Default;
			Invalidate(false);
		}

		protected override void ScaleCore(float dx, float dy)
		{
		//	base.ScaleCore (dx, dy);
		}

	}

	[Flags]
	public enum AllowedBorders
	{
		None	= 0x00, // Only floating
		Top		= 0x01,
		Left	= 0x02,
		Bottom	= 0x04,
		Right	= 0x08,
		All		= Top | Left | Bottom | Right
	}
}
