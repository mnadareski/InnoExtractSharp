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
    /// Filter to decode executable files stored by Inno Setup versions before 5.2.0.
    /// 
    /// Essentially, it tries to change the addresses stored for x86 CALL and JMP instructions
    /// to be relative to the instruction's position.
    /// </summary>
    public class InnoExeDecoder4108
    {
        private uint Addr;
        private int AddrBytesLeft;
        private uint AddrOffset;

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
}
