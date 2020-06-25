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
using SevenZip.Compression.LZMA;

// LZMA 1 and 2 (aka xz) descompression filters to be used with boost::iostreams.
namespace InnoExtractSharp.Streams
{
    public class LzmaDecompressor<T> : IFilter where T: LzmaDecompressorImplBase
    {
        public T Decompressor;
        public int BufferSize;

        public LzmaDecompressor(T decomp, int bufferSize = 8192)
        {
            Decompressor = decomp;
            BufferSize = bufferSize;
        }

        public LzmaDecompressor(int bufferSize = 1024)
        {
            BufferSize = bufferSize;
        }

        public static Decoder InitRawLzmaStream(byte[] options)
        {
            Decoder decoder = new Decoder();
            decoder.SetDecoderProperties(options);
            return decoder;
        }

        public int Read(Stream src, byte[] dest, int offset, int n)
        {
            return Decompressor.Filter(src, dest, offset, n);
        }
    }
}
