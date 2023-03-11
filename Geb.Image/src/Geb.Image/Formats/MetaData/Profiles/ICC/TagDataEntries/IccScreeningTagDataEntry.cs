﻿// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Linq;

namespace Geb.Image.Formats.MetaData.Profiles.Icc
{
    /// <summary>
    /// This type describes various screening parameters including
    /// screen frequency, screening angle, and spot shape.
    /// </summary>
    internal sealed class IccScreeningTagDataEntry : IccTagDataEntry, IEquatable<IccScreeningTagDataEntry>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IccScreeningTagDataEntry"/> class.
        /// </summary>
        /// <param name="flags">Screening flags</param>
        /// <param name="channels">Channel information</param>
        public IccScreeningTagDataEntry(IccScreeningFlag flags, IccScreeningChannel[] channels)
            : this(flags, channels, IccProfileTag.Unknown)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IccScreeningTagDataEntry"/> class.
        /// </summary>
        /// <param name="flags">Screening flags</param>
        /// <param name="channels">Channel information</param>
        /// <param name="tagSignature">Tag Signature</param>
        public IccScreeningTagDataEntry(IccScreeningFlag flags, IccScreeningChannel[] channels, IccProfileTag tagSignature)
            : base(IccTypeSignature.Screening, tagSignature)
        {
            Guard.NotNull(channels, nameof(channels));

            this.Flags = flags;
            this.Channels = channels;
        }

        /// <summary>
        /// Gets the screening flags
        /// </summary>
        public IccScreeningFlag Flags { get; }

        /// <summary>
        /// Gets the channel information
        /// </summary>
        public IccScreeningChannel[] Channels { get; }

        /// <inheritdoc/>
        public override bool Equals(IccTagDataEntry other)
        {
            return other is IccScreeningTagDataEntry entry && this.Equals(entry);
        }

        /// <inheritdoc />
        public bool Equals(IccScreeningTagDataEntry other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return base.Equals(other)
                && this.Flags == other.Flags
                && this.Channels.SequenceEqual(other.Channels);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj is IccScreeningTagDataEntry other && this.Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)this.Flags;
                hashCode = (hashCode * 397) ^ (this.Channels?.GetHashCode() ?? 0);
                return hashCode;
            }
        }
    }
}
