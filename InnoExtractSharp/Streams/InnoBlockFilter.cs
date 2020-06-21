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
using System.IO;
using System.Text;
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
    public class InnoBlockFilter : Stream
    {
        /// <summary>
        /// Current read position in the buffer
        /// </summary>
        public int Pos;

        /// <summary>
        /// Length of the buffer. This is always 4096 except for the last chunk
        /// </summary>
        public int BufferLength;

        private byte[] Buffer = new byte[4096];

        public override bool CanRead => throw new NotImplementedException();

        public override bool CanSeek => throw new NotImplementedException();

        public override bool CanWrite => throw new NotImplementedException();

        public override long Length => throw new NotImplementedException();

        public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public InnoBlockFilter()
        {
            Pos = 0;
            BufferLength = 0;
        }

        public bool ReadChunk(Stream src)
        {
            using (BinaryReader br = new BinaryReader(src, Encoding.Default, true))
            {
                byte[] temp = new byte[sizeof(uint)];
                int tempSize = temp.Length;
                int nread = src.Read(temp, 0, tempSize);

                if (nread == 0)
                    return false;
                else if (nread != temp.Length)
                    throw new Exception("Unexpcted block end"); // TODO: We don't want to do this

                LittleEndian le = new LittleEndian();
                uint blockCrc32 = le.LoadUInt32(temp, 0);

                BufferLength = src.Read(Buffer, 0, Buffer.Length);
                if (Length == 0)
                    throw new Exception("Unexpected block end"); // TODO: We don't want to do this

                CRC32 actual = new CRC32();
                actual.Init();
                actual.Update(Buffer, 0, BufferLength);
                if (actual.Finalize() != blockCrc32)
                    throw new Exception("Block CRC32 mismatch"); // TODO: We don't want to do this

                Pos = 0;
                return true;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int read = 0;
            while (count > 0)
            {
                if (Pos == BufferLength && !ReadChunk(this))
                    return read > 0 ? read : -1;

                int size = Math.Min(count, BufferLength - Pos);

                Array.Copy(Buffer, Pos, buffer, read, size);

                Pos += size;
                count -= size;
                read += size;
            }

            return read;
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}
