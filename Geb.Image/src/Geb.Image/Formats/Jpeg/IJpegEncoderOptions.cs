﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Geb.Image.Formats.Jpeg
{
    /// <summary>
    /// Encoder for writing the data image to a stream in jpeg format.
    /// </summary>
    internal interface IJpegEncoderOptions
    {
        /// <summary>
        /// Gets a value indicating whether the metadata should be ignored when the image is being decoded.
        /// </summary>
        bool IgnoreMetadata { get; }

        /// <summary>
        /// Gets the quality, that will be used to encode the image. Quality
        /// index must be between 0 and 100 (compression from max to min).
        /// </summary>
        /// <value>The quality of the jpg image from 0 to 100.</value>
        int Quality { get; }

        /// <summary>
        /// Gets the subsample ration, that will be used to encode the image.
        /// </summary>
        /// <value>The subsample ratio of the jpg image.</value>
        JpegSubsample? Subsample { get; }
    }
}
