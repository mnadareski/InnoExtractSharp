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

using System.IO;
using SharpCompress.Compressors.LZMA;

// LZMA 1 and 2 (aka xz) descompression filters to be used with boost::iostreams.
namespace InnoExtractSharp.Streams
{
    public abstract class LzmaDecompressorImplBase
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

        public virtual bool Filter(ref char[] input, int beginIn, int endIn, int beginOut, int endOut, bool flush)
        {
            LzmaStream strm = Stream as LzmaStream;

            /*
            // TODO: Figure out what any of this does
            strm->next_in = reinterpret_cast <const boost::uint8_t*> (begin_in);
            strm->avail_in = size_t(end_in - begin_in);

            strm->next_out = reinterpret_cast<boost::uint8_t*>(begin_out);
            strm->avail_out = size_t(end_out - begin_out);

            lzma_ret ret = lzma_code(strm, LZMA_RUN);

            if (flush && ret == LZMA_BUF_ERROR && strm->avail_out > 0)
            {
                throw lzma_error("truncated lzma stream", ret);
            }

            begin_in = reinterpret_cast <const char*> (strm->next_in);
            begin_out = reinterpret_cast<char*>(strm->next_out);

            if (ret != LZMA_OK && ret != LZMA_STREAM_END && ret != LZMA_BUF_ERROR)
            {
                throw lzma_error("lzma decrompression error", ret);
            }

            return (ret != LZMA_STREAM_END);
            */

            return true;
        }

        public virtual void Close()
        {
            if (Stream != null)
            {
                Stream.Close();
                Stream = null;
            }
        }
    }
}
