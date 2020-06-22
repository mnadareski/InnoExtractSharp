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
using System.Linq;

namespace InnoExtractSharp.Streams
{
    /// <summary>
    /// Wrapper class for a boost::iostreams-compatible source that can be used to restrict
    /// sources to appear smaller than they really are.
    /// </summary>
    public class RestrictedSource
    {
        public Stream BaseSource;   //!< The base source to read from.
        public ulong Remaining;     //!< Number of bytes remaining in the restricted source.

        public RestrictedSource(RestrictedSource o)
        {
            BaseSource = o.BaseSource;
            Remaining = o.Remaining;
        }

        public RestrictedSource(Stream source, ulong size)
        {
            BaseSource = source;
            Remaining = size;
        }

        public int Read(ref char[] buffer, int bufferPtr, int bytes)
        {
            if (bytes <= 0)
                return 0;

            // Restrict the number of bytes to read
            bytes = (int)Math.Min((ulong)bytes, Remaining);
            if (bytes == 0)
                return -1; // End of the restricted source reached

            byte[] byteBuffer = new byte[bytes];
            int nread = BaseSource.Read(byteBuffer, bufferPtr, bytes);

            // Remember how many bytes were read so far
            if (nread > 0)
            {
                Array.Copy(byteBuffer.Select(b => (char)b).ToArray(), 0, buffer, bufferPtr, bytes);
                Remaining -= Math.Min((ulong)nread, Remaining);
            }

            return nread;
        }

        /// <summary>
        /// Restricts a source to a specific size from the current position and makes
        /// it non-seekable.
        /// 
        /// Like boost::iostreams::restrict, but always has a 64-bit counter.
        /// </summary>
        public static RestrictedSource Restrict(Stream source, ulong size)
        {
            return new RestrictedSource(source, size);
        }
    }
}
