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

using System.IO;
using InnoExtractSharp.Crypto;

namespace InnoExtractSharp.Streams
{
    /// <summary>
    /// Filters to be used with boost::iostreams for calculating a \ref crypto::checksum.
    /// 
    /// An internal checksum state is updated as bytes are read and the final checksum is
    /// written to the given checksum object when the end of the source stream is reached.
    /// </summary>
    public class ChecksumFilter : IFilter
    {
        private Hasher Hasher;
        private Checksum Output;

        /// <param name="dest">Location to store the final checksum at.</param>
        /// <param name="type">The type of checksum to calculate.</param>
        public ChecksumFilter(Checksum dest, ChecksumType type)
        {
            Hasher = new Hasher(type);
            Output = dest;
        }

        public int Read(Stream src, byte[] dest, int offset, int n)
        {
            int nread = src.Read(dest, 0, n);

            if (nread > 0)
                Hasher.Update(dest, 0, nread);
            else if (Output != null)
                Output = Hasher.Finalize();

            return nread;
        }
    }
}
