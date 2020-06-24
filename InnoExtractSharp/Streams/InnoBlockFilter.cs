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
using InnoExtractSharp.Crypto;
using InnoExtractSharp.Util;

namespace InnoExtractSharp.Streams
{
    /// <summary>
    /// A filter that reads a block of 4096-byte chunks where each chunk is preceeded by
    /// a CRC32 checksum. The last chunk can be shorter than 4096 bytes.
    /// 
    /// If chunk checksum is wrong a block_error is thrown before any data of that
    /// chunk is returned.
    /// 
    /// block_error is also thrown if there is trailing data: 0 < (total size % (4096 + 4)) < 5
    /// </summary>
    public class InnoBlockFilter : IFilter
    {
        /// <summary>
        /// Current read position in the buffer
        /// </summary>
        private int pos;

        /// <summary>
        /// Length of the buffer. This is always 4096 except for the last chunk
        /// </summary>
        private int length;

        private byte[] buffer = new byte[4096];

        public InnoBlockFilter()
        {
            pos = 0;
            length = 0;
        }

        public int Read(Stream src, byte[] dest, int offset, int n)
        {
            int nread = 0;
            while (n > 0)
            {
                if (pos == length && !ReadChunk(src))
                    return nread > 0 ? nread : -1;

                int size = Math.Min(n, length - pos);

                Array.Copy(buffer, pos, dest, nread, size);

                pos += size;
                n -= size;
                nread += size;
            }

            return nread;
        }

        private bool ReadChunk(Stream src)
        {
            byte[] temp = new byte[sizeof(uint)];
            int tempSize = temp.Length;
            int nread = src.Read(temp, 0, tempSize);

            if (nread == 0)
                return false;
            else if (nread != temp.Length)
                throw new Exception("Unexpcted block end");

            uint blockCrc32 = new LittleEndian<uint>().Load(temp, 0);

            length = src.Read(buffer, 0, buffer.Length);
            if (length == 0)
                throw new Exception("Unexpected block end");

            CRC32 actual = new CRC32();
            actual.Init();
            actual.Update(buffer, 0, length);
            if (actual.Finalize() != blockCrc32)
                throw new Exception("Block CRC32 mismatch");

            pos = 0;
            return true;
        }
    }
}
