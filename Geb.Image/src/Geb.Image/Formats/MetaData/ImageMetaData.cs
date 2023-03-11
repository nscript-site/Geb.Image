﻿// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;
using Geb.Image.Formats.MetaData.Profiles.Exif;
using Geb.Image.Formats.MetaData.Profiles.Icc;

namespace Geb.Image.Formats.MetaData
{
    /// <summary>
    /// Encapsulates the metadata of an image.
    /// </summary>
    public sealed class ImageMetaData
    {
        /// <summary>
        /// The default horizontal resolution value (dots per inch) in x direction.
        /// <remarks>The default value is 96 dots per inch.</remarks>
        /// </summary>
        public const double DefaultHorizontalResolution = 96;

        /// <summary>
        /// The default vertical resolution value (dots per inch) in y direction.
        /// <remarks>The default value is 96 dots per inch.</remarks>
        /// </summary>
        public const double DefaultVerticalResolution = 96;

        private double horizontalResolution;
        private double verticalResolution;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageMetaData"/> class.
        /// </summary>
        internal ImageMetaData()
        {
            this.horizontalResolution = DefaultHorizontalResolution;
            this.verticalResolution = DefaultVerticalResolution;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageMetaData"/> class
        /// by making a copy from other metadata.
        /// </summary>
        /// <param name="other">
        /// The other <see cref="ImageMetaData"/> to create this instance from.
        /// </param>
        private ImageMetaData(ImageMetaData other)
        {
            this.HorizontalResolution = other.HorizontalResolution;
            this.VerticalResolution = other.VerticalResolution;
            this.RepeatCount = other.RepeatCount;

            foreach (ImageProperty property in other.Properties)
            {
                this.Properties.Add(property);
            }

            this.ExifProfile = other.ExifProfile != null
                ? new ExifProfile(other.ExifProfile)
                : null;

            this.IccProfile = other.IccProfile != null
                ? new IccProfile(other.IccProfile)
                : null;
        }

        /// <summary>
        /// Gets or sets the resolution of the image in x- direction.
        /// It is defined as the number of dots per inch and should be an positive value.
        /// </summary>
        /// <value>The density of the image in x- direction.</value>
        public double HorizontalResolution
        {
            get => this.horizontalResolution;

            set
            {
                if (value > 0)
                {
                    this.horizontalResolution = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the resolution of the image in y- direction.
        /// It is defined as the number of dots per inch and should be an positive value.
        /// </summary>
        /// <value>The density of the image in y- direction.</value>
        public double VerticalResolution
        {
            get => this.verticalResolution;

            set
            {
                if (value > 0)
                {
                    this.verticalResolution = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the Exif profile.
        /// </summary>
        public ExifProfile ExifProfile { get; set; }

        /// <summary>
        /// Gets or sets the list of ICC profiles.
        /// </summary>
        public IccProfile IccProfile { get; set; }

        /// <summary>
        /// Gets the list of properties for storing meta information about this image.
        /// </summary>
        public IList<ImageProperty> Properties { get; } = new List<ImageProperty>();

        /// <summary>
        /// Gets or sets the number of times any animation is repeated.
        /// <remarks>0 means to repeat indefinitely.</remarks>
        /// </summary>
        public ushort RepeatCount { get; set; }

        /// <summary>
        /// Looks up a property with the provided name.
        /// </summary>
        /// <param name="name">The name of the property to lookup.</param>
        /// <param name="result">The property, if found, with the provided name.</param>
        /// <returns>Whether the property was found.</returns>
        internal bool TryGetProperty(string name, out ImageProperty result)
        {
            foreach (ImageProperty property in this.Properties)
            {
                if (property.Name == name)
                {
                    result = property;

                    return true;
                }
            }

            result = default;

            return false;
        }

        /// <summary>
        /// Clones this into a new instance
        /// </summary>
        /// <returns>The cloned metadata instance</returns>
        public ImageMetaData Clone()
        {
            return new ImageMetaData(this);
        }

        /// <summary>
        /// Synchronizes the profiles with the current meta data.
        /// </summary>
        internal void SyncProfiles()
        {
            this.ExifProfile?.Sync(this);
        }
    }
}
