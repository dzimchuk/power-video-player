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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using Pvp.Core.MediaEngine;
using Pvp.Core.MediaEngine.Renderers;

namespace Pvp.Core.Wpf
{
    [TemplatePart(Name = "PART_Border", Type = typeof(Border))]
    public abstract class MediaWindowHost2 : Control, IMediaWindowHost
    {
        static MediaWindowHost2()
        {
        //    DefaultStyleKeyProperty.OverrideMetadata(typeof(MediaWindowHost), new FrameworkPropertyMetadata(typeof(MediaWindowHost)));
        }

        public static readonly DependencyProperty LogoBrushProperty =
            DependencyProperty.Register("LogoBrush", typeof(Brush), typeof(MediaWindowHost), new PropertyMetadata(default(Brush)));

        public static readonly DependencyProperty LogoMaxWidthProperty =
            DependencyProperty.Register("LogoMaxWidth", typeof(double), typeof(MediaWindowHost), new PropertyMetadata(double.PositiveInfinity));

        public static readonly DependencyProperty LogoMaxHeightProperty =
            DependencyProperty.Register("LogoMaxHeight", typeof(double), typeof(MediaWindowHost), new PropertyMetadata(double.PositiveInfinity));

        private Border _border;
        private EVRRenderer _evrRenderer;
        private IPvpPresenterHook _pvpPresenterHook;
        private MediaWindow2 _mediaWindow;
        private PvpD3dImage _d3dImage;

        protected MediaWindowHost2()
        {
            InitializeMediaEngine();
        }

        private void InitializeMediaEngine()
        {
            MediaEngine = MediaEngineServiceProvider.GetMediaEngine(this);
            MediaEngine.MediaWindowDisposed += (sender, args) => SetMediaWindowState(false);
        }

        #region Public properties and methods

        public Brush LogoBrush
        {
            get
            {
                return (Brush)GetValue(LogoBrushProperty);
            }
            set
            {
                SetValue(LogoBrushProperty, value);
            }
        }

        public double LogoMaxWidth
        {
            get
            {
                return (double)GetValue(LogoMaxWidthProperty);
            }
            set
            {
                SetValue(LogoMaxWidthProperty, value);
            }
        }

        public double LogoMaxHeight
        {
            get
            {
                return (double)GetValue(LogoMaxHeightProperty);
            }
            set
            {
                SetValue(LogoMaxHeightProperty, value);
            }
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
                _mediaWindow = new MediaWindow2();
                if (_border != null && _d3dImage != null)
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
                    var rect = new Rectangle { StrokeThickness = 0.0 };

                    var binding = new Binding("LogoBrush") { Source = this, Mode = BindingMode.OneWay };
                    rect.SetBinding(Shape.FillProperty, binding);

                    binding = new Binding("LogoMaxWidth") { Source = this, Mode = BindingMode.OneWay };
                    rect.SetBinding(FrameworkElement.MaxWidthProperty, binding);

                    binding = new Binding("LogoMaxHeight") { Source = this, Mode = BindingMode.OneWay };
                    rect.SetBinding(FrameworkElement.MaxHeightProperty, binding);

                    _border.Child = rect;
                }

                //if (_hwndHost != null)
                //{
                //    _hwndHost.MessageHook -= new System.Windows.Interop.HwndSourceHook(_hwndHost_MessageHook);
                //    _hwndHost.Dispose();
                //    _hwndHost = null;
                //}

                if (_pvpPresenterHook != null)
                {
                    _pvpPresenterHook.Dispose();
                    _pvpPresenterHook = null;
                }

                if (_evrRenderer != null)
                {
                    _evrRenderer.Dispose(); // it should have been already disposed by the engine
                    _evrRenderer = null;
                }

                if (_mediaWindow != null)
                {
                    _mediaWindow.Dispose(); // it should have been already disposed by the engine
                    _mediaWindow = null;
                }
            }
        }

        IMediaWindow IMediaWindowHost.GetMediaWindow()
        {
            if (_mediaWindow == null)
                SetMediaWindowState(true);

            return _mediaWindow;
        }

        protected IMediaEngine MediaEngine { get; private set; }

        protected RendererBase Renderer
        {
            get
            {
                if (_evrRenderer == null)
                {
                    _d3dImage = new PvpD3dImage();
                    _pvpPresenterHook = PvpPresenterFactory.GetPvpPresenter(_d3dImage.D3dImage);
                    _evrRenderer = new EVRRenderer(_pvpPresenterHook);
                }

                return _evrRenderer;
            }
        }
    }
}