using System.Windows.Controls;
using System.Windows.Interop;

namespace Pvp.Core.Wpf
{
    internal class PvpD3dImage : Image
    {
        private readonly D3DImage _d3dImage;
        
        public PvpD3dImage()
        {
            _d3dImage = new D3DImage();
            Source = D3dImage;
        }

        public D3DImage D3dImage
        {
            get { return _d3dImage; }
        }
    }
}
