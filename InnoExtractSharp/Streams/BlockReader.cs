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

// TODO: Probably rewrite every singe thing in InnoExtractSharp.Streams

using System;
using System.IO;
using System.Text;
using InnoExtractSharp.Crypto;
using InnoExtractSharp.Setup;
using InnoExtractSharp.Util;

namespace InnoExtractSharp.Streams
{
    public enum BlockCompression
    {
        Stored,
        Zlib,
        LZMA1,
    }

    /// <summary>
    /// Wrapper to read compressed and checksumed block of data used to store setup headers.
    /// 
    /// The decompressed headers are parsed in \ref setup::info, which also uses this class.
    /// </summary>
    public class BlockReader
    {
        /// <summary>
        /// Wrap an input stream to read and decompress setup header blocks.
        /// 
        /// Only one wrapper can be used at the same time for each \c base.
        /// </summary>
        /// <param name="input">The input stream for the main setup files.
        /// It must already be positioned at start of the block stream.
        /// The first block stream starts directly after the \ref setup::version
        /// identifier whose position is given by
        /// \ref loader::offsets::header_offset.
        /// A second block stream directly follows the first one and contains
        /// the \ref setup::data_entry "data entries".</param>
        /// <param name="version">The version of the setup data</param>
        /// <returns>a pointer to a non-seekable input stream for the uncompressed headers.
        /// Reading from this stream may throw a \ref block_error if a block checksum
        /// was invalid.</returns>
        public static Stream Get(Stream input, InnoVersion version)
        {
            using (BinaryReader br = new BinaryReader(input, Encoding.Default, true))
            {
                uint expectedChecksum = br.ReadUInt32();
                CRC32 actualChecksum = new CRC32();
                actualChecksum.Init();

                uint storedSize;
                BlockCompression compression;

                if (version >= InnoVersion.INNO_VERSION(4, 0, 9))
                {
                    storedSize = actualChecksum.LoadUInt32(input);
                    byte compressed = actualChecksum.LoadByte(input);

                    compression = (compressed == 1) ? (version >= InnoVersion.INNO_VERSION(4, 1, 6) ? BlockCompression.LZMA1 : BlockCompression.Zlib) : BlockCompression.Stored;
                }
                else
                {
                    uint compressedSize = actualChecksum.LoadUInt32(input);
                    uint uncompressedSize = actualChecksum.LoadUInt32(input);

                    if (compressedSize == UInt32.MaxValue)
                    {
                        storedSize = uncompressedSize;
                        compression = BlockCompression.Stored;
                    }
                    else
                    {
                        storedSize = compressedSize;
                        compression = BlockCompression.Zlib;
                    }

                    // Add the size of a CRC32 checksum for each 4KiB subblock
                    storedSize += (Utility.CeilDiv(storedSize, 4096) * 4);
                }

                if (actualChecksum.Finalize() != expectedChecksum)
                    throw new Exception("Block header CRC32 mismatch"); // TODO: We don't want to do this

                switch (compression)
                {
                    case BlockCompression.Stored:
                        break;
                    case BlockCompression.Zlib:
                        // fis.push(zlib_decompressor(), 8192);
                        break;
                    case BlockCompression.LZMA1:
                        // fis.push(inno_lzma_decompressor(), 8192);
                        break;
                }

                InnoBlockFilter fis = input as InnoBlockFilter;
                fis.BufferLength = 4096;
                // fis.push(io.restrict(base, 0, storedSize);
                // fis.exceptions(badbit | failbit);

                // return pointer(fis.release());
                return fis;
            }
        }
    }
}
