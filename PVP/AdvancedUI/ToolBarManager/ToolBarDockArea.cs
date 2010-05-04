using System;
using System.Windows.Forms;
using System.Drawing;
using System.Collections;

namespace Dzimchuk.AUI
{
	/// <summary>
	/// 
	/// </summary>
	public class ToolBarDockArea : UserControl
	{
		ToolBarManager dockManager = null;
		int lastLineCount = 1;

		public ToolBarDockArea(ToolBarManager dockManager, DockStyle dockStyle)
		{
			this.dockManager = dockManager;
			SetStyle(ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint | 
				ControlStyles.UserPaint, true);
			dockManager.DockStation.Controls.Add(this);
			if(dockStyle == DockStyle.Fill || dockStyle == DockStyle.None)
				dockStyle = DockStyle.Top;
			Dock = dockStyle;
			SendToBack();
			FitHolders();
		}

		#region Public Properties
		public ToolBarManager DockManager 
		{
			get { return dockManager; }
		}
		
		public bool Horizontal
		{
			get { return Dock != DockStyle.Left && Dock != DockStyle.Right; }
		}

		#endregion

		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp (e);
			dockManager.ShowContextMenu(PointToScreen(new Point(e.X, e.Y)));
		}

		protected override void OnLayout(LayoutEventArgs levent)
		{
			base.OnLayout (levent);
			SuspendLayout();

			int lineSzForCalc = 23;

			SortedList lineList = new SortedList();
			foreach(ToolBarDockHolder holder in Controls) 
			{
				if(holder.Visible) 
				{
					int prefLine = GetPreferredLine(lineSzForCalc, holder);	
					int prefPos = GetPreferredPosition(holder);	
					LineHolder line = (LineHolder)lineList[prefLine];
					if(line == null) 
					{
						line = new LineHolder(prefLine);
						lineList.Add(prefLine, line);
					}
					int csize = GetHolderWidth(holder);
					int lsize = GetHolderLineSize(holder);
					line.AddColumn(new ColumnHolder(prefPos, holder, csize+1));
					if(line.Size-1 < lsize)
						line.Size = lsize+1;
				}
			}

			int pos = 0;
			lastLineCount = lineList.Count;
			if(lastLineCount == 0)
				lastLineCount = 1;
			for(int ndx = 0; ndx < lineList.Count; ndx++) 
			{
				LineHolder line = (LineHolder)lineList.GetByIndex(ndx);
				if(line != null) 
				{
					line.Distribute();
					foreach(ColumnHolder col in line.Columns) 
					{
						if(Horizontal) 
						{
							col.Holder.Location = new Point(col.Position, pos);
							col.Holder.PreferredDockedLocation = new Point(col.Holder.PreferredDockedLocation.X, pos + col.Holder.Height/2);
						}
						else
						{
							col.Holder.Location = new Point(pos, col.Position);
							col.Holder.PreferredDockedLocation = new Point(pos + col.Holder.Width/2, col.Holder.PreferredDockedLocation.Y);
						}
					}
					pos += line.Size+1;
				}
			}

			FitHolders();
			ResumeLayout();
		}

		protected void FitHolders() 
		{
			Size sz = new Size(0,0);
			foreach(Control c in Controls) 
				if(c.Visible) 
				{
					if(c.Right > sz.Width)
						sz.Width = c.Right;
					if(c.Bottom > sz.Height)
						sz.Height = c.Bottom;
				}			
			if(Horizontal) 
				Height = sz.Height;
			else 
				Width = sz.Width;
		}

		protected int GetPreferredLine(int lineSz, ToolBarDockHolder holder) 
		{
			int pos, sz;
			if(Horizontal) 
			{
				pos = holder.PreferredDockedLocation.Y;
				sz = holder.Size.Height;
				if(pos < 0) 
					return Int32.MinValue;
				if(pos > this.Height) 
					return Int32.MaxValue;
			} 
			else 
			{
				pos = holder.PreferredDockedLocation.X;
				sz = holder.Size.Width;
				if(pos < 0) 
					return Int32.MinValue;
				if(pos > this.Width) 
					return Int32.MaxValue;
			}
			int line = pos / lineSz;
			int posLine = line * lineSz;
			if(posLine + 3 > pos)
				return line*2;
			if(posLine + lineSz - 3 < pos)
				return line*2+2;
			return line*2 + 1;
		}

		protected int GetPreferredPosition(ToolBarDockHolder holder) 
		{
			return Horizontal ? holder.PreferredDockedLocation.X : 
											holder.PreferredDockedLocation.Y;
		}

		protected int GetHolderLineSize(ToolBarDockHolder holder) 
		{
			return Horizontal ? holder.Height : holder.Width;
		}
		protected int GetMyLineSize() 
		{
			return Horizontal ? Height : Width;
		}
		protected int GetHolderWidth(ToolBarDockHolder holder) 
		{
			return Horizontal ? holder.Width : holder.Height;
		}

		#region Internal Classes
		class LineHolder 
		{
			public ArrayList Columns = new ArrayList();			
			public int Index = 0;		
			public int Size = 0;

			public LineHolder(int index)
			{
				Index = index;
			}
			
			public void AddColumn(ColumnHolder column) 
			{
				int indx = 0;
				foreach(ColumnHolder col in Columns) 
				{
					if(col.Position > column.Position) 
					{
						Columns.Insert(indx, column);
						break;
					}
					indx++;
				}
				if(indx == Columns.Count)
					Columns.Add(column);
			}

			public void Distribute() 
			{
				int pos = 0;
				foreach(ColumnHolder col in Columns) 
				{
					if(col.Position < pos)
						col.Position = pos;	
					pos = col.Position + col.Size;
				}
			}
		}

		class ColumnHolder 
		{
			public int Position = 0;	
			public int Size = 0;		
			public ToolBarDockHolder Holder;

			public ColumnHolder(int pos, ToolBarDockHolder holder, int size)
			{
				Position = pos;
				Holder = holder;
				Size = size;
			}			
		}
		#endregion
	}
}
