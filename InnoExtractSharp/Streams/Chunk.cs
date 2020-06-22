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

namespace InnoExtractSharp.Streams
{
    /// <summary>
    /// Compression methods supported by chunks.
    /// </summary>
    public enum CompressionMethod
    {
        Stored,
        Zlib,
        BZip2,
        LZMA1,
        LZMA2,
        UnknownCompression,
    }

    /// <summary>
    /// Encryption methods supported by chunks.
    /// </summary>
    public enum EncryptionMethod
    {
        Plaintext,
        ARC4_MD5,
        ARC4_SHA1,
    }

    /// <summary>
    /// Information specifying a compressed chunk.
    /// 
    /// This data is stored in \ref setup::data_entry "data entries"
    /// 
    /// Chunks specified by this struct can be read using \ref chunk_reader
    /// </summary>
    public class Chunk
    {
        public uint FirstSlice; // Slice where the chunk starts.
        public uint LastSlice; // Slice where the chunk ends.

        public uint SortOffset;

        public uint Offset; // Offset of the compressed chunk in firstSlice.
        public ulong Size; // Total compressed size of the chunk.

        public CompressionMethod Compression; // Compression method used by the chunk.
        public EncryptionMethod Encryption; // Encryption method used by the chunk.

        public static bool operator <(Chunk c1, Chunk c2)
        {
            if (c1.FirstSlice != c2.FirstSlice)
                return c1.FirstSlice < c2.FirstSlice;
            else if (c1.Offset != c2.Offset)
                return c1.Offset < c2.Offset;
            else if (c1.Size != c2.Size)
                return c1.Size < c2.Size;
            else if (c1.Compression != c2.Compression)
                return c1.Compression < c2.Compression;
            else if (c1.Encryption != c2.Encryption)
                return c1.Encryption < c2.Encryption;

            return false;
        }
        public static bool operator >(Chunk c1, Chunk c2)
        {
            return (c2 < c1);
        }

        public static bool operator ==(Chunk c1, Chunk c2)
        {
            return (c1.FirstSlice == c2.FirstSlice
                && c1.Offset == c2.Offset
                && c1.Size == c2.Size
                && c1.Compression == c2.Compression
                && c1.Encryption == c2.Encryption);
        }
        public static bool operator !=(Chunk c1, Chunk c2)
        {
            return !(c1 == c2);
        }
    }
}
