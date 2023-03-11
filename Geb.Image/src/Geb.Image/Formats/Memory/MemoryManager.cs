﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Geb.Image.Formats
{
    /// <summary>
    /// Memory managers are used to allocate memory for image processing operations.
    /// </summary>
    public abstract class MemoryManager
    {
        /// <summary>
        /// Allocates an <see cref="IBuffer{T}"/> of size <paramref name="length"/>, optionally
        /// clearing the buffer before it gets returned.
        /// </summary>
        /// <typeparam name="T">Type of the data stored in the buffer</typeparam>
        /// <param name="length">Size of the buffer to allocate</param>
        /// <param name="clear">True to clear the backing memory of the buffer</param>
        /// <returns>A buffer of values of type <typeparamref name="T"/>.</returns>
        internal abstract IBuffer<T> Allocate<T>(int length, bool clear)
            where T : struct;

        /// <summary>
        /// Allocates an <see cref="IManagedByteBuffer"/>
        /// </summary>
        /// <param name="length">The requested buffer length</param>
        /// <param name="clear">A value indicating whether to clean the buffer</param>
        /// <returns>The <see cref="IManagedByteBuffer"/></returns>
        internal abstract IManagedByteBuffer AllocateManagedByteBuffer(int length, bool clear);

        /// <summary>
        /// Temporal workaround. A method providing a "Buffer" based on a generic array without the 'Unsafe.As()' hackery.
        /// Should be replaced with 'Allocate()' as soon as SixLabors.Shapes has Span-based API-s!
        /// </summary>
        internal BasicArrayBuffer<T> AllocateFake<T>(int length, bool dummy = false)
            where T : struct
        {
            return new BasicArrayBuffer<T>(new T[length]);
        }

        /// <summary>
        /// Releases all retained resources not being in use.
        /// Eg: by resetting array pools and letting GC to free the arrays.
        /// </summary>
        public virtual void ReleaseRetainedResources()
        {
        }
    }
}
