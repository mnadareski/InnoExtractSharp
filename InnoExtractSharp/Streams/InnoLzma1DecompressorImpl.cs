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

using SharpCompress.Compressors.LZMA;

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
        private int nread; // Number of bytes read into header
        private char[] Header = new char[5];

        public InnoLzma1DecompressorImpl()
        {
            nread = 0;
        }

        public override bool Filter(ref char[] input, int beginIn, int endIn, int beginOut, int endOut, bool flush)
        {
            // Decode the header.
            if (Stream == null)
            {
                // Read enough bytes to decode the header.
                while (nread != 5)
                {
                    if (beginIn == endIn)
                        return true;

                    Header[nread++] = input[beginIn++];
                }

                LzmaEncoderProperties options = new LzmaEncoderProperties();

                byte properties = (byte)Header[0];
                if (properties > (9 * 5 * 5))
                    throw new LzmaError("inno lzma1 property error", 7 /* LZMA_FORMAT_ERROR */);

                // TODO: Figure out these options
                //options.pb = boost::uint32_t(properties / (9 * 5));
                //options.lp = boost::uint32_t((properties % (9 * 5)) / 9);
                //options.lc = boost::uint32_t(properties % 9);

                //options.dict_size = util::little_endian::load<boost::uint32_t>(header + 1);

                Stream = LzmaDecompressor.InitRawLzmaStream(false, options);
            }

            return base.Filter(ref input, beginIn, endIn, beginOut, endOut, flush);
        }

        public override void Close()
        {
            base.Close();
            nread = 0;
        }
    }
}
