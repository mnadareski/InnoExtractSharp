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
    /// A filter that decompressess LZMA2 streams found in Inno Setup installers,
    /// to be used with boost::iostreams.
    /// 
    /// Inno Setup uses raw LZMA2 streams.
    /// (preceded only by the dictionary size encoded as one byte)
    /// </summary>
    public class InnoLzma2DecompressorImpl : LzmaDecompressorImplBase
    {
        public override bool Filter(ref char[] input, int beginIn, int endIn, int beginOut, int endOut, bool flush)
        {
			// Decode the header.
			if (Stream == null)
			{
				if (beginIn == endIn)
					return true;

				LzmaEncoderProperties options = new LzmaEncoderProperties();

				byte prop = (byte)input[beginIn++];
				if (prop > 40)
					throw new LzmaError("inno lzma2 property error", 7 /* LZMA_FORMAT_ERROR */);

				// TODO: Figure out these options
				//if (prop == 40)
				//	options.dict_size = 0xffffffff;
				//else
				//	options.dict_size = ((boost::uint32_t(2) | boost::uint32_t((prop) & 1)) << ((prop) / 2 + 11));

				Stream = LzmaDecompressor.InitRawLzmaStream(true, options);
			}

			return base.Filter(ref input, beginIn, endIn, beginOut, endOut, flush);
		}
    }
}
