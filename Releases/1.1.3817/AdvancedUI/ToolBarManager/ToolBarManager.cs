/*
Version 1.0 - Rogerio Paulo
Base version - see CodeProject article http://www.codeproject.com/cs/menu/ToolBarDock.asp

Version 2.0 - Martin Muller (aka mav.northwind)
All changes marked with a comment (// Added by mav). 
- ToolBarManager c'tor now takes a ScrollableControl and a Form so that you can define 
  an independent docking region and do not always have to use the form. 
  The form itself is still needed as owner of floating toolbars, though.
- ToolBarManager.AddControl() now returns the newly added ToolBarDockHolder, 
  so that its properties can be modified. 
- ToolBarDockHolder now has a new property ToolbarTitle, allowing you to specify 
  an independent name for a floating toolbar. The default is still the control's Text, 
  but the Control.TextChanged event will no longer modify the title.
- ToolBarDockHolder now has a new property AllowedBorders where you can specify 
  which borders the toolbar is allowed to dock to. I saw that by holding Ctrl 
  you can keep a toolbar from docking, so this made the process of restraining 
  docking capabilities to certain borders only vey simple!
- When building the context menu I've optimized the sorting of the toolbars a bit 
- The example now includes a MainMenu to show how the framework behaves in this 
  context and added a View>Toolbars menu where the toolbars can be shown or hidden, 
  pretty much like what the context menu does.
- Modified the size calculation for vertical toolbars with separators, because the last 
  button was clipped. Used but.Rectangle.Width*2, which seems to be quite accurate.
  
Version 2.1 - Andy Dzimchuk
- Code refactoring.
- Turned off autoscaling.
*/

using System;
using System.Windows.Forms;
using System.Drawing;
using System.Collections;

namespace Dzimchuk.AUI
{
	/// <summary>
	/// This class handles all UI operations on the toolbars.
	/// </summary>
	public class ToolBarManager : IMessageFilter
	{
		ScrollableControl dockStation = null;
		Form mainForm = null;
		ToolBarDockArea _left, _right, _top, _bottom;
		ArrayList holders = new ArrayList();

		ToolBarDockHolder dragged;
		Point ptStart;
		Point ptOffset;

		const int WM_KEYDOWN = 0x100;
		const int WM_KEYUP = 0x101; 
		bool ctrlDown = false;
		
		public ToolBarManager(ScrollableControl dockStation, Form mainForm)
		{
			this.dockStation = dockStation;
			this.mainForm = mainForm;
			_left = new ToolBarDockArea(this, DockStyle.Left);
			_right = new ToolBarDockArea(this, DockStyle.Right);
			_top = new ToolBarDockArea(this, DockStyle.Top);
			_bottom = new ToolBarDockArea(this, DockStyle.Bottom);

			Application.AddMessageFilter(this);
		}

		#region IMessageFilter Members

		public bool PreFilterMessage(ref Message m)
		{
			if(m.Msg == WM_KEYDOWN) 
			{
				Keys keyCode = (Keys)(int)m.WParam & Keys.KeyCode;
				if(keyCode == Keys.ControlKey) 
				{
					if(!ctrlDown && dragged!=null && IsDocked(dragged)) 
					{
						ToolBarDockArea docked = GetDockedArea(dragged);
						docked.SuspendLayout();
						dragged.Parent = dragged.FloatForm;
						dragged.Location = new Point(0,0);
						dragged.DockStyle = DockStyle.None;
						dragged.FloatForm.Visible = true;
						dragged.FloatForm.Location = new Point(Control.MousePosition.X-ptOffset.X, Control.MousePosition.Y-8);
						dragged.FloatForm.Size = dragged.Size;
						docked.ResumeLayout();
						docked.PerformLayout();
					}
					ctrlDown = true;
				}
			} 
			else if(m.Msg == WM_KEYUP) 
			{
				Keys keyCode = (Keys)(int)m.WParam & Keys.KeyCode;
				if(keyCode == Keys.ControlKey) 
				{
					if(ctrlDown && dragged!=null && !IsDocked(dragged)) 
					{
						ToolBarDockArea closest = GetClosestArea(Control.MousePosition, dragged.PreferredDockedArea);
						if(closest != null)  
						{
							closest.SuspendLayout();
							Point newLoc = closest.PointToClient(Control.MousePosition);
							dragged.DockStyle = closest.Dock;
							dragged.Parent = closest;
							dragged.PreferredDockedLocation = newLoc;
							dragged.FloatForm.Visible = false;
							dragged.PreferredDockedArea = closest;
							closest.ResumeLayout();
							closest.PerformLayout();
						}
					}
					ctrlDown = false;
				}
			}
			return false;
		}

		#endregion

		#region Public Properties
		public ScrollableControl DockStation 
		{
			get { return dockStation; }
			set { dockStation = value; }
		}

		public Form MainForm
		{
			get {  return mainForm; }
			set { mainForm = value; }    
		}

		public ToolBarDockArea Left { get { return _left; } }
		public ToolBarDockArea Right { get { return _right; } }
		public ToolBarDockArea Top { get { return _top; } }
		public ToolBarDockArea Bottom { get { return _bottom; } }

		#endregion

		#region Public Methods
		public virtual void ShowContextMenu(Point ptScreen) 
		{

		}

		// Added by mav
		public ToolBarDockHolder GetHolder(Control c)
		{		
			foreach(ToolBarDockHolder holder in holders) 
				if(holder.Control == c)
					return holder;
			return null;
		}
		public ToolBarDockHolder GetHolder(string title)
		{		
			foreach(ToolBarDockHolder holder in holders) 
				if(holder.ToolbarTitle == title)
					return holder;
			return null;
		}

		public ArrayList GetControls()
		{
			ArrayList list = new ArrayList();			
			foreach(ToolBarDockHolder holder in holders) 
				list.Add(holder.Control);
			return list;
		}

		public bool ContainsControl(Control c)
		{
			return GetControls().Contains(c);
		}

		public void ShowControl(Control c, bool show) 
		{
			ToolBarDockHolder holder = GetHolder(c);
			if(holder != null) 
			{
				if(holder.Visible != show) 
				{
					if(IsDocked(holder))
					{
						holder.Visible = show;
					}
					else
					{
						holder.FloatForm.Visible = show;
					}
				}
			}
		}

		// Added by mav
		public ToolBarDockHolder AddControl(Control c, DockStyle site) 
		{
			return AddControl(c, site, null, DockStyle.Right);
		}

		public ToolBarDockHolder AddControl(Control c) 
		{
			return AddControl(c, DockStyle.Top, null, DockStyle.Right);
		}

		public ToolBarDockHolder AddControl(Control c, DockStyle site, Control refControl, DockStyle refSite) 
		{
			if(site == DockStyle.Fill) 
				site = DockStyle.Top;

			ToolBarDockHolder holder = new ToolBarDockHolder(this, c, site);

			if(refControl != null) 
			{
				ToolBarDockHolder refHolder = GetHolder(refControl);
				if(refHolder != null) 
				{
					Point p = refHolder.PreferredDockedLocation;
					if(refSite == DockStyle.Left) 
					{
						p.X -= 1;
					} 
					else if(refSite == DockStyle.Right) 
					{
						p.X += refHolder.Width+1;
					}
					else if(refSite == DockStyle.Bottom) 
					{
						p.Y += refHolder.Height+1;
					} 
					else
					{
						p.Y -= 1;
					}
					holder.PreferredDockedLocation = p;
				}
			}


			holders.Add(holder);
			if(site != DockStyle.None) 
			{
				holder.DockStyle = site;
				holder.Parent = holder.PreferredDockedArea;
			} 
			else 
			{
				holder.Parent = holder.FloatForm;
				holder.Location = new Point(0,0);
				holder.DockStyle = DockStyle.None;
				holder.FloatForm.Size = holder.Size;
				holder.FloatForm.Visible = true;
			}

			holder.MouseUp += new MouseEventHandler(ToolBarMouseUp);
			holder.DoubleClick += new EventHandler(ToolBarDoubleClick);
			holder.MouseMove += new MouseEventHandler(ToolBarMouseMove);
			holder.MouseDown += new MouseEventHandler(ToolBarMouseDown);

			return holder;
		}

		public void RemoveControl(Control c) 
		{
			ToolBarDockHolder holder = GetHolder(c);
			if(holder != null)
			{
				holder.MouseUp -= new MouseEventHandler(this.ToolBarMouseUp);
				holder.DoubleClick -= new EventHandler(this.ToolBarDoubleClick);
				holder.MouseMove -= new MouseEventHandler(this.ToolBarMouseMove);
				holder.MouseDown -= new MouseEventHandler(this.ToolBarMouseDown);
				
				holders.Remove(holder);
				holder.Parent = null;
				holder.FloatForm.Close();
			}			
		}

		#endregion

		protected ToolBarDockArea GetClosestArea(Point ptScreen, ToolBarDockArea preferred)
		{
			if(preferred != null) 
			{
				Rectangle p = preferred.RectangleToScreen(preferred.ClientRectangle);
				p.Inflate(8,8);
				if(p.Contains(ptScreen)) return preferred;
			}

			Rectangle l = _left.RectangleToScreen(_left.ClientRectangle); 
			l.Inflate(8,8);
			Rectangle r = _right.RectangleToScreen(_right.ClientRectangle);
			r.Inflate(8,8);
			Rectangle t = _top.RectangleToScreen(_top.ClientRectangle);
			t.Inflate(8,8);
			Rectangle b = _bottom.RectangleToScreen(_bottom.ClientRectangle);
			b.Inflate(8,8);

			if(t.Contains(ptScreen)) return _top;
			if(b.Contains(ptScreen)) return _bottom;
			if(l.Contains(ptScreen)) return _left;
			if(r.Contains(ptScreen)) return _right;

			return null;
		}
		
		private void ToolBarMouseDown(object sender, MouseEventArgs e)
		{
			ToolBarDockHolder holder = (ToolBarDockHolder) sender;

			if(dragged==null 
				&& e.Button.Equals(MouseButtons.Left) 
				&& e.Clicks == 1
				&& holder.CanDrag(new Point(e.X, e.Y)) )
			{
				ptStart = Control.MousePosition;
				dragged = (ToolBarDockHolder)sender;
				ptOffset = new Point(e.X, e.Y);
			}
		}

		private bool IsDocked(ToolBarDockHolder holder)
		{
			return holder.Parent == Top
				|| holder.Parent == Left
				|| holder.Parent == Right
				|| holder.Parent == Bottom;
		}

		private ToolBarDockArea GetDockedArea(ToolBarDockHolder holder)
		{
			if(holder.Parent == Top) return Top;
			if(holder.Parent == Left) return Left;
			if(holder.Parent == Right) return Right;
			if(holder.Parent == Bottom) return Bottom;
			return null;
		}

		private void ToolBarMouseMove(object sender, MouseEventArgs e)
		{
			Point ptPos = new Point(e.X, e.Y);

			if(dragged != null)
			{
				Point ptDelta = new Point(ptStart.X - Control.MousePosition.X, ptStart.Y - Control.MousePosition.Y);

				Point newLoc = dragged.PointToScreen(new Point(0,0));
				newLoc = new Point(newLoc.X - ptDelta.X, newLoc.Y - ptDelta.Y);
				ToolBarDockArea closest = GetClosestArea(Control.MousePosition, dragged.PreferredDockedArea);
				// Added by mav
				if(closest != null && !dragged.IsAllowed(closest.Dock))
					closest = null;

				ToolBarDockArea docked = GetDockedArea(dragged);

				if(ctrlDown)
					closest = null;

				if(docked != null)
				{
					if(closest == null) 
					{
						docked.SuspendLayout();
						dragged.Parent = dragged.FloatForm;
						dragged.Location = new Point(0,0);
						dragged.DockStyle = DockStyle.None;
						dragged.FloatForm.Visible = true;
						dragged.FloatForm.Location = new Point(Control.MousePosition.X-ptOffset.X, Control.MousePosition.Y-8);
						dragged.FloatForm.Size = dragged.Size;
						docked.ResumeLayout();
						docked.PerformLayout();
					} 
					else if(closest != docked) 
					{
						closest.SuspendLayout();
						newLoc = closest.PointToClient(Control.MousePosition);
						dragged.DockStyle = closest.Dock;
						dragged.Parent = closest;
						dragged.PreferredDockedLocation = newLoc;
						dragged.FloatForm.Visible = false;
						dragged.PreferredDockedArea = closest;
						closest.ResumeLayout();
						closest.PerformLayout();
					} 
					else 
					{
						closest.SuspendLayout();
						newLoc = closest.PointToClient(Control.MousePosition);
						//						if(closest.Horizontal)
						//							newLoc = new Point(newLoc.X - 4, newLoc.Y - dragged.Height/2);
						//						else
						//							newLoc = new Point(newLoc.X - dragged.Width/2, newLoc.Y - 4);
						dragged.PreferredDockedLocation = newLoc;
						closest.ResumeLayout();
						closest.PerformLayout();
					}
				}
				else
				{
					if(closest == null) 
					{
						dragged.FloatForm.Location = newLoc;
					}
					else
					{
						closest.SuspendLayout();
						newLoc = closest.PointToClient(Control.MousePosition);
						dragged.DockStyle = closest.Dock;
						dragged.Parent = closest;
						dragged.PreferredDockedLocation = newLoc;
						dragged.FloatForm.Visible = false;
						dragged.PreferredDockedArea = closest;
						closest.ResumeLayout();
						closest.PerformLayout();
					}
				}
				ptStart = Control.MousePosition;
			}
		}

		private void ToolBarMouseUp(object sender, MouseEventArgs e)
		{
			if(dragged != null)
			{
				dragged = null;
				ptOffset.X = 8;
				ptOffset.Y = 8;
			}
		}

		private void ToolBarDoubleClick(object sender, System.EventArgs e)
		{
			ToolBarDockHolder holder = (ToolBarDockHolder)sender;
			if(IsDocked(holder))
			{
				ToolBarDockArea docked = GetDockedArea(holder);
				docked.SuspendLayout();
				holder.Parent = holder.FloatForm;
				holder.Location = new Point(0,0);
				holder.DockStyle = DockStyle.None;
				holder.FloatForm.Visible = true;
				holder.FloatForm.Size = holder.Size;
				docked.ResumeLayout();
				docked.PerformLayout();
			}
			else
			{
				ToolBarDockArea area = holder.PreferredDockedArea;
				area.SuspendLayout();
				Point newLoc = holder.PreferredDockedLocation;
				holder.DockStyle = area.Dock;
				holder.Parent = area;
				holder.PreferredDockedLocation = newLoc;
				holder.FloatForm.Visible = false;
				holder.PreferredDockedArea = area;
				area.ResumeLayout();				
				area.PerformLayout();
			}
		}

	}
}
