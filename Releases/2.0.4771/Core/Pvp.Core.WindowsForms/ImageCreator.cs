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
using System.Drawing.Imaging;
using System.IO;
using Pvp.Core.DirectShow;
using Pvp.Core.MediaEngine;

namespace Pvp.Core.WindowsForms
{
    /// <summary>
    /// GDI+ specific image creator.
    /// </summary>
    public class ImageCreator : IImageCreator, IDisposable
    {
        private readonly MemoryStream _stream = new MemoryStream();
        
        public void CreateImage(ref BITMAPINFOHEADER header, IntPtr bytes)
        {
            Bitmap bitmap = null;
            try
            {
                bitmap = new Bitmap(header.biWidth,
                                    header.biHeight,
                                    (header.biBitCount / 8) * header.biWidth, // basically a horizontal line
                                    PixelFormat.Format32bppArgb,
                                    bytes);
                bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
                
                bitmap.Save(_stream, ImageFormat.Jpeg);
                _stream.Position = 0;
            }
            finally
            {
                if (bitmap != null)
                    bitmap.Dispose();
            }
        }

        public bool Created
        {
            get { return _stream.Length != 0; }
        }

        public void Save(string filename)
        {
            File.WriteAllBytes(filename, _stream.ToArray());
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~ImageCreator()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
                _stream.Dispose();
        }
    }
}
