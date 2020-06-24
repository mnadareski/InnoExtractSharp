/*
 * Copyright (C) 2011-2019 Daniel Scharrer
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
using System.IO;

// LZMA 1 and 2 (aka xz) descompression filters to be used with boost::iostreams.
namespace InnoExtractSharp.Streams
{
    public unsafe abstract class LzmaDecompressorImplBase
    {
        protected Stream Stream;

        // Abstract base class, subclasses need to intialize stream.
        protected LzmaDecompressorImplBase()
        {
            Stream = null;
        }

        ~LzmaDecompressorImplBase()
        {
            Close();
        }

        public virtual bool Filter(char* beginIn, char* endIn, char* beginOut, char* endOut, bool flush)
        {
            LzmaStream strm = Stream as LzmaStream;

            strm.NextIn = (byte*)beginIn;
            strm.AvailIn = (int)(endIn - beginIn);

            strm.NextOut = (byte*)beginOut;
            strm.AvailOut = (int)(endOut - beginOut);

            LzmaRet ret = LzmaCode(strm, LZMA_RUN);

            if (flush && ret == LZMA_BUF_ERROR && strm.AvailOut > 0)
            {
                throw new LzmaError("truncated lzma stream", (int)ret);
            }

            beginIn = (char*)strm.NextIn;
            beginOut = (char*)strm.NextOut;

            if (ret != LZMA_OK && ret != LZMA_STREAM_END && ret != LZMA_BUF_ERROR)
            {
                throw new LzmaError("lzma decrompression error", (int)ret);
            }

            return (ret != LZMA_STREAM_END);
        }

        public virtual void Close()
        {
            if (Stream != null)
            {
                LzmaStream strm = Stream as LzmaStream;
                LzmaEnd(strm);
                strm.Dispose(); Stream.Dispose();
            }
        }
    }
}
