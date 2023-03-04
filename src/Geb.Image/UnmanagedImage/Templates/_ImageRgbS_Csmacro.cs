/*************************************************************************
 *  Copyright (c) 2010 Hu Fei(xiaotie@geblab.com; geblab, www.geblab.com)
 ************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Geb.Image
{
    using TPixel = RgbS;
    using TCache = System.Single;
    using TKernel = System.Single;
    using TImage = Geb.Image.ImageRgbS;
    using TChannel = System.Single;

    public static partial class ImageRgbSClassHelper
    {
        /// <summary>
        /// 对每个像素进行操作
        /// </summary>
        /// <param name="p">指向像素的指针</param>
        public unsafe delegate void ActionOnPixel(TPixel* p);

        /// <summary>
        /// 对每个位置的像素进行操作
        /// </summary>
        /// <param name="row">像素所在行</param>
        /// <param name="column">像素所在列</param>
        /// <param name="p">指向像素的指针</param>
        public unsafe delegate void ActionWithPosition(Int32 row, Int32 column, TPixel* p);

        /// <summary>
        /// 对每个像素进行判断
        /// </summary>
        /// <param name="p">指向像素的指针</param>
        /// <returns></returns>
        public unsafe delegate Boolean PredicateOnPixel(TPixel* p);

        /// <summary>
        /// 遍历图像，对每个像素进行操作
        /// </summary>
        /// <param name="src">图像</param>
        /// <param name="handler">void ActionOnPixel(TPixel* p)</param>
        /// <returns>处理后的图像（同传入图像是一个对象）</returns>
        public unsafe static TImage ForEach(this TImage src, ActionOnPixel handler)
        {
            TPixel* start = src.Start;
            if (start == null) return src;

            TPixel* end = start + src.Length;
            while (start != end)
            {
                handler(start);
                ++start;
            }
            return src;
        }

        /// <summary>
        /// 遍历图像，对每个位置的像素进行操作
        /// </summary>
        /// <param name="src">图像</param>
        /// <param name="handler">void ActionWithPosition(Int32 row, Int32 column, TPixel* p)</param>
        /// <returns>处理后的图像（同传入图像是一个对象）</returns>
        public unsafe static TImage ForEach(this TImage src, ActionWithPosition handler)
        {
            Int32 width = src.Width;
            Int32 height = src.Height;

            TPixel* p = src.Start;
            if (p == null) return src;

            for (Int32 r = 0; r < height; r++)
            {
                for (Int32 w = 0; w < width; w++)
                {
                    handler(w, r, p);
                    p++;
                }
            }
            return src;
        }

        /// <summary>
        /// 遍历图像中的一段，对每个像素进行操作
        /// </summary>
        /// <param name="src">图像</param>
        /// <param name="start">指向开始像素的指针</param>
        /// <param name="length">处理的像素数量</param>
        /// <param name="handler">void ActionOnPixel(TPixel* p)</param>
        /// <returns>处理后的图像（同传入图像是一个对象）</returns>
        public unsafe static TImage ForEach(this TImage src, TPixel* start, uint length, ActionOnPixel handler)
        {
            if (start == null) return src;

            TPixel* end = start + src.Length;
            while (start != end)
            {
                handler(start);
                ++start;
            }
            return src;
        }

        /// <summary>
        /// 统计符合条件的像素数量
        /// </summary>
        /// <param name="src">图像</param>
        /// <param name="handler">Boolean PredicateOnPixel(TPixel* p)</param>
        /// <returns>符合条件的像素数量</returns>
        public unsafe static Int32 Count(this TImage src, PredicateOnPixel handler)
        {
            TPixel* start = src.Start;
            TPixel* end = start + src.Length;

            if (start == null) return 0;

            Int32 count = 0;
            while (start != end)
            {
                if (handler(start) == true) count++;
                ++start;
            }
            return count;
        }

        /// <summary>
        /// 统计符合条件的像素数量
        /// </summary>
        /// <param name="src">图像</param>
        /// <param name="handler">Boolean Predicate<TPixel></param>
        /// <returns>符合条件的像素数量</returns>
        public unsafe static Int32 Count(this TImage src, Predicate<TPixel> handler)
        {
            TPixel* start = src.Start;
            TPixel* end = start + src.Length;
            if (start == null) return 0;

            Int32 count = 0;
            while (start != end)
            {
                if (handler(*start) == true) count++;
                ++start;
            }
            return count;
        }

        /// <summary>
        /// 查找模板。模板中值代表实际像素值。负数代表任何像素。返回查找得到的像素的左上端点的位置。
        /// </summary>
        /// <param name="template">TPixel[,]</param>
        /// <returns>查找到的模板集合</returns>
        public static unsafe List<PointS> FindTemplate(this TImage src, TPixel[,] template)
        {
            List<PointS> finds = new List<PointS>();
            int tHeight = template.GetUpperBound(0) + 1;
            int tWidth = template.GetUpperBound(1) + 1;
            int toWidth = src.Width - tWidth + 1;
            int toHeight = src.Height - tHeight + 1;
            int stride = src.Width;
            TPixel* start = src.Start;
            for (int r = 0; r < toHeight; r++)
            {
                for (int c = 0; c < toWidth; c++)
                {
                    TPixel* srcStart = start + r * stride + c;
                    for (int rr = 0; rr < tHeight; rr++)
                    {
                        for (int cc = 0; cc < tWidth; cc++)
                        {
                            TPixel pattern = template[rr, cc];
                            if (srcStart[rr * stride + cc] != pattern)
                            {
                                goto Next;
                            }
                        }
                    }

                    finds.Add(new PointS(c, r));

                Next:
                    continue;
                }
            }

            return finds;
        }
    }

    public partial class ImageRgbS
    {
        /// <summary>
        /// 图像所占字节数。
        /// </summary>
        public Int32 ByteCount { get; private set; }

        /// <summary>
        /// 图像的像素数量
        /// </summary>
        public Int32 Length { get; private set; }

        /// <summary>
        /// 每像素的尺寸（字节数）
        /// </summary>
        public Int32 SizeOfType { get; private set; }

        /// <summary>
        /// 图像宽（像素）
        /// </summary>
        public Int32 Width { get; protected set; }

        /// <summary>
        /// 图像的高（像素）
        /// </summary>
        public Int32 Height { get; protected set; }

        /// <summary>
        /// 图像的尺寸（以像素为单位）
        /// </summary>
        public Size Size
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return new Size(Width, Height); }
        }

        public Int32 Cols {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return Width;}
        }

        public Int32 Rows {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return Height; }
        }

        public unsafe Span<TPixel> Span
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return new Span<TPixel>((void*)Start, Length); }
        }

        /// <summary>
        /// 图像的起始指针。
        /// </summary>
        public unsafe TPixel* Start { get; private set; }

        public unsafe IntPtr StartIntPtr
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return (IntPtr)Start; }
        }

        public Int32 Stride { get; private set; }

        public unsafe TPixel this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return Start[index];
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                Start[index] = value;
            }
        }

        public unsafe TPixel this[int row, int col]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return Start[row * this.Width + col];
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                Start[row * this.Width + col] = value;
            }
        }

        public unsafe TPixel this[System.Drawing.Point location]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return Start[location.Y * this.Width + location.X];
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                Start[location.Y * this.Width + location.X] = value;
            }
        }

        public unsafe TPixel this[Geb.Image.Point location]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return Start[location.Y * this.Width + location.X];
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                Start[location.Y * this.Width + location.X] = value;
            }
        }


        public unsafe TPixel this[PointS location]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return Start[location.Y * this.Width + location.X];
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                Start[location.Y * this.Width + location.X] = value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe TPixel* Row(Int32 row)
        {
            if (row < 0 || row >= this.Height) throw new ArgumentOutOfRangeException("row");
            return Start + row * this.Width;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Span<TPixel> RowSpan(Int32 row)
        {
            if (row < 0 || row >= this.Height) throw new ArgumentOutOfRangeException("row");
            return new Span<TPixel>(Start + row * this.Width, this.Width);
        }

        /// <summary>
        /// 截取图像的一部分
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        public TImage this[Rect rect]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                TImage image = new TImage(rect.Width, rect.Height);
                image.CopyFrom(this, rect, new PointS(0, 0));
                return image;
            }
        }

        public TImage this[RotatedRectF rect]
        {
            get
            {
                AffineTransMatrix m = AffineTransMatrix.CreateRotationMatrix(rect.Center, -rect.Angle * Math.PI/180.0);
                List<PointF> points = new List<PointF>();
                foreach (var p in rect.Points())
                    points.Add(m * p);

                double xMin, xMax, yMin, yMax;
                xMin = xMax = points[0].X;
                yMin = yMax = points[0].Y;
                foreach (var p in points)
                {
                    xMin = Math.Min(p.X, xMin);
                    yMin = Math.Min(p.Y, yMin);
                    xMax = Math.Max(p.X, xMax);
                    yMax = Math.Max(p.Y, yMax);
                }

                m = AffineTransMatrix.CreatePanningMatrix(-xMin, -yMin) * m;
                int width = Math.Max(2, (int)Math.Round(xMax - xMin));
                int height = Math.Max(2, (int)Math.Round(yMax - yMin));

                TImage image = new TImage(width, height);
                image.Fill(default(TPixel));
                image.DrawImage(this, m);
                return image;
            }
        }

        public Rect Rect
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return new Rect(0, 0, Width, Height); }
        }

        private Boolean _isOwner;

        /// <summary>
        /// 是否是图像数据所在内存的拥有者。如果非所在内存的拥有者，则不负责释放内存。
        /// </summary>
        public Boolean IsOwner
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _isOwner; }
        }

        /// <summary>
        /// 是否图像内存的拥有权。
        /// </summary>
        /// <returns>如果释放前有所属内存，则返回所属内存的指针，否则返回空指针</returns>
        public unsafe void* ReleaseOwner()
        {
            if (Start == null || _isOwner == false) return null;
            else
            {
                _isOwner = false;
                return Start;
            }
        }

        /// <summary>
        /// 创建图像。
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public unsafe ImageRgbS(Int32 width, Int32 height)
        {
            if (width <= 0) throw new ArgumentOutOfRangeException("width");
            else if (height <= 0) throw new ArgumentOutOfRangeException("height");
            _isOwner = true;
            AllocMemory(width, height, 1);
        }

        /// <summary>
        /// 创建图像。
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public unsafe ImageRgbS(Int32 width, Int32 height, int lineSizeAlignmentBytes)
        {
            if (width <= 0) throw new ArgumentOutOfRangeException("width");
            else if (height <= 0) throw new ArgumentOutOfRangeException("height");
            _isOwner = true;
            AllocMemory(width, height,lineSizeAlignmentBytes);
        }

        /// <summary>
        /// 创建图像
        /// </summary>
        /// <param name="size"></param>
        public ImageRgbS(Size size):this(size.Width,size.Height)
        {
        }

        /// <summary>
        /// 创建图像，默认所创建的图像并不是图像数据的拥有者。
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="data"></param>
        /// <param name="isOwner">是否是此块内存数据的拥有者，默认为 false</param>
        public unsafe ImageRgbS(Int32 width, Int32 height, void* data, bool isOwner = false)
        {
            if (width <= 0) throw new ArgumentOutOfRangeException("width");
            else if (height <= 0) throw new ArgumentOutOfRangeException("height");
            _isOwner = isOwner;
            Width = width;
            Height = height;
            Length = Width * Height;
            SizeOfType = SizeOfPixel();
            Stride = SizeOfType * Width;
            ByteCount = SizeOfType * Length;
            Start = (TPixel*)data;
        }

        public int LineSizeAlignmentBytes { get; private set; } = 1;

        private unsafe void AllocMemory(int width, int height, int lineSizeAlignmentBytes = 1)
        {
            _isOwner = true;
            Height = height;
            Width = width;
            Length = Width * Height;
            SizeOfType = SizeOfPixel();
            Stride = SizeOfType * Width;
            LineSizeAlignmentBytes = Math.Max(1, lineSizeAlignmentBytes);
            int tail = Stride % LineSizeAlignmentBytes;
            if (tail > 0) Stride += tail;
            ByteCount = SizeOfType * Length;
            Start = (TPixel*)Marshal.AllocHGlobal(ByteCount);
            //Console.WriteLine("Thread " + System.Threading.Thread.CurrentThread.ManagedThreadId + " Alloc Memory:" + StartIntPtr);
        }

        public unsafe virtual void Dispose()
        {
            if (_isOwner == true)
            {
                if (Start != null)
                {
                    Marshal.FreeHGlobal((IntPtr)Start);
                    //Console.WriteLine("Thread " + System.Threading.Thread.CurrentThread.ManagedThreadId + " Dispose Memory:" + StartIntPtr);
                    Start = null;
                }
                _isOwner = false;
            }
        }

        ~ImageRgbS()
        {
            Dispose();
        }

        /// <summary>
        /// 每个像素所占的 bytes 数
        /// </summary>
        /// <returns></returns>
        public static Int32 SizeOfPixel()
        {
            return Marshal.SizeOf(typeof(TPixel));
        }

        public void ApplyMatrix(float a, float b, float c, float d, float e, float f)
        {
            //TODO: ApplyMatrix
            throw new NotImplementedException();
        }

        /// <summary>
        /// 代表当前图像内容的二维数组。
        /// .Net 的 IDE 均不支持直接查看.Net程序中的指针内容，DataSnapshot 提供了调试时查看
        /// 图像数据的唯一途径。请谨慎使用本方法。
        /// </summary>
        public unsafe TPixel[][] DataSnapshot
        {
            get
            {
                TPixel[][] data = new TPixel[Height][];
                for (int h = 0; h < Height; h++)
                {
                    TPixel[] row = new TPixel[Width];
                    for (int w = 0; w < Width; w++)
                    {
                        row[w] = this[h, w];
                    }
                    data[h] = row;
                }
                return data;
            }
        }

        /// <summary>
        /// 克隆一个副本。
        /// </summary>
        /// <returns></returns>
        public TImage Clone()
        {
            TImage img = new TImage(this.Width, this.Height);
            img.CloneFrom(this);
            return img;
        }

        /// <summary>
        /// 浅复制。只复制属性和指针，不复制图像的内存数据。复制后得到的新图像的IsOwner为false，代表它并不拥有图像内存的所有权。
        /// </summary>
        /// <returns>浅复制后的图像</returns>
        public unsafe TImage ShadowClone()
        {
            TImage img = new TImage(this.Width, this.Height);
            img.Start = this.Start;
            img._isOwner = false;
            return img;
        }

        /// <summary>
        /// 从另一个图像中复制
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        public unsafe TImage CloneFrom(TImage src)
        {
            if (src == null) throw new ArgumentNullException("src");
            if (src.ByteCount != this.ByteCount) throw new NotSupportedException("与src图像的像素数量不一致，无法复制.");

            TPixel* start = Start;
            TPixel* end = start + Length;
            TPixel* from = src.Start;

            while (start != end)
            {
                *start = *from;
                ++start;
                ++from;
            }
            return this;
        }

        public void Reshape(int width, int height)
        {
            if (width <= 0) throw new ArgumentException("Width must large than 0");
            if (height <= 0) throw new ArgumentException("Height must large than 0");
            if (width * height != this.Width * this.Height) throw new ArgumentException("width*height must be same as this image's length");
            this.Width = width;
            this.Height = height;
        }

        /// <summary>
        /// 在图像边缘添加 border
        /// </summary>
        /// <param name="paddingLeft">左侧填充像素数</param>
        /// <param name="paddingTop">上侧填充像素数</param>
        /// <param name="paddingRight">右侧填充像素数</param>
        /// <param name="paddingBottom">下侧填充像素数</param>
        /// <param name="val">填充值</param>
        /// <returns></returns>
        public unsafe TImage MakeBorder(uint paddingLeft, uint paddingTop, uint paddingRight, uint paddingBottom, TPixel val)
        {
            int newWidth = (int)(this.Width + paddingLeft + paddingRight);
            int newHeight = (int)(this.Height + paddingTop + paddingBottom);
            TImage img = new TImage(newWidth, newHeight);
            img.Fill(val);  // 这里可以进一步优化，只填充 border 部分
            TPixel* src0 = this.Start;
            TPixel* dst0 = img.Start + paddingLeft + paddingTop * img.Width;
            for(int h = 0; h < this.Height; h++)
            {
                Span<TPixel> srcSpan = new Span<TPixel>(src0 + h * this.Width, this.Width);
                Span<TPixel> dstSpan = new Span<TPixel>(dst0 + h * img.Width, this.Width);
                srcSpan.CopyTo(dstSpan);
            }
            return img;
        }

        /// <summary>
        /// 对图像进行转置，行变成列，列变成行。转置结果直接存在当前的图像中。
        /// </summary>
        /// <returns></returns>
        public unsafe TImage ApplyTranspose()
        {
            TImage img = new TImage(Width, Height);
            img.CloneFrom(this);
            this.Width = img.Height;
            this.Height = img.Width;
            img.TransposeTo(this);
            img.Dispose();
            return this;
        }

        /// <summary>
        /// 将当前图像转置到另一幅图像中。也就是说，imgDst[i,j] = this[j,i]
        /// </summary>
        /// <param name="imgDst">转置标的图像，图像宽是本图像的高，图像的高是本图像的宽</param>
        /// <returns>转置标的图像</returns>
        public unsafe TImage TransposeTo(TImage imgDst)
        {
            if (this.Width != imgDst.Height || this.Height != imgDst.Width)
            {
                throw new ArgumentException("两幅图像的尺寸不匹配，无法进行转置操作.");
            }

            int width = this.Width;
            int height = this.Height;

            TPixel* src = this.Start;
            TPixel* dst = imgDst.Start;

            for (int y = 0; y < height; y++)
            {
                TPixel* srcLine = src + y * width;
                TPixel* dstLine = dst + y;
                for (int x = 0; x < width; x++)
                {
                    *dstLine = srcLine[x];
                    dstLine += height;
                }
            }

            return imgDst;
        }

        public unsafe TImage Fill(TPixel pixel)
        {
            TPixel* p = this.Start;
            TPixel* end = p + this.Length;
            while (p != end)
            {
                *p = pixel;
                p++;
            }
            return this;
        }

        public unsafe TImage Fill(int x, int y, int width, int height, TPixel pixel)
        {
            int x0 = Math.Max(x, 0);
            int y0 = Math.Max(y, 0);
            int x1 = Math.Min(Width, x + width);
            int y1 = Math.Min(Height, y + height);
            if (x1 <= x0 || y1 <= y0) return this;

            int ww = x1 - x0;
            TPixel* p0 = this.Start;

            for (int h = y0; h < y1; h++)
            {
                TPixel* h0 = p0 + h * Width + x0;
                TPixel* h1 = h0 + ww;
                while (h0 < h1)
                {
                    *h0 = pixel;
                    h0++;
                }
            }

            return this;
        }

        public unsafe TImage Replace(TPixel pixel, TPixel replaced)
        {
            TPixel* p = this.Start;
            TPixel* end = p + this.Length;
            while (p != end)
            {
                if (*p == pixel)
                {
                    *p = replaced;
                }
                p++;
            }
            return this;
        }

        public unsafe TImage CopyFrom(TImage src, System.Drawing.Rectangle region, System.Drawing.Point destAnchor)
        {
            return CopyFrom(src, new Rect(region.X, region.Y, region.Width, region.Height), new PointS(destAnchor.X, destAnchor.Y));
        }

        public unsafe TImage CopyFrom(TImage src, Rect region, PointS destAnchor)
        {
            if (region.X >= src.Width || region.Y >= src.Height) return this;
            int startSrcX = Math.Max(0, (int)region.X);
            int startSrcY = Math.Max(0, (int)region.Y);
            int endSrcX = Math.Min(region.X + region.Width, src.Width);
            int endSrcY = Math.Min(region.Y + region.Height, src.Height);
            int offsetX = region.X < 0? -region.X : 0;
            int offsetY = region.Y < 0? -region.Y : 0;
            offsetX = destAnchor.X + offsetX;
            offsetY = destAnchor.Y + offsetY;
            int startDstX = Math.Max(0, offsetX);
            int startDstY = Math.Max(0, offsetY);
            offsetX = offsetX < 0 ? -offsetX : 0;
            offsetY = offsetY < 0 ? -offsetY : 0;
            startSrcX += offsetX;
            startSrcY += offsetY;
            int endDstX = Math.Min(destAnchor.X + region.Width, this.Width);
            int endDstY = Math.Min(destAnchor.Y + region.Height, this.Height);
            int copyWidth = Math.Min(endSrcX - startSrcX, endDstX - startDstX);
            int copyHeight = Math.Min(endSrcY - startSrcY, endDstY - startDstY);
            if (copyWidth <= 0 || copyHeight <= 0) return this;

            int srcWidth = src.Width;
            int dstWidth = this.Width;

            TPixel* srcLine = src.Start + srcWidth * startSrcY + startSrcX;
            TPixel* dstLine = this.Start + dstWidth * startDstY + startDstX;
            TPixel* endSrcLine = srcLine + srcWidth * copyHeight;
            int alpha1, alpha2, blendAlpha,alpha;
            if (srcLine[0] is Bgra32)
            {
                int beta;
                while (srcLine < endSrcLine)
                {
                    Bgra32* pSrc = (Bgra32*)srcLine;
                    Bgra32* endPSrc = pSrc + copyWidth;
                    Bgra32* pDst = (Bgra32*)dstLine;
                    while (pSrc < endPSrc)
                    {
                        if (pSrc->Alpha == 255 || pDst->Alpha == 0)
                        {
                            *pDst = *pSrc;
                        }
                        else if (pSrc->Alpha > 0)
                        {
                               //BlendAlpha = A1 * A2 \ 255
                               //ImageData(Speed + 3) = A1 + A2 - BlendAlpha                // Alpha
                               //ImageData(Speed) = (B1 * A1 + B2 * A2 - BlendAlpha * (B1 + B2 - Blue)) \ 255
                               //ImageData(Speed + 1) = (G1 * A1 + G2 * A2 - BlendAlpha * (G1 + G2 - Green)) \ 255
                               //ImageData(Speed + 2) = (R1 * A1 + R2 * A2 - BlendAlpha * (R1 + R2 - Red)) \ 255

                            //beta = 255 - pSrc->Alpha;
                            //pDst->Blue = (Byte)((pSrc->Blue * pSrc->Alpha + pDst->Blue * beta) >> 8);
                            //pDst->Green = (Byte)((pSrc->Green * pSrc->Alpha + pDst->Green * beta) >> 8);
                            //pDst->Red = (Byte)((pSrc->Red * pSrc->Alpha + pDst->Red * beta) >> 8);

                            alpha1 = pSrc->Alpha;
                            alpha2 = pDst->Alpha;
                            blendAlpha = alpha1 * alpha2 / 255;
                            beta = 255 - pSrc->Alpha;
                            pDst->Alpha = (Byte)(alpha1 + alpha2 - blendAlpha);

                            // 严格来说，下面的转换算法只是近似算法，不是准确算法。准确算法太耗时间
                            pDst->Blue = (Byte)((pSrc->Blue * alpha1 + pDst->Blue * beta) >> 8);
                            pDst->Green = (Byte)((pSrc->Green * alpha1 + pDst->Green * beta) >> 8);
                            pDst->Red = (Byte)((pSrc->Red * alpha1 + pDst->Red * beta) >> 8);
                        }
                        pSrc++;
                        pDst++;
                    }
                    srcLine += srcWidth;
                    dstLine += dstWidth;
                }
            }
            else
            {
                while (srcLine < endSrcLine)
                {
                    TPixel* pSrc = srcLine;
                    TPixel* endPSrc = pSrc + copyWidth;
                    TPixel* pDst = dstLine;
                    while (pSrc < endPSrc)
                    {
                        *pDst = *pSrc;
                        pSrc++;
                        pDst++;
                    }
                    srcLine += srcWidth;
                    dstLine += dstWidth;
                }
            }
            return this;
        }

        /// <summary>
        /// 将图像进行仿射变换后，绘制到原图像上
        /// </summary>
        /// <param name="src"></param>
        /// <param name="m"></param>
        /// <exception cref="NotImplementedException"></exception>
        public unsafe void DrawImage(TImage src, AffineTransMatrix m)
        {
            Rect rect = src.Rect;
            Span<PointF> points = stackalloc PointF[4];
            points[0] = m * rect.LeftTop;
            points[1] = m * rect.LeftBottom;
            points[2] = m * rect.RightTop;
            points[3] = m * rect.RightBottom;
            double xMin, xMax, yMin, yMax;
            xMin = xMax = points[0].X;
            yMin = yMax = points[0].Y;
            foreach(var p in points)
            {
                xMin = Math.Min(p.X, xMin);
                yMin = Math.Min(p.Y, yMin);
                xMax = Math.Max(p.X, xMax);
                yMax = Math.Max(p.Y, yMax);
            }

            // 如果映射后的区域在图像的边界外面，则不用进行处理
            if (xMin >= Width || yMin >= Height || xMax <= 0 || yMax <= 0) return;
            
            int xFrom = (int)xMin; xFrom = Math.Max(0, xFrom);
            int yFrom = (int)yMin; yFrom = Math.Max(0, yFrom);
            int xEnd = (int)xMax + 1; xEnd = Math.Min(Width, xEnd);
            int yEnd = (int)yMax + 1; yEnd = Math.Min(Height, yEnd);

            int nChannel = sizeof(TPixel) / sizeof(TChannel);
            TChannel* rootSrc = (TChannel*)src.Start;
            TChannel* rootDst = (TChannel*)this.Start;

            // 逆矩阵
            var im = m.Inverse();

            float xAlpha = 0;
            float yAlpha = 0;
            float xBeta = 0;
            float yBeta = 0;

            TChannel* sLine0 = null;
            TChannel* sLine1 = null;

            for (int n = 0; n < nChannel; n++)
            {
                TChannel* s0 = rootSrc + n;
                TChannel* d0 = rootDst + n;
                for (int h = yFrom; h < yEnd; h++)
                {
                    TChannel* dLine = d0 + h * this.Stride;

                    for (int w = xFrom; w < xEnd; w++)
                    {
                        // 求 (w,h) 在 src 中的坐标
                        PointF p = im * new Point(w, h);

                        // 不需要复制
                        if (rect.IsContains(p) == false) continue;

                        xAlpha = p.X - (int)p.X; xBeta = 1 - xAlpha;
                        yAlpha = p.Y - (int)p.Y; yBeta = 1 - yAlpha;

                        // 避免指针越界
                        p.X = Math.Min(rect.Width - 1 - 0.00001f, p.X);
                        p.Y = Math.Min(rect.Height - 1 - 0.00001f, p.Y);

                        //this[h, w] = src[(int)p.Y, (int)p.X];

                        sLine0 = s0 + src.Stride * (int)p.Y + nChannel * (int)p.X;
                        sLine1 = sLine0 + src.Stride;

                        TChannel val = (TChannel)(sLine0[0] * xBeta * yBeta + sLine0[nChannel] * xAlpha * yBeta + sLine1[0] * xBeta * yAlpha + sLine1[nChannel] * xAlpha * yAlpha + 0.5f);
                        dLine[w * nChannel] = val;
                    }
                }
            }
        }

        public TImage FloodFill(System.Drawing.Point location, TPixel anchorColor, TPixel replecedColor)
        {
            int width = this.Width;
            int height = this.Height;
            if (location.X < 0 || location.X >= width || location.Y < 0 || location.Y >= height) return this;

            if (anchorColor == replecedColor) return this;
            if (this[location.Y, location.X] != anchorColor) return this;

            Stack<System.Drawing.Point> points = new Stack<System.Drawing.Point>();
            points.Push(location);

            int ww = width - 1;
            int hh = height - 1;

            while (points.Count > 0)
            {
                System.Drawing.Point p = points.Pop();
                this[p.Y, p.X] = replecedColor;
                if (p.X > 0 && this[p.Y, p.X - 1] == anchorColor)
                {
                    this[p.Y, p.X - 1] = replecedColor;
                    points.Push(new System.Drawing.Point(p.X - 1, p.Y));
                }

                if (p.X < ww && this[p.Y, p.X + 1] == anchorColor)
                {
                    this[p.Y, p.X + 1] = replecedColor;
                    points.Push(new System.Drawing.Point(p.X + 1, p.Y));
                }

                if (p.Y > 0 && this[p.Y - 1, p.X] == anchorColor)
                {
                    this[p.Y - 1, p.X] = replecedColor;
                    points.Push(new System.Drawing.Point(p.X, p.Y - 1));
                }

                if (p.Y < hh && this[p.Y + 1, p.X] == anchorColor)
                {
                    this[p.Y + 1, p.X] = replecedColor;
                    points.Push(new System.Drawing.Point(p.X, p.Y + 1));
                }
            }
            return this;
        }

        /// <summary>
        /// 使用众值滤波
        /// </summary>
        public unsafe TImage ApplyModeFilter(int size)
        {
            if (size <= 1) throw new ArgumentOutOfRangeException("size 必须大于1.");
            else if (size > 127) throw new ArgumentOutOfRangeException("size 最大为127.");
            else if (size % 2 == 0) throw new ArgumentException("size 应该是奇数.");

            int* vals = stackalloc int[size * size + 1];
            TPixel* keys = stackalloc TPixel[size * size + 1];

            ImageRgbS mask = this.Clone();
            int height = this.Height;
            int width = this.Width;

            TPixel* pMask = mask.Start;
            TPixel* pThis = this.Start;

            int radius = size / 2;

            for (int h = 0; h < height; h++)
            {
                for (int w = 0; w < width; w++)
                {
                    int count = 0;

                    // 建立直方图
                    for (int y = -radius; y <= radius; y++)
                    {
                        for (int x = -radius; x <= radius; x++)
                        {
                            int yy = y + h;
                            int xx = x + w;
                            if (xx >= 0 && xx < width && yy >= 0 && yy < height)
                            {
                                TPixel color = pMask[yy * width + xx];

                                bool find = false;
                                for (int i = 0; i < count; i++)
                                {
                                    if (keys[i] == color)
                                    {
                                        vals[i]++;
                                        find = true;
                                        break;
                                    }
                                }

                                if (find == false)
                                {
                                    keys[count] = color;
                                    vals[count] = 1;
                                    count++;
                                }
                            }
                        }
                    }

                    if (count > 0)
                    {
                        // 求众数
                        int index = -1;
                        int max = int.MinValue;
                        for (int i = 0; i < count; i++)
                        {
                            if (vals[i] > max)
                            {
                                index = i;
                                max = vals[i];
                            }
                        }

                        if (max > 1)
                        {
                            pThis[h * width + w] = keys[index];
                        }
                    }
                }
            }

            mask.Dispose();

            return this;
        }

        public void DrawRect(RectF rect, TPixel color)
        {
            DrawLine(new PointF(rect.X, rect.Y), new PointF(rect.X + rect.Width, rect.Y), color);
            DrawLine(new PointF(rect.X, rect.Y), new PointF(rect.X, rect.Y + rect.Height), color);
            DrawLine(new PointF(rect.X + rect.Width, rect.Y), new PointF(rect.X + rect.Width, rect.Y + rect.Height), color);
            DrawLine(new PointF(rect.X, rect.Y + rect.Height), new PointF(rect.X + rect.Width, rect.Y + rect.Height), color);
        }

        public void DrawRect(RectF rect, TPixel color, int radius)
        {
            DrawLine(new PointF(rect.X, rect.Y), new PointF(rect.X + rect.Width, rect.Y), color, radius);
            DrawLine(new PointF(rect.X, rect.Y), new PointF(rect.X, rect.Y + rect.Height), color, radius);
            DrawLine(new PointF(rect.X + rect.Width, rect.Y), new PointF(rect.X + rect.Width, rect.Y + rect.Height), color, radius);
            DrawLine(new PointF(rect.X, rect.Y + rect.Height), new PointF(rect.X + rect.Width, rect.Y + rect.Height), color, radius);
        }

        public unsafe void DrawLine(PointF start, PointF end, TPixel color)
        {
            // Bresenham画线算法
            int x1 = (int)Math.Floor(start.X);
            int y1 = (int)Math.Floor(start.Y);
            int x2 = (int)Math.Floor(end.X);
            int y2 = (int)Math.Floor(end.Y);
            int xMin = Math.Min(x1, x2);
            int yMin = Math.Min(y1, y2);
            int xMax = Math.Max(x1, x2);
            int yMax = Math.Max(y1, y2);

            // 线一定在图像外部
            if (xMin >= Width || yMin >= Height || yMax < 0 || xMax < 0)
            {
                return;
            }

            int dx = Math.Abs(x2 - x1);
            int dy = Math.Abs(y2 - y1);
            Boolean dyIsLargerThanDx = false;
            int tmp;
            if (dx < dy)
            {
                dyIsLargerThanDx = true;

                tmp = x1;
                x1 = y1;
                y1 = tmp;

                tmp = x2;
                x2 = y2;
                y2 = tmp;

                tmp = dx;
                dx = dy;
                dy = tmp;
            }

            int ix = (x2 - x1) > 0 ? 1 : -1;
            int iy = (y2 - y1) > 0 ? 1 : -1;
            int cx = x1;
            int cy = y1;
            int n2dy = dy * 2;
            int n2dydx = (dy - dx) * 2;
            int d = dy * 2 - dx;

            // 线在图像内部，则不检查是否指针越位
            if (xMin >= 0 && yMin >= 0 && yMax < (Height - 1) && xMax < Width)
            {
                if (dyIsLargerThanDx == true)
                { // 如果直线与 x 轴的夹角大于 45 度
                    this[cx, cy] = color;
                    while (cx != x2)
                    {
                        cx += ix;
                        if (d < 0)
                        {
                            d += n2dy;
                        }
                        else
                        {
                            cy += iy;
                            d += n2dydx;
                        }
                        this[cx, cy] = color;
                    }
                }
                else
                { // 如果直线与 x 轴的夹角小于 45 度
                    this[cy, cx] = color;
                    while (cx != x2)
                    {
                        cx += ix;
                        if (d < 0)
                        {
                            d += n2dy;
                        }
                        else
                        {
                            cy += iy;
                            d += n2dydx;
                        }
                        this[cy, cx] = color;
                    }
                }
            }
            else
            {
                TPixel* p0 = Start;
                int width = this.Width;
                int height = this.Height;
                int count = 0;

                if (dyIsLargerThanDx == true)
                { // 如果直线与 x 轴的夹角大于 45 度
                    if (cy >= 0 && cy < width && cx >= 0 && cx < height)
                    {
                        p0[width * cx + cy] = color;
                        count++;
                    }
                    while (cx != x2)
                    {
                        cx += ix;

                        if (d < 0)
                        {
                            d += n2dy;
                        }
                        else
                        {
                            cy += iy;
                            d += n2dydx;
                        }

                        if (cy >= 0 && cy < width && cx >= 0 && cx < height)
                        {
                            p0[width * cx + cy] = color;
                            count++;
                        }
                        else
                        {
                            if (count > 0) return;
                        }
                    }
                }
                else
                { // 如果直线与 x 轴的夹角小于 45 度
                    if (cx >= 0 && cx < width && cy >= 0 && cy < height)
                    {
                        p0[width * cy + cx] = color;
                        count++;
                    }
                    while (cx != x2)
                    {
                        cx += ix;

                        if (d < 0)
                        {
                            d += n2dy;
                        }
                        else
                        {
                            cy += iy;
                            d += n2dydx;
                        }

                        if (cx >= 0 && cx < width && cy >= 0 && cy < height)
                        {
                            p0[width * cy + cx] = color;
                            count++;
                        }
                        else
                        {
                            if (count > 0) return;
                        }
                    }
                }
            }
        }

        public unsafe void ForEachPixelsOnLine(PointF start, PointF end, Action<PointS> action)
        {
            if (action == null) return;

            // Bresenham画线算法
            int x1 = (int)Math.Floor(start.X);
            int y1 = (int)Math.Floor(start.Y);
            int x2 = (int)Math.Floor(end.X);
            int y2 = (int)Math.Floor(end.Y);
            int xMin = Math.Min(x1, x2);
            int yMin = Math.Min(y1, y2);
            int xMax = Math.Max(x1, x2);
            int yMax = Math.Max(y1, y2);

            // 线一定在图像外部
            if (xMin >= Width || yMin >= Height || yMax < 0 || xMax < 0)
            {
                return;
            }

            int dx = Math.Abs(x2 - x1);
            int dy = Math.Abs(y2 - y1);
            Boolean dyIsLargerThanDx = false;
            int tmp;
            if (dx < dy)
            {
                dyIsLargerThanDx = true;

                tmp = x1;
                x1 = y1;
                y1 = tmp;

                tmp = x2;
                x2 = y2;
                y2 = tmp;

                tmp = dx;
                dx = dy;
                dy = tmp;
            }

            int ix = (x2 - x1) > 0 ? 1 : -1;
            int iy = (y2 - y1) > 0 ? 1 : -1;
            int cx = x1;
            int cy = y1;
            int n2dy = dy * 2;
            int n2dydx = (dy - dx) * 2;
            int d = dy * 2 - dx;

            // 线在图像内部，则不检查是否指针越位
            if (xMin >= 0 && yMin >= 0 && yMax < (Height - 1) && xMax < Width)
            {
                if (dyIsLargerThanDx == true)
                { // 如果直线与 x 轴的夹角大于 45 度
                    
                    action(new PointS(cx,cy));
                    while (cx != x2)
                    {
                        cx += ix;
                        if (d < 0)
                        {
                            d += n2dy;
                        }
                        else
                        {
                            cy += iy;
                            d += n2dydx;
                        }
                        action(new PointS(cx, cy));
                    }
                }
                else
                { // 如果直线与 x 轴的夹角小于 45 度
                    action(new PointS(cx, cy));
                    while (cx != x2)
                    {
                        cx += ix;
                        if (d < 0)
                        {
                            d += n2dy;
                        }
                        else
                        {
                            cy += iy;
                            d += n2dydx;
                        }
                        action(new PointS(cx, cy));
                    }
                }
            }
            else
            {
                TPixel* p0 = Start;
                int width = this.Width;
                int height = this.Height;
                int count = 0;

                if (dyIsLargerThanDx == true)
                { // 如果直线与 x 轴的夹角大于 45 度
                    if (cy >= 0 && cy < width && cx >= 0 && cx < height)
                    {
                        action(new PointS(width * cx, cy));
                        count++;
                    }
                    while (cx != x2)
                    {
                        cx += ix;

                        if (d < 0)
                        {
                            d += n2dy;
                        }
                        else
                        {
                            cy += iy;
                            d += n2dydx;
                        }

                        if (cy >= 0 && cy < width && cx >= 0 && cx < height)
                        {
                            action(new PointS(width * cx, cy));
                            count++;
                        }
                        else
                        {
                            if (count > 0) return;
                        }
                    }
                }
                else
                { // 如果直线与 x 轴的夹角小于 45 度
                    if (cx >= 0 && cx < width && cy >= 0 && cy < height)
                    {
                        action(new PointS(cx, width * cy));
                        count++;
                    }
                    while (cx != x2)
                    {
                        cx += ix;

                        if (d < 0)
                        {
                            d += n2dy;
                        }
                        else
                        {
                            cy += iy;
                            d += n2dydx;
                        }

                        if (cx >= 0 && cx < width && cy >= 0 && cy < height)
                        {
                            action(new PointS(cx, width * cy));
                            count++;
                        }
                        else
                        {
                            if (count > 0) return;
                        }
                    }
                }
            }
        }

        public void DrawLine(PointF start, PointF end, TPixel color, int radius)
        {
            float deltaX = end.X - start.X;
            float deltaY = end.Y - start.Y;
            int ww = this.Width - 1;
            int hh = this.Height - 1;

            if (Math.Abs(deltaX) < 0.0001)
            {
                if (Math.Abs(deltaY) < 0.0001)
                {
                    SetColor(start.X, start.Y, color, 0, radius, ww, hh);
                    return;
                };

                float yStart = start.Y;
                float yEnd = end.Y;
                float x = start.X;

                if (yEnd < yStart)
                {
                    float tmp = yEnd;
                    yEnd = yStart;
                    yStart = tmp;
                }

                yStart = Math.Max(0, yStart);
                yEnd = Math.Min(hh, yEnd);

                for (float y = yStart; y <= yEnd; y++)
                {
                    SetColor(x, y, color, 0, radius, ww, hh);
                }
            }
            else
            {
                float xStart = start.X;
                float xEnd = end.X;
                if (xEnd < xStart)
                {
                    float tmp = xEnd;
                    xEnd = xStart;
                    xStart = tmp;
                }

                float step = 1;
                float grad = Math.Abs(deltaY / deltaX);
                if (grad > 1)
                {
                    step = 1 / grad;
                }

                for (float x = xStart; x <= xEnd; x += step)
                {
                    float deltaXX = start.X - x;
                    float deltaYY = deltaY * (deltaXX / deltaX);
                    float y = start.Y - deltaYY;

                    SetColor(x, y, color, 0, radius, ww, hh);
                }
            }
        }

        public void Draw(float x, float y, TPixel color, int radius)
        {
            SetColor(x, y, color, 0, radius, Width - 1, Height - 1);
        }

        /// <summary>
        /// 绘制文字
        /// </summary>
        /// <param name="txt">文字内容。目前支持ASCII内容。</param>
        /// <param name="color">文字颜色</param>
        /// <param name="pos">文字绘制的位置</param>
        /// <param name="scale">缩放比例</param>
        public unsafe void DrawText(String txt, TPixel color, PointS org, double fontSize = 1)
        {
            int[] ascii = HersheyGlyphs.HersheyPlain;
            String[] faces = HersheyGlyphs.Data;
            int base_line = +(ascii[0] & 15);
            double hscale = fontSize, vscale = hscale;
            double view_x = org.X;
            double view_y = org.Y + base_line * vscale;
            List<PointS> pts = new List<PointS>(1 << 10);
            for (int i = 0; i < txt.Length; i++)
            {
                Char c = txt[i];
                if (c >= 127 || c < ' ')
                    c = '?';
                String fontData = faces[ascii[(c - ' ') + 1]];

                PointS p = new PointS();
                p.X = (short)(fontData[0] - 'R');
                p.Y = (short)(fontData[1] - 'R');
                double dx = p.Y * hscale;
                view_x -= p.X * hscale;
                pts.Clear();
                for (int k = 2; k <= fontData.Length; )
                {
                    if (k == fontData.Length || fontData[k] == ' ')
                    {
                        // Draw Poly Line
                        if (pts.Count > 1)
                        {
                            for (int j = 1; j < pts.Count; j++)
                            {
                                DrawLine(pts[j - 1].ToPointF(), pts[j].ToPointF(), color);
                            }
                        }
                        pts.Clear();
                        k++;
                    }
                    else
                    {
                        p.X = (short)(fontData[k] - 'R');
                        p.Y = (short)(fontData[k+1] - 'R');
                        k += 2;
                        pts.Add(new PointS((short)Math.Round(p.X * hscale + view_x),
                            (short)Math.Round(p.Y * vscale + view_y)));
                    }
                }

                view_x += dx;
            }
        }

        /// <summary>
        /// 绘制圆
        /// </summary>
        /// <param name="x">圆心横坐标</param>
        /// <param name="y">圆心纵坐标</param>
        /// <param name="color">颜色</param>
        /// <param name="radius">圆的半径</param>
        public void DrawCircle(float x, float y, TPixel color, int radius = 1)
        {
            SetColor(x, y, color, radius-1, radius + 1, Width - 1, Height - 1);
        }

        /// <summary>
        /// 绘制圆
        /// </summary>
        /// <param name="x">圆心横坐标</param>
        /// <param name="y">圆心纵坐标</param>
        /// <param name="color">颜色</param>
        /// <param name="radius">圆的半径</param>
        public void FillCircle(float x, float y, TPixel color, int radius = 1)
        {
            SetColor(x, y, color, 0, radius, Width - 1, Height - 1);
        }

        public void SetColor(float x, float y, TPixel color, int radius = 1)
        {
            SetColor(x, y, color, 0, radius, Width - 1, Height - 1);
        }

        private void SetColor(float x, float y, TPixel color, int innerRadius, int radius, int ww, int hh)
        {
            int xStart = (int)(x - radius - 1);
            int xEnd = (int)(x + radius + 1);
            int yStart = (int)(y - radius - 1);
            int yEnd = (int)(y + radius + 1);

            int maxDistanceSquare = radius * radius;
            int minDistanceSquare = innerRadius * innerRadius;
            for (int yy = yStart; yy < yEnd; yy++)
            {
                for (int xx = xStart; xx < xEnd; xx++)
                {
                    if (xx < 0 || yy < 0 || xx > ww || yy > hh) continue;
                    float deltaX = xx - x;
                    float deltaY = yy - y;
                    float distance = deltaX * deltaX + deltaY * deltaY;
                    if (distance <= maxDistanceSquare && distance >= minDistanceSquare)
                        this[yy, xx] = color;
                }
            }
        }

        public unsafe TPixel[] ToArray()
        {
            TPixel[] array = new TPixel[this.Length];
            for (int i = 0; i < Length; i++)
            {
                array[i] = this[i];
            }
            return array;
        }

        public unsafe TImage Resize(int width, int height, InterpolationMode mode = InterpolationMode.NearestNeighbor)
        {
            if (width < 1) throw new ArgumentException("width must > 0");
            if (height < 1) throw new ArgumentException("height must > 0");

            // 计算 channel 数量
            int nChannel = sizeof(TPixel) / sizeof(TChannel);
            TImage imgDst = new ImageRgbS(width, height);
            TChannel* rootSrc = (TChannel*)this.Start;
            TChannel* rootDst = (TChannel*)imgDst.Start;

            int wSrc = this.Width;
            int hSrc = this.Height;
            int wSrcIdxMax = wSrc - 1;
            int hSrcIdxMax = hSrc - 1;
            float wCoeff = (float)wSrc / width;
            float hCoeff = (float)hSrc / height;

            if (mode == InterpolationMode.NearestNeighbor)
            {
                // 对每个 channel 进行分别处理
                for (int n = 0; n < nChannel; n++)
                {
                    TChannel* s0 = rootSrc + n;
                    TChannel* d0 = rootDst + n;
                    for (int h = 0; h < height; h++)
                    {
                        float yDstF = h * hCoeff;
                        int yDst = (int)Math.Round(yDstF);
                        yDst = Math.Min(yDst, hSrcIdxMax);

                        TChannel* sLine = s0 + yDst * wSrc * nChannel;
                        TChannel* dLine = d0 + h * width * nChannel;

                        for (int w = 0; w < width; w++)
                        {
                            float xDstF = w * wCoeff;
                            int xDst = (int)Math.Round(xDstF);
                            xDst = Math.Min(xDst, wSrcIdxMax);
                            dLine[w * nChannel] = sLine[xDst * nChannel];
                        }
                    }
                }
            }
            else
            {
                // 先行计算
                float* alphaW = stackalloc float[width];
                float* betaW = stackalloc float[width];
                Int32* idxW0 = stackalloc Int32[width];
                Int32* idxW1 = stackalloc Int32[width];

                for (int w = 0; w < width; w++)
                {
                    float offsetF = (w * wCoeff);
                    int offsetInt = (int)offsetF;
                    idxW0[w] = offsetInt * nChannel;
                    idxW1[w] = Math.Min(offsetInt + 1, wSrc-1) * nChannel;
                    alphaW[w] = offsetF - offsetInt;
                    betaW[w] = 1 + offsetInt - offsetF;
                }

                float xAlpha = 0;
                float xBeta = 0;

                // 对每个 channel 进行分别处理
                TChannel test = (TChannel)1.5f;
                if(1.5f - test > 0.00001f)  // TChannel 是非浮点类型
                {
                    for (int n = 0; n < nChannel; n++)
                    {
                        TChannel* s0 = rootSrc + n;
                        TChannel* d0 = rootDst + n;
                        for (int h = 0; h < height; h++)
                        {
                            float yDstF = h * hCoeff;
                            int yDst = (int)yDstF;
                            float yAlpha = yDstF - yDst;
                            float yBeta = 1 - yAlpha;

                            TChannel* sLine0 = s0 + yDst * wSrc * nChannel;
                            TChannel* sLine1 = s0 + Math.Min(yDst + 1, hSrc - 1) * wSrc * nChannel;
                            TChannel* dLine = d0 + h * width * nChannel;

                            for (int w = 0; w < width; w++)
                            {
                                xAlpha = alphaW[w];
                                xBeta = 1 - xAlpha;
                                TChannel val = (TChannel)(sLine0[idxW0[w]] * xBeta * yBeta + sLine0[idxW1[w]] * xAlpha * yBeta + sLine1[idxW0[w]] * xBeta * yAlpha + sLine1[idxW1[w]] * xAlpha * yAlpha + 0.5f);
                                dLine[w * nChannel] = val;
                            }
                        }
                    }
                }
                else // TChannel 是浮点类型
                {
                    for (int n = 0; n < nChannel; n++)
                    {
                        TChannel* s0 = rootSrc + n;
                        TChannel* d0 = rootDst + n;
                        for (int h = 0; h < height; h++)
                        {
                            float yDstF = h * hCoeff;
                            int yDst = (int)yDstF;
                            float yAlpha = yDstF - yDst;
                            float yBeta = 1 - yAlpha;

                            TChannel* sLine0 = s0 + yDst * wSrc * nChannel;
                            TChannel* sLine1 = s0 + Math.Min(yDst + 1, hSrc - 1) * wSrc * nChannel;
                            TChannel* dLine = d0 + h * width * nChannel;

                            for (int w = 0; w < width; w++)
                            {
                                xAlpha = alphaW[w];
                                xBeta = 1 - xAlpha;
                                TChannel val = (TChannel)(sLine0[idxW0[w]] * xBeta * yBeta + sLine0[idxW1[w]] * xAlpha * yBeta + sLine1[idxW0[w]] * xBeta * yAlpha + sLine1[idxW1[w]] * xAlpha * yAlpha);
                                dLine[w * nChannel] = val;
                            }
                        }
                    }
                }
            }
            return imgDst;
        }

        /// <summary>
        /// 从外部复制数据。数据的 width 和 height 应该和图像的 width 和 height 一致。
        /// </summary>
        /// <param name="pData"></param>
        /// <param name="dataStride">外部数据的 stride </param>
        public unsafe void CopyFrom(void* pData, int dataStride)
        {
            Byte* pDst0 =  (Byte*)this.Start;
            Byte* pSrc0 = (Byte*)pData;
            int rowBytes = this.Width * this.SizeOfType;
            for(int i = 0; i < this.Height; i++)
            {
                Span<Byte> spanSrc = new Span<Byte>(pSrc0, rowBytes);
                Span<Byte> spanDst = new Span<Byte>(pDst0, rowBytes);
                spanSrc.CopyTo(spanDst);
                pSrc0 += dataStride;
                pDst0 += this.Stride;
            }
        }

        /// <summary>
        /// 将数据复制到外部。数据的 width 和 height 应该和图像的 width 和 height 一致。
        /// </summary>
        /// <param name="pData"></param>
        /// <param name="dataStride">外部数据的 stride </param>
        public unsafe void CopyTo(void* pData, int dataStride)
        {
            Byte* pSrc0 = (Byte*)this.Start;
            Byte* pDst0 = (Byte*)pData;
            int rowBytes = this.Width * this.SizeOfType;
            for (int i = 0; i < this.Height; i++)
            {
                Span<Byte> spanSrc = new Span<Byte>(pSrc0, rowBytes);
                Span<Byte> spanDst = new Span<Byte>(pDst0, rowBytes);
                spanSrc.CopyTo(spanDst);
                pSrc0 += this.Stride;
                pDst0 += dataStride;
            }
        }

        public unsafe TImage ApplyCopyFrom(void* pData, int dataStride)
        {
            CopyFrom(pData, dataStride);
            return this;
        }

        /// <summary>
        /// 创建钳边(clamp)图像。
        /// </summary>
        /// <param name="paddingSize">边缘填充的尺寸</param>
        /// <returns>填充边缘后的图像</returns>
        public unsafe TImage CreatePaddingImage(int paddingSize)
        {
            if (paddingSize < 1) throw new ArgumentException("PaddingSize must > 0");

            TImage img = new TImage(Width + paddingSize * 2, Height + paddingSize * 2);
            img.Fill(default(TPixel));//这里效率不高。原本只需要填充四周扩大的部分即可

            img.CopyFrom(this, new Rect(0, 0, this.Width, this.Height), new PointS(paddingSize, paddingSize));

            int width = this.Width;
            int height = this.Height;
            TPixel* start = this.Start;

            // 复制边界像素
            TPixel* dstStart = img.Start + paddingSize;
            int extendWidth = this.Width + paddingSize * 2;
            int extendHeight = this.Height + paddingSize * 2;

            // 复制上方的像素
            for (int y = 0; y < paddingSize; y++)
            {
                TPixel* dstP = dstStart + y * extendWidth;
                TPixel* srcStart = start;
                TPixel* srcEnd = srcStart + width;

                while (srcStart != srcEnd)
                {
                    *dstP = *srcStart;
                    srcStart++;
                    dstP++;
                }
            }

            // 复制下方的像素
            for (int y = height + paddingSize; y < extendHeight; y++)
            {
                TPixel* dstP = dstStart + y * extendWidth;
                TPixel* srcStart = start + (height - 1) * width;
                TPixel* srcEnd = srcStart + width;

                while (srcStart != srcEnd)
                {
                    *dstP = *srcStart;
                    srcStart++;
                    dstP++;
                }
            }

            // 复制左右两侧的像素
            TPixel* dstLine = img.Start + extendWidth * paddingSize;
            TPixel* srcLine = start;
            TPixel p = default(TPixel);
            for (int y = paddingSize; y < height + paddingSize; y++)
            {
                for (int x = 0; x < paddingSize; x++)
                {
                    p = srcLine[0];
                    dstLine[x] = p;
                }

                p = srcLine[width - 1];
                for (int x = width + paddingSize; x < extendWidth; x++)
                {
                    dstLine[x] = p;
                }
                dstLine += extendWidth;
                srcLine += width;
            }

            // 复制四个角落的像素

            // 左上
            p = start[0];
            for (int y = 0; y < paddingSize; y++)
            {
                for (int x = 0; x < paddingSize; x++)
                {
                    img[y, x] = p;
                }
            }

            // 右上
            p = start[width - 1];
            for (int y = 0; y < paddingSize; y++)
            {
                for (int x = width + paddingSize; x < extendWidth; x++)
                {
                    img[y, x] = p;
                }
            }

            // 左下
            p = start[(height - 1) * width];
            for (int y = height + paddingSize; y < extendHeight; y++)
            {
                for (int x = 0; x < paddingSize; x++)
                {
                    img[y, x] = p;
                }
            }

            // 右下
            p = start[height * width - 1];
            for (int y = height + paddingSize; y < extendHeight; y++)
            {
                for (int x = width + paddingSize; x < extendWidth; x++)
                {
                    img[y, x] = p;
                }
            }

            return img;
        }

        /// <summary>
        /// 应用双指数保边平滑算法，这是一种计算速度比较快的保边平滑算法。算法描述：
        /// Philippe Thévenaz, Daniel Sage, and Michael Unser. Bi-Exponential Edge-Preserving Smoother.
        /// IEEE TRANSACTIONS ON IMAGE PROCESSING, VOL. 21, NO. 9, SEPTEMBER 2012
        /// url：http://bigwww.epfl.ch/publications/thevenaz1202.pdf
        /// </summary>
        public unsafe void ApplyBiExponentialEdgePreservingSmoother(double photometricStandardDeviation = 30, double spatialDecay = 0.01)
        {
            // BiExponentialEdgePreservingSmoother 算法是 java 算法的移植
            // java 算法见： http://bigwww.epfl.ch/thevenaz/beeps/

            TChannel* p0 = (TChannel*)this.Start;
            int length = this.Width * this.Height;

            // 对每个channel进行处理
            for (int idxChannel = 0; idxChannel < ChannelCount; idxChannel++)
            {
                double[] data1 = new double[Width * Height];
                double[] data2 = new double[Width * Height];

                for (int i = 0; i < length; i++)
                {
                    double val = p0[i * ChannelCount];
                    data1[i] = data2[i] = val;
                }

                BEEPSHorizontalVertical hv = new BEEPSHorizontalVertical(data1, Width, Height, photometricStandardDeviation, spatialDecay);
                BEEPSVerticalHorizontal vh = new BEEPSVerticalHorizontal(data2, Width, Height, photometricStandardDeviation, spatialDecay);
                hv.run();
                vh.run();

                for (int i = 0; i < length; i++)
                {
                    double val = (data1[i] + data2[i]) * 0.5f;
                    val = Math.Min(TChannel.MaxValue, val);
                    p0[i * ChannelCount] = (TChannel)val;
                }

                p0++;
            }
        }

        #region BiExponentialEdgePreservingSmooth 的具体实现类

        internal class BEEPSGain
        {
            private double[] data;
            private int length;
            private int startIndex;
            private static double mu;

            internal BEEPSGain(double[] data, int startIndex, int length)
            {
                this.data = data;
                this.startIndex = startIndex;
                this.length = length;
            }

            internal static void setup(double spatialContraDecay)
            {
                mu = (1.0 - spatialContraDecay) / (1.0 + spatialContraDecay);
            }

            public void run()
            {
                for (int k = startIndex, K = startIndex + length; (k < K); k++)
                {
                    data[k] *= mu;
                }
            }

        }

        internal class BEEPSHorizontalVertical
        {
            private double photometricStandardDeviation;
            private double spatialDecay;
            private double[] data;
            private int height;
            private int width;

            internal BEEPSHorizontalVertical(double[] data, int width, int height, double photometricStandardDeviation, double spatialDecay)
            {
                this.data = data;
                this.width = width;
                this.height = height;
                this.photometricStandardDeviation = photometricStandardDeviation;
                this.spatialDecay = spatialDecay;
            }

            public void run()
            {
                BEEPSProgressive.setup(photometricStandardDeviation,
                    1.0 - spatialDecay);
                BEEPSGain.setup(1.0 - spatialDecay);
                BEEPSRegressive.setup(photometricStandardDeviation,
                    1.0 - spatialDecay);
                double[] g = new double[width * height];
                for (int k = 0, K = data.Length; (k < K); k++)
                {
                    g[k] = (double)data[k];
                }

                double[] p = new double[height * width];
                double[] r = new double[height * width];

                Array.Copy(g, p, height * width);
                Array.Copy(g, r, height * width);

                for (int k2 = 0; (k2 < height); k2++)
                {
                    BEEPSProgressive progressive = new BEEPSProgressive(p, k2 * width, width);
                    BEEPSGain gain = new BEEPSGain(g, k2 * width, width);
                    BEEPSRegressive regressive = new BEEPSRegressive(r, k2 * width, width);
                    progressive.run();
                    gain.run();
                    regressive.run();
                }

                for (int k = 0, K = data.Length; (k < K); k++)
                {
                    r[k] += p[k] - g[k];
                }
                int m = 0;
                for (int k2 = 0; (k2 < height); k2++)
                {
                    int n = k2;
                    for (int k1 = 0; (k1 < width); k1++)
                    {
                        g[n] = r[m++];
                        n += height;
                    }
                }

                Array.Copy(g, p, height * width);
                Array.Copy(g, r, height * width);
                for (int k1 = 0; (k1 < width); k1++)
                {
                    BEEPSProgressive progressive = new BEEPSProgressive(p, k1 * height, height);
                    BEEPSGain gain = new BEEPSGain(g, k1 * height, height);
                    BEEPSRegressive regressive = new BEEPSRegressive(r, k1 * height, height);
                    progressive.run();
                    gain.run();
                    regressive.run();
                }

                for (int k = 0, K = data.Length; (k < K); k++)
                {
                    r[k] += p[k] - g[k];
                }
                m = 0;
                for (int k1 = 0; (k1 < width); k1++)
                {
                    int n = k1;
                    for (int k2 = 0; (k2 < height); k2++)
                    {
                        data[n] = r[m++];
                        n += width;
                    }
                }
            }
        }

        const double BEEP_ZETA_3 = 1.2020569031595942853997381615114499907649862923404988817922715553418382057;
        internal class BEEPSProgressive
        {
            private double[] data;
            private int length;
            private int startIndex;
            private static double c;
            private static double rho;
            private static double spatialContraDecay;

            internal BEEPSProgressive(double[] data, int startIndex, int length)
            {
                this.data = data;
                this.startIndex = startIndex;
                this.length = length;
            }

            internal static void setup(double photometricStandardDeviation, double sharedSpatialContraDecay)
            {
                spatialContraDecay = sharedSpatialContraDecay;
                rho = 1.0 + spatialContraDecay;
                c = -0.5 / (photometricStandardDeviation * photometricStandardDeviation);
            }

            public void run()
            {
                double mu = 0.0;
                data[startIndex] /= rho;
                for (int k = startIndex + 1, K = startIndex + length;
                               (k < K); k++)
                {
                    mu = data[k] - rho * data[k - 1];
                    mu = spatialContraDecay * Math.Exp(c * mu * mu);
                    data[k] = data[k - 1] * mu + data[k] * (1.0 - mu) / rho;
                }
            }
        }

        internal class BEEPSRegressive
        {
            private double[] data;
            private int length;
            private int startIndex;
            private static double c;
            private static double rho;
            private static double spatialContraDecay;

            internal BEEPSRegressive(double[] data, int startIndex, int length)
            {
                this.data = data;
                this.startIndex = startIndex;
                this.length = length;
            }

            internal static void setup(double photometricStandardDeviation, double sharedSpatialContraDecay)
            {
                spatialContraDecay = sharedSpatialContraDecay;
                rho = 1.0 + spatialContraDecay;
                c = -0.5 / (photometricStandardDeviation * photometricStandardDeviation);
            }

            public void run()
            {
                double mu = 0.0;
                data[startIndex + length - 1] /= rho;
                for (int k = startIndex + length - 2; (startIndex <= k); k--)
                {
                    mu = data[k] - rho * data[k + 1];
                    mu = spatialContraDecay * Math.Exp(c * mu * mu);
                    data[k] = data[k + 1] * mu + data[k] * (1.0 - mu) / rho;
                }
            }
        }

        internal class BEEPSVerticalHorizontal
        {
            private double photometricStandardDeviation;
            private double spatialDecay;
            private double[] data;
            private int height;
            private int width;

            internal BEEPSVerticalHorizontal(double[] data, int width, int height, double photometricStandardDeviation, double spatialDecay)
            {
                this.data = data;
                this.width = width;
                this.height = height;
                this.photometricStandardDeviation = photometricStandardDeviation;
                this.spatialDecay = spatialDecay;
            }

            public void run()
            {
                BEEPSProgressive.setup(photometricStandardDeviation, 1.0 - spatialDecay);
                BEEPSGain.setup(1.0 - spatialDecay);
                BEEPSRegressive.setup(photometricStandardDeviation, 1.0 - spatialDecay);
                double[] g = new double[height * width];
                int m = 0;
                for (int k2 = 0; (k2 < height); k2++)
                {
                    int n = k2;
                    for (int k1 = 0; (k1 < width); k1++)
                    {
                        g[n] = (double)data[m++];
                        n += height;
                    }
                }

                double[] p = new double[height * width];
                double[] r = new double[height * width];

                Array.Copy(g, p, height * width);
                Array.Copy(g, r, height * width);

                for (int k1 = 0; (k1 < width); k1++)
                {
                    BEEPSProgressive progressive = new BEEPSProgressive(p, k1 * height, height);
                    BEEPSGain gain = new BEEPSGain(g, k1 * height, height);
                    BEEPSRegressive regressive = new BEEPSRegressive(r, k1 * height, height);
                    progressive.run();
                    gain.run();
                    regressive.run();
                }

                for (int k = 0, K = data.Length; (k < K); k++)
                {
                    r[k] += p[k] - g[k];
                }
                m = 0;
                for (int k1 = 0; (k1 < width); k1++)
                {
                    int n = k1;
                    for (int k2 = 0; (k2 < height); k2++)
                    {
                        g[n] = r[m++];
                        n += width;
                    }
                }

                Array.Copy(g, p, height * width);
                Array.Copy(g, r, height * width);

                for (int k2 = 0; (k2 < height); k2++)
                {
                    BEEPSProgressive progressive = new BEEPSProgressive(p, k2 * width, width);
                    BEEPSGain gain = new BEEPSGain(g, k2 * width, width);
                    BEEPSRegressive regressive = new BEEPSRegressive(r, k2 * width, width);
                    progressive.run();
                    gain.run();
                    regressive.run();
                }

                for (int k = 0, K = data.Length; (k < K); k++)
                {
                    data[k] = (double)(p[k] - g[k] + r[k]);
                }
            }
        }

        #endregion
    }

    public partial struct RgbS
    {
        public static Boolean operator ==(TPixel lhs, int rhs)
        {
            throw new NotImplementedException();
        }

        public static Boolean operator !=(TPixel lhs, int rhs)
        {
            throw new NotImplementedException();
        }

        public static Boolean operator ==(TPixel lhs, double rhs)
        {
            throw new NotImplementedException();
        }

        public static Boolean operator !=(TPixel lhs, double rhs)
        {
            throw new NotImplementedException();
        }

        public static Boolean operator ==(TPixel lhs, float rhs)
        {
            throw new NotImplementedException();
        }

        public static Boolean operator !=(TPixel lhs, float rhs)
        {
            throw new NotImplementedException();
        }

        public static Boolean operator ==(TPixel lhs, TPixel rhs)
        {
            return lhs.Equals(rhs);
        }
        
        public static Boolean operator !=(TPixel lhs, TPixel rhs)
        {
            return !lhs.Equals(rhs);
        }
    }
}

