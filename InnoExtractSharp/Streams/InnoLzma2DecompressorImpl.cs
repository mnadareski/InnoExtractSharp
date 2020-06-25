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
using System.IO;

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
        public override bool Filter(Stream src, byte[] dest, int offset, int n)
        {
			// Decode the header.
			if (Decoder == null)
			{
                byte[] options = new byte[5];
                src.Read(options, 0, 5);
				if (options[0] > 40)
					throw new LzmaError("inno lzma2 property error", 7 /* (int)LZMA_FORMAT_ERROR */);

                if (options[0] == 40)
                {
                    options[1] = 0xff;
                    options[2] = 0xff;
                    options[3] = 0xff;
                    options[4] = 0xff;
                }
                else
                {
                    uint dictSize = ((2 | (uint)((options[0]) & 1)) << (options[0] / 2 + 11));
                    byte[] dictSizeArr = BitConverter.GetBytes(dictSize);
                    options[1] = dictSizeArr[0];
                    options[2] = dictSizeArr[1];
                    options[3] = dictSizeArr[2];
                    options[4] = dictSizeArr[3];
                }

                Decoder = InnoLzma2Decompressor.InitRawLzmaStream(options);
			}

            return base.Filter(src, dest, offset, n);
        }
    }
}
