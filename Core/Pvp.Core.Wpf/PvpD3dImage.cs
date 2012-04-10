using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows;

namespace Pvp.Core.Wpf
{
    internal class PvpD3dImage : Image, IDisposable
    {
        public delegate int GetBackBufferFunc(out IntPtr pSurface);

        private readonly GetBackBufferFunc _getD3DSurfaceFunc;
        private readonly D3DImage _d3dImage;
        private TimeSpan _lastRender;

        public PvpD3dImage(GetBackBufferFunc getD3DSurfaceFunc)
        {
            _getD3DSurfaceFunc = getD3DSurfaceFunc;

            _d3dImage = new D3DImage();
            Source = _d3dImage;

            CompositionTarget.Rendering += new EventHandler(CompositionTarget_Rendering);
        }

        public void Dispose()
        {
            CompositionTarget.Rendering -= new EventHandler(CompositionTarget_Rendering);
        }

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            RenderingEventArgs args = (RenderingEventArgs)e;

            // It's possible for Rendering to call back twice in the same frame 
            // so only render when we haven't already rendered in this frame.

            if (_d3dImage.IsFrontBufferAvailable && _lastRender != args.RenderingTime)
            {
                IntPtr pSurface = IntPtr.Zero;
                _getD3DSurfaceFunc(out pSurface);
                if (pSurface != IntPtr.Zero)
                {
                    _d3dImage.Lock();

                    // Repeatedly calling SetBackBuffer with the same IntPtr is 
                    // a no-op. There is no performance penalty.

                    _d3dImage.SetBackBuffer(D3DResourceType.IDirect3DSurface9, pSurface);

                    _d3dImage.AddDirtyRect(new Int32Rect(0, 0, _d3dImage.PixelWidth, _d3dImage.PixelHeight));
                    _d3dImage.Unlock();

                    _lastRender = args.RenderingTime;
                }
            }
        }
    }
}
