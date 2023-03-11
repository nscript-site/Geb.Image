﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Geb.Image.Formats
{
    /// <summary>
    /// Describes an image format.
    /// </summary>
    public interface IImageFormat
    {
        /// <summary>
        /// Gets the name that describes this image format.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the default mimetype that the image foramt uses
        /// </summary>
        string DefaultMimeType { get; }

        /// <summary>
        /// Gets all the mimetypes that have been used by this image foramt.
        /// </summary>
        IEnumerable<string> MimeTypes { get; }

        /// <summary>
        /// Gets the file extensions this image format commonly uses.
        /// </summary>
        IEnumerable<string> FileExtensions { get; }
    }
}
