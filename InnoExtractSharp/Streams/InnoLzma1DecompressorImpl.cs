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

// LZMA 1 and 2 (aka xz) descompression filters to be used with boost::iostreams.
namespace InnoExtractSharp.Streams
{
    /// <summary>
    /// A filter that decompressess LZMA1 streams found in Inno Setup installers,
    /// to be used with boost::iostreams.
    /// 
    /// The LZMA1 streams used by Inno Setup differ slightly from the LZMA Alone file format:
    /// The stream header only stores the properties (lc, lp, pb) and the dictionary size and
    /// is missing the uncompressed size field. The fiels that are present are encoded
    /// identically.
    /// </summary>
    public class InnoLzma1DecompressorImpl : LzmaDecompressorImplBase
    {
        private byte[] Header = new byte[5];

        public InnoLzma1DecompressorImpl()
        {
        }

        public override bool Filter(Stream src, byte[] dest, int offset, int n)
        {
            // Decode the header.
            if (Decoder == null)
            {
                // Read enough bytes to decode the header.
                src.Read(Header, 0, 5);
                if (Header[0] > (9 * 5 * 5))
                    throw new LzmaError("inno lzma1 property error", 7 /* (int)LZMA_FORMAT_ERROR */);

                Decoder = InnoLzma1Decompressor.InitRawLzmaStream(Header);
            }

            return base.Filter(src, dest, offset, n);
        }

        public override void Close()
        {
            base.Close();
        }
    }
}
