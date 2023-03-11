﻿// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

namespace Geb.Image.Formats.Gif
{
    /// <summary>
    /// Registers the image encoders, decoders and mime type detectors for the gif format.
    /// </summary>
    public sealed class GifConfigurationModule : IConfigurationModule
    {
        /// <inheritdoc/>
        public void Configure(Configuration config)
        {
            //config.ImageFormatsManager.SetEncoder(ImageFormats.Gif, new GifEncoder());
            //config.ImageFormatsManager.SetDecoder(ImageFormats.Gif, new GifDecoder());

            //config.ImageFormatsManager.AddImageFormatDetector(new GifImageFormatDetector());
        }
    }
}