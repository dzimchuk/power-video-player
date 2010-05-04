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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using Dzimchuk.DirectShow;
using Dzimchuk.Native;
using Dzimchuk.MediaEngine.Core.Render;

namespace Dzimchuk.MediaEngine.Core
{
	public delegate void InitSizeHandler(ref GDI.RECT rcSrc);
	public delegate void ContextMenuHandler(Point ptScreen);
	
	/// <summary>
	/// Media Window.
	/// </summary>
	public class MediaWindow : System.Windows.Forms.UserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public const long ONE_SECOND = 10000000;
		
		public event InitSizeHandler MW_InitSize;
		public event EventHandler MW_DoubleClick;
		public event EventHandler MW_Click;
		public event ContextMenuHandler MW_ContextMenu;
		public event EventHandler MW_Update;
		public event EventHandler MW_MouseEnter;
		public event EventHandler MW_MouseLeave;
		public event EventHandler MW_MouseMove;
		public event EventHandler MW_ModifyMenu;
        public event FailedStreamsHandler FailedStreamsAvailable;
				
		public const int SIZE_FREE = 0;
		public const int SIZE100 = 1;
		const int DIVIDESIZE50 = 2;
		public const int SIZE200 = 2;
		public const int SIZE50 = 3;
		
		string caption; // used in the messageboxes
		bool bMouseOnWindow;
		bool bShowLogo;
		bool bAutoPlay;
		Renderer PreferredRenderer;
		bool bRepeat;
		GDI.RECT rcDest;
		AspectRatio aspectRatio = AspectRatio.AR_ORIGINAL;
		// Video Size stuff
		bool bIsFixed = true;		//FIXED (true) of FREE (false)
		int nFixedSize = SIZE100;	//FIXED video size (SIZE100 or SIZE 200)
		int nDivideSize = 1;
		
		RegularFilterGraphBuilder regularBuilder;
		DVDFilterGraphBuilder dvdBuilder;
		FilterGraph pFilterGraph;
		IntPtr hnwnd;
		MediaWindowHook hook;
        MediaWindowHandler mwHandler;

        ManualResetEvent mreInvokeUnhandledPinsHandler = new ManualResetEvent(false);

		#region Imported functions from nwnd.dll

        private static class NwndWrapper
        {
            [DllImport("nwnd.dll")]
            public static extern IntPtr CreateMediaWindow(IntPtr hParent, int nWidth, int nHeight);

            [DllImport("nwnd.dll")]
            public static extern void SetDestinationRect(ref GDI.RECT lpDest);

            [DllImport("nwnd.dll")]
            public static extern void SetRunning([MarshalAs(UnmanagedType.Bool)] bool bRunning,
                IVMRWindowlessControl pVMR, IVMRWindowlessControl9 pVMR9, IMFVideoDisplayControl pEVR);

            [DllImport("nwnd.dll")]
            public static extern void SetLogo(IntPtr hLogo);

            [DllImport("nwnd.dll")]
            public static extern void IsShowLogo([MarshalAs(UnmanagedType.Bool)] bool bShow);

            [DllImport("nwnd.dll")]
            public static extern void InvalidateMediaWindow();
        }

        private static class Nwnd64Wrapper
        {
            [DllImport("nwnd64.dll")]
            public static extern IntPtr CreateMediaWindow(IntPtr hParent, int nWidth, int nHeight);

            [DllImport("nwnd64.dll")]
            public static extern void SetDestinationRect(ref GDI.RECT lpDest);

            [DllImport("nwnd64.dll")]
            public static extern void SetRunning([MarshalAs(UnmanagedType.Bool)] bool bRunning,
                IVMRWindowlessControl pVMR, IVMRWindowlessControl9 pVMR9, IMFVideoDisplayControl pEVR);

            [DllImport("nwnd64.dll")]
            public static extern void SetLogo(IntPtr hLogo);

            [DllImport("nwnd64.dll")]
            public static extern void IsShowLogo([MarshalAs(UnmanagedType.Bool)] bool bShow);

            [DllImport("nwnd64.dll")]
            public static extern void InvalidateMediaWindow();
        }

        private static IntPtr CreateMediaWindow(IntPtr hParent, int nWidth, int nHeight)
        {
            return IntPtr.Size == 8 ? Nwnd64Wrapper.CreateMediaWindow(hParent, nWidth, nHeight) : NwndWrapper.CreateMediaWindow(hParent, nWidth, nHeight);
        }

        private static void SetDestinationRect(ref GDI.RECT lpDest)
        {
            if (IntPtr.Size == 8)
                Nwnd64Wrapper.SetDestinationRect(ref lpDest);
            else 
                NwndWrapper.SetDestinationRect(ref lpDest);
        }

        private static void SetRunning(bool bRunning, IVMRWindowlessControl pVMR, IVMRWindowlessControl9 pVMR9, IMFVideoDisplayControl pEVR)
        {
            if (IntPtr.Size == 8)
                Nwnd64Wrapper.SetRunning(bRunning, pVMR, pVMR9, pEVR);
            else
                NwndWrapper.SetRunning(bRunning, pVMR, pVMR9, pEVR);
        }

        private static void SetLogo(IntPtr hLogo)
        {
            if (IntPtr.Size == 8)
                Nwnd64Wrapper.SetLogo(hLogo);
            else
                NwndWrapper.SetLogo(hLogo);
        }

        private static void IsShowLogo(bool bShow)
        {
            if (IntPtr.Size == 8)
                Nwnd64Wrapper.IsShowLogo(bShow);
            else
                NwndWrapper.IsShowLogo(bShow);
        }

        private static void InvalidateMediaWindow()
        {
            if (IntPtr.Size == 8)
                Nwnd64Wrapper.InvalidateMediaWindow();
            else
                NwndWrapper.InvalidateMediaWindow();
        }

		#endregion

		#region MediaWindowHook
        private class MediaWindowHandler
        {
            private bool bDoubleClick; // fix for extra mouse up message we want to discard
            private bool bTrackingContextMenu; // fix for additional WM_CONTEXTMENU from MediaWindow when it's already sent by nwnd
			MediaWindow mw;
            public MediaWindowHandler(MediaWindow mw)
			{
				this.mw = mw;
			}

			public void HandleMessage(ref Message m)
			{
				switch(m.Msg)
				{
					case (int)WindowsMessages.WM_LBUTTONDBLCLK:
						bDoubleClick = true;
						if (mw.MW_DoubleClick != null)
							mw.MW_DoubleClick(mw, EventArgs.Empty);
						break;
					case (int)WindowsMessages.WM_CONTEXTMENU:
                        if (!bTrackingContextMenu)
                        {
                            if (m.HWnd == mw.hnwnd)
                                bTrackingContextMenu = true;
                            if (mw.MW_ContextMenu != null)
                                mw.MW_ContextMenu(Cursor.Position);
                        }
                        else
                            bTrackingContextMenu = false;
						break;
					case (int)WindowsMessages.WM_LBUTTONUP:
						if (mw.pFilterGraph != null && mw.pFilterGraph.bMenuOn && m.HWnd == mw.hnwnd)
						{
							uint lParam = (uint)m.LParam;
							uint x = lParam & 0x0000FFFF;
							uint y = lParam & 0xFFFF0000;
							y >>= 16;
								
							GDI.POINT pt = new GDI.POINT();
							pt.x=(int)x;
							pt.y=(int)y;
							mw.pFilterGraph.pDvdControl2.ActivateAtPosition(pt);
						}
						else if (!bDoubleClick && mw.MW_Click != null)
							mw.MW_Click(mw, EventArgs.Empty);
						bDoubleClick = false;
						break;
					case (int)WindowsMessages.WM_MOUSEMOVE:
						if (!mw.bMouseOnWindow)
						{
							WindowsManagement.TRACKMOUSEEVENT tme = 
								new WindowsManagement.TRACKMOUSEEVENT();
							tme.cbSize=Marshal.SizeOf(tme);
							tme.dwFlags=WindowsManagement.TME_LEAVE;
							tme.dwHoverTime=WindowsManagement.HOVER_DEFAULT;
							tme.hwndTrack=m.HWnd;
							
							WindowsManagement._TrackMouseEvent(ref tme);
							mw.bMouseOnWindow = true;
							if (mw.MW_MouseEnter != null)
								mw.MW_MouseEnter(mw, EventArgs.Empty);
						}
                        if (mw.pFilterGraph != null && mw.pFilterGraph.bMenuOn && m.HWnd == mw.hnwnd)
						{
							uint lParam = (uint)m.LParam;
							uint x = lParam & 0x0000FFFF;
							uint y = lParam & 0xFFFF0000;
							y >>= 16;
								
							GDI.POINT pt = new GDI.POINT();
							pt.x=(int)x;
							pt.y=(int)y;
							mw.pFilterGraph.pDvdControl2.SelectAtPosition(pt);
						}
						if (mw.MW_MouseMove != null)
							mw.MW_MouseMove(mw, EventArgs.Empty);
						break;
					case (int)WindowsMessages.WM_MOUSELEAVE:
						mw.bMouseOnWindow = false;
						if (mw.MW_MouseLeave != null)
							mw.MW_MouseLeave(mw, EventArgs.Empty);
						break;
					default:
						if (m.Msg == (int)FilterGraph.UWM_GRAPH_NOTIFY)
						{
							mw.HandleGraphEvent();
							if (mw.MW_Update != null)
								mw.MW_Update(mw, EventArgs.Empty);
						}
						break;
				}
			}
        }
        
        private class MediaWindowHook : NativeWindow
		{
			MediaWindowHandler mwh;
            public MediaWindowHook(MediaWindowHandler mwh)
			{
				this.mwh = mwh;
			}

			protected override void WndProc(ref Message m)
			{
                mwh.HandleMessage(ref m);
				base.WndProc (ref m);
			}
		}

        protected override void WndProc(ref Message m)
        {
            if (mwHandler != null)
                mwHandler.HandleMessage(ref m);
            base.WndProc(ref m);
        }
		#endregion
        
        private static IList<Renderer> _renderers = null;
        public static Renderer RecommendedRenderer
        {
            get
            {
                if (_renderers == null)
                    _renderers = MediaTypeManager.GetInstance().GetPresentVideoRenderers();

                Renderer r = Renderer.VR;
                OperatingSystem os = Environment.OSVersion;
                if (os.Platform == PlatformID.Win32NT || os.Platform == PlatformID.Win32S || os.Platform == PlatformID.Win32Windows || os.Platform == PlatformID.WinCE)
                {
                    if (os.Version.Major >= 6 && _renderers.Contains(Renderer.EVR))
                        r = Renderer.EVR;
                    else if (os.Version.Major == 5 && os.Version.Minor == 1 && _renderers.Contains(Renderer.VMR_Windowless))
                        r = Renderer.VMR_Windowless;
                }
                return r;
            }
        }

        public static IList<Renderer> PresentRenderers
        {
            get
            {
                if (_renderers == null)
                    _renderers = MediaTypeManager.GetInstance().GetPresentVideoRenderers();
                return new List<Renderer>(_renderers);
            }
        }

		public MediaWindow()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();
			SetStyle(ControlStyles.Selectable, false);

            mwHandler = new MediaWindowHandler(this);
            hook = new MediaWindowHook(mwHandler);

            BackColor = Color.Black;

			regularBuilder = RegularFilterGraphBuilder.GetGraphBuilder();
			dvdBuilder = DVDFilterGraphBuilder.GetGraphBuilder();

            regularBuilder.FailedStreamsAvailable += new FailedStreamsHandler(OnFailedStreamsAvailable);
            dvdBuilder.FailedStreamsAvailable += new FailedStreamsHandler(OnFailedStreamsAvailable);
		}

        // we should not allow showing a dialog before returning from the 'render graph' method because it causes WinForms to call 'render graph' twice
        // so we make sure it's done afterwards with mreInvokeUnhandledPinsHandler but on the UI thread (Invoke)
        private void OnFailedStreamsAvailable(IList<StreamInfo> streams)
        {
            ThreadPool.RegisterWaitForSingleObject(mreInvokeUnhandledPinsHandler,
                                                   delegate(object state, bool timedOut)
                                                   {
                                                       if (FailedStreamsAvailable != null)
                                                           Invoke(FailedStreamsAvailable, new object[] {streams});
                                                   },
                                                   streams,
                                                   Timeout.Infinite,
                                                   true);
        }

		public new Control Parent
		{
			get { return base.Parent; }
			set
			{
				value.KeyDown += new KeyEventHandler(OnParentKeyDown);
				base.Parent = value;
			}
		}

        public void OnCultureChanged()
        {
            FilterGraph.ReadErrorMessages();
        }

		private void OnParentKeyDown(object sender, KeyEventArgs e)
		{
			if (pFilterGraph != null && pFilterGraph.bMenuOn)
			{
				switch(e.KeyCode)
				{
					case Keys.Enter:
						pFilterGraph.pDvdControl2.ActivateButton();
						break;
					case Keys.Left:
						pFilterGraph.pDvdControl2.SelectRelativeButton(DVD_RELATIVE_BUTTON.DVD_Relative_Left);
						break;
					case Keys.Right:
						pFilterGraph.pDvdControl2.SelectRelativeButton(DVD_RELATIVE_BUTTON.DVD_Relative_Right);
						break;
					case Keys.Up:
						pFilterGraph.pDvdControl2.SelectRelativeButton(DVD_RELATIVE_BUTTON.DVD_Relative_Upper);
						break;
					case Keys.Down:
						pFilterGraph.pDvdControl2.SelectRelativeButton(DVD_RELATIVE_BUTTON.DVD_Relative_Lower);
						break;
				}
			}
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
		
		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated (e);
            CreateNativeWindow();
		}

        private void CreateNativeWindow()
        {
            hnwnd = CreateMediaWindow(Handle, Width, Height);
            hook.AssignHandle(hnwnd);
        }

        private void ReCreateNativeWindow()
        {
            hook.DestroyHandle();
            CreateNativeWindow();
        }
				
		protected override void OnResize(EventArgs e)
		{
			base.OnResize (e);
			if (!(pFilterGraph != null && pFilterGraph.pRenderer is EVR)) // don't resize nwnd to the full client area when using EVR (because it's buggy)
                WindowsManagement.MoveWindow(hnwnd, 0, 0, Width, Height, true);
			ResizeNormal();
		}
		
		protected override void OnPaint(PaintEventArgs e)
		{
		//	base.OnPaint (e);
		}

		protected override void OnPaintBackground(PaintEventArgs pevent)
		{
			base.OnPaintBackground (pevent);
		}

		#region Resizing stuff
		private void OnInitSize()
		{
			if (pFilterGraph != null)
			{
				if (MW_InitSize != null)
					MW_InitSize(ref pFilterGraph.rcSrc);
				ResizeNormal();
			}
		}
		
		private void ResizeNormal()
		{
			if (pFilterGraph == null) 
				return;

		/*	if (pFilterGraph.SourceType==SourceType.DVD)
			{
				ResizeDVD();
				return;
			}*/
            // we will always control aspect ratio
	
			Size rcClient = ClientSize;
			double w=rcClient.Width;
			double h=rcClient.Height;
			double ratio=w/h;
			double dAspectRatio;
	
			switch(aspectRatio)
			{
				case AspectRatio.AR_ORIGINAL:
					dAspectRatio=pFilterGraph.dAspectRatio;
					break;
				case AspectRatio.AR_16x9:
					dAspectRatio=16.0/9.0;
					break;
				case AspectRatio.AR_4x3:
					dAspectRatio=4.0/3.0;
					break;
				case AspectRatio.AR_47x20:
					dAspectRatio=47.0/20.0;
					break;
				default:
				{
					// free aspect ratio
					rcDest.left=0;
					rcDest.top=0;
					rcDest.right=rcClient.Width;
					rcDest.bottom=rcClient.Height;
					ApplyDestinationRect();
					return;
				}
			}
	
			int hor;
			int vert;
		
			if (bIsFixed)
			{
				if (ratio>=dAspectRatio)
				{
					vert = ((int) (pFilterGraph.rcSrc.bottom*nFixedSize/nDivideSize))-rcClient.Height;
					rcDest.top= (vert>=0) ? 0 : -vert/2;
					rcDest.bottom= (vert>=0) ? rcClient.Height : rcDest.top+((int) (pFilterGraph.rcSrc.bottom*nFixedSize/nDivideSize));
					h=rcDest.bottom-rcDest.top;
					w=h*dAspectRatio;
					hor = rcClient.Width - (int) w;
					rcDest.left= (hor<=0) ? 0 : hor/2;
					rcDest.right= rcDest.left+(int) w;
				}
				else
				{
					hor  = ((int) (pFilterGraph.rcSrc.right*nFixedSize/nDivideSize))-rcClient.Width;
					// hor>=0 - client area is smaller than video hor size
					rcDest.left= (hor>=0) ? 0 : -hor/2;
					rcDest.right= (hor>=0) ? rcClient.Width : rcDest.left+((int) (pFilterGraph.rcSrc.right*nFixedSize/nDivideSize));
					w=rcDest.right-rcDest.left;
					h=w/dAspectRatio;
					vert=rcClient.Height - (int) h;
					rcDest.top= (vert<=0) ? 0 : vert/2;
					rcDest.bottom= rcDest.top+(int) h;
				}
				
			}
			else
			{
				if (ratio>=dAspectRatio)
				{
					rcDest.top=0;
					rcDest.bottom=rcClient.Height;
					h=rcDest.bottom-rcDest.top;
					w=h*dAspectRatio;
					hor = rcClient.Width - (int) w;
					rcDest.left= (hor<=0) ? 0 : hor/2;
					rcDest.right= rcDest.left+(int) w;
				}
				else
				{
					rcDest.left=0;
					rcDest.right=rcClient.Width;
					w=rcDest.right-rcDest.left;
					h=w/dAspectRatio;
					vert=rcClient.Height - (int) h;
					rcDest.top= (vert<=0) ? 0 : vert/2;
					rcDest.bottom= rcDest.top+(int) h;
				}
		  
			}

			ApplyDestinationRect();
		}

		private void ResizeDVD()
		{
			Size rcClient = ClientSize;
			if (bIsFixed) 
			{
				int vert = ((int) (pFilterGraph.rcSrc.bottom*nFixedSize/nDivideSize))-rcClient.Height;
				int hor  = ((int) (pFilterGraph.rcSrc.right*nFixedSize/nDivideSize))-rcClient.Width;
				rcDest.top= (vert>=0) ? 0 : -vert/2;
				rcDest.bottom= (vert>=0) ? rcClient.Height : rcDest.top+((int) (pFilterGraph.rcSrc.bottom*nFixedSize/nDivideSize));
				rcDest.left= (hor>=0) ? 0: -hor/2;
				rcDest.right= (hor>=0) ? rcClient.Width : rcDest.left+((int) (pFilterGraph.rcSrc.right*nFixedSize/nDivideSize));
			}
			else
			{
				rcDest.left=0;
				rcDest.top=0;
				rcDest.right=rcClient.Width;
				rcDest.bottom=rcClient.Height;
			}
			ApplyDestinationRect();
		}

		private void ApplyDestinationRect()
		{
			SetDestinationRect(ref rcDest);

            pFilterGraph.pRenderer.SetVideoPosition(ref pFilterGraph.rcSrc, ref rcDest);
		}
		#endregion

		private void ReportError(string error)
		{
			MessageBox.Show(error.Replace("\\n", "\n"), caption, MessageBoxButtons.OK, MessageBoxIcon.Stop);
		}

		#region HandleGraphEvent
		private void HandleGraphEvent()
		{
			if (pFilterGraph == null) 
				return;
			
			int evCode, lParam1, lParam2;
    
			bool bEjected=false;
			while (DsHlp.SUCCEEDED(pFilterGraph.pMediaEventEx.GetEvent(out evCode, out lParam1, out lParam2, 0)))
			{
				int hr;
				switch (evCode)
				{
					case (int)DsEvCode.Complete:  
						if (bRepeat)
						{
							if (pFilterGraph.bSeekable) 
								SetCurrentPosition(0);
							else // gonna have to think it over!
								SetCurrentPosition(0);
						}
						else 
							StopGraph();
						break;
					case (int)DsEvCode.ErrorAbort:
						ResetGraph();
						ReportError(String.Format("{0} {1}", Resources.Resources.mw_error_occured, 
                            Resources.Resources.mw_play_aborted));
						break;
					////// DVD cases ///////
					case (int)DsEvCode.DvdCurrentHmsfTime:
						Guid guid = new Guid(lParam1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
						byte[] abyte = guid.ToByteArray();
						pFilterGraph.CurTime.bHours = abyte[0];
						pFilterGraph.CurTime.bMinutes = abyte[1];
						pFilterGraph.CurTime.bSeconds = abyte[2];
						pFilterGraph.CurTime.bFrames = abyte[3];
						break;
					case (int)DsEvCode.DvdChaptStart:
						pFilterGraph.ulCurChapter = lParam1;
						break;
					case (int)DsEvCode.DvdAngleChange:
						// lParam1 is the number of available angles (1 means no multiangle support)
						// lParam2 is the current angle, Angle numbers range from 1 to 9
						pFilterGraph.ulAnglesAvailable = lParam1;
						pFilterGraph.ulCurrentAngle = lParam2;  
						break;
					case (int)DsEvCode.DvdAnglesAvail:
						// Read the number of available angles
						dvdBuilder.GetAngleInfo(pFilterGraph);
						if (MW_ModifyMenu != null)
							MW_ModifyMenu(this, EventArgs.Empty);
						break;
					case (int)DsEvCode.DvdNoFpPgc: // disc doesn't have a First Play Program Chain
						IDvdCmd pObj;
						hr = pFilterGraph.pDvdControl2.PlayTitle(1, 
							DVD_CMD_FLAGS.DVD_CMD_FLAG_None, out pObj);
						if (DsHlp.SUCCEEDED(hr) && pObj != null)
						{
							pObj.WaitForEnd();
							Marshal.ReleaseComObject(pObj);
						}
						break;
					case (int)DsEvCode.DvdDomChange:
						switch (lParam1)
						{
							case (int)DVD_DOMAIN.DVD_DOMAIN_FirstPlay:  // = 1 (Performing default initialization of a DVD disc)
								break;
							case (int)DVD_DOMAIN.DVD_DOMAIN_Stop:       // = 5
								dvdBuilder.ClearTitleInfo(pFilterGraph);
								pFilterGraph.bMenuOn = false;
								HandleDiscEject(ref bEjected);
								break;
							case (int)DVD_DOMAIN.DVD_DOMAIN_VideoManagerMenu:  // = 2
							case (int)DVD_DOMAIN.DVD_DOMAIN_VideoTitleSetMenu: // = 3
								// Inform the app to update the menu option to show "Resume" now
								pFilterGraph.bMenuOn = true;  // now menu is "On"
								dvdBuilder.GetMenuLanguageInfo(pFilterGraph);
								break;
							case (int)DVD_DOMAIN.DVD_DOMAIN_Title:      // = 4
								// Inform the app to update the menu option to show "Menu" again
								pFilterGraph.bMenuOn = false; // we are no longer in a menu
								pFilterGraph.bShowMenuCalledFromTitle=false;
								dvdBuilder.UpdateTitleInfo(pFilterGraph);
                                ResizeNormal(); // video size and aspect ratio might have changed when entering new title
                                InvalidateMediaWindow();
								break;
						} // end of domain change switch
						pFilterGraph.CurDomain=(DVD_DOMAIN) lParam1;
						if (MW_ModifyMenu != null)
							MW_ModifyMenu(this, EventArgs.Empty); 
						break;
					case (int)DsEvCode.DvdValidUopsChange:
						pFilterGraph.UOPS = (VALID_UOP_FLAG) lParam1;
						break;
					case (int)DsEvCode.DvdPlaybStopped:
					//	StopGraph();
						break;
					case (int)DsEvCode.DvdParentalLChange:
						string str = String.Format(Resources.Resources.mw_accept_parental_level_format, lParam1);
						if (DialogResult.Yes == MessageBox.Show(str, Resources.Resources.mw_accept_change_question, 
							MessageBoxButtons.YesNo, MessageBoxIcon.Question))
							pFilterGraph.pDvdControl2.AcceptParentalLevelChange(true);
						else
							pFilterGraph.pDvdControl2.AcceptParentalLevelChange(false);
						break;
					case (int)DsEvCode.DvdError:
						switch (lParam1)
						{
							case (int)DVD_ERROR.DVD_ERROR_Unexpected: // Playback is stopped.
								ReportError(Resources.Resources.mw_dvd_unexpected_error);
								pFilterGraph.pMediaControl.Stop();
								pFilterGraph.GraphState=GraphState.Stopped;
								break;
							case (int)DVD_ERROR.DVD_ERROR_CopyProtectFail:
								ReportError(Resources.Resources.mw_dvd_copyprotect_failed);
								pFilterGraph.pMediaControl.Stop();
								pFilterGraph.GraphState=GraphState.Stopped;
								break;
							case (int)DVD_ERROR.DVD_ERROR_InvalidDVD1_0Disc:
								ReportError(Resources.Resources.mw_dvd_invalid_disc);
								pFilterGraph.pMediaControl.Stop();
								pFilterGraph.GraphState=GraphState.Stopped;
								break;
							case (int)DVD_ERROR.DVD_ERROR_InvalidDiscRegion:
								ReportError(Resources.Resources.mw_dvd_invalid_region);
								pFilterGraph.pMediaControl.Stop();
								pFilterGraph.GraphState=GraphState.Stopped;
							//	ChangeDvdRegion(); // details in the dvdcore.cpp
								break;
							case (int)DVD_ERROR.DVD_ERROR_LowParentalLevel:
								ReportError(Resources.Resources.mw_dvd_low_parental_level);
								pFilterGraph.pMediaControl.Stop();
								pFilterGraph.GraphState=GraphState.Stopped;
								break;
							case (int)DVD_ERROR.DVD_ERROR_MacrovisionFail:
								ReportError(Resources.Resources.mw_dvd_macrovision_error);
								pFilterGraph.pMediaControl.Stop();
								pFilterGraph.GraphState=GraphState.Stopped;
								break;
							case (int)DVD_ERROR.DVD_ERROR_IncompatibleSystemAndDecoderRegions:
								ReportError(Resources.Resources.mw_dvd_system_decoder_regions);
								pFilterGraph.pMediaControl.Stop();
								pFilterGraph.GraphState=GraphState.Stopped;
								break;
							case (int)DVD_ERROR.DVD_ERROR_IncompatibleDiscAndDecoderRegions:
								ReportError(Resources.Resources.mw_dvd_disc_decoder_regions);
								pFilterGraph.pMediaControl.Stop();
								pFilterGraph.GraphState=GraphState.Stopped;
								break;
						}  // end of switch (lParam1)
						break;
					// Next is warning
					case (int)DsEvCode.DvdWarning:
						switch (lParam1)
						{
							case (int)DVD_WARNING.DVD_WARNING_InvalidDVD1_0Disc:
						//		ReportError("DVD Warning: Current disc is not v1.0 spec compliant");
								break;
							case (int)DVD_WARNING.DVD_WARNING_FormatNotSupported:
						//		ReportError("DVD Warning: The decoder does not support the new format.");
								break;
							case (int)DVD_WARNING.DVD_WARNING_IllegalNavCommand:
						//		ReportError("DVD Warning: An illegal navigation command was encountered.");
								break;
							case (int)DVD_WARNING.DVD_WARNING_Open:
								ReportError(Resources.Resources.mw_dvd_warning_cant_open_file);
								break;
							case (int)DVD_WARNING.DVD_WARNING_Seek:
								ReportError(Resources.Resources.mw_dvd_warning_cant_seek);
								break;
							case (int)DVD_WARNING.DVD_WARNING_Read:
								ReportError(Resources.Resources.mw_dvd_warning_cant_read);
								break;
							default:
						//		ReportError("DVD Warning: An unknown (%ld) warning received.");
								break;
						}
						break;
					case (int)DsEvCode.DvdButtonChange:
						break;
					case (int)DsEvCode.DvdStillOn:
						if (lParam1 != 0) // if there is a still without buttons, we can call StillOff
							pFilterGraph.bStillOn = true;
						break;
					case (int)DsEvCode.DvdStillOff:
						pFilterGraph.bStillOn = false; // we are no longer in a still
						break;
				} // end of switch(..)

				pFilterGraph.pMediaEventEx.FreeEventParams(evCode, lParam1, lParam2);
			} // end of while(...)

			if (bEjected)
				ResetGraph();
		}

		void HandleDiscEject(ref bool bEjected)
		{
			IntPtr ptr = Marshal.AllocCoTaskMem(Storage.MAX_PATH*2);
			int ulActualSize;
			int hr=pFilterGraph.pDvdInfo2.GetDVDDirectory(ptr, Storage.MAX_PATH, out ulActualSize);
			if (hr==DsHlp.S_OK)
			{
				string path = Marshal.PtrToStringUni(ptr, ulActualSize);
				if (path.Length >= 3)
				{
					path = path.Substring(0, 3);
					uint MaximumComponentLength, FileSystemFlags, VolumeSerialNumber;
					int nMode=NoCat.SetErrorMode(NoCat.SEM_FAILCRITICALERRORS);
					if (Storage.GetVolumeInformation(path, null, 0, out VolumeSerialNumber, 
						out MaximumComponentLength, out FileSystemFlags, null, 0) == 0)
						bEjected=true;
					NoCat.SetErrorMode(nMode);
				}
			}
			Marshal.FreeCoTaskMem(ptr);
		}
		#endregion

		//This function will return FALSE if the state is RESET or unidentified!!!!
		//The application must determine whether to stop the graph.
		private bool UpdateGraphState()
		{
			if (pFilterGraph == null) 
				return false;

			int hr;
			FilterState fs;
			hr=pFilterGraph.pMediaControl.GetState(2000, out fs);
			if (hr == DsHlp.S_OK)
			{
				switch(fs)
				{
					case FilterState.State_Stopped:
						pFilterGraph.GraphState = GraphState.Stopped;
						break;
					case FilterState.State_Paused:
						pFilterGraph.GraphState = GraphState.Paused;
						break;
					case FilterState.State_Running:
						pFilterGraph.GraphState = GraphState.Running;
						break;
				}
		
				return true;
			}
	
			if(hr == DsHlp.VFW_S_CANT_CUE)
			{
				pFilterGraph.GraphState = GraphState.Paused;
				return true;
			}
			else if(hr == DsHlp.VFW_S_STATE_INTERMEDIATE) //Don't know what the state is so just stay at old state.
				return true;
			else		
				return false;
		}

		#region Public properties
		public bool UsePreferredFilters
		{
			get { return regularBuilder.UsePreferredFilters; }
			set { regularBuilder.UsePreferredFilters = value; }
		}

		public bool UsePreferredFilters4DVD
		{
			get { return dvdBuilder.UsePreferredFilters; }
			set { dvdBuilder.UsePreferredFilters = value; }
		}
		
		public bool ShowLogo
		{
			get { return bShowLogo; }
			set
			{
				bShowLogo = value;
				IsShowLogo(value);
				InvalidateMediaWindow();
			}
		}

		public Bitmap Logo
		{
			set{ SetLogo(value.GetHbitmap()); }
		}

		public bool AutoPlay
		{
			get { return bAutoPlay; }
			set	{ bAutoPlay = value; }
		}

		public Renderer PreferredVideoRenderer
		{
			get { return PreferredRenderer; }
			set	{ PreferredRenderer = value; }
		}

		public bool Repeat
		{
			get { return bRepeat; }
			set	{ bRepeat = value; }
		}

		public string Caption
		{
			get { return caption; }
			set	{ caption = value; }
		}

		public GraphState GraphState
		{
			get
			{
				return pFilterGraph != null ? pFilterGraph.GraphState : GraphState.Reset;
			}
		}

		public bool IsGraphSeekable
		{
			get
			{
				if (pFilterGraph != null)
				{
					if (pFilterGraph.bSeekable && 
						(pFilterGraph.UOPS & VALID_UOP_FLAG.UOP_FLAG_Play_Title_Or_AtTime)==0)
						return true;

					return false;
				}
				else
					return false;
			}
		}
		
		public AspectRatio AspectRatio
		{
			get { return aspectRatio; }
			set
			{
				aspectRatio = value;
				ResizeNormal();
				InvalidateMediaWindow();
			}
		}

		public SourceType SourceType
		{
			get { return pFilterGraph != null ? pFilterGraph.SourceType : SourceType.Unknown; }
		}

		public MediaInfo MediaInfo
		{
			get { return pFilterGraph != null ? pFilterGraph.info : null; }
		}

		public int FilterCount
		{
			get{ return pFilterGraph != null ? pFilterGraph.aFilters.Count : 0; }
		}

		public int AudioStreams
		{
			get { return pFilterGraph != null ? pFilterGraph.nAudioStreams : 0; }
		}

		public int CurrentAudioStream
		{
			get
			{
				if (pFilterGraph == null)
					return -1;
	
				if (pFilterGraph.SourceType == SourceType.DVD)
				{
					int ulStreamsAvailable=0, ulCurrentStream=0;

					int hr = pFilterGraph.pDvdInfo2.GetCurrentAudio(out ulStreamsAvailable, 
						out ulCurrentStream);
					if (DsHlp.SUCCEEDED(hr))
					{
						// Update the current audio language selection
						pFilterGraph.nCurrentAudioStream=ulCurrentStream;
					}
				}
	
				return pFilterGraph.nCurrentAudioStream;
			}
			set
			{
				if (pFilterGraph == null)
					return;
				else if (pFilterGraph.nAudioStreams == 0)
					return;
				else if (value > (pFilterGraph.nAudioStreams - 1))
					return;
				else if (pFilterGraph.SourceType != SourceType.DVD)
				{
					if (pFilterGraph.arrayBasicAudio.Count == 0)
						return;

					int lVolume;
					GetVolume(out lVolume);
	
					int lMute = -10000;
					IBasicAudio pBA;
					for (int i=0; i<pFilterGraph.nAudioStreams; i++)
					{
						pBA = (IBasicAudio) pFilterGraph.arrayBasicAudio[i];
						pBA.put_Volume(lMute);
					}
	
					pFilterGraph.nCurrentAudioStream=value;
					SetVolume(lVolume);
				}
				else
				{
					int hr;
 
					// Set the audio stream to the requested value
					// Note that this does not affect the subpicture data (subtitles)
					IDvdCmd pObj;
					hr = pFilterGraph.pDvdControl2.SelectAudioStream(value, 
						DVD_CMD_FLAGS.DVD_CMD_FLAG_None, out pObj);
					if (DsHlp.SUCCEEDED(hr))
					{
						if (pObj != null)
						{
							pObj.WaitForEnd();
							Marshal.ReleaseComObject(pObj);
						}
						pFilterGraph.nCurrentAudioStream=value;
					}
				}
			}
		}

		public int AnglesAvailable
		{
			get { return pFilterGraph == null ? 1 : pFilterGraph.ulAnglesAvailable; }
		}

		public int CurrentAngle
		{
			get{ return pFilterGraph == null ? 1 : pFilterGraph.ulCurrentAngle; }
			set
			{
				if (pFilterGraph == null)
					return;
				else if (pFilterGraph.pDvdControl2 == null)
					return;
				else if (pFilterGraph.ulAnglesAvailable < 2)
					return;
				else if (value > pFilterGraph.ulAnglesAvailable)
					return;
				else
				{
					int hr;
 
					// Set the angle to the requested value.
					IDvdCmd pObj;
					hr = pFilterGraph.pDvdControl2.SelectAngle(value, 
						DVD_CMD_FLAGS.DVD_CMD_FLAG_None, out pObj);
					if (DsHlp.SUCCEEDED(hr))
					{
						if (pObj != null)
						{
							pObj.WaitForEnd();
							Marshal.ReleaseComObject(pObj);
						}
						pFilterGraph.ulCurrentAngle=value;
					}
				}
			}
		}

		public int NumberOfTitles
		{
			get { return pFilterGraph == null ? 0 : pFilterGraph.ulNumTitles; }
		}

		public int CurrentTitle
		{
			get { return pFilterGraph == null ? 0 : pFilterGraph.ulCurTitle; }
		}

		public int CurrentChapter
		{
			get { return pFilterGraph == null ? 0 : pFilterGraph.ulCurChapter; }
		}

		public int MenuLangCount
		{
			get { return pFilterGraph == null ? 0 : pFilterGraph.arrayMenuLang.Count; }
		}

		public VALID_UOP_FLAG UOPS
		{
			get { return pFilterGraph == null ? 0 : pFilterGraph.UOPS; }
		}

		public int NumberOfSubpictureStreams
		{
			get { return pFilterGraph == null ? 0 : (int)pFilterGraph.arraySubpictureStream.Count; }
		}

		public int CurrentSubpictureStream
		{
			get
			{
				if (pFilterGraph == null)
					return -1;
	
				if (pFilterGraph.pDvdInfo2 == null)
					return -1;

				int ulStreamsAvailable=0, ulCurrentStream=0;
				bool bIsDisabled; // TRUE means it is disabled

				int hr = pFilterGraph.pDvdInfo2.GetCurrentSubpicture(out ulStreamsAvailable, 
					out ulCurrentStream, out bIsDisabled);
				if (DsHlp.SUCCEEDED(hr))
					return ulCurrentStream;
				
				return -1;
			}
			set
			{
				if (pFilterGraph == null)
					return;
	
				if (pFilterGraph.arraySubpictureStream.Count == 0)
					return;
	
				if (value > (pFilterGraph.arraySubpictureStream.Count - 1))
					return;

				if (pFilterGraph.pDvdControl2 == null)
					return;

				IDvdCmd pObj;
				int hr=pFilterGraph.pDvdControl2.SelectSubpictureStream(value, 
					DVD_CMD_FLAGS.DVD_CMD_FLAG_None, out pObj);
				if (DsHlp.SUCCEEDED(hr) && pObj != null)
				{
					pObj.WaitForEnd();
					Marshal.ReleaseComObject(pObj);
				}
			}
		}

		#endregion

		#region Public methods
		public bool BuildGraph(string source, WhatToPlay CurrentlyPlaying)
		{
			ResetGraph();
            pFilterGraph = FilterGraphBuilder.BuildFilterGraph(source, CurrentlyPlaying, hnwnd, PreferredRenderer, caption);
            if (pFilterGraph != null)
			{
                mreInvokeUnhandledPinsHandler.Set();
                SetRunning(true, 
                    pFilterGraph.pRenderer is VMRWindowless ? ((VMRWindowless)pFilterGraph.pRenderer).VMRWindowlessControl : null, 
                    pFilterGraph.pRenderer is VMR9Windowless ? ((VMR9Windowless)pFilterGraph.pRenderer).VMRWindowlessControl : null,
                    pFilterGraph.pRenderer is EVR ? ((EVR)pFilterGraph.pRenderer).MFVideoDisplayControl : null);
				OnInitSize();
				if (bAutoPlay)
					return ResumeGraph();
				else
					pFilterGraph.GraphState = GraphState.Stopped;
				return true;
			}
			else
				return false;
		}

		public void ResetGraph()
		{
            mreInvokeUnhandledPinsHandler.Reset();
            if (pFilterGraph != null)
			{
                bool bEVR = pFilterGraph.pRenderer is EVR;
                
                SetRunning(false, null, null, null);
				pFilterGraph.Dispose();
				pFilterGraph = null;

                if (bEVR) // EVR will keep displaying a rectangle... the only way around is to recreate a window
                    ReCreateNativeWindow();

                Invalidate(true);
            //    InvalidateMediaWindow();
			}
		}

		public bool ResumeGraph()
		{
			if (pFilterGraph == null) 
				return false;
			if (DsHlp.SUCCEEDED(pFilterGraph.pMediaControl.Run()))
			{
				pFilterGraph.GraphState = GraphState.Running;
				return true; // ok, we're running
			}
			else if (UpdateGraphState())
			{
				pFilterGraph.GraphState = GraphState.Running;
				return true; // ok, we're running
			}
			else
			{
				ResetGraph();
				ReportError(Resources.Resources.mw_play_aborted);
				return false;
			}
		}

		public bool StopGraph()
		{
			if (pFilterGraph == null) 
				return false;
			if (pFilterGraph.SourceType != SourceType.DVD)
			{	
				PauseGraph();
				SetCurrentPosition(0);
				pFilterGraph.pMediaControl.Stop();
				pFilterGraph.GraphState=GraphState.Stopped;
			}
			else
			{
				pFilterGraph.pDvdControl2.SetOption(DVD_OPTION_FLAG.DVD_ResetOnStop, true);
		
				pFilterGraph.pMediaControl.Stop();
				pFilterGraph.GraphState=GraphState.Stopped;
		
				pFilterGraph.pDvdControl2.SetOption(DVD_OPTION_FLAG.DVD_ResetOnStop, false);
			}
		
			return true;
		}

		public bool PauseGraph()
		{
			if (pFilterGraph == null) 
				return false;
			int hr=pFilterGraph.pMediaControl.Pause();
			if (hr==DsHlp.S_OK)
			{
				pFilterGraph.GraphState=GraphState.Paused;
				return true;
			}
			else if (hr==DsHlp.S_FALSE)
			{
				if (UpdateGraphState())
					return pFilterGraph.GraphState==GraphState.Paused;
					
				pFilterGraph.pMediaControl.Stop();
			}

			pFilterGraph.GraphState=GraphState.Stopped; // Pause() failed
			return false;
		}

		public void SetCurrentPosition(long time)
		{
			if (pFilterGraph == null) 
				return;
			if (IsGraphSeekable)
			{
				int hr;
				pFilterGraph.rtCurrentTime=time;
				if (pFilterGraph.SourceType != SourceType.DVD)
				{
					GraphState state=pFilterGraph.GraphState;
					PauseGraph();
					long pStop = 0;
					hr=pFilterGraph.pMediaSeeking.SetPositions(ref pFilterGraph.rtCurrentTime, 
						SeekingFlags.AbsolutePositioning,
						ref pStop, SeekingFlags.NoPositioning);
		
					switch(state)
					{
						case GraphState.Running:
							ResumeGraph();
							break;
						case GraphState.Stopped:
							pFilterGraph.pMediaControl.Stop();
							pFilterGraph.GraphState=GraphState.Stopped;
							break;
					}

				}
				else
				{
					DVD_PLAYBACK_LOCATION2 loc;
					hr = pFilterGraph.pDvdInfo2.GetCurrentLocation(out loc);
					if (hr==DsHlp.S_OK) 
					{
						long second;
						long minute;
						long h;
						long remain;
		
						second = time/ONE_SECOND;
						remain = second%3600;
						h=second/3600;
						minute=remain/60;
						second = remain%60;
				
						loc.TimeCode.bHours=(byte) h;
						loc.TimeCode.bMinutes=(byte) minute;
						loc.TimeCode.bSeconds=(byte) second;
			
						double rate = GetRate();
						IDvdCmd pObj;
						GraphState state=pFilterGraph.GraphState;
						if (state == GraphState.Paused)
							ResumeGraph();
						hr = pFilterGraph.pDvdControl2.PlayAtTime(ref loc.TimeCode, 
							DVD_CMD_FLAGS.DVD_CMD_FLAG_Flush, out pObj);
				
						if(DsHlp.SUCCEEDED(hr))
						{
							pFilterGraph.CurTime=loc.TimeCode;
							if (pObj != null)
							{
								pObj.WaitForEnd();
								Marshal.ReleaseComObject(pObj);
							}
							if (rate != 1.0)
								SetRate(rate);
						}
						if (state == GraphState.Paused)
							PauseGraph();
					}
				}
			}
		}

		public long GetCurrentPosition()
		{
			if (pFilterGraph == null) 
				return 0;
			if (pFilterGraph.bSeekable)
			{
				if (pFilterGraph.SourceType != SourceType.DVD)
				{
					pFilterGraph.pMediaSeeking.GetCurrentPosition(out pFilterGraph.rtCurrentTime);
				}
				else
				{
					pFilterGraph.rtCurrentTime=pFilterGraph.CurTime.bHours*3600 + 
						pFilterGraph.CurTime.bMinutes*60 + pFilterGraph.CurTime.bSeconds;
					pFilterGraph.rtCurrentTime *= ONE_SECOND;
				}

				return pFilterGraph.rtCurrentTime;
			}

			return 0;
		}

		public long GetDuration()
		{
			if (pFilterGraph == null) 
				return 0;
			if (pFilterGraph.bSeekable)
				return pFilterGraph.rtDuration;
			return 0;
		}

		public void SetRate(double dRate)
		{
			if (pFilterGraph == null) 
				return;
			int hr;
			if (pFilterGraph.bSeekable)
			{
				if (pFilterGraph.SourceType != SourceType.DVD)
				{
					hr=pFilterGraph.pMediaSeeking.SetRate(dRate);
					if (hr==DsHlp.S_OK)
						pFilterGraph.dRate=dRate;
				}
				else
				{
					// set rate for DVD
					IDvdCmd pObj;
					hr = pFilterGraph.pDvdControl2.PlayForwards(dRate, 
						DVD_CMD_FLAGS.DVD_CMD_FLAG_Flush, out pObj);
					if(DsHlp.SUCCEEDED(hr))
					{
						if (pObj != null)
						{
							pObj.WaitForEnd();
							Marshal.ReleaseComObject(pObj);
						}
						pFilterGraph.dRate=dRate;
					}
				}

			}
		}

		public double GetRate()
		{
			return pFilterGraph != null ? pFilterGraph.dRate : 1.0;
		}
		
		public bool GetVolume(out int volume)
		{
			volume = 0;
			if (pFilterGraph == null)
				return false;
			if (pFilterGraph.SourceType == SourceType.Asf || pFilterGraph.SourceType == SourceType.DVD)
				return pFilterGraph.pBasicAudio.get_Volume(out volume) == DsHlp.S_OK;
			if (pFilterGraph.arrayBasicAudio.Count == 0)
				return false;
			IBasicAudio pBA = (IBasicAudio) pFilterGraph.arrayBasicAudio[pFilterGraph.nCurrentAudioStream];
			return pBA.get_Volume(out volume) == DsHlp.S_OK;
		}

		public bool SetVolume(int volume)
		{
			if (pFilterGraph == null)
				return false;
			if (pFilterGraph.SourceType == SourceType.Asf || pFilterGraph.SourceType == SourceType.DVD)
				return pFilterGraph.pBasicAudio.put_Volume(volume) == DsHlp.S_OK;
			if (pFilterGraph.arrayBasicAudio.Count == 0)
				return false;
			IBasicAudio pBA = (IBasicAudio) pFilterGraph.arrayBasicAudio[pFilterGraph.nCurrentAudioStream];
			return pBA.put_Volume(volume) == DsHlp.S_OK;
		}

		public int GetVideoSize()
		{
			if (!bIsFixed)
				return SIZE_FREE;
			int ret = 0;
			switch(nFixedSize)
			{
				case SIZE100:
				{
					ret=nDivideSize==DIVIDESIZE50 ? SIZE50 : SIZE100;
					break;
				}
				case SIZE200:
				{
					ret=SIZE200;
					break;
				}
			}
			return ret;
		}
		
		public void SetVideoSize(int size, bool bInitSize)
		{
			switch(size)
			{
				case SIZE100:
				{
					bIsFixed=true;
					nFixedSize=SIZE100;
					nDivideSize=1;
					break;
				}
				case SIZE200:
				{
					bIsFixed=true;
					nFixedSize=SIZE200;
					nDivideSize=1;
					break;
				}
				case SIZE50:
				{
					bIsFixed=true;
					nFixedSize=SIZE100;
					nDivideSize=DIVIDESIZE50;
					break;
				}
				default:
				{
					bIsFixed = !bIsFixed;
					break;
				}
			}

			if (bInitSize)
				OnInitSize();
		}

		public void SetVideoSize(int size)
		{
			SetVideoSize(size, true);
		}

		public string GetFilterName(int nFilterNum)
		{
			if (pFilterGraph != null && pFilterGraph.aFilters.Count > nFilterNum)
				return (string)pFilterGraph.aFilters[nFilterNum];
			else
				return String.Empty;
		}

		public bool DisplayFilterPropPage(IntPtr hParent, string strFilter, bool bDisplay)
		{
			if (pFilterGraph == null)
				return false;
			
			IBaseFilter pFilter=null;
			pFilterGraph.pGraphBuilder.FindFilterByName(strFilter, out pFilter);
			if (pFilter==null)
				return false;

			bool bRet=false;
			ISpecifyPropertyPages pProp = pFilter as ISpecifyPropertyPages;
			if (pProp != null) 
			{
				bRet=true;
				if (bDisplay)
				{
					// Show the page. 
					CAUUID caGUID = new CAUUID();
					pProp.GetPages(out caGUID);
				
					object pFilterUnk = (object)pFilter;
					DsUtils.OleCreatePropertyFrame(
						hParent,                // Parent window
						0, 0,                   // Reserved
						strFilter,				// Caption for the dialog box
						1,                      // Number of objects (just the filter)
						ref pFilterUnk,			// Array of object pointers. 
						caGUID.cElems,          // Number of property pages
						caGUID.pElems,          // Array of property page CLSIDs
						0,                      // Locale identifier
						0, IntPtr.Zero          // Reserved
						);

					// Clean up.
					Marshal.FreeCoTaskMem(caGUID.pElems);
				}
		
			//	Marshal.ReleaseComObject(pProp);
			}
	
		//	Marshal.ReleaseComObject(pFilter);
			return bRet;
		}

		public bool DisplayFilterPropPage(IntPtr hParent, int nFilterNum, bool bDisplay)
		{
			return DisplayFilterPropPage(hParent, GetFilterName(nFilterNum), bDisplay);
		}

		public string GetAudioStreamName(int nStream)
		{
			string str = Resources.Resources.error;
			if (pFilterGraph == null)
				return str;
	
			if (pFilterGraph.nAudioStreams == 0)
				return str;
	
			if (nStream > (pFilterGraph.nAudioStreams - 1))
				return str;

			if (pFilterGraph.SourceType != SourceType.DVD)
			{
				str = String.Format(Resources.Resources.mw_stream_format, nStream+1);
			}
			else
			{
				if (pFilterGraph.arrayAudioStream.Count == 0)
					return str;

				str=(string)pFilterGraph.arrayAudioStream[nStream];
			}
	
			return str;
		}

		public bool IsAudioStreamEnabled(int ulStreamNum)
		{
			if (pFilterGraph == null)
				return false;
	
			if (pFilterGraph.pDvdInfo2 == null)
				return false;

			bool bEnabled;
			int hr=pFilterGraph.pDvdInfo2.IsAudioStreamEnabled(ulStreamNum, out bEnabled);
			return (hr==DsHlp.S_OK) ? bEnabled : false;
		}

		public int GetNumChapters(int ulTitle)
		{
			if (pFilterGraph == null)
				return 0;

			int ulcount=pFilterGraph.arrayNumChapters.Count;
			if (ulcount < ulTitle)
				return 0;

			return (int)pFilterGraph.arrayNumChapters[ulTitle-1];
		}

		public bool GetCurrentDomain(out DVD_DOMAIN pDomain)
		{
			//	DVD_DOMAIN_FirstPlay
			//  DVD_DOMAIN_VideoManagerMenu 
			//  DVD_DOMAIN_VideoTitleSetMenu  
			//  DVD_DOMAIN_Title         
			//  DVD_DOMAIN_Stop     
			pDomain = 0;
			if (pFilterGraph == null)
				return false;
	
			if (pFilterGraph.pDvdInfo2 == null)
				return false;
	
			int hr=pFilterGraph.pDvdInfo2.GetCurrentDomain(out pDomain);
			return hr==DsHlp.S_OK;
		}

		public void GoTo(int ulTitle, int ulChapter)
		{
			if (pFilterGraph != null && pFilterGraph.pDvdControl2 != null && 
				ulTitle <= pFilterGraph.ulNumTitles && ulChapter <= GetNumChapters(ulTitle))
			{
				IDvdCmd pObj;
				int hr=pFilterGraph.pDvdControl2.PlayChapterInTitle(ulTitle, ulChapter, 
					DVD_CMD_FLAGS.DVD_CMD_FLAG_Flush, out pObj);
				if (DsHlp.SUCCEEDED(hr))
				{
					if (pObj != null)
					{
						pObj.WaitForEnd();
						Marshal.ReleaseComObject(pObj);
					}
					if (pFilterGraph.CurDomain == DVD_DOMAIN.DVD_DOMAIN_Title && 
						pFilterGraph.ulCurTitle != ulTitle)
					{
						dvdBuilder.UpdateTitleInfo(pFilterGraph);
                        ResizeNormal(); // video size and aspect ratio might have changed when entering new title
                        InvalidateMediaWindow();
						if (MW_ModifyMenu != null)
							MW_ModifyMenu(this, EventArgs.Empty);
					}

				}
			}
		}

		public string GetMenuLangName(int nLang)
		{
			string str = Resources.Resources.error;
			if (pFilterGraph == null)
				return str;
	
			if (pFilterGraph.arrayMenuLang.Count == 0)
				return str;
	
			if (nLang > (pFilterGraph.arrayMenuLang.Count - 1))
				return str;

			return (string)pFilterGraph.arrayMenuLang[nLang];
		}

		public void SetMenuLang(int nLang)
		{
			if (pFilterGraph == null)
				return;
	
			if (pFilterGraph.arrayMenuLangLCID.Count == 0)
				return;
	
			if (nLang > (pFilterGraph.arrayMenuLangLCID.Count - 1))
				return;
	
			int hr;
			hr=pFilterGraph.pDvdControl2.SetOption(DVD_OPTION_FLAG.DVD_ResetOnStop, true);
	
			// Changing menu language is only valid in the DVD_DOMAIN_Stop domain
			hr=pFilterGraph.pDvdControl2.Stop();
			if (DsHlp.SUCCEEDED(hr))
			{
				// Change the default menu language
				hr = pFilterGraph.pDvdControl2.SelectDefaultMenuLanguage((int)pFilterGraph.arrayMenuLangLCID[nLang]);

				// Display the root menu
				ShowMenu(DVD_MENU_ID.DVD_MENU_Title);
			}
	
			// Turn off ResetOnStop option 
			hr=pFilterGraph.pDvdControl2.SetOption(DVD_OPTION_FLAG.DVD_ResetOnStop, false);
		}

		public void ShowMenu(DVD_MENU_ID menuID)
		{
			if (pFilterGraph == null)
				return;
	
			DVD_DOMAIN domain=pFilterGraph.CurDomain;
			IDvdCmd pObj;
			int hr=pFilterGraph.pDvdControl2.ShowMenu(menuID, 
				DVD_CMD_FLAGS.DVD_CMD_FLAG_Flush, out pObj);
			if (DsHlp.SUCCEEDED(hr))
			{
				if (pObj != null)
				{
					pObj.WaitForEnd();
					Marshal.ReleaseComObject(pObj);
				}
				pFilterGraph.bMenuOn=true;
				if (!pFilterGraph.bShowMenuCalledFromTitle)
				{
					pFilterGraph.bShowMenuCalledFromTitle = domain == DVD_DOMAIN.DVD_DOMAIN_Title;
				}
			}
		}

		public bool EnableResumeDVD()
		{
			if (pFilterGraph == null)
				return false;

			DVD_DOMAIN domain=pFilterGraph.CurDomain;
			return ((domain==DVD_DOMAIN.DVD_DOMAIN_VideoManagerMenu) || (domain==DVD_DOMAIN.DVD_DOMAIN_VideoTitleSetMenu))
				&& (pFilterGraph.UOPS & VALID_UOP_FLAG.UOP_FLAG_Resume)==0 && 
				pFilterGraph.bShowMenuCalledFromTitle;
		}

		// The Resume method leaves a menu and resumes playback
		public bool ResumeDVD()
		{
			if (pFilterGraph == null)
				return false;
	
			if (pFilterGraph.pDvdControl2 == null)
				return false;
	
			IDvdCmd pObj;
			int hr = pFilterGraph.pDvdControl2.Resume(DVD_CMD_FLAGS.DVD_CMD_FLAG_Flush, out pObj);
			if (DsHlp.SUCCEEDED(hr))
			{
				if (pObj != null)
				{
					pObj.WaitForEnd();
					Marshal.ReleaseComObject(pObj);
				}
				pFilterGraph.bMenuOn=false;
				return true;
			}
			
			return false;
		}

		public void ReturnFromSubmenu()
		{
			if (pFilterGraph == null)
				return;
	
			if (pFilterGraph.pDvdControl2 == null)
				return;
	
			IDvdCmd pObj;
			int hr = pFilterGraph.pDvdControl2.ReturnFromSubmenu(DVD_CMD_FLAGS.DVD_CMD_FLAG_Flush, 
				out pObj);
			if (DsHlp.SUCCEEDED(hr) && pObj != null)
			{
				pObj.WaitForEnd();
				Marshal.ReleaseComObject(pObj);
			}
		}

		public string GetSubpictureStreamName(int nStream)
		{
			string str = Resources.Resources.error;
			if (pFilterGraph == null)
				return str;
	
			if (pFilterGraph.arraySubpictureStream.Count == 0)
				return str;
	
			if (nStream > (pFilterGraph.arraySubpictureStream.Count - 1))
				return str;

			return (string)pFilterGraph.arraySubpictureStream[nStream];
		}

		public bool IsSubpictureStreamEnabled(int ulStreamNum)
		{
			if (pFilterGraph == null)
				return false;

			if (pFilterGraph.pDvdInfo2 == null)
				return false;

			bool bEnabled;
			int hr=pFilterGraph.pDvdInfo2.IsSubpictureStreamEnabled(ulStreamNum, out bEnabled);
			return (hr==DsHlp.S_OK) ? bEnabled : false;
		}

		public bool IsSubpictureEnabled()
		{
			if (pFilterGraph == null)
				return false;
	
			if (pFilterGraph.pDvdInfo2 == null)
				return false;

			int ulStreamsAvailable=0, ulCurrentStream=0;
			bool bIsDisabled; // TRUE means it is disabled

			int hr = pFilterGraph.pDvdInfo2.GetCurrentSubpicture(out ulStreamsAvailable, 
				out ulCurrentStream, out bIsDisabled);
			if (DsHlp.SUCCEEDED(hr))
				return !bIsDisabled;
			
			return false;
		}

		public bool EnableSubpicture(bool bEnable)
		{
			if (pFilterGraph == null)
				return false;
	
			if (pFilterGraph.pDvdControl2 == null)
				return false;

			IDvdCmd pObj;
			int hr=pFilterGraph.pDvdControl2.SetSubpictureState(bEnable, 
				DVD_CMD_FLAGS.DVD_CMD_FLAG_None, out pObj);
			if (DsHlp.SUCCEEDED(hr) && pObj != null)
			{
				pObj.WaitForEnd();
				Marshal.ReleaseComObject(pObj);
				return true;
			}
			return false;
		}

		#endregion

	}
}
