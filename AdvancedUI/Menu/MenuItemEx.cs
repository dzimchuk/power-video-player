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

namespace Dzimchuk.AUI
{
	/// <summary>
	/// 
	/// </summary>
	public class MenuItemEx : MenuItem
	{
	//	object tag;
		string strText;
	//	Keys shortcut;
		
		public MenuItemEx() : base()
		{
		}

		public MenuItemEx(string text) : base(text)
		{
			strText = text;
		}

		public MenuItemEx(string text, EventHandler onClick) : base(text, onClick)
		{
			strText = text;
		}

		public MenuItemEx(string text, MenuItem[] items) : base(text, items)
		{
			strText = text;
		}

		public MenuItemEx(string text, 
						EventHandler onClick, 
						Shortcut shortcut) : base(text, onClick, shortcut)
		{
			strText = text;
		}

		public MenuItemEx(MenuMerge mergeType, 
						int mergeOrder, 
						Shortcut shortcut, 
						string text, 
						EventHandler onClick, 
						EventHandler onPopup, 
						EventHandler onSelect, 
						MenuItem[] items) : base(mergeType, mergeOrder, shortcut, 
												text, onClick, onPopup, onSelect, 
												items)
		{
			strText = text;
		}

		public override MenuItem CloneMenu()
		{
			MenuItemEx item = new MenuItemEx();
			item.CloneMenu(this);
			return item;
		}

	/*	public object Tag
		{
			get { return tag; }
			set { tag = value; }
		}*/

		public int ID
		{
			get { return MenuID; }
		}

		public new string Text
		{
			get { return strText; }
			set
			{
				strText = value;
				string str = value;
				base.Text = str;
			}
		}

	/*	public new Shortcut Shortcut
		{
			get { return base.Shortcut; }
			set {}
		}

		public Keys ShortcutEx
		{
			get { return shortcut; }
		}*/

		public void PerformPopup()
		{
			OnPopup(EventArgs.Empty);
		}

		public void PerformClickEx()
		{
			Menu parent = Parent;
			if (parent != null && parent is MenuItemEx)
				((MenuItemEx)parent).PerformPopup();
			if (Enabled)
				PerformClick();
		}
	}
}
