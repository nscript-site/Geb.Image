// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Numerics;

namespace Geb.Image.Formats.Jpeg.Components
{
    /// <summary>
    /// Extension methods for <see cref="Size"/>
    /// </summary>
    internal static class SizeExtensions
    {
        /// <summary>
        /// Multiplies 'a.Width' with 'b.Width' and 'a.Height' with 'b.Height'.
        /// TODO: Shouldn't we expose this as operator in SixLabors.Core?
        /// </summary>
        public static Size MultiplyBy(this Size a, Size b) => new Size(a.Width * b.Width, a.Height * b.Height);

        /// <summary>
        /// Divides 'a.Width' with 'b.Width' and 'a.Height' with 'b.Height'.
        /// TODO: Shouldn't we expose this as operator in SixLabors.Core?
        /// </summary>
        public static Size DivideBy(this Size a, Size b) => new Size(a.Width / b.Width, a.Height / b.Height);

        /// <summary>
        /// Divide Width and Height as real numbers and return the Ceiling.
        /// </summary>
        public static Size DivideRoundUp(this Size originalSize, int divX, int divY)
        {
            var sizeVect = new Vector2(originalSize.Width, originalSize.Height);
            sizeVect /= new Vector2(divX, divY);
            sizeVect.X = (float)Math.Ceiling(sizeVect.X);
            sizeVect.Y = (float)Math.Ceiling(sizeVect.Y);
            return new Size((int)sizeVect.X, (int)sizeVect.Y);
        }

        /// <summary>
        /// Divide Width and Height as real numbers and return the Ceiling.
        /// </summary>
        public static Size DivideRoundUp(this Size originalSize, int divisor) =>
            DivideRoundUp(originalSize, divisor, divisor);

        /// <summary>
        /// Divide Width and Height as real numbers and return the Ceiling.
        /// </summary>
        public static Size DivideRoundUp(this Size originalSize, Size divisor) =>
            DivideRoundUp(originalSize, divisor.Width, divisor.Height);
    }
}