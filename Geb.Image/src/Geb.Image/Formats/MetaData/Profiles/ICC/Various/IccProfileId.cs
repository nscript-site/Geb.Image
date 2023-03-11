﻿// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;

namespace Geb.Image.Formats.MetaData.Profiles.Icc
{
    /// <summary>
    /// ICC Profile ID
    /// </summary>
    public readonly struct IccProfileId : IEquatable<IccProfileId>
    {
        /// <summary>
        /// A profile ID with all values set to zero
        /// </summary>
        public static readonly IccProfileId Zero = new IccProfileId(0, 0, 0, 0);

        /// <summary>
        /// Initializes a new instance of the <see cref="IccProfileId"/> struct.
        /// </summary>
        /// <param name="p1">Part 1 of the ID</param>
        /// <param name="p2">Part 2 of the ID</param>
        /// <param name="p3">Part 3 of the ID</param>
        /// <param name="p4">Part 4 of the ID</param>
        public IccProfileId(uint p1, uint p2, uint p3, uint p4)
        {
            this.Part1 = p1;
            this.Part2 = p2;
            this.Part3 = p3;
            this.Part4 = p4;
        }

        /// <summary>
        /// Gets the first part of the ID
        /// </summary>
        public uint Part1 { get; }

        /// <summary>
        /// Gets the second part of the ID
        /// </summary>
        public uint Part2 { get; }

        /// <summary>
        /// Gets the third part of the ID
        /// </summary>
        public uint Part3 { get; }

        /// <summary>
        /// Gets the fourth part of the ID
        /// </summary>
        public uint Part4 { get; }

        /// <summary>
        /// Gets a value indicating whether the ID is set or just consists of zeros
        /// </summary>
        public bool IsSet
        {
            get
            {
                return this.Part1 != 0
                    && this.Part2 != 0
                    && this.Part3 != 0
                    && this.Part4 != 0;
            }
        }

        /// <summary>
        /// Compares two <see cref="IccProfileId"/> objects for equality.
        /// </summary>
        /// <param name="left">
        /// The <see cref="IccProfileId"/> on the left side of the operand.
        /// </param>
        /// <param name="right">
        /// The <see cref="IccProfileId"/> on the right side of the operand.
        /// </param>
        /// <returns>
        /// True if the <paramref name="left"/> parameter is equal to the <paramref name="right"/> parameter; otherwise, false.
        /// </returns>
        public static bool operator ==(IccProfileId left, IccProfileId right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Compares two <see cref="IccProfileId"/> objects for equality.
        /// </summary>
        /// <param name="left">The <see cref="IccProfileId"/> on the left side of the operand.</param>
        /// <param name="right">The <see cref="IccProfileId"/> on the right side of the operand.</param>
        /// <returns>
        /// True if the <paramref name="left"/> parameter is not equal to the <paramref name="right"/> parameter; otherwise, false.
        /// </returns>
        public static bool operator !=(IccProfileId left, IccProfileId right)
        {
            return !left.Equals(right);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is IccProfileId other && this.Equals(other);
        }

        /// <inheritdoc/>
        public bool Equals(IccProfileId other) =>
            this.Part1 == other.Part1 &&
            this.Part2 == other.Part2 &&
            this.Part3 == other.Part3 &&
            this.Part4 == other.Part4;

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = this.Part1.GetHashCode();
                hashCode = (hashCode * 397) ^ this.Part2.GetHashCode();
                hashCode = (hashCode * 397) ^ this.Part3.GetHashCode();
                hashCode = (hashCode * 397) ^ this.Part4.GetHashCode();
                return hashCode;
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{ToHex(this.Part1)}-{ToHex(this.Part2)}-{ToHex(this.Part3)}-{ToHex(this.Part4)}";
        }

        private static string ToHex(uint value)
        {
            return value.ToString("X").PadLeft(8, '0');
        }
    }
}
