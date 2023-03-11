using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Geb.Image
{
    public interface IImage
    {
        int Width { get; }
        int Height { get; }
        int BytesPerPixel { get; }
        unsafe void CopyFrom(void* pData, int dataStride);
        unsafe void CopyTo(void* pData, int dataStride);
    }
}
