/*
 * Copyright (C) 2011-2018 Daniel Scharrer
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
    /// Wrapper to read a single file from a \ref chunk_reader
    /// Restrics the stream to the file size and applies the appropriate filters.
    /// </summary>
    public class FileReader
    {
        /// <summary>
        /// Wrap a \ref chunk_reader to read a single file
        /// 
        /// Only one wrapper can be used at the same time for each \c base
        /// </summary>
        /// <param name="input">The chunk reader containing the file. It must already be positioned at the file's offset.</param>
        /// <param name="file">Informtion specifying the file to read.</param>
        /// <param name="checksum">Optional pointer to a checksum that is updated as the file is read.
        /// The type of the checksum will be the same as that stored in the file
        /// struct.</param>
        /// <returns>a pointer to a non-seekable input stream for the requested file.</returns>
        public Stream Get(Stream input, InnoFile file, Checksum checksum)
        {
            FilteredStream result = new FilteredStream(input);

            if (file.Filter == CompressionFilter.ZlibFilter)
                result.Push(io::zlib_decompressor(), 8192);

            if (checksum != null)
                result.Push(new ChecksumFilter(checksum, file.Checksum.Type), 8192);

            switch (file.Filter)
            {
                case CompressionFilter.NoFilter:
                    break;
                case CompressionFilter.InstructionFilter4108:
                    result.Push(new InnoExeDecoder4108(), 8192);
                    break;
                case CompressionFilter.InstructionFilter5200:
                    result.Push(new InnoExeDecoder5200(false), 8192);
                    break;
                case CompressionFilter.InstructionFilter5309:
                    result.Push(new InnoExeDecoder5200(true), 8192);
                    break;
                case CompressionFilter.ZlibFilter:
                    // applied *after* calculating the checksum
                    break;
            }

            result.Push(RestrictedSource.Restrict(input, (long)file.Size), 0);

            result.Exceptions(std::ios_base::badbit);

            return result;
        }
    }
}
