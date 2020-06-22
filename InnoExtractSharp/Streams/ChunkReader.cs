/*
 * Copyright (C) 2011-2019 Daniel Scharrer
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
using System.IO;
using System.Linq;
using InnoExtractSharp.Crypto;

namespace InnoExtractSharp.Streams
{
    /// <summary>
    /// Wrapper to read and decompress a chunk from a \ref slice_reader.
    /// Restrics the stream to the chunk size and applies the appropriate decompression.
    /// </summary>
    public class ChunkReader
    {
        private static char[] chunkId = { 'z', 'l', 'b', (char)0x1a };

        /// <summary>
        ///  Wrap a \ref slice_reader to read and decompress a single chunk.
        ///  
        /// Only one wrapper can be used at the same time for each \c base.
        /// </summary>
        /// <param name="reader">The slice reader for the setup file(s).</param>
        /// <param name="chunk">Information specifying the chunk to read.</param>
        /// <param name="key">Key used for encrypted chunks.</param>
        /// <returns>a pointer to a non-seekable input filter chain for the requested file.</returns>
        public static int Get(Stream input, Chunk chunk, string key)
        {
            if (!input.CanSeek)
                throw new Exception("could not seek to chunk start"); // TODO: We shouldn't do this

            input.Seek(chunk.FirstSlice + chunk.Offset, SeekOrigin.Begin);
            byte[] magic = new byte[chunkId.Length];
            if (input.Read(magic, 0, 4) != 4 || !magic.SequenceEqual(chunkId))
                throw new Exception("bad chunk magic"); // TODO: We shouldn't do this

            int result = (int)input.Position;

            switch (chunk.Compression)
            {
                case CompressionMethod.Stored:
                    break;
                case CompressionMethod.Zlib:
                    // result.Push(zlib_decompressor(), 8192);
                    break;
                case CompressionMethod.BZip2:
                    // result.Push(bzip2_decompressor(), 8192);
                    break;
                case CompressionMethod.LZMA1:
                    // result.Push(inno_lzma1_decompressor(), 8192);
                    break;
                case CompressionMethod.LZMA2:
                    // result.Push(inno_lzma2_decompressor(), 8192);
                    break;
                default:
                    throw new Exception("unknown chunk compression"); // TODO: We shouldn't do this
            }

            if (chunk.Encryption != EncryptionMethod.Plaintext)
            {
                byte[] salt = new byte[8];
                if (input.Read(salt, 0, 8) != 8)
                    throw new Exception("could not read chunk salt"); // TODO: We shouldn't do this

                Hasher hasher = new Hasher(chunk.Encryption == EncryptionMethod.ARC4_SHA1 ? ChecksumType.SHA1 : ChecksumType.MD5);
                hasher.Update(salt, 0, salt.Length);
                hasher.Update(key.ToCharArray().Select(c => (byte)c).ToArray(), 0, key.Length);
                Checksum checksum = hasher.Finalize();
                char[] saltedKey = chunk.Encryption == EncryptionMethod.ARC4_SHA1 ? checksum.SHA1 : checksum.MD5;
                int keyLength = saltedKey.Length;
                // result.Push(InnoArc4Crypter(salted_key, key_length), 8192);
            }

            // result.Push(restrict(base, chunk.size);

            return result;
        }
    }
}
