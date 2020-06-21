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

// Taken from Crypto++ and modified to fit the project.
// adler32.cpp - written and placed in the public domain by Wei Dai

namespace InnoExtractSharp.Crypto
{
    /// <summary>
    /// Adler-32 checksum calculations
    /// </summary>
    public class Adler32 : Checksum
    {
        private uint state;

        public Adler32()
        {
            this.Type = ChecksumType.Adler32;
        }

        public void Init()
        {
            state = 1;
        }

        public override void Update(byte[] data, int dataPtr, int length)
        {
            const uint baseSeed = 65521;

            uint s1 = this.state;
            uint s2 = this.state >> 16;

            if (length % 8 != 0)
            {
                do
                {
                    s1 += data[dataPtr++];
                    s2 += s1;
                    length--;
                } while (length % 8 != 0);

                if (s1 >= baseSeed)
                    s1 -= baseSeed;

                s2 %= baseSeed;
            }

            while (length > 0)
            {
                s1 += data[dataPtr + 0]; s2 += s1;
                s1 += data[dataPtr + 1]; s2 += s1;
                s1 += data[dataPtr + 2]; s2 += s1;
                s1 += data[dataPtr + 3]; s2 += s1;
                s1 += data[dataPtr + 4]; s2 += s1;
                s1 += data[dataPtr + 5]; s2 += s1;
                s1 += data[dataPtr + 6]; s2 += s1;
                s1 += data[dataPtr + 7]; s2 += s1;

                length -= 8;
                dataPtr += 8;

                if (s1 >= baseSeed)
                    s1 -= baseSeed;

                if (length % 0x8000 == 0)
                    s2 %= baseSeed;
            }

            this.state = (s2 << 16 | s1);
        }

        public uint Finalize()
        {
            return state;
        }
    }
}
