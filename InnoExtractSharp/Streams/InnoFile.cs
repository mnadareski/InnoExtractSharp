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

using InnoExtractSharp.Crypto;

namespace InnoExtractSharp.Streams
{
    public enum CompressionFilter
    {
        NoFilter,
        InstructionFilter4108,
        InstructionFilter5200,
        InstructionFilter5309,
        ZlibFilter,
    }

    /// <summary>
    /// Information specifying a single file inside a compressed chunk.
    /// 
    /// This data is stored in \ref setup::data_entry "data entries".
    /// 
    /// Files specified by this struct can be read using \ref file_reader.
    /// </summary>
    public class InnoFile
    {
        public ulong Offset; // Offset of this file within the decompressed chunk.
        public ulong Size; // Pre-filter size of this file in the decompressed chunk.

        public Checksum Checksum; // Checksum for the file.

        public CompressionFilter Filter; // Additional filter used before compression

        public static bool operator <(InnoFile if1, InnoFile if2)
        {
            if (if1.Offset != if2.Offset)
                return if1.Offset < if2.Offset;
            else if (if1.Size != if2.Size)
                return if1.Size < if2.Size;
            else if (if1.Filter != if2.Filter)
                return if1.Filter < if2.Filter;

            return false;
        }
        public static bool operator >(InnoFile if1, InnoFile if2)
        {
            return if2 < if1;
        }

        public static bool operator ==(InnoFile if1, InnoFile if2)
        {
            return (if1.Offset == if2.Offset
                && if1.Size == if2.Size
                && if1.Filter == if2.Filter);
        }
        public static bool operator !=(InnoFile if1, InnoFile if2)
        {
            return !(if1 == if2);
        }
    }
}
