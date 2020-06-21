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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public virtual bool Filter(int beginIn, int endIn, int beginOut, int endOut, bool flush)
        {

        }

        public virtual void Close();
    }

    public class InnoLzma1DecompressorImpl : LzmaDecompressorImplBase
    {
        private int Nread; // Number of bytes read into header
        private char[] Header = new char[5];

        public InnoLzma1DecompressorImpl()
        {
            Nread = 0;
        }

        public override bool Filter(int beginIn, int endIn, int beginOut, int endOut, bool flush)
        {
            throw new NotImplementedException();
        }

        public override void Close()
        {
            base.Close();
            Nread = 0;
        }
    }

    public class InnoLzma2DecompressorImpl : LzmaDecompressorImplBase
    {
        public override bool Filter(int beginIn, int endIn, int beginOut, int endOut, bool flush)
        {
            throw new NotImplementedException();
        }
    }

    public class LzmaDecompressor
    {
        public LzmaDecompressorImplBase Decompressor;
        public int BufferSize;

        public LzmaDecompressor(LzmaDecompressorImplBase decomp, int bufferSize = 8192)
        {
            Decompressor = decomp;
            BufferSize = bufferSize;
        }
    }
}
