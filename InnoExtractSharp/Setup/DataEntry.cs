/*
 * Copyright (C) 2011-2014 Daniel Scharrer
 * Converted code Copyright (C) 2018 Matt Nadareski
 *
 * This software is provided 'as-is', without any express or implied
 * warranty.  In no event will the author(s) be held liable for any damages
 * arising from the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 *
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would be
 *    appreciated but is not required.
 * 2. Altered source versions must be plainly marked as such, and must not be
 *    misrepresented as being the original software.
 * 3. This notice may not be removed or altered from any source distribution.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using InnoExtractSharp.Crypto;
using InnoExtractSharp.Streams;

namespace InnoExtractSharp.Setup
{
    public class DataEntry : Entry
    {
        [Flags]
        public enum Flags
        {
            VersionInfoValid = 1 << 0,
            VersionInfoNotValid = 1 << 1,
            TimeStampInUTC = 1 << 2,
            IsUninstallerExe = 1 << 3,
            CallInstructionOptimized = 1 << 4,
            Touch = 1 << 5,
            ChunkEncrypted = 1 << 6,
            ChunkCompressed = 1 << 7,
            SolidBreak = 1 << 8,
            Sign = 1 << 9,
            SignOnce = 1 << 10,

            // obsolete:
            BZipped = 1 << 11,
        }

        public Chunk Chunk;

        public InnoFile File;

        public ulong UncompressedSize;

        public long Timestamp;
        public uint TimestampNsec;

        public ulong FileVersion;

        public Flags Options;

        /// <summary>
        /// Load one data entry
        /// </summary>
        public override void Load(Stream input, InnoVersion version, List<Entry> entries = null)
        {
            using (BinaryReader br = new BinaryReader(input, Encoding.Default, true))
            {
                Chunk.FirstSlice = (int)br.ReadUInt32();
                Chunk.LastSlice = (int)br.ReadUInt32();
                if (version < InnoVersion.INNO_VERSION(4, 0, 0))
                {
                    if (Chunk.FirstSlice >= 1 && Chunk.LastSlice >= 1)
                    {
                        Chunk.FirstSlice--;
                        Chunk.LastSlice--;
                    }
                }

                Chunk.Offset = br.ReadUInt32();

                if (version >= InnoVersion.INNO_VERSION(4, 0, 1))
                    File.Offset = br.ReadUInt64();
                else
                    File.Offset = 0;

                if (version >= InnoVersion.INNO_VERSION(4, 0, 0))
                {
                    File.Size = br.ReadUInt64();
                    Chunk.Size = br.ReadUInt64();
                }
                else
                {
                    File.Size = br.ReadUInt32();
                    Chunk.Size = br.ReadUInt32();
                }
                UncompressedSize = File.Size;

                if (version >= InnoVersion.INNO_VERSION(5, 3, 9))
                {
                    File.Checksum.SHA1 = br.ReadChars(File.Checksum.SHA1.Length);
                    File.Checksum.Type = ChecksumType.SHA1;
                }
                else if (version >= InnoVersion.INNO_VERSION(4, 2, 0))
                {
                    File.Checksum.MD5 = br.ReadChars(File.Checksum.MD5.Length);
                    File.Checksum.Type = ChecksumType.MD5;
                }
                else if (version >= InnoVersion.INNO_VERSION(4, 0, 1))
                {
                    File.Checksum.CRC32 = br.ReadUInt32();
                    File.Checksum.Type = ChecksumType.CRC32;
                }
                else
                {
                    File.Checksum.Adler32 = br.ReadUInt32();
                    File.Checksum.Type = ChecksumType.Adler32;
                }

                if (version.Bits == 16)
                {
                    // 16-bit installers use the FAT Filetime format
                    ushort time = br.ReadUInt16();
                    ushort date = br.ReadUInt16();

                    int second = Util.Utility.GetBits(time, 0, 4) * 2;          // [0, 58]
                    int minute = Util.Utility.GetBits(time, 5, 10);             // [0, 59]
                    int hour = Util.Utility.GetBits(time, 11, 15);              // [0, 23]
                    int day = Util.Utility.GetBits(date, 0, 4);                 // [1, 31]
                    int month = Util.Utility.GetBits(date, 5, 8) - 1;           // [0, 11]
                    int year = Util.Utility.GetBits(date, 9, 15) + 1980 - 1900; // [80, 199]
                    DateTime t = new DateTime(year, month, day, hour, minute, second);

                    Timestamp = t.Ticks;
                    TimestampNsec = 0;
                }
                else
                {
                    // 32-bit installers use the Win32 FILETIME format
                    long Filetime = br.ReadInt64();
                    const long FiletimeOffset = 0x19DB1DED53E8000;
                    if (Filetime < FiletimeOffset)
                    {
                        // log_warning << "Unexpected Filetime: " << Filetime;
                    }

                    Filetime -= FiletimeOffset;

                    Timestamp = Filetime / 10000000;
                    TimestampNsec = (uint)(Filetime % 10000000) * 100;
                }

                uint fileVersionMs = br.ReadUInt32();
                uint fileVersionLs = br.ReadUInt32();
                FileVersion = ((ulong)(fileVersionMs) << 32) | (ulong)(fileVersionLs);

                Options = 0;

                Flags flagreader = Flags.VersionInfoValid | Flags.VersionInfoNotValid;
                if (version >= InnoVersion.INNO_VERSION(2, 0, 17) && version < InnoVersion.INNO_VERSION(4, 0, 1))
                    flagreader |= Flags.BZipped;

                if (version >= InnoVersion.INNO_VERSION(4, 0, 10))
                    flagreader |= Flags.TimeStampInUTC;

                if (version >= InnoVersion.INNO_VERSION(4, 1, 0))
                    flagreader |= Flags.IsUninstallerExe;

                if (version >= InnoVersion.INNO_VERSION(4, 1, 8))
                    flagreader |= Flags.CallInstructionOptimized;

                if (version >= InnoVersion.INNO_VERSION(4, 2, 0))
                    flagreader |= Flags.Touch;

                if (version >= InnoVersion.INNO_VERSION(4, 2, 2))
                    flagreader |= Flags.ChunkEncrypted;

                if (version >= InnoVersion.INNO_VERSION(4, 2, 5))
                    flagreader |= Flags.ChunkCompressed;
                else
                    Options |= Flags.ChunkCompressed;

                if (version >= InnoVersion.INNO_VERSION(5, 1, 13))
                    flagreader |= Flags.SolidBreak;

                if (version >= InnoVersion.INNO_VERSION(5, 5, 7))
                {
                    // Actually added in Inno Setup 5.5.9 but the data version was not bumped
                    flagreader |= Flags.Sign;
                    flagreader |= Flags.SignOnce;
                }

                Options |= flagreader;

                if ((Options & Flags.ChunkCompressed) != 0)
                    Chunk.Compression = CompressionMethod.UnknownCompression;
                else
                    Chunk.Compression = CompressionMethod.Stored;

                if ((Options & Flags.BZipped) != 0)
                {
                    Options |= Flags.ChunkCompressed;
                    Chunk.Compression = CompressionMethod.BZip2;
                }

                if ((Options & Flags.ChunkEncrypted) != 0)
                {
                    if (version >= InnoVersion.INNO_VERSION(5, 3, 9))
                        Chunk.Encryption = EncryptionMethod.ARC4_SHA1;
                    else
                        Chunk.Encryption = EncryptionMethod.ARC4_MD5;
                }
                else
                    Chunk.Encryption = EncryptionMethod.Plaintext;

                if ((Options & Flags.CallInstructionOptimized) != 0)
                {
                    if (version < InnoVersion.INNO_VERSION(5, 2, 0))
                        File.Filter = CompressionFilter.InstructionFilter4108;
                    else if (version < InnoVersion.INNO_VERSION(5, 3, 9))
                        File.Filter = CompressionFilter.InstructionFilter5200;
                    else
                        File.Filter = CompressionFilter.InstructionFilter5309;
                }
                else
                    File.Filter = CompressionFilter.NoFilter;
            }
        }
    }
}
