/*
 * Copyright (C) 2011-2014 Daniel Scharrer
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
using System.Text;

namespace InnoExtractSharp.Loader
{
    /// <summary>
    /// Reader for VXD binaries
    /// </summary>
    public class LeReader : ExeReader
    {
        public static bool GetFileVersionInternal(Stream input)
        {
            try
            {
                using (BinaryReader br = new BinaryReader(input, Encoding.Default, true))
                {
                    br.BaseStream.Seek(0xb8 - 2, SeekOrigin.Current); // Already read the magic
                    uint resourcesOffset = br.ReadUInt32();
                    uint resourcesSize = br.ReadUInt32();

                    if (resourcesSize <= 12)
                        return false;

                    br.BaseStream.Seek(resourcesOffset, SeekOrigin.Begin);
                    byte type = br.ReadByte();
                    ushort id = br.ReadUInt16();
                    byte name = br.ReadByte();
                    br.BaseStream.Seek(4, SeekOrigin.Current); // skip ordinal + flags
                    uint size = br.ReadUInt32();
                    if (type != 0xff || id != 16 || name != 0xff || size <= 20 + 52)
                        return false;

                    ushort node = br.ReadUInt16();
                    ushort data = br.ReadUInt16();
                    br.BaseStream.Seek(16, SeekOrigin.Current); // skip key
                    if (node < 20 + 52 || data < 52)
                        return false;

                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
