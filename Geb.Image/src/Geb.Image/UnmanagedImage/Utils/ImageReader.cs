using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Geb.Image
{
    using Geb.Image.Formats.Jpeg;
    using Geb.Image.Formats.Png;
    using Geb.Image.Formats.Bmp;
    using Geb.Image.Formats;
    using Geb.Image.Formats.Gif;

    public class UnsupportedImageFormatException: Exception
    {
        public UnsupportedImageFormatException(String fileName):base("File's Image Format Unsupported: " + fileName)
        {
        }
    }

    public class ImageReader
    {
        public static ImageBgra32 Read(String imgFilePath)
        {
            return Instance.ReadFile(imgFilePath);
        }

        public static ImageBgra32 Read(Stream stream)
        {
            return Instance.ReadStream(stream);
        }

        public static ImageReader Instance = new ImageReader();

        private JpegDecoder jpegDecoder = new JpegDecoder();
        private PngDecoder pngDecoder = new PngDecoder();
        private BmpDecoder bmpDecoder = new BmpDecoder();
        private GifDecoder gifDecoder = new GifDecoder();
        private List<IImageFormatDetector> formatDetectors;
        private ImageReader() {
            formatDetectors = new List<IImageFormatDetector>();
            formatDetectors.Add(new JpegImageFormatDetector());
            formatDetectors.Add(new PngImageFormatDetector());
            formatDetectors.Add(new BmpImageFormatDetector());
            formatDetectors.Add(new GifImageFormatDetector());
        }

        private IImageFormat DetectFormat(Stream stream)
        {
            stream.Position = 0;
            int headerLength = (int)Math.Min(stream.Length, 1024);
            Byte[] buff = new byte[headerLength];
            stream.Read(buff, 0, headerLength);
            stream.Position = 0;
            ReadOnlySpan<Byte> span = new ReadOnlySpan<byte>(buff);
            foreach(var item in formatDetectors)
            {
                IImageFormat fmt = item.DetectFormat(buff);
                if (fmt != null) return fmt; 
            }
            return null;
        }

        public ImageBgra32 ReadFile(String imgFilePath)
        {
            using(Stream stream = new FileStream(imgFilePath, FileMode.Open))
            {
                IImageFormat fmt = DetectFormat(stream);
                if (fmt is JpegFormat)
                    return jpegDecoder.Decode(stream);
                else if (fmt is PngFormat)
                    return pngDecoder.Decode(stream);
                else if (fmt is BmpFormat)
                    return bmpDecoder.Decode(stream);
            }

            throw new UnsupportedImageFormatException(imgFilePath);

            return null;
        }

        public ImageBgra32 ReadStream(Stream stream)
        {
            IImageFormat fmt = DetectFormat(stream);
            if (fmt is JpegFormat)
                return jpegDecoder.Decode(stream);
            else if (fmt is PngFormat)
                return pngDecoder.Decode(stream);
            else if (fmt is BmpFormat)
                return bmpDecoder.Decode(stream);

            throw new UnsupportedImageFormatException("stream");

            return null;
        }
    }
}
