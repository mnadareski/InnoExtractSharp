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

using System.IO;
using InnoExtractSharp.Crypto;

namespace InnoExtractSharp.Streams
{
    /// <summary>
    /// Filter to be used with boost::iostreams for calculating a \ref crypto::checksum.
    /// </summary>
    public class ChecksumFilter
    {
        private Hasher Hasher;
        private Checksum Output;

        public ChecksumFilter(Checksum output, ChecksumType type)
        {
            Hasher = new Hasher(type);
            Output = output;
        }

        public int Read(Stream src, out byte[] dest, int n)
        {
            dest = new byte[n];
            int nread = src.Read(dest, 0, n);

            if (nread > 0)
                Hasher.Update(dest, 0, nread);
            else if (Output != null)
                Output = Hasher.Finalize(); // Output = null; ??????

            return nread;
        }
    }
}
