/*************************************************************************
 *  Copyright (c) 2010 Hu Fei(xiaotie@geblab.com; geblab, www.geblab.com)
 ************************************************************************/

using System;
using System.IO;

namespace Geb.Image
{
    public partial class ImageBgra32 : IImage, IDisposable
    {
        public const int ChannelCount = 4;

        public int BytesPerPixel { get; } = 4;

        #region Image <-> Bitmap 所需的方法

        private unsafe void Copy(Bgr24* from, void* to, int length)
        {
            UnmanagedImageConverter.ToBgra32(from, (Bgra32*)to, length);
        }

        private unsafe void Copy(Bgra32* from, void* to, int length)
        {
            UnmanagedImageConverter.Copy((byte*)from, (byte*)to, 4 * length);
        }

        private unsafe void Copy(byte* from, void* to, int length)
        {
            UnmanagedImageConverter.ToBgra32(from, (Bgra32*)to, length);
        }

        private PixelFormat GetOutputBitmapPixelFormat()
        {
            return PixelFormat.Format32bppBgra;
        }

        private unsafe void ToBitmapCore(byte* src, byte* dst, int width)
        {
            UnmanagedImageConverter.Copy(src, dst, width * 4);
        }

        #endregion

        #region 转换为编码图像
        public void SaveBmp(String imagePath)
        {
            new Formats.Bmp.BmpEncoder().Encode(this, imagePath);
        }

        public void SaveJpeg(String imagePath,int quality = 70, Formats.Jpeg.JpegPixelFormats fmt = Formats.Jpeg.JpegPixelFormats.YCbCr)
        {
            Formats.Jpeg.JpegEncoder.Encode(this, imagePath, quality, fmt);
        }

        public Byte[] ToJpegData(int quality = 70, Formats.Jpeg.JpegPixelFormats fmt = Formats.Jpeg.JpegPixelFormats.YCbCr)
        {
            return Formats.Jpeg.JpegEncoder.Encode(this, quality, fmt);
        }

        public void SavePng(String imagePath, Formats.Png.PngEncoderOptions options = null)
        {
            Formats.Png.PngEncoder.Encode(this, imagePath, options);
        }

        public Byte[] ToPngData(Formats.Png.PngEncoderOptions options = null)
        {
            return Formats.Png.PngEncoder.Encode(this, options);
        }
        #endregion

        public static ImageBgra32 Read(String filePath)
        {
            if(File.Exists(filePath) == false) throw new FileNotFoundException("File Not Exist", filePath);
            return ImageReader.Read(filePath);
        }

        public static ImageBgra32 ReadStream(Stream stream)
        {
            return ImageReader.Read(stream);
        }

        public ImageU8 ToGrayscaleImage()
        {
            return ToGrayscaleImage(0.299, 0.587, 0.114);
        }

        public ImageU8 ToGrayscaleImage(byte transparentColor)
        {
            return ToGrayscaleImage(0.299, 0.587, 0.114, transparentColor);
        }

        public unsafe ImageU8 ToGrayscaleImage(double rCoeff, double gCoeff, double bCoeff, byte transparentColor)
        {
            ImageU8 img = new ImageU8(this.Width, this.Height);
            Bgra32* p = Start;
            Byte* to = img.Start;
            Bgra32* end = p + Length;

            while (p != end)
            {
                if (p->Alpha == 0)
                {
                    *to = transparentColor;
                }
                else
                {
                    *to = (Byte)(p->Red * rCoeff + p->Green * gCoeff + p->Blue * bCoeff);
                }
                p++;
                to++;
            }

            return img;
        }

        public enum Channel
        {
            Blue = 0, Green = 1, Red = 2, Alpha = 3
        }

        public unsafe ImageU8 ToGrayscaleImage(Channel channel)
        {
            int offset = (int)channel;
            if (offset < 0) offset = 0;
            else if (offset > 3) offset = 3;

            ImageU8 img = new ImageU8(this.Width, this.Height);
            Byte* p = (Byte*)Start + offset;
            Byte* to = img.Start;
            Byte* end = p + Length * 4;

            while (p < end)
            {
                *to = *p;
                p+=4;
                to++;
            }

            return img;
        }

        public unsafe ImageU8 ToGrayscaleImage(double rCoeff, double gCoeff, double bCoeff)
        {
            ImageU8 img = new ImageU8(this.Width, this.Height);
            Bgra32* p = Start;
            Byte* to = img.Start;
            Bgra32* end = p + Length;

            if (Length < 1024)
            {
                while (p != end)
                {
                    *to = (Byte)(p->Red * rCoeff + p->Green * gCoeff + p->Blue * bCoeff);
                    p++;
                    to++;
                }
            }
            else
            {
                int* bCache = stackalloc int[256];
                int* gCache = stackalloc int[256];
                int* rCache = stackalloc int[256];

                const int shift = 1 << 12;
                int rShift = (int)(rCoeff * shift);
                int gShift = (int)(gCoeff * shift);
                int bShift = shift - rShift - gShift;

                int r = 0, g = 0, b = 0;
                for (int i = 0; i < 256; i++)
                {
                    bCache[i] = b;
                    gCache[i] = g;
                    rCache[i] = r;
                    b += bShift;
                    g += gShift;
                    r += rShift;
                }

                while (p != end)
                {
                    *to = (Byte)((bCache[p->Blue] + gCache[p->Green] + rCache[p->Red]) >> 12);

                    p++;
                    to++;
                }
            }
            return img;
        }

        /// <summary>
        /// 将当前图像转换为 ImageBgr24 格式的图像
        /// </summary>
        /// <returns>转换后的图像</returns>
        public unsafe ImageBgr24 ToImageBgr24()
        {
            ImageBgr24 image = new ImageBgr24(this.Width, this.Height);
            Bgra32* pSrc = this.Start;
            Bgr24* pDst = image.Start;
            Bgra32* pSrcEnd = this.Start + this.Length;
            while(pSrc < pSrcEnd)
            {
                pDst->Red = pSrc->Red;
                pDst->Green = pSrc->Green;
                pDst->Blue = pSrc->Blue;
                pSrc++;pDst++;
            }
            return image;
        }

        public unsafe void SetAlpha(byte alpha)
        {
            Bgra32* start = (Bgra32*)this.Start;
            Bgra32* end = start + this.Length;
            while (start != end)
            {
                start->Alpha = alpha;
                start++;
            }
        }

        public unsafe void CombineAlpha(ImageBgra32 src, Rect srcRect, Point dest)
        {
            if (srcRect.X >= src.Width || srcRect.Y >= src.Height) return;
            int startSrcX = Math.Max(0, srcRect.X);
            int startSrcY = Math.Max(0, srcRect.Y);
            int endSrcX = Math.Min(srcRect.X + srcRect.Width, src.Width);
            int endSrcY = Math.Min(srcRect.Y + srcRect.Height, src.Height);
            int offsetX = srcRect.X < 0 ? -srcRect.X : 0;
            int offsetY = srcRect.Y < 0 ? -srcRect.Y : 0;
            offsetX = dest.X + offsetX;
            offsetY = dest.Y + offsetY;
            int startDstX = Math.Max(0, offsetX);
            int startDstY = Math.Max(0, offsetY);
            offsetX = offsetX < 0 ? -offsetX : 0;
            offsetY = offsetY < 0 ? -offsetY : 0;
            startSrcX += offsetX;
            startSrcY += offsetY;
            int endDstX = Math.Min(dest.X + srcRect.Width, this.Width);
            int endDstY = Math.Min(dest.Y + srcRect.Height, this.Height);
            int copyWidth = Math.Min(endSrcX - startSrcX, endDstX - startDstX);
            int copyHeight = Math.Min(endSrcY - startSrcY, endDstY - startDstY);
            if (copyWidth <= 0 || copyHeight <= 0) return;

            int srcWidth = src.Width;
            int dstWidth = this.Width;

            Bgra32* srcLine = (Bgra32*)(src.Start) + srcWidth * startSrcY + startSrcX;
            Bgra32* dstLine = this.Start + dstWidth * startDstY + startDstX;
            Bgra32* endSrcLine = srcLine + srcWidth * copyHeight;
            while (srcLine < endSrcLine)
            {
                Bgra32* pSrc = srcLine;
                Bgra32* endPSrc = pSrc + copyWidth;
                Bgra32* pDst = dstLine;
                while (pSrc < endPSrc)
                {
                    Bgra32 p0 = *pSrc;
                    Bgra32 p1 = *pDst;
                    switch (p0.Alpha)
                    {
                        case 255:
                            *pDst = p0;
                            break;
                        case 0:
                        default:
                            break;
                    }
                    pSrc++;
                    pDst++;
                }
                srcLine += srcWidth;
                dstLine += dstWidth;
            }
        }

    }
}
