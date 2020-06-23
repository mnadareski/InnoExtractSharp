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

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace InnoExtractSharp.Loader
{
    public enum BinaryType : ushort
    {
        UnknownBinary = 0,
        DOSMagic = 0x5a4d, // "MZ"
        OS2Magic = 0x454E, // "NE"
        VXDMagic = 0x454C, // "LE"
        PEMagic = 0x4550, // "PE"
        PEMagic2 = 0x0000, // "\0\0"
    }

    public enum FileVersion : ulong
    {
        FileVersionUnknown = UInt64.MaxValue,
    }

    public enum ResourceId : uint
    {
        NameVersionInfo = 1,
        TypeCursor = 1,
        TypeBitmap = 2,
        TypeIcon = 3,
        TypeMenu = 4,
        TypeDialog = 5,
        TypeString = 6,
        TypeFontDir = 7,
        TypeFont = 8,
        TypeAccelerator = 9,
        TypeData = 10,
        TypeMessageTable = 11,
        TypeGroupCursor = 12,
        TypeGroupIcon = 14,
        TypeVersion = 16,
        TypeDlgInclude = 17,
        TypePlugPlay = 19,
        TypeVXD = 20,
        TypeAniCursor = 21,
        TypeAniIcon = 22,
        TypeHTML = 23,
        Default = UInt32.MaxValue,
    }

    /// <summary>
    /// Minimal NE/LE/PE parser that can find resources by ID in binary (exe/dll) files
    /// 
    /// This implementation is optimized to look for exactly one resource.
    /// </summary>
    public class ExeReader
    {
        /// <summary>
        /// \brief Find where a resource with a given ID is stored in a NE or PE binary.
        /// 
        /// Resources are addressed using a (\pname{name}, \pname{type}, \pname{language}) tuple.
        /// </summary>
        /// <param name="input">a seekable stream of the binary containing the resource</param>
        /// <param name="name">the user-defined name of the resource<param>
        /// <param name="type">the type of the resource</param>
        /// <param name="language">the localised variant of the resource</param>
        /// <returns>vthe location of the resource or `(0, 0)` if the requested resource does not exist.</returns>
        internal static Resource FindResource(Stream input, uint name, uint type = (uint)ResourceId.TypeData, uint language = (uint)ResourceId.Default)
        {
            BinaryType bintype = DetermineBinaryType(input);
            switch(bintype)
            {
                case BinaryType.OS2Magic:
                    return NeReader.FindResourceInternal(input, name, type);
                case BinaryType.PEMagic:
                    return PeReader.FindResourceInternal(input, name, type, language);
                default:
                    return new Resource() { Offset = 0, Size = 0, };
            }
        }

        /// <summary>
        /// Get the file version number of a NE, LE or PE binary.
        /// </summary>
        /// <param name="input">a seekable stream of the binary file containing the resource</param>
        /// <returns>the file version number or FileVersionUnknown.</returns>
        public static ulong GetFileVersion(Stream input)
        {
            bool found = false;
            BinaryType bintype = DetermineBinaryType(input);
            switch(bintype)
            {
                case BinaryType.OS2Magic:
                    found = NeReader.GetFileVersionInternal(input);
                    break;
                case BinaryType.VXDMagic:
                    found = LeReader.GetFileVersionInternal(input);
                    break;
                case BinaryType.PEMagic:
                    found = PeReader.GetFileVersionInternal(input);
                    break;
            }

            if (!found)
                return (ulong)FileVersion.FileVersionUnknown;

            try
            {
                using (BinaryReader br = new BinaryReader(input, Encoding.Default, true))
                {
                    uint magic = br.ReadUInt32();
                    if (magic != 0xfeef04bd)
                        return (ulong)FileVersion.FileVersionUnknown;

                    br.BaseStream.Seek(4, SeekOrigin.Current); // skip struct version
                    uint fileVersionMs = br.ReadUInt32();
                    uint fileVersionLs = br.ReadUInt32();

                    return ((ulong)(fileVersionMs) << 32) | (ulong)(fileVersionLs);
                }
            }
            catch
            {
                return (ulong)FileVersion.FileVersionUnknown;
            }                
        }

        internal static BinaryType DetermineBinaryType(Stream input)
        {
            BinaryType defaultType = BinaryType.UnknownBinary;
            try
            {
                using (BinaryReader br = new BinaryReader(input, Encoding.Default, true))
                {
                    br.BaseStream.Seek(0, SeekOrigin.Begin);
                    if (br.ReadUInt16() != (ushort)BinaryType.DOSMagic)
                        return BinaryType.UnknownBinary; // Not a DOS file

                    // Skip the DOS stub
                    defaultType = BinaryType.DOSMagic;
                    br.BaseStream.Seek(0x3c, SeekOrigin.Begin);
                    ushort newOffset = br.ReadUInt16();

                    ushort newMagic = br.ReadUInt16();
                    if (newMagic == (ushort)BinaryType.PEMagic)
                    {
                        ushort pe2Magic = br.ReadUInt16();
                        if (pe2Magic != (ushort)BinaryType.PEMagic2)
                            return BinaryType.DOSMagic;
                    }

                    return (BinaryType)newMagic;
                }
            }
            catch
            {
                return defaultType;
            }
        }

        internal static bool SkipToFixedFileInfo<T>(Stream input, uint resourceOffset, uint offset)
        {
            try
            {
                using (BinaryReader br = new BinaryReader(input, Encoding.Default, true))
                {
                    br.BaseStream.Seek((int)(resourceOffset + offset), SeekOrigin.Begin);
                    var tt = typeof(T);
                    ulong key = 0;
                    do
                    {
                        if (tt == typeof(byte))
                        {
                            key = br.ReadByte();
                            offset += sizeof(byte);
                        }
                        else if (tt == typeof(ushort))
                        {
                            key = br.ReadUInt16();
                            offset += sizeof(ushort);
                        }
                        else if (tt == typeof(uint))
                        {
                            key = br.ReadUInt32();
                            offset += sizeof(uint);
                        }
                        else if (tt == typeof(ulong))
                        {
                            key = br.ReadUInt64();
                            offset += sizeof(ulong);
                        }
                    } while (key == 0);

                    // Align to DWORD
                    offset = (offset + 3) & (uint)(3);

                    br.BaseStream.Seek((int)(resourceOffset + offset), SeekOrigin.Begin);

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
