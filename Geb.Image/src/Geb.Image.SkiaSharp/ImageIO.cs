namespace Geb.Image.Skia
{
    using SkiaSharp;
    using BitMiracle.LibTiff.Classic;

    /// <summary>
    /// 图像 IO 库
    /// </summary>
    public static class ImageIO
    {
        #region 读取文件

        public static unsafe ImageBgra32? ReadBgra32(String imageFilePath)
        {
            using (var bmp = Read(imageFilePath))
            {
                return bmp?.ToImageBgra32()??null;
            }
        }

        public static unsafe ImageBgr24? ReadBgr24(String imageFilePath)
        {
            using (var bmp = Read(imageFilePath))
            {
                return bmp?.ToImageBgr24() ?? null;
            }
        }

        public static unsafe ImageU8? ReadU8(String imageFilePath)
        {
            using (var bmp = Read(imageFilePath))
            {
                return bmp?.ToImageU8() ?? null;
            }
        }

        public static unsafe ImageBgra32? ReadBgra32(Stream stream)
        {
            using (var bmp = Read(stream))
            {
                return bmp?.ToImageBgra32() ?? null;
            }
        }

        public static unsafe ImageBgr24? ReadBgr24(Stream stream)
        {
            using (var bmp = Read(stream))
            {
                return bmp?.ToImageBgr24() ?? null;
            }
        }

        public static unsafe ImageU8? ReadU8(Stream stream)
        {
            using (var bmp = Read(stream))
            {
                return bmp?.ToImageU8() ?? null;
            }
        }

        public static SKBitmap Read(String imageFilePath, SKColorType colorType)
        {
            SKBitmap bmp = Read(imageFilePath);
            return bmp.CheckOrConvertTo(colorType, true);
        }

        public static SKBitmap Read(Stream stream, SKColorType colorType)
        {
            SKBitmap bmp = Read(stream);
            return bmp.CheckOrConvertTo(colorType, true);
        }

        public static SKBitmap Read(String imageFilePath)
        {
            SKBitmap bmp = null;
            using (FileStream fs = new FileStream(imageFilePath, FileMode.Open))
            {
                bmp = SKBitmap.Decode(fs);
            }
            return bmp;
        }

        public static SKBitmap Read(Stream stream)
        {
            return SKBitmap.Decode(stream);
        }

        #endregion

        #region 文件转换

        /// <summary>
        /// 检查图像的颜色类型或转换到对应的颜色类型。如果该图像的颜色类型正确，则直接返回该图像。否则，基
        /// 于该图像的数据，采用新的颜色类型，生成新图像返回。
        /// </summary>
        /// <param name="bitmap">输入 bitmap</param>
        /// <param name="colorType">需要确认的颜色类型</param>
        /// <param name="disposeOldBitmap">如果进行了图像转换，则将旧图像 dispose 掉</param>
        /// <returns>具备对应颜色类型的图像</returns>
        /// <exception cref="SkException"></exception>
        public static SKBitmap CheckOrConvertTo(this SKBitmap bitmap, SKColorType colorType, bool disposeOldBitmap = false)
        {
            if (bitmap == null) return null;
            else if (bitmap.ColorType == colorType) return bitmap;
            SKAlphaType dstAlphaType = bitmap.AlphaType;
            if (colorType == SKColorType.Gray8) dstAlphaType = SKAlphaType.Unknown;
            var newBmp = new SKBitmap(bitmap.Width, bitmap.Height, colorType, dstAlphaType);
            using (SKCanvas canvas = new SKCanvas(newBmp))
            {
                canvas.DrawBitmap(bitmap, new SKPoint(0, 0));
            }
            if (disposeOldBitmap == true) bitmap.Dispose();
            return newBmp;
        }

        /// <summary>
        /// 检查图像的颜色类型和Alpha类型或转换到对应的颜色类型和Alpha类型。如果该图像的颜色类型和Alpha类型正确，则直接返回该图像。否则，基
        /// 于该图像的数据，采用新的颜色类型和Alpha类型，生成新图像返回。
        /// </summary>
        /// <param name="bitmap">输入 bitmap</param>
        /// <param name="colorType">需要确认的颜色类型</param>
        /// <param name="alphaType">需要确认的Alpha类型</param>
        /// <param name="disposeOldBitmap">如果进行了图像转换，则将旧图像 dispose 掉</param>
        /// <returns>具备对应颜色类型和Alpha类型的图像</returns>
        /// <exception cref="SkException"></exception>
        public static SKBitmap CheckOrConvertTo(this SKBitmap bitmap, SKColorType colorType, SKAlphaType alphaType, bool disposeOldBitmap = false)
        {
            if (bitmap == null) return null;
            else if (bitmap.AlphaType == alphaType && bitmap.ColorType == colorType) return bitmap;
            var newBmp = new SKBitmap(bitmap.Width, bitmap.Height, colorType, alphaType);
            using (SKCanvas canvas = new SKCanvas(newBmp))
            {
                canvas.DrawBitmap(bitmap, new SKPoint(0, 0));
            }
            if (disposeOldBitmap == true) bitmap.Dispose();
            return newBmp;
        }


        /// <summary>
        /// 转换为 ImageBgra32
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static unsafe ImageBgra32 ToImageBgra32(this SKBitmap bitmap)
        {
            var cvtBmp = CheckOrConvertTo(bitmap, SKColorType.Bgra8888,SKAlphaType.Unpremul);
            if (cvtBmp == null) return null;

            ImageBgra32 image = new ImageBgra32(bitmap.Width, bitmap.Height);
            void* Data = (void*)cvtBmp.GetPixels();
            image.CopyFrom(Data, cvtBmp.RowBytes);
            if (cvtBmp != bitmap) cvtBmp.Dispose();
            return image;
        }

        /// <summary>
        /// 转换为 ImageBgr24
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static unsafe ImageBgr24 ToImageBgr24(this SKBitmap bitmap)
        {
            var cvtBmp = CheckOrConvertTo(bitmap, SKColorType.Bgra8888, SKAlphaType.Unpremul);
            if (cvtBmp == null) return null;

            ImageBgr24 image = new ImageBgr24(bitmap.Width, bitmap.Height);
            byte* Data = (byte*)cvtBmp.GetPixels();
            Bgr24* pDst0 = image.Start;
            for(int h = 0; h < image.Height; h++)
            {
                byte* pByte = Data + h * cvtBmp.RowBytes;
                Bgr24* pBgr24 = pDst0 + h * image.Width;
                Bgr24* pBgr24End = pBgr24 + image.Width;
                while(pBgr24 < pBgr24End)
                {
                    *pBgr24 = *(Bgr24*)pByte;
                    pByte+=4;
                    pBgr24++;
                }
            }
            if (cvtBmp != bitmap) cvtBmp.Dispose();
            return image;
        }

        /// <summary>
        /// 转换为 ImageU8
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static unsafe ImageU8 ToImageU8(this SKBitmap bitmap)
        {
            var cvtBmp = CheckOrConvertTo(bitmap, SKColorType.Gray8);
            if (cvtBmp == null) return null;

            ImageU8 image = new ImageU8(bitmap.Width, bitmap.Height);
            void* Data = (void*)cvtBmp.GetPixels();
            image.CopyFrom(Data, cvtBmp.RowBytes);
            if (cvtBmp != bitmap) cvtBmp.Dispose();
            return image;
        }

        /// <summary>
        /// 转换为 SKBitmap
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static unsafe SKBitmap ToBitmap(this ImageU8 image)
        {
            if (image == null) return null;
            SKBitmap bmp = new SKBitmap(image.Width, image.Height, SKColorType.Gray8, SKAlphaType.Unknown);
            void* Data = (void*)bmp.GetPixels();
            image.CopyTo(Data, bmp.RowBytes);
            return bmp;
        }

        /// <summary>
        /// 转换为 SKBitmap
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static unsafe SKBitmap ToBitmap(this ImageBgra32 image)
        {
            if (image == null) return null;
            SKBitmap bmp = new SKBitmap(image.Width, image.Height, SKColorType.Bgra8888, SKAlphaType.Opaque);
            void* Data = (void*)bmp.GetPixels();
            image.CopyTo(Data, bmp.RowBytes);
            return bmp;
        }

        #endregion

        #region DecodeTiff
        public static ImageBgra32 ReadTiff(String imageFilePath)
        {
            if (imageFilePath.EndsWith(".tiff") || imageFilePath.EndsWith(".tif")) // tif 文件
            {
                var bytes = File.ReadAllBytes(imageFilePath);
                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    ImageBgra32 imImff = ReadTiff(ms);
                    return imImff;
                }
            }
            return null;
        }

        public static ImageBgra32 ReadTiff(MemoryStream tifImage)
        {
            int width, height;
            // open a Tiff stored in the memory stream, and grab its pixels
            using (Tiff tifImg = Tiff.ClientOpen("in-memory", "r", tifImage, new TiffStream()))
            {
                FieldValue[] value = tifImg.GetField(TiffTag.IMAGEWIDTH);
                width = value[0].ToInt();

                value = tifImg.GetField(TiffTag.IMAGELENGTH);
                height = value[0].ToInt();

                // Read the image into the memory buffer 
                int[] raster = new int[width * height];
                if (!tifImg.ReadRGBAImageOriented(width, height, raster, Orientation.TOPLEFT))
                {
                    return null;
                }

                ImageBgra32 im = new ImageBgra32(width, height);
                Span<Bgra32> span = im.Span;
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int arrayOffset = y * width + x;
                        int rgba = raster[arrayOffset];
                        Bgra32 c = new Bgra32((byte)Tiff.GetB(rgba), (byte)Tiff.GetG(rgba), (byte)Tiff.GetR(rgba), (byte)Tiff.GetA(rgba));
                        span[arrayOffset] = c;
                    }
                }
                return im;
            }
        }
        #endregion
    }
}