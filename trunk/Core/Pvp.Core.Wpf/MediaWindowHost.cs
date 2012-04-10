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

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using Pvp.Core.MediaEngine;
using Pvp.Core.MediaEngine.Render;
using System;

namespace Pvp.Core.Wpf
{
    [TemplatePart(Name = "PART_Border", Type = typeof(Border))]
    public abstract class MediaWindowHost : Control, IMediaWindowHost
    {
        static MediaWindowHost()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MediaWindowHost), new FrameworkPropertyMetadata(typeof(MediaWindowHost)));
        }

        public static readonly DependencyProperty LogoBrushProperty =
            DependencyProperty.Register("LogoBrush", typeof(Brush), typeof(MediaWindowHost), new PropertyMetadata(default(Brush)));

        public static readonly DependencyProperty LogoMaxWidthProperty =
            DependencyProperty.Register("LogoMaxWidth", typeof(double), typeof(MediaWindowHost), new PropertyMetadata(double.PositiveInfinity));

        public static readonly DependencyProperty LogoMaxHeightProperty =
            DependencyProperty.Register("LogoMaxHeight", typeof(double), typeof(MediaWindowHost), new PropertyMetadata(double.PositiveInfinity));

        private Border _border;
        private IMediaEngine _engine;

        private EVRRenderer _evrRenderer;
        private MediaWindow _mediaWindow;
        private PvpD3dImage _d3dImage;

        protected MediaWindowHost()
        {
            InitializeMediaEngine();
        }

        private void InitializeMediaEngine()
        {
            _engine = MediaEngineServiceProvider.GetMediaEngine(this);
            _engine.MediaWindowDisposed += (sender, args) => SetMediaWindowState(false);
        }


        #region Public properties and methods

        public Brush LogoBrush
        {
            get { return (Brush)GetValue(LogoBrushProperty); }
            set { SetValue(LogoBrushProperty, value); }
        }

        public double LogoMaxWidth
        {
            get { return (double)GetValue(LogoMaxWidthProperty); }
            set { SetValue(LogoMaxWidthProperty, value); }
        }

        public double LogoMaxHeight
        {
            get { return (double)GetValue(LogoMaxHeightProperty); }
            set { SetValue(LogoMaxHeightProperty, value); }
        }

        #endregion
        

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _border = Template.FindName("PART_Border", this) as Border;
            SetMediaWindowState(false);
        }
        
        private void SetMediaWindowState(bool active)
        {
            if (active)
            {
                _mediaWindow = new MediaWindow();
                _d3dImage = new PvpD3dImage((out IntPtr pSurface) => 
                    {
                        if (_evrRenderer != null && _evrRenderer.PvpPresenter != null)
                        {
                            return _evrRenderer.PvpPresenter.GetBackBufferNoRef(out pSurface);
                        }
                        else
                        {
                            pSurface = IntPtr.Zero;
                            return -1;
                        }
                    });
                if (_border != null)
                    _border.Child = _d3dImage;
                //_hwndHost = new MediaWindowHwndHost();
                //_hwndHost.MessageHook += new System.Windows.Interop.HwndSourceHook(_hwndHost_MessageHook);
                //if (_border != null)
                //    _border.Child = _hwndHost;

            }
            else
            {
                if (_border != null)
                {
                    var rect = new Rectangle {StrokeThickness = 0.0};

                    var binding = new Binding("LogoBrush") {Source = this, Mode = BindingMode.OneWay};
                    rect.SetBinding(Shape.FillProperty, binding);

                    binding = new Binding("LogoMaxWidth") {Source = this, Mode = BindingMode.OneWay};
                    rect.SetBinding(FrameworkElement.MaxWidthProperty, binding);

                    binding = new Binding("LogoMaxHeight") {Source = this, Mode = BindingMode.OneWay};
                    rect.SetBinding(FrameworkElement.MaxHeightProperty, binding);

                    _border.Child = rect;
                }

                //if (_hwndHost != null)
                //{
                //    _hwndHost.MessageHook -= new System.Windows.Interop.HwndSourceHook(_hwndHost_MessageHook);
                //    _hwndHost.Dispose();
                //    _hwndHost = null;
                //}

                if (_d3dImage != null)
                {
                    _d3dImage.Dispose();
                    _d3dImage = null;
                }

                if (_mediaWindow != null)
                {
                    _mediaWindow.Dispose(); // it should have been already disposed by the engine
                    _mediaWindow = null;
                }

                if (_evrRenderer != null)
                {
                    _evrRenderer.Dispose(); // it should have been already disposed by the engine
                    _evrRenderer = null;
                }
            }
        }

        public IMediaWindow GetMediaWindow()
        {
            SetMediaWindowState(true);
            return _mediaWindow;
        }

        protected IMediaEngine MediaEngine
        {
            get { return _engine; }
        }

        protected RendererBase Renderer
        {
            get
            {
                if (_evrRenderer == null)
                    _evrRenderer = new EVRRenderer();

                return _evrRenderer;
            }
        }
    }
}
