﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Geb.Image.Formats
{
    /// <summary>
    /// Extension methods for <see cref="MemoryManager"/>.
    /// </summary>
    internal static class MemoryManagerExtensions
    {
        /// <summary>
        /// Allocates a <see cref="IBuffer{T}"/> of size <paramref name="length"/>.
        /// Note: Depending on the implementation, the buffer may not cleared before
        /// returning, so it may contain data from an earlier use.
        /// </summary>
        /// <typeparam name="T">Type of the data stored in the buffer</typeparam>
        /// <param name="memoryManager">The <see cref="MemoryManager"/></param>
        /// <param name="length">Size of the buffer to allocate</param>
        /// <returns>A buffer of values of type <typeparamref name="T"/>.</returns>
        public static IBuffer<T> Allocate<T>(this MemoryManager memoryManager, int length)
            where T : struct
        {
            return memoryManager.Allocate<T>(length, false);
        }

        public static IBuffer<T> AllocateClean<T>(this MemoryManager memoryManager, int length)
            where T : struct
        {
            return memoryManager.Allocate<T>(length, true);
        }

        public static IManagedByteBuffer AllocateManagedByteBuffer(this MemoryManager memoryManager, int length)
        {
            return memoryManager.AllocateManagedByteBuffer(length, false);
        }

        public static IManagedByteBuffer AllocateCleanManagedByteBuffer(this MemoryManager memoryManager, int length)
        {
            return memoryManager.AllocateManagedByteBuffer(length, true);
        }

        public static Buffer2D<T> Allocate2D<T>(this MemoryManager memoryManager, int width, int height, bool clear)
            where T : struct
        {
            IBuffer<T> buffer = memoryManager.Allocate<T>(width * height, clear);

            return new Buffer2D<T>(buffer, width, height);
        }

        public static Buffer2D<T> Allocate2D<T>(this MemoryManager memoryManager, Size size)
            where T : struct =>
            Allocate2D<T>(memoryManager, size.Width, size.Height, false);

        public static Buffer2D<T> Allocate2D<T>(this MemoryManager memoryManager, int width, int height)
            where T : struct =>
            Allocate2D<T>(memoryManager, width, height, false);

        public static Buffer2D<T> AllocateClean2D<T>(this MemoryManager memoryManager, int width, int height)
            where T : struct =>
            Allocate2D<T>(memoryManager, width, height, true);

        /// <summary>
        /// Allocates padded buffers for BMP encoder/decoder. (Replacing old PixelRow/PixelArea)
        /// </summary>
        /// <param name="memoryManager">The <see cref="MemoryManager"/></param>
        /// <param name="width">Pixel count in the row</param>
        /// <param name="pixelSizeInBytes">The pixel size in bytes, eg. 3 for RGB</param>
        /// <param name="paddingInBytes">The padding</param>
        /// <returns>A <see cref="IManagedByteBuffer"/></returns>
        public static IManagedByteBuffer AllocatePaddedPixelRowBuffer(
            this MemoryManager memoryManager,
            int width,
            int pixelSizeInBytes,
            int paddingInBytes)
        {
            int length = (width * pixelSizeInBytes) + paddingInBytes;
            return memoryManager.AllocateManagedByteBuffer(length);
        }
    }
}
