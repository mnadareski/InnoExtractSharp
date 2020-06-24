/*
 * Copyright (C) 2013-2019 Daniel Scharrer
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
    /// Wrapper class for a boost::iostreams-compatible source that can be used to restrict
    /// sources to appear smaller than they really are.
    /// </summary>
    public class RestrictedSource : IFilter
    {
        private Stream baseSource;   //!< The base source to read from.
        private long remaining;     //!< Number of bytes remaining in the restricted source.

        public RestrictedSource(RestrictedSource o)
        {
            baseSource = o.baseSource;
            remaining = o.remaining;
        }

        public RestrictedSource(Stream source, long size)
        {
            baseSource = source;
            remaining = size;
        }

        public int Read(Stream _, byte[] buffer, int offset, int bytes)
        {
            if (bytes <= 0)
                return 0;

            // Restrict the number of bytes to read
            bytes = (int)Math.Min(bytes, remaining);
            if (bytes == 0)
                return -1; // End of the restricted source reached

            int nread = baseSource.Read(buffer, offset, bytes);

            // Remember how many bytes were read so far
            if (nread > 0)
                remaining -= Math.Min(nread, remaining);

            return nread;
        }

        /// <summary>
        /// Restricts a source to a specific size from the current position and makes
        /// it non-seekable.
        /// 
        /// Like boost::iostreams::restrict, but always has a 64-bit counter.
        /// </summary>
        public static RestrictedSource Restrict(Stream source, long size)
        {
            return new RestrictedSource(source, size);
        }
    }
}
