﻿using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Geb.Image.Formats
{
    internal static class BufferExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Length<T>(this IBuffer<T> buffer)
            where T : struct => buffer.Span.Length;

        /// <summary>
        /// Gets a <see cref="Span{T}"/> to an offseted position inside the buffer.
        /// </summary>
        /// <param name="buffer">The buffer</param>
        /// <param name="start">The start</param>
        /// <returns>The <see cref="Span{T}"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> Slice<T>(this IBuffer<T> buffer, int start)
            where T : struct
        {
            return buffer.Span.Slice(start);
        }

        /// <summary>
        /// Gets a <see cref="Span{T}"/> to an offsetted position inside the buffer.
        /// </summary>
        /// <param name="buffer">The buffer</param>
        /// <param name="start">The start</param>
        /// <param name="length">The length of the slice</param>
        /// <returns>The <see cref="Span{T}"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> Slice<T>(this IBuffer<T> buffer, int start, int length)
            where T : struct
        {
            return buffer.Span.Slice(start, length);
        }

        /// <summary>
        /// Clears the contents of this buffer.
        /// </summary>
        /// <param name="buffer">The buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Clear<T>(this IBuffer<T> buffer)
            where T : struct
        {
            buffer.Span.Clear();
        }

        public static ref T DangerousGetPinnableReference<T>(this IBuffer<T> buffer)
            where T : struct =>
            ref MemoryMarshal.GetReference(buffer.Span);

        public static void Read(this Stream stream, IManagedByteBuffer buffer)
        {
            stream.Read(buffer.Array, 0, buffer.Length());
        }

        public static void Write(this Stream stream, IManagedByteBuffer buffer)
        {
            stream.Write(buffer.Array, 0, buffer.Length());
        }
    }
}
