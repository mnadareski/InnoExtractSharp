﻿/*
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
    public unsafe class InnoLzma2DecompressorImpl : LzmaDecompressorImplBase
    {
        public override bool Filter(char* beginIn, char* endIn, char* beginOut, char* endOut, bool flush)
        {
			// Decode the header.
			if (Stream == null)
			{
				if (beginIn == endIn)
					return true;

				LzmaOptionsLzma options = new LzmaOptionsLzma();

				byte prop = (byte)*beginIn++;
				if (prop > 40)
					throw new LzmaError("inno lzma2 property error", (int)LZMA_FORMAT_ERROR);

                if (prop == 40)
                    options.DictSize = 0xffffffff;
                else
                    options.DictSize = ((2 | (uint)((prop) & 1)) << (prop / 2 + 11));

                Stream = InnoLzma2Decompressor.InitRawLzmaStream(LZMA_FILTER_LZMA2, options);
			}

			return base.Filter(beginIn, endIn, beginOut, endOut, flush);
		}
    }
}
