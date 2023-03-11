// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Linq;
using System.Numerics;

using JpegColorConverter = Geb.Image.Formats.Jpeg.Components.Decoder.ColorConverters.JpegColorConverter;

namespace Geb.Image.Formats.Jpeg.Components.Decoder
{
    /// <summary>
    /// Encapsulates the execution od post-processing algorithms to be applied on a <see cref="IRawJpegData"/> to produce a valid <see cref="Image{TPixel}"/>: <br/>
    /// (1) Dequantization <br/>
    /// (2) IDCT <br/>
    /// (3) Color conversion form one of the <see cref="JpegColorSpace"/>-s into a <see cref="Vector4"/> buffer of RGBA values <br/>
    /// (4) Packing <see cref="Image{TPixel}"/> pixels from the <see cref="Vector4"/> buffer. <br/>
    /// These operations are executed in <see cref="NumberOfPostProcessorSteps"/> steps.
    /// <see cref="PixelRowsPerStep"/> image rows are converted in one step,
    /// which means that size of the allocated memory is limited (does not depend on <see cref="ImageFrame{TPixel}.Height"/>).
    /// </summary>
    internal class JpegImagePostProcessor : IDisposable
    {
        /// <summary>
        /// The number of block rows to be processed in one Step.
        /// </summary>
        public const int BlockRowsPerStep = 4;

        /// <summary>
        /// The number of image pixel rows to be processed in one step.
        /// </summary>
        public const int PixelRowsPerStep = 4 * 8;

        /// <summary>
        /// Temporal buffer to store a row of colors.
        /// </summary>
        private readonly IBuffer<Vector4> rgbaBuffer;

        /// <summary>
        /// The <see cref="JpegColorConverter"/> corresponding to the current <see cref="JpegColorSpace"/> determined by <see cref="IRawJpegData.ColorSpace"/>.
        /// </summary>
        private readonly JpegColorConverter colorConverter;

        /// <summary>
        /// Initializes a new instance of the <see cref="JpegImagePostProcessor"/> class.
        /// </summary>
        /// <param name="memoryManager">The <see cref="MemoryManager"/> to use for buffer allocations.</param>
        /// <param name="rawJpeg">The <see cref="IRawJpegData"/> representing the uncompressed spectral Jpeg data</param>
        public JpegImagePostProcessor(MemoryManager memoryManager, IRawJpegData rawJpeg)
        {
            this.RawJpeg = rawJpeg;
            IJpegComponent c0 = rawJpeg.Components.First();
            this.NumberOfPostProcessorSteps = c0.SizeInBlocks.Height / BlockRowsPerStep;
            this.PostProcessorBufferSize = new Size(c0.SizeInBlocks.Width * 8, PixelRowsPerStep);

            this.ComponentProcessors = rawJpeg.Components.Select(c => new JpegComponentPostProcessor(memoryManager, this, c)).ToArray();
            this.rgbaBuffer = memoryManager.Allocate<Vector4>(rawJpeg.ImageSizeInPixels.Width);
            this.colorConverter = JpegColorConverter.GetConverter(rawJpeg.ColorSpace);
        }

        /// <summary>
        /// Gets the <see cref="JpegComponentPostProcessor"/> instances.
        /// </summary>
        public JpegComponentPostProcessor[] ComponentProcessors { get; }

        /// <summary>
        /// Gets the <see cref="IRawJpegData"/> to be processed.
        /// </summary>
        public IRawJpegData RawJpeg { get; }

        /// <summary>
        /// Gets the total number of post processor steps deduced from the height of the image and <see cref="PixelRowsPerStep"/>.
        /// </summary>
        public int NumberOfPostProcessorSteps { get; }

        /// <summary>
        /// Gets the size of the temporary buffers we need to allocate into <see cref="JpegComponentPostProcessor.ColorBuffer"/>.
        /// </summary>
        public Size PostProcessorBufferSize { get; }

        /// <summary>
        /// Gets the value of the counter that grows by each step by <see cref="PixelRowsPerStep"/>.
        /// </summary>
        public int PixelRowCounter { get; private set; }

        /// <inheritdoc />
        public void Dispose()
        {
            foreach (JpegComponentPostProcessor cpp in this.ComponentProcessors)
            {
                cpp.Dispose();
            }

            this.rgbaBuffer.Dispose();
        }

        /// <summary>
        /// Process all pixels into 'destination'. The image dimensions should match <see cref="RawJpeg"/>.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type</typeparam>
        /// <param name="destination">The destination image</param>
        public void PostProcess(ImageBgra32 destination)
        {
            this.PixelRowCounter = 0;

            if (this.RawJpeg.ImageSizeInPixels != destination.Size)
            {
                throw new ArgumentException("Input image is not of the size of the processed one!");
            }

            while (this.PixelRowCounter < this.RawJpeg.ImageSizeInPixels.Height)
            {
                this.DoPostProcessorStep(destination);
            }
        }

        /// <summary>
        /// Execute one step processing <see cref="PixelRowsPerStep"/> pixel rows into 'destination'.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type</typeparam>
        /// <param name="destination">The destination image.</param>
        public void DoPostProcessorStep(ImageBgra32 destination)
        {
            foreach (JpegComponentPostProcessor cpp in this.ComponentProcessors)
            {
                cpp.CopyBlocksToColorBuffer();
            }

            this.ConvertColorsInto(destination);

            this.PixelRowCounter += PixelRowsPerStep;
        }

        /// <summary>
        /// Convert and copy <see cref="PixelRowsPerStep"/> row of colors into 'destination' starting at row <see cref="PixelRowCounter"/>.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type</typeparam>
        /// <param name="destination">The destination image</param>
        private void ConvertColorsInto(ImageBgra32 destination)
        {
            int maxY = Math.Min(destination.Height, this.PixelRowCounter + PixelRowsPerStep);

            Buffer2D<float>[] buffers = this.ComponentProcessors.Select(cp => cp.ColorBuffer).ToArray();

            for (int yy = this.PixelRowCounter; yy < maxY; yy++)
            {
                int y = yy - this.PixelRowCounter;

                var values = new JpegColorConverter.ComponentValues(buffers, y);
                this.colorConverter.ConvertToRgba(values, this.rgbaBuffer.Span);
                var span = this.rgbaBuffer.Span;
                for(int w = 0; w < destination.Width; w++)
                {
                    Bgra32 c = new Bgra32();
                    var v = span[w];
                    c.Red = (byte)Math.Max(0,Math.Min(255,(v.X * 255)));
                    c.Green = (byte)Math.Max(0, Math.Min(255, (v.Y * 255)));
                    c.Blue = (byte)Math.Max(0, Math.Min(255, (v.Z * 255)));
                    c.Alpha = 255;
                    destination[yy, w] = c;
                }
            }
        }
    }
}