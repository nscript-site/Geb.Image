﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Geb.Image.Formats
{
    /// <summary>
    /// Provides initialization code which allows extending the library.
    /// </summary>
    public sealed class Configuration
    {
        /// <summary>
        /// A lazily initialized configuration default instance.
        /// </summary>
        private static readonly Lazy<Configuration> Lazy = new Lazy<Configuration>(CreateDefaultInstance);

        /// <summary>
        /// Initializes a new instance of the <see cref="Configuration" /> class.
        /// </summary>
        public Configuration()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Configuration" /> class.
        /// </summary>
        /// <param name="configurationModules">A collection of configuration modules to register</param>
        public Configuration(params IConfigurationModule[] configurationModules)
        {
            if (configurationModules != null)
            {
                foreach (IConfigurationModule p in configurationModules)
                {
                    p.Configure(this);
                }
            }
        }

        /// <summary>
        /// Gets the default <see cref="Configuration"/> instance.
        /// </summary>
        public static Configuration Default { get; } = Lazy.Value;

        ///// <summary>
        ///// Gets the global parallel options for processing tasks in parallel.
        ///// </summary>
        //public ParallelOptions ParallelOptions { get; private set; } = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };

        /// <summary>
        /// Gets the currently registered <see cref="IImageFormat"/>s.
        /// </summary>
        //public IEnumerable<IImageFormat> ImageFormats => this.ImageFormatsManager.ImageFormats;

        ///// <summary>
        ///// Gets or sets the position in a stream to use for reading when using a seekable stream as an image data source.
        ///// </summary>
        //public ReadOrigin ReadOrigin { get; set; } = ReadOrigin.Current;

        ///// <summary>
        ///// Gets or sets the <see cref="ImageFormatManager"/> that is currently in use.
        ///// </summary>
        //public ImageFormatManager ImageFormatsManager { get; set; } = new ImageFormatManager();

        /// <summary>
        /// Gets or sets the <see cref="MemoryManager"/> that is currently in use.
        /// </summary>
        public MemoryManager MemoryManager { get; set; } = ArrayPoolMemoryManager.CreateDefault();

        //        /// <summary>
        //        /// Gets the maximum header size of all the formats.
        //        /// </summary>
        //        internal int MaxHeaderSize => this.ImageFormatsManager.MaxHeaderSize;

        //#if !NETSTANDARD1_1
        //        /// <summary>
        //        /// Gets or sets the filesystem helper for accessing the local file system.
        //        /// </summary>
        //        internal IFileSystem FileSystem { get; set; } = new LocalFileSystem();
        //#endif

        //        /// <summary>
        //        /// Gets or sets the image operations provider factory.
        //        /// </summary>
        //        internal IImageProcessingContextFactory ImageOperationsProvider { get; set; } = new DefaultImageOperationsProviderFactory();

        //        /// <summary>
        //        /// Registers a new format provider.
        //        /// </summary>
        //        /// <param name="configuration">The configuration provider to call configure on.</param>
        //        public void Configure(IConfigurationModule configuration)
        //        {
        //            Guard.NotNull(configuration, nameof(configuration));
        //            configuration.Configure(this);
        //        }

        //        /// <summary>
        //        /// Creates a shallow copy of the <see cref="Configuration"/>
        //        /// </summary>
        //        /// <returns>A new configuration instance</returns>
        //        public Configuration ShallowCopy()
        //        {
        //            return new Configuration
        //            {
        //                ParallelOptions = this.ParallelOptions,
        //                ImageFormatsManager = this.ImageFormatsManager,
        //                MemoryManager = this.MemoryManager,
        //                ImageOperationsProvider = this.ImageOperationsProvider,
        //                ReadOrigin = this.ReadOrigin,

        //#if !NETSTANDARD1_1
        //                FileSystem = this.FileSystem
        //#endif
        //            };
        //        }

        /// <summary>
        /// Creates the default instance with the following <see cref="IConfigurationModule"/>s preregistered:
        /// <para><see cref="PngConfigurationModule"/></para>
        /// <para><see cref="JpegConfigurationModule"/></para>
        /// <para><see cref="GifConfigurationModule"/></para>
        /// <para><see cref="BmpConfigurationModule"/></para>
        /// </summary>
        /// <returns>The default configuration of <see cref="Configuration"/></returns>
        internal static Configuration CreateDefaultInstance()
        {
            //return new Configuration(
            //    new PngConfigurationModule(),
            //    new JpegConfigurationModule(),
            //    new GifConfigurationModule(),
            //    new BmpConfigurationModule());
            return new Configuration();
        }
    }
}
