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

namespace InnoExtractSharp.Streams
{
    /// <summary>
    /// Filter to decode executable files stored by Inno Setup versions after 5.2.0.
    /// 
    /// It tries to change the addresses stored for x86 CALL and JMP instructions to be
    /// relative to the instruction's position, plus a few other tweaks.
    /// </summary>
    public class InnoExeDecoder5200 : IFilter
    {
        /*
	     * call_instruction_decoder_5200 has three states:
	     *
	     * "initial" (flush_bytes == 0)
	     *  - Read individual bytes and write them directly to output.
	     *  - If the byte could be the start of a CALL or JMP instruction that doesn't span blocks,
	     *    set addr_bytes_left to -4.
	     *
	     * "address" (flush_bytes < 0 && flush_bytes >= -4)
	     *  - Read all four address bytes into buffer, incrementing flush_bytes for each byte read.
	     *  - Once the last byte has been read, transform the address and set flush_bytes to 4.
	     *  - If an EOF is encountered before all four bytes have been read, set flush_bytes to
	     *    4 + flush_bytes.
	     *
	     * "flush" (flush_bytes > 0 && flush_bytes <= 4)
	     *  - Write the first flush_bytes bytes of buffer to output.
	     *  - If there is not enough output space, write as much as possible and move to rest to
	     *    the start of buffer.
	     */

        private const int blockSize = 0x10000;
        private bool flipHighByte;

        private uint totalOffset; // Total number of bytes read from the source

        private sbyte flushBytes;
        private byte[] buffer = new byte[4];

        /// <param name="flipHighByte">true if the high byte of addresses is flipped if bit 23 is set.
        /// This optimization is used in Inno Setup 5.3.9 and later.</param>
        public InnoExeDecoder5200(bool flipHighByte)
        {
            this.flipHighByte = flipHighByte;
            Close();
        }

        public int Read(Stream src, byte[] dest, int offset, int n)
        {
            int end = offset + n;

            // Flush already processed address bytes.
            Flush(n, end, offset, flushBytes);

            while (offset != end)
            {
                if (flushBytes == 0)
                {
                    // Check if this is a CALL or JMP instruction
                    int byt = src.ReadByte();
                    if (byt == -1) { return TotalRead(n, end, offset) != 0 ? TotalRead(n, end, offset) : -1; }
                    dest[offset++] = (byte)byt;
                    totalOffset++;

                    // Not a CALL or JMP instruction
                    if (byt != 0xe8 && byt != 0xe9)
                        continue;

                    int blockSizeLeft = (int)(blockSize - ((totalOffset - 1) % blockSize));

                    // Ignore instructions that span blocks.
                    if (blockSizeLeft < 5)
                        continue;

                    flushBytes = -4;
                }

                if (flushBytes > 0)
                    throw new Exception("assert(FlushBytes < 0)");

                // Read all four address bytes.
                // TODO: Ensure behavior is the same
                //char* dst = reinterpret_cast<char*>(buffer + 4 + flushBytes);
                //int nread = src.Read(dst, 0, -flushBytes);
                int nread = src.Read(buffer, 0, 4);
                if (nread == -1)
                {
                    Flush(n, end, offset, (sbyte)(4 + flushBytes));
                    return TotalRead(n, end, offset) != 0 ? TotalRead(n, end, offset) : -1;
                }

                flushBytes = (sbyte)(flushBytes + nread);
                totalOffset += (uint)nread;
                if (flushBytes > 0) { return TotalRead(n, end, offset); }

                // Verify that the high byte of the address is 0x00 or 0xff
                if (buffer[3] == 0x00 || buffer[3] == 0xff)
                {
                    uint addr = totalOffset & 0xffffff; // may wrap, but OK

                    uint rel = (uint)(buffer[0] | (buffer[1] << 8) | (buffer[2] << 16));
                    rel -= addr;
                    buffer[0] = (byte)rel;
                    buffer[1] = (byte)(rel >> 8);
                    buffer[2] = (byte)(rel >> 16);

                    if (flipHighByte)
                    {
                        // For a slightly higher compression ratio, we want the resulting high
                        // byte to be 0x00 for both forward and backward jumps. The high byte
                        // of the original relative address is likely to be the sign extension
                        // of bit 23, so if bit 23 is set, toggle all bits in the high byte.
                        if ((rel & 0x800000) != 0)
                            buffer[3] = (byte)(~buffer[3]);
                    }
                }
                else
                {
                    // This is most likely not a CALL or JUMP
                }

                Flush(n, end, offset, 4);
            }

            return TotalRead(n, end, offset);
        }

        /// <summary>
        /// Total number of filtered bytes read and written to dest.
        /// </summary>
        private int TotalRead(int n, int end, int offset)
        {
            return n - (end - offset);
        }

        private int Flush(int n, int end, int offset, sbyte N)
        {
            if (N > 0)
            {
                flushBytes = N;
                int buffer_i = 0;
                do
                {
                    if (offset == end)
                    {
                        Array.Copy(buffer, offset, buffer, offset + buffer_i, (int)flushBytes);
                        return TotalRead(n, end, offset);
                    }

                    buffer[offset++] = buffer[buffer_i++];
                } while (--flushBytes != 0);
            }

            return 0;
        }

        public void Close()
        {
            totalOffset = 0;
            flushBytes = 0;
        }
    }
}
