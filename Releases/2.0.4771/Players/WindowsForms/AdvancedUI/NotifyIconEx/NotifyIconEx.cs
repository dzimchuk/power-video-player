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
using Pvp.Core.Native;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Reflection;

namespace Dzimchuk.AUI
{
	public class NotifyIconEx : System.ComponentModel.Component
	{
		#region Notify Icon Target Window
		private class NotifyIconTarget : Control
		{
			public int NotifyMsg =(int) WindowsManagement.RegisterWindowMessage("NotifyIcon-{F823A53B-3CB9-4fec-BBD4-2F2E2CE2831A}");
			
			public NotifyIconTarget()
			{
			}

			protected override void DefWndProc(ref Message msg)
			{
				if(msg.Msg == NotifyMsg) // WM_USER
				{
					int msgId = (int)msg.LParam;
					int id = (int)msg.WParam;

					switch(msgId)
					{
						case (int)WindowsMessages.WM_LBUTTONDOWN:
							break;
						case (int)WindowsMessages.WM_LBUTTONUP:
							if(ClickNotify != null)
								ClickNotify(this, id);
							break;
						case (int)WindowsMessages.WM_LBUTTONDBLCLK:
							if(DoubleClickNotify != null)
								DoubleClickNotify(this, id);
							break;
						case (int)WindowsMessages.WM_RBUTTONUP:
							if(RightClickNotify != null)
								RightClickNotify(this, id);
							break;
						case (int)WindowsMessages.WM_MOUSEMOVE:
							break;
						case (int)WindowsMessages.NIN_BALLOONSHOW:
							break;
							// this should happen when the balloon is closed using the x
							// - we never seem to get this message!
						case (int)WindowsMessages.NIN_BALLOONHIDE:
							break;
							// we seem to get this next message whether the balloon times
							// out or whether it is closed using the x
						case (int)WindowsMessages.NIN_BALLOONTIMEOUT:
							break;
						case (int)WindowsMessages.NIN_BALLOONUSERCLICK:
							if(ClickBalloonNotify != null)
								ClickBalloonNotify(this, id);
							break;
					}
				}
				else if(msg.Msg == (int) WindowsMessages.WM_TASKBAR_CREATED)
				{
					if(TaskbarCreated != null)
						TaskbarCreated(this, System.EventArgs.Empty);
				}
				else if (msg.Msg == (int)WindowsMessages.WM_INITMENUPOPUP)
				{
					if (MenuPopup != null)
						MenuPopup(msg.WParam);
				}
				else
				{
					base.DefWndProc(ref msg);
				}
			}
			
			public delegate void NotifyIconHandler(object sender, int id);
			public delegate void MenuPopupHandler(IntPtr hMenu);
		
			public event NotifyIconHandler ClickNotify;
			public event NotifyIconHandler DoubleClickNotify;
			public event NotifyIconHandler RightClickNotify;
			public event NotifyIconHandler ClickBalloonNotify;
			public event EventHandler TaskbarCreated;
			public event MenuPopupHandler MenuPopup;
		}
		#endregion

		#region Parent Window Hook
		private class Parent : NativeWindow
		{
			WindowsManagement.WINDOWPLACEMENT wndpl = new WindowsManagement.WINDOWPLACEMENT();
			Form parent;
			NotifyIconEx nicon;
			public bool bBeenMinimized;
			bool bParentMaximized;

			public Parent(Form parent, NotifyIconEx nicon)
			{
				parent.HandleCreated += new EventHandler(parent_HandleCreated);
				parent.HandleDestroyed += new EventHandler(parent_HandleDestroyed);
				nicon.Click += new EventHandler(nicon_Click);
				this.parent = parent;
				this.nicon = nicon;
			}

			private void parent_HandleCreated(object sender, EventArgs e)
			{
				AssignHandle(((Form)sender).Handle);
			}

			private void parent_HandleDestroyed(object sender, EventArgs e)
			{
				ReleaseHandle();
			}

		//	[System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name="FullTrust")]
			protected override void WndProc(ref Message m)
			{
				if (m.Msg == (int)WindowsMessages.WM_SIZE && nicon.icon != null)
				{
					WindowsManagement.GetWindowPlacement(Handle, ref wndpl);
					if (wndpl.showCmd == (uint)WindowsManagement.SW_SHOWMINIMIZED)
					{
						bParentMaximized = wndpl.flags != 0;
						if (nicon.nSysTray != 0)
						{
							nicon.Visible = true;
							parent.Visible = false;
						}
						
						bBeenMinimized = true;
					}
					else
					{
						if (!parent.Visible)
							parent.Visible = true;
						if (!nicon.bShowTrayAlways && nicon.nSysTray == 1)
							nicon.Visible = false;
						bBeenMinimized = false;
					}
				}
				
				base.WndProc (ref m);
			}

			private void nicon_Click(object sender, EventArgs e)
			{
				if (bBeenMinimized)
				{
					parent.Visible = true;
					parent.WindowState = bParentMaximized ? FormWindowState.Maximized : FormWindowState.Normal;
					if (!nicon.bShowTrayAlways && nicon.nSysTray == 1)
						nicon.Visible = false;
				}
				else
				{
					parent.WindowState = FormWindowState.Minimized;
				}
			}

			public void Restore()
			{
				if (bBeenMinimized)
				{
					parent.Visible = true;
					parent.WindowState = bParentMaximized ? FormWindowState.Maximized : FormWindowState.Normal;
					if (!nicon.bShowTrayAlways && nicon.nSysTray == 1)
						nicon.Visible = false;
				}
			}
		}
		#endregion

		private Shell.NotifyIconData data = new Shell.NotifyIconData();
	
		private static NotifyIconTarget messageSink = new NotifyIconTarget();
		private static int nextId = 1;
		
		private string text = "";
		private Icon icon = null;
		private Icon[] icons = null;
		private Timer timer = null;
		private float fAnimPeriod = 1;
		private int nCurrentIcon;
		private ContextMenu contextMenu = null;
		private bool bVisible = false;
		private bool bDoubleClick = false; // fix for extra mouse up message we want to discard
		private Shell.NotifyFlags flags;
		private Parent parent = null;
		private int nSysTray = 0;
		private bool bShowTrayAlways;
		
		public event EventHandler Click;
		public event EventHandler DoubleClick;
		public event EventHandler BalloonClick;
		public event EventHandler BeforeShowMenu;
		public event EventHandler AfterShowMenu;

		#region Properties
		public Form HandleMinMax
		{
			set
			{
				if (parent != null)
				{
					if (parent.Handle != IntPtr.Zero)
						parent.ReleaseHandle();
					parent = null;
				}

				if (value != null)
					parent = new Parent(value, this);
			}
		}

		public int SystemTray
		{
			get { return nSysTray; } // 0 - taksbar only, 1 - minimize to systray, 2 - systray only
			set { nSysTray = (value > 2 || value < 0) ? 0 : value; }
		}

		public bool ShowTrayAlways
		{
			get { return bShowTrayAlways; }
			set { bShowTrayAlways = value; }
		}

		public bool IsMinimized
		{
			get { return parent != null ? parent.bBeenMinimized : false; }
		}

		public string Text
		{
			set
			{
				if(value != null && value.Length < 128)
				{
					text = value;
					if (data.uID != 0)
					{
						data.szTip = text;
						Shell.Shell_NotifyIcon(Shell.NotifyCommand.Modify, ref data);
					}
				}
			}
			get	{ return text; }
		}

		public Icon Icon
		{
			set
			{
				if (value == null)
				{
					Animation = false;
					Visible = false;
					data.uID = 0;
				}
				icon = value;
				if (value != null && data.uID != 0)
				{
					data.hIcon = icon.Handle;
					Shell.Shell_NotifyIcon(Shell.NotifyCommand.Modify, ref data);
				}
			}
			get	{ return icon; }
		}

		public ContextMenu ContextMenu
		{
			set { contextMenu = value; }
			get { return contextMenu; }
		}

		public bool Visible
		{
			set
			{
				if (icon != null)
				{
					if (data.uID == 0)
						Create(nextId++);
					if (value)
						ShowIcon();
					else
						RemoveIcon();
				}
			}
			get	{ return bVisible; }
		}

		public Icon[] AnimationIcons
		{
			get { return icons; }
			set
			{
				bool bAnim = Animation;
				if (bAnim)
					Animation = false;
				icons = value;
				if (bAnim)
					Animation = true;
			}
		}

		public bool Animation
		{
			get { return timer != null; }
			set
			{
				if (icons != null && data.uID != 0)
				{
					if ((value && timer != null) || (!value && timer == null))
						return;
					else if (value && timer == null)
					{
						nCurrentIcon = 0;
						timer = new Timer();
						timer.Tick += new EventHandler(OnTimer);
						timer.Interval = (int) AnimPeriod*1000/icons.Length;
						timer.Enabled = true;
					}
					else
					{
						timer.Enabled = false;
						timer = null;
						data.hIcon = icon.Handle;
						Shell.Shell_NotifyIcon(Shell.NotifyCommand.Modify, ref data);
					}
				}
			}
		}

		public float AnimPeriod
		{
			get { return fAnimPeriod; }
			set { fAnimPeriod = (value<=0) ? 1 : value; }
		}

		#endregion

		public NotifyIconEx()
		{
			data.cbSize = Marshal.SizeOf(data);
			
			// add handlers
			messageSink.ClickNotify += new NotifyIconTarget.NotifyIconHandler(OnClick);
			messageSink.DoubleClickNotify += new NotifyIconTarget.NotifyIconHandler(OnDoubleClick);
			messageSink.RightClickNotify += new NotifyIconTarget.NotifyIconHandler(OnRightClick);
			messageSink.ClickBalloonNotify += new NotifyIconTarget.NotifyIconHandler(OnClickBalloon);
			messageSink.TaskbarCreated += new EventHandler(OnTaskbarCreated);
			messageSink.MenuPopup += new NotifyIconTarget.MenuPopupHandler(OnMenuPopup);
		}
		
		private void Create(int id)
		{
			data.hWnd = messageSink.Handle;
			data.uID = id;
			data.uCallbackMessage = messageSink.NotifyMsg;
			data.hIcon = icon.Handle; // this should always be valid
			data.szTip = text;
			data.uFlags = Shell.NotifyFlags.Message | Shell.NotifyFlags.Icon | Shell.NotifyFlags.Tip;

			flags = data.uFlags; // Store in case we need to recreate in OnTaskBarCreate
		}

		private void ShowIcon()
		{
			if (!bVisible)
				bVisible = (Shell.Shell_NotifyIcon(Shell.NotifyCommand.Add, ref data) != 0);
		}

		private void RemoveIcon()
		{
			if (bVisible)
				bVisible = (Shell.Shell_NotifyIcon(Shell.NotifyCommand.Delete, ref data) == 0);
		}
		
		protected override void Dispose(bool disposing)
		{
			RemoveIcon();
			base.Dispose(disposing);
		}

		public void ShowBalloon(string title, string text, Shell.NotifyInfoFlags type, int timeoutInMilliSeconds)
		{
			if(timeoutInMilliSeconds < 0)
				throw new ArgumentException("The parameter must be >= 0", "timeoutInMilliseconds");

			if (bVisible)
			{
				data.uFlags |= Shell.NotifyFlags.Info;
				data.uTimeoutOrVersion = timeoutInMilliSeconds; // this value does not seem to work - any ideas?
				data.szInfoTitle = title;
				data.szInfo = text;
				data.dwInfoFlags = type;

				Shell.Shell_NotifyIcon(Shell.NotifyCommand.Modify, ref data);
			}
		}

		public void HideBalloon()
		{
			if (bVisible)
			{
				data.uFlags = Shell.NotifyFlags.Message | Shell.NotifyFlags.Icon | Shell.NotifyFlags.Tip;
				Shell.Shell_NotifyIcon(Shell.NotifyCommand.Modify, ref data);
			}
		}

		public void Restore()
		{
			if (parent != null)
				parent.Restore();
		}

		#region Message Handlers

		private void OnClick(object sender, int id)
		{
			if(id == data.uID)
			{
				if(!bDoubleClick && Click != null)
					Click(this, EventArgs.Empty);
				bDoubleClick = false;
			}
		}

		private void OnRightClick(object sender, int id)
		{
			if (id == data.uID && contextMenu != null)
			{
				Point pt = Control.MousePosition;
				GDI.POINT point = new GDI.POINT();
				point.x = pt.X;
				point.y = pt.Y;

				// this ensures that if we show the menu and then click on another window the menu will close
				WindowsManagement.SetForegroundWindow(messageSink.Handle);
				
				if (BeforeShowMenu != null)
					BeforeShowMenu(this, EventArgs.Empty);
				// call non public member of ContextMenu
				contextMenu.GetType().InvokeMember("OnPopup",
					BindingFlags.NonPublic|BindingFlags.InvokeMethod|BindingFlags.Instance,
					null, contextMenu, new Object[] {System.EventArgs.Empty});
				WindowsManagement.TrackPopupMenuEx(contextMenu.Handle, 64, point.x, point.y, messageSink.Handle, IntPtr.Zero);
				if (AfterShowMenu != null)
					AfterShowMenu(this, EventArgs.Empty);
			}
		}

		private void OnDoubleClick(object sender, int id)
		{
			if(id == data.uID)
			{
				bDoubleClick = true;
				if(DoubleClick != null)
					DoubleClick(this, EventArgs.Empty);
			}
		}

		private void OnClickBalloon(object sender, int id)
		{
			if(id == data.uID)
				if(BalloonClick != null)
					BalloonClick(this, EventArgs.Empty);
		}

		private void OnTaskbarCreated(object sender, EventArgs e)
		{
			if(bVisible)
			{
				data.uFlags = flags;
				ShowIcon();
			}
		}

		private void OnTimer(object sender, EventArgs e)
		{
			if(bVisible)
			{
				data.hIcon = icons[nCurrentIcon].Handle;
				Shell.Shell_NotifyIcon(Shell.NotifyCommand.Modify, ref data);
				if (++nCurrentIcon == icons.Length)
					nCurrentIcon = 0;
			}
		}

		private void OnMenuPopup(IntPtr hMenu)
		{
			if (contextMenu != null)
			{
				bool bFound = false;
				PerformPopup(contextMenu, hMenu, ref bFound);
			}
		}

		private void PerformPopup(Menu popup, IntPtr hMenu, ref bool bFound)
		{
			if (hMenu == popup.Handle && !(popup is ContextMenu))
			{
				// no need to handle for ContextMenu
				((MenuItemEx)popup).PerformPopup();
				bFound = true;
				return;
			}

			MenuItem item;
			int count = popup.MenuItems.Count;
			for(int i=0; i<count; i++)
			{
				if(bFound)
					break;
				item = popup.MenuItems[i];
				if(item.IsParent)
					PerformPopup(item, hMenu, ref bFound);
			}
		}
		#endregion
	}
}
