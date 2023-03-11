﻿// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Linq;

namespace Geb.Image.Formats.MetaData.Profiles.Icc
{
    /// <summary>
    /// This tag structure contains a set of records each referencing
    /// a multilingual string associated with a profile.
    /// </summary>
    internal sealed class IccMultiLocalizedUnicodeTagDataEntry : IccTagDataEntry, IEquatable<IccMultiLocalizedUnicodeTagDataEntry>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IccMultiLocalizedUnicodeTagDataEntry"/> class.
        /// </summary>
        /// <param name="texts">Localized Text</param>
        public IccMultiLocalizedUnicodeTagDataEntry(IccLocalizedString[] texts)
            : this(texts, IccProfileTag.Unknown)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IccMultiLocalizedUnicodeTagDataEntry"/> class.
        /// </summary>
        /// <param name="texts">Localized Text</param>
        /// <param name="tagSignature">Tag Signature</param>
        public IccMultiLocalizedUnicodeTagDataEntry(IccLocalizedString[] texts, IccProfileTag tagSignature)
            : base(IccTypeSignature.MultiLocalizedUnicode, tagSignature)
        {
            Guard.NotNull(texts, nameof(texts));
            this.Texts = texts;
        }

        /// <summary>
        /// Gets the localized texts
        /// </summary>
        public IccLocalizedString[] Texts { get; }

        /// <inheritdoc/>
        public override bool Equals(IccTagDataEntry other)
        {
            return other is IccMultiLocalizedUnicodeTagDataEntry entry && this.Equals(entry);
        }

        /// <inheritdoc/>
        public bool Equals(IccMultiLocalizedUnicodeTagDataEntry other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return base.Equals(other) && this.Texts.SequenceEqual(other.Texts);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj is IccMultiLocalizedUnicodeTagDataEntry && this.Equals((IccMultiLocalizedUnicodeTagDataEntry)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (this.Texts?.GetHashCode() ?? 0);
            }
        }
    }
}