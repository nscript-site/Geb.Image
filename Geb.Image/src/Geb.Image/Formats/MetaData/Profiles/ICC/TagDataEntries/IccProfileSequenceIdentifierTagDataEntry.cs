﻿// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Linq;

namespace Geb.Image.Formats.MetaData.Profiles.Icc
{
    /// <summary>
    /// This type is an array of structures, each of which contains information
    /// for identification of a profile used in a sequence.
    /// </summary>
    internal sealed class IccProfileSequenceIdentifierTagDataEntry : IccTagDataEntry, IEquatable<IccProfileSequenceIdentifierTagDataEntry>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IccProfileSequenceIdentifierTagDataEntry"/> class.
        /// </summary>
        /// <param name="data">Profile Identifiers</param>
        public IccProfileSequenceIdentifierTagDataEntry(IccProfileSequenceIdentifier[] data)
            : this(data, IccProfileTag.Unknown)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IccProfileSequenceIdentifierTagDataEntry"/> class.
        /// </summary>
        /// <param name="data">Profile Identifiers</param>
        /// <param name="tagSignature">Tag Signature</param>
        public IccProfileSequenceIdentifierTagDataEntry(IccProfileSequenceIdentifier[] data, IccProfileTag tagSignature)
            : base(IccTypeSignature.ProfileSequenceIdentifier, tagSignature)
        {
            Guard.NotNull(data, nameof(data));
            this.Data = data;
        }

        /// <summary>
        /// Gets the profile identifiers
        /// </summary>
        public IccProfileSequenceIdentifier[] Data { get; }

        /// <inheritdoc/>
        public override bool Equals(IccTagDataEntry other)
        {
            return other is IccProfileSequenceIdentifierTagDataEntry entry && this.Equals(entry);
        }

        /// <inheritdoc />
        public bool Equals(IccProfileSequenceIdentifierTagDataEntry other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return base.Equals(other) && this.Data.SequenceEqual(other.Data);
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

            return obj is IccProfileSequenceIdentifierTagDataEntry other && this.Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (this.Data?.GetHashCode() ?? 0);
            }
        }
    }
}
