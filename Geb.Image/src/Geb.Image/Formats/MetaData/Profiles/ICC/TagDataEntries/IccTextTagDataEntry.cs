﻿// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;

namespace Geb.Image.Formats.MetaData.Profiles.Icc
{
    /// <summary>
    /// This is a simple text structure that contains a text string.
    /// </summary>
    internal sealed class IccTextTagDataEntry : IccTagDataEntry, IEquatable<IccTextTagDataEntry>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IccTextTagDataEntry"/> class.
        /// </summary>
        /// <param name="text">The Text</param>
        public IccTextTagDataEntry(string text)
            : this(text, IccProfileTag.Unknown)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IccTextTagDataEntry"/> class.
        /// </summary>
        /// <param name="text">The Text</param>
        /// <param name="tagSignature">Tag Signature</param>
        public IccTextTagDataEntry(string text, IccProfileTag tagSignature)
            : base(IccTypeSignature.Text, tagSignature)
        {
            Guard.NotNull(text, nameof(text));
            this.Text = text;
        }

        /// <summary>
        /// Gets the Text
        /// </summary>
        public string Text { get; }

        /// <inheritdoc/>
        public override bool Equals(IccTagDataEntry other)
        {
            return other is IccTextTagDataEntry entry && this.Equals(entry);
        }

        /// <inheritdoc />
        public bool Equals(IccTextTagDataEntry other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return base.Equals(other) && string.Equals(this.Text, other.Text);
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

            return obj is IccTextTagDataEntry other && this.Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (this.Text?.GetHashCode() ?? 0);
            }
        }
    }
}