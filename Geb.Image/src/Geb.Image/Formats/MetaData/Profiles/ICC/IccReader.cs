﻿// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;

namespace Geb.Image.Formats.MetaData.Profiles.Icc
{
    /// <summary>
    /// Reads and parses ICC data from a byte array
    /// </summary>
    internal sealed class IccReader
    {
        /// <summary>
        /// Reads an ICC profile
        /// </summary>
        /// <param name="data">The raw ICC data</param>
        /// <returns>The read ICC profile</returns>
        public IccProfile Read(byte[] data)
        {
            Guard.NotNull(data, nameof(data));
            Guard.IsTrue(data.Length >= 128, nameof(data), "Data length must be at least 128 to be a valid ICC profile");

            var reader = new IccDataReader(data);
            IccProfileHeader header = this.ReadHeader(reader);
            IccTagDataEntry[] tagData = this.ReadTagData(reader);

            return new IccProfile(header, tagData);
        }

        /// <summary>
        /// Reads an ICC profile header
        /// </summary>
        /// <param name="data">The raw ICC data</param>
        /// <returns>The read ICC profile header</returns>
        public IccProfileHeader ReadHeader(byte[] data)
        {
            Guard.NotNull(data, nameof(data));
            Guard.IsTrue(data.Length >= 128, nameof(data), "Data length must be at least 128 to be a valid profile header");

            var reader = new IccDataReader(data);
            return this.ReadHeader(reader);
        }

        /// <summary>
        /// Reads the ICC profile tag data
        /// </summary>
        /// <param name="data">The raw ICC data</param>
        /// <returns>The read ICC profile tag data</returns>
        public IccTagDataEntry[] ReadTagData(byte[] data)
        {
            Guard.NotNull(data, nameof(data));
            Guard.IsTrue(data.Length >= 128, nameof(data), "Data length must be at least 128 to be a valid ICC profile");

            var reader = new IccDataReader(data);
            return this.ReadTagData(reader);
        }

        private IccProfileHeader ReadHeader(IccDataReader reader)
        {
            reader.SetIndex(0);

            return new IccProfileHeader
            {
                Size = reader.ReadUInt32(),
                CmmType = reader.ReadAsciiString(4),
                Version = reader.ReadVersionNumber(),
                Class = (IccProfileClass)reader.ReadUInt32(),
                DataColorSpace = (IccColorSpaceType)reader.ReadUInt32(),
                ProfileConnectionSpace = (IccColorSpaceType)reader.ReadUInt32(),
                CreationDate = reader.ReadDateTime(),
                FileSignature = reader.ReadAsciiString(4),
                PrimaryPlatformSignature = (IccPrimaryPlatformType)reader.ReadUInt32(),
                Flags = (IccProfileFlag)reader.ReadInt32(),
                DeviceManufacturer = reader.ReadUInt32(),
                DeviceModel = reader.ReadUInt32(),
                DeviceAttributes = (IccDeviceAttribute)reader.ReadInt64(),
                RenderingIntent = (IccRenderingIntent)reader.ReadUInt32(),
                PcsIlluminant = reader.ReadXyzNumber(),
                CreatorSignature = reader.ReadAsciiString(4),
                Id = reader.ReadProfileId(),
            };
        }

        private IccTagDataEntry[] ReadTagData(IccDataReader reader)
        {
            IccTagTableEntry[] tagTable = this.ReadTagTable(reader);
            var entries = new IccTagDataEntry[tagTable.Length];
            var store = new Dictionary<uint, IccTagDataEntry>();
            for (int i = 0; i < tagTable.Length; i++)
            {
                IccTagDataEntry entry;
                uint offset = tagTable[i].Offset;
                if (store.ContainsKey(offset))
                {
                    entry = store[offset];
                }
                else
                {
                    entry = reader.ReadTagDataEntry(tagTable[i]);
                    store.Add(offset, entry);
                }

                entry.TagSignature = tagTable[i].Signature;
                entries[i] = entry;
            }

            return entries;
        }

        private IccTagTableEntry[] ReadTagTable(IccDataReader reader)
        {
            reader.SetIndex(128);   // An ICC header is 128 bytes long

            uint tagCount = reader.ReadUInt32();
            var table = new IccTagTableEntry[tagCount];

            for (int i = 0; i < tagCount; i++)
            {
                uint tagSignature = reader.ReadUInt32();
                uint tagOffset = reader.ReadUInt32();
                uint tagSize = reader.ReadUInt32();
                table[i] = new IccTagTableEntry((IccProfileTag)tagSignature, tagOffset, tagSize);
            }

            return table;
        }
    }
}
