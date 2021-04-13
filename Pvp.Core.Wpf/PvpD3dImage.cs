using System.Windows.Controls;
using System.Windows.Interop;

namespace Pvp.Core.Wpf
{
    internal class PvpD3dImage : Image
    {
        public PvpD3dImage()
        {
            D3dImage = new D3DImage();
            Source = D3dImage;
        }

        public D3DImage D3dImage { get; private set; }
    }
}