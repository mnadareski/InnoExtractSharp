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

namespace InnoExtractSharp.Streams
{
    /// <summary>
    /// Filter to decode executable files stored by Inno Setup versions before 5.2.0.
    /// 
    /// Essentially, it tries to change the addresses stored for x86 CALL and JMP instructions
    /// to be relative to the instruction's position.
    /// </summary>
    public class InnoExeDecoder4108 : Stream
    {
        private uint Addr;
        private int AddrBytesLeft;
        private uint AddrOffset;

        public override bool CanRead => throw new NotImplementedException();

        public override bool CanSeek => throw new NotImplementedException();

        public override bool CanWrite => throw new NotImplementedException();

        public override long Length => throw new NotImplementedException();

        public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public InnoExeDecoder4108()
            : base()
        {
            Close();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int bufferPtr = offset;

            for (int i = 0; i < count; i++, AddrOffset++)
            {
                if (this.Position == this.Length - 1)
                    return (i > 0 ? i : -1);
                int byt = this.ReadByte();

                if (AddrBytesLeft == 0)
                {
                    // Check if this is a CALL or JMP instruction
                    if (byt == 0xe8 || byt == 0xe9)
                    {
                        Addr = ~AddrOffset + 1;
                        AddrBytesLeft = 4;
                    }
                }
                else
                {
                    Addr += (byte)byt;
                    byt = (int)Addr;
                    Addr >>= 8;
                    AddrBytesLeft--;
                }

                buffer[bufferPtr++] = (byte)byt;
            }

            return count;
        }

        public override void Close()
        {
            Addr = 0;
            AddrBytesLeft = 0;
            AddrOffset = 0;
            base.Close();
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

    /// <summary>
    /// Filter to decode executable files stored by Inno Setup versions after 5.2.0.
    /// 
    /// It tries to change the addresses stored for x86 CALL and JMP instructions to be
    /// relative to the instruction's position, plus a few other tweaks.
    /// </summary>
    public class InnoExeDecoder5200 : Stream
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

        private const int BlockSize = 0x10000;
        private bool FlipHighByte;

        private uint Offset; // Total number of bytes read from the source

        private sbyte FlushBytes;
        private byte[] Buffer = new byte[4];

        public override bool CanRead => throw new NotImplementedException();

        public override bool CanSeek => throw new NotImplementedException();

        public override bool CanWrite => throw new NotImplementedException();

        public override long Length => throw new NotImplementedException();

        public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        /// <param name="flipHighByte">true if the high byte of addresses is flipped if bit 23 is set.
        /// This optimization is used in Inno Setup 5.3.9 and later.</param>
        public InnoExeDecoder5200(bool flipHighByte)
        {
            FlipHighByte = flipHighByte;
            Close();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int bufferPtr = offset;
            int end = bufferPtr + count;

            // Total number of filtered bytes read and written to dest.
            int totalRead = (count - (end - bufferPtr));

            // Flush already processed address bytes.
            if (FlushBytes > 0)
            {
                int bufferI = 0;
                do
                {
                    if (bufferPtr == end)
                    {
                        Array.Copy(Buffer, bufferI, Buffer, 0, FlushBytes);
                        return totalRead;
                    }
                } while (--FlushBytes > 0);
            }

            while (bufferPtr != end)
            {
                if (FlushBytes == 0)
                {
                    // Check if this is a CALL or JMP instruction
                    if (this.Position == this.Length - 1)
                        return (totalRead > 0 ? totalRead : -1);
                    int byt = this.ReadByte();
                    buffer[bufferPtr++] = (byte)byt;
                    Offset++;

                    // Not a CALL or JMP instruction
                    if (byt != 0xe8 && byt != 0xe9)
                        continue;

                    int blockSizeLeft = (int)(BlockSize - ((Offset - 1) % BlockSize));

                    // Ignore instructions that span blocks.
                    if (blockSizeLeft < 5)
                        continue;

                    FlushBytes = -4;
                }

                // assert(FlushBytes < 0);

                // Read all four address bytes.
                byte[] dstBytes = new byte[-FlushBytes];
                int nread = this.Read(dstBytes, 0, -FlushBytes);
                Array.Copy(dstBytes, Buffer, -FlushBytes);
                if (nread == -1)
                {
                    if (4 + FlushBytes > 0)
                    {
                        FlushBytes = (sbyte)(4 + FlushBytes);
                        int bufferI = 0;
                        do
                        {
                            if (bufferPtr == end)
                            {
                                Array.Copy(Buffer, bufferI, Buffer, 0, FlushBytes);
                                return totalRead;
                            }
                        } while (--FlushBytes > 0);
                    }
                    return (totalRead > 0 ? totalRead : -1);
                }

                FlushBytes = (sbyte)(FlushBytes + nread);
                Offset += (uint)nread;
                if (FlushBytes > 0)
                    return totalRead;

                // Verify that the high byte of the address is 0x00 or 0xff
                if (Buffer[3] == 0x00 || Buffer[3] == 0xff)
                {
                    uint addr = Offset & 0xffffff; // may wrap, but OK

                    uint rel = (uint)(Buffer[0] | (Buffer[1] << 8) | (Buffer[2] << 16));
                    rel -= addr;
                    Buffer[0] = (byte)rel;
                    Buffer[1] = (byte)(rel >> 8);
                    Buffer[2] = (byte)(rel >> 16);

                    if (FlipHighByte)
                    {
                        // For a slightly higher compression ratio, we want the resulting high
                        // byte to be 0x00 for both forward and backward jumps. The high byte
                        // of the original relative address is likely to be the sign extension
                        // of bit 23, so if bit 23 is set, toggle all bits in the high byte.
                        if ((rel & 0x800000) > 0)
                            Buffer[3] = (byte)(~Buffer[3]);
                    }
                }
                else
                {
                    // This is most likely not a CALL or JUMP
                }

                if (nread == -1)
                {
                    if (4 > 0)
                    {
                        FlushBytes = 4;
                        int bufferI = 0;
                        do
                        {
                            if (bufferPtr == end)
                            {
                                Array.Copy(Buffer, bufferI, Buffer, 0, FlushBytes);
                                return totalRead;
                            }
                        } while (--FlushBytes > 0);
                    }
                    return (totalRead > 0 ? totalRead : -1);
                }
            }

            return totalRead;
        }

        public override void Close()
        {
            Offset = 0;
            FlushBytes = 0;
            base.Close();
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
