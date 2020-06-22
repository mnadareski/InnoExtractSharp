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

using System;
using System.Collections.Generic;
using System.IO;
using InnoExtractSharp.Crypto;
using SharpCompress.Compressors;
using SharpCompress.Compressors.Deflate;

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
        public List<Tuple<object, int>> Get(Stream input, InnoFile file, Checksum checksum)
        {
            var result = new List<Tuple<object, int>>();

            if (file.Filter == CompressionFilter.ZlibFilter)
                result.Add(Tuple.Create(new DeflateStream(input, CompressionMode.Decompress) as object, 8192));

            if (checksum != null)
                result.Add(Tuple.Create(new ChecksumFilter(checksum, file.Checksum.Type) as object, 8192));

            switch (file.Filter)
            {
                case CompressionFilter.NoFilter:
                    break;
                case CompressionFilter.InstructionFilter4108:
                    result.Add(Tuple.Create(new InnoExeDecoder4108() as object, 8192));
                    break;
                case CompressionFilter.InstructionFilter5200:
                    result.Add(Tuple.Create(new InnoExeDecoder5200(false) as object, 8192));
                    break;
                case CompressionFilter.InstructionFilter5309:
                    result.Add(Tuple.Create(new InnoExeDecoder5200(true) as object, 8192));
                    break;
                case CompressionFilter.ZlibFilter:
                    // applied *after* calculating the checksum
                    break;
            }

            result.Add(Tuple.Create(RestrictedSource.Restrict(input, file.Size) as object, 0));

            // result.exception(std::ios_base::badbit);

            return result;
        }
    }
}
