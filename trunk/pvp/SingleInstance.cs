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
using System.Reflection;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Drawing;
using Dzimchuk.Native;

namespace Dzimchuk.Common
{
	[Serializable]
	public struct ArgsPacket
	{
		public Guid guid;
		public string[] args;
	}
	
	public delegate void ArgsHandler(Form form, string[] args);
	/// <summary>
	/// 
	/// </summary>
	public class SingleInstance : IDisposable
	{
		#region MainForm Hook
		private class Hook : NativeWindow
		{
			SingleInstance si;
			Form form;
			
			public Hook(Form form, SingleInstance si)
			{
				AssignHandle(form.Handle);
				form.HandleDestroyed += new EventHandler(parent_HandleDestroyed);
				this.si = si;
				this.form = form;
			}
			
			private void parent_HandleDestroyed(object sender, EventArgs e)
			{
				ReleaseHandle();
			}

			protected override void WndProc(ref Message m)
			{
				if (m.Msg == (int) WindowsMessages.WM_COPYDATA)
				{
					WindowsManagement.COPYDATASTRUCT cds = 
						(WindowsManagement.COPYDATASTRUCT) Marshal.PtrToStructure(
						m.LParam, typeof(WindowsManagement.COPYDATASTRUCT));
					if (cds.dwData == SingleInstance.COPYDATA_TYPE_FILENAME)
					{
						MemoryStream stream = null;
						try
						{
							byte[] abyte = new byte[cds.cbData];
							Marshal.Copy(cds.lpData, abyte, 0, cds.cbData);
							stream = new MemoryStream(abyte);

							BinaryFormatter formatter = new BinaryFormatter();
							ArgsPacket packet = (ArgsPacket) formatter.Deserialize(stream);
							
							if (packet.guid == si.guid)
							{
								if (si.ArgsRecieved != null)
									si.ArgsRecieved(form, packet.args);
								m.Result = (IntPtr) 1;
							}
						}
						catch
						{
						}
						finally
						{
							if (stream != null)
								stream.Close();
						}
					}
					
				}
				else if (m.Msg == (int)si.UWM_ARE_YOU_ME)
				{
					m.Result = (IntPtr) si.UWM_ARE_YOU_ME;
				}
				else
					base.WndProc (ref m);
			}
		}
		#endregion
		
		Mutex mutex;
		bool bOwned;
		IntPtr hWndOther = IntPtr.Zero;
		Hook hook;
		public Guid guid;
		public const int COPYDATA_TYPE_FILENAME = 0x1616;
		public event ArgsHandler ArgsRecieved;
		readonly uint UWM_ARE_YOU_ME;
	
		private SingleInstance(string guid)
		{								    
			string strAsm = Assembly.GetExecutingAssembly().GetName().Name;
			mutex = new Mutex(true, strAsm + guid, out bOwned);
			UWM_ARE_YOU_ME = WindowsManagement.RegisterWindowMessage("AreYouMe" + guid);
			if (!bOwned)
				WindowsManagement.EnumWindows(new WindowsManagement.EnumWindowsProc(searcher), IntPtr.Zero);
		}

		~SingleInstance()
		{
			Dispose(false);
		}

		private int searcher(IntPtr hWnd, IntPtr lParam)
		{
			int result;
			int ok = WindowsManagement.SendMessageTimeout(hWnd, 
				(int)UWM_ARE_YOU_ME, 
				IntPtr.Zero, IntPtr.Zero, 
				WindowsManagement.SMTO_BLOCK | WindowsManagement.SMTO_ABORTIFHUNG, 
				100, out result);
			if (ok == 0)
				return 1; // ignore this and continue
			if (result == (int)UWM_ARE_YOU_ME)
			{ // found it
				hWndOther = hWnd;
				return 0; // stop search
			}
			return 1; // continue
		}

		public static SingleInstance CreateSingleInstance()
		{
			return CreateSingleInstance(Guid.Empty);
		}

		public static SingleInstance CreateSingleInstance(Guid guid)
		{
			SingleInstance si = new SingleInstance(guid.ToString());
			si.guid = guid;
			return si;
		}
		
		public bool IsSingleInstance
		{
			get { return bOwned; }
		}

		public void Run(Type type) // type is a 'Form' or a derived class
		{
			if (bOwned)
			{
				Form form = (Form) type.GetConstructor(new Type[] {}).Invoke(new Object[0]);
				hook = new Hook(form, this);
				Application.Run(form);
			}
			else
			{
				BringToFront();
				SendCommandLineArgs();
			}
		}

		public void BringToFront()
		{
			if (hWndOther != IntPtr.Zero)
			{
				if (WindowsManagement.IsIconic(hWndOther) != 0)
					WindowsManagement.ShowWindowAsync(hWndOther, WindowsManagement.SW_RESTORE);
				WindowsManagement.SetForegroundWindow(hWndOther);
			}
		}

		public void SendCommandLineArgs()
		{
			string[] args = Environment.GetCommandLineArgs();
			
			if (hWndOther != IntPtr.Zero)
			{
				IntPtr buffer = IntPtr.Zero;
				IntPtr pcds = IntPtr.Zero;
				MemoryStream stream = null;
				try
				{
					ArgsPacket packet = new ArgsPacket();
					packet.guid = guid;
					packet.args = args;

					stream = new MemoryStream();
					BinaryFormatter formatter = new BinaryFormatter();
					formatter.Serialize(stream, packet);

					byte[] abyte = stream.ToArray();
					buffer = Marshal.AllocCoTaskMem(abyte.Length);
					Marshal.Copy(abyte, 0, buffer, abyte.Length);
					
					WindowsManagement.COPYDATASTRUCT cds = new WindowsManagement.COPYDATASTRUCT();
					cds.dwData = COPYDATA_TYPE_FILENAME;
					cds.cbData = abyte.Length;
					cds.lpData = buffer;
					
					pcds = Marshal.AllocCoTaskMem(Marshal.SizeOf(cds));
					Marshal.StructureToPtr(cds, pcds, true);

					WindowsManagement.SendMessage(hWndOther, (int) WindowsMessages.WM_COPYDATA, IntPtr.Zero, pcds);
						
				}
				catch
				{
				}
				finally
				{
					if (buffer != IntPtr.Zero)
						Marshal.FreeCoTaskMem(buffer);
					if (pcds != IntPtr.Zero)
						Marshal.FreeCoTaskMem(pcds);
					if (stream != null)
						stream.Close();
				}
			}
		}
		
		#region IDisposable Members

		public void Dispose()
		{
			GC.SuppressFinalize(this);
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (bOwned && disposing)
			{
				bOwned = false;
				mutex.ReleaseMutex();
			}
		}

		#endregion
	}
}
