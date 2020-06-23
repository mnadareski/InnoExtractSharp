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
    /// Reader for OS2 binaries
    /// </summary>
    public class NeReader : ExeReader
    {
        public static Resource FindResourceInternal(Stream input, uint name, uint type = (uint)ResourceId.TypeData)
        {
            Resource result = new Resource();
            result.Offset = result.Size = 0;

            try
            {
                using (BinaryReader br = new BinaryReader(input, Encoding.Default, true))
                {
                    br.BaseStream.Seek(0x24 - 2, SeekOrigin.Current); // Already read the magic
                    ushort resourcesOffset = br.ReadUInt16();
                    ushort resourcesEnd = br.ReadUInt16();

                    if (resourcesOffset == resourcesEnd)
                        return result;

                    br.BaseStream.Seek(resourcesOffset - 0x28, SeekOrigin.Current);

                    ushort shift = br.ReadUInt16();
                    if (shift >= 32)
                        return result;

                    ushort nameCount;
                    for (; ; )
                    {
                        ushort typeId = br.ReadUInt16();
                        nameCount = br.ReadUInt16();
                        br.BaseStream.Seek(4, SeekOrigin.Current);
                        if (typeId == 0)
                            return result;

                        if (typeId == (ushort)(type | 0x8000))
                            break;

                        br.BaseStream.Seek(nameCount * 12, SeekOrigin.Current);
                    }

                    for (ushort i = 0; i < nameCount; i++)
                    {
                        ushort offset = br.ReadUInt16();
                        ushort size = br.ReadUInt16();
                        br.BaseStream.Seek(2, SeekOrigin.Current);
                        ushort nameId = br.ReadUInt16();
                        br.BaseStream.Seek(4, SeekOrigin.Current);

                        if (nameId == (ushort)(name | 0x8000))
                        {
                            result.Offset = (uint)offset << shift;
                            result.Size = (uint)size << shift;
                            break;
                        }
                    }
                }
            }
            catch { }

            return result;
        }

        public static bool GetFileVersionInternal(Stream input)
        {
            Resource res = FindResource(input, (uint)ResourceId.NameVersionInfo, (uint)ResourceId.TypeVersion);
            if (!res)
                return false;

            return SkipToFixedFileInfo<byte>(input, res.Offset, 4);
        }
    }
}
