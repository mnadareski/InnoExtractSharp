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
    /// <summary>
    /// Minimal NE/LE/PE parser that can find resources by ID in binary (exe/dll) files
    /// 
    /// This implementation is optimized to look for exactly one resource.
    /// </summary>
    public class ExeReader
    {
        /// <summary>
        /// Position and size of a resource entry
        /// </summary>
        public class Resource
        {
            /// <summary>
            /// File offset of the resource data in bytes
            /// </summary>
            public uint Offset;

            /// <summary>
            /// Size of the resource data in bytes
            /// </summary>
            public uint Size;

            public static explicit operator bool(Resource res)
            {
                return (res.Offset != 0);
            }

            public static bool operator !(Resource res)
            {
                return (res.Offset == 0);
            }
        }

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

        /// <summary>
        /// Reader for Win32 binaries
        /// </summary>
        public class PeReader : ExeReader
        {
            protected class Header
            {
                /// <summary>
                /// Number of CoffSection structures following this header after optionalHeaderSize bytes
                /// </summary>
                public ushort NSections;

                /// <summary>
                /// Offset of the section table in the file
                /// </summary>
                public uint SectionTableOffet;

                /// <summary>
                /// Virtual memory address of the resource root table
                /// </summary>
                public uint ResourceTableAddress;

                public bool Load(Stream input)
                {
                    try
                    {
                        using (BinaryReader br = new BinaryReader(input, Encoding.Default, true))
                        {
                            br.BaseStream.Seek(2, SeekOrigin.Current); // machine
                            NSections = br.ReadUInt16();
                            br.BaseStream.Seek(4 + 4 + 4, SeekOrigin.Current); // creation time + symbol table offset + nbsymbols
                            ushort optionalHeaderSize = br.ReadUInt16();
                            br.BaseStream.Seek(2, SeekOrigin.Current); // characteristics

                            SectionTableOffet = br.ReadUInt32() + optionalHeaderSize;

                            // Skip the optional header
                            ushort optionalHeaderMagic = br.ReadUInt16();
                            if (optionalHeaderMagic == 0x20b) // PE32+
                                br.BaseStream.Seek(106, SeekOrigin.Current);
                            else
                                br.BaseStream.Seek(90, SeekOrigin.Current);

                            uint ndirectories = br.ReadUInt32();
                            if (ndirectories < 3)
                                return false;

                            int directoryHeaderSize = 4 + 4; // address + size
                            br.BaseStream.Seek(2 * directoryHeaderSize, SeekOrigin.Current);

                            // Virtual memory address and size of the start of resource directory
                            ResourceTableAddress = br.ReadUInt32();
                            uint resourceSize = br.ReadUInt32();
                            if (ResourceTableAddress == 0 || resourceSize == 0)
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

            protected class Section
            {
                /// <summary>
                /// Section size in virtual memory
                /// </summary>
                public uint VirtualSize;

                /// <summary>
                /// Base virtual memory address
                /// </summary>
                public uint VirtualAddress;

                /// <summary>
                /// Base file offset
                /// </summary>
                public uint RawAddress;
            }

            protected class SectionTable
            {
                public List<Section> sections;

                public bool Load(Stream input, Header coff)
                {
                    try
                    {
                        using (BinaryReader br = new BinaryReader(input, Encoding.Default, true))
                        {
                            br.BaseStream.Seek(coff.SectionTableOffet, SeekOrigin.Begin);

                            sections = new List<Section>(coff.NSections);

                            for (int i = 0; i < coff.NSections; i++)
                            {
                                sections[i] = new Section();
                                br.BaseStream.Seek(8, SeekOrigin.Current); // name

                                sections[i].VirtualSize = br.ReadUInt32();
                                sections[i].VirtualAddress = br.ReadUInt32();

                                br.BaseStream.Seek(4, SeekOrigin.Current); // raw size
                                sections[i].RawAddress = br.ReadUInt32();

                                // relocation addr + line number addr + relocation count
                                // + line number count + characteristics
                                br.BaseStream.Seek(4 + 4 + 2 + 2 + 4, SeekOrigin.Current);
                            }

                            return true;
                        }
                    }
                    catch
                    {
                        return false;
                    }
                }

                /// <summary>
                /// Convert a memory address to a file offset according to the given section list.
                /// </summary>
                public uint ToFileOffset(uint address)
                {
                    foreach (Section s in sections)
                    {
                        if (address >= s.VirtualAddress && address < s.VirtualAddress + s.VirtualSize)
                            return address + s.RawAddress - s.VirtualAddress;
                    }

                    return 0;
                }
            }

            public static bool GetResourceTable(ref uint entry, uint offset)
            {
                bool isTable = ((entry & (uint)1 << 31)) != 0;

                entry &= ~(1 << 31);
                entry += offset;

                return isTable;
            }

            public static uint FindResourceEntry(Stream input, uint id)
            {
                try
                {
                    using (BinaryReader br = new BinaryReader(input, Encoding.Default, true))
                    {
                        // skip: characteristics + timestamp + major version + minor version
                        br.BaseStream.Seek(4 + 4 + 2 + 2, SeekOrigin.Current);

                        // Number of named resource entries.
                        ushort nbnames = br.ReadUInt16();

                        // Number of id resource entries
                        ushort nbids = br.ReadUInt16();

                        if (id == (uint)ResourceId.Default)
                        {
                            br.BaseStream.Seek(4, SeekOrigin.Current);
                            return br.ReadUInt32();
                        }

                        // Ignore named resource entries
                        const uint entrySize = 4 + 4; // id / string address + offset
                        br.BaseStream.Seek(nbnames * entrySize, SeekOrigin.Begin);

                        for (int i = 0; i < nbids; i++)
                        {
                            uint entryId = br.ReadUInt32();
                            uint entryOffset = br.ReadUInt32();
                            if (entryId == id)
                                return entryOffset;
                        }

                        return 0;
                    }
                }
                catch
                {
                    return 0;
                }
            }

            public static Resource FindResourceInternal(Stream input, uint name, uint type = (uint)ResourceId.TypeData, uint language = (uint)ResourceId.Default)
            {
                Resource result = new Resource();
                result.Offset = result.Size = 0;

                try
                {
                    using (BinaryReader br = new BinaryReader(input, Encoding.Default, true))
                    {
                        Header coff = new Header();
                        if (!coff.Load(input))
                            return result;

                        SectionTable sections = new SectionTable();
                        if (!sections.Load(input, coff))
                            return result;

                        uint resourceOffset = sections.ToFileOffset(coff.ResourceTableAddress);
                        if (resourceOffset == 0)
                            return result;

                        br.BaseStream.Seek((int)resourceOffset, SeekOrigin.Begin);
                        uint typeOffset = FindResourceEntry(input, type);
                        if (!GetResourceTable(ref typeOffset, resourceOffset))
                            return result;

                        br.BaseStream.Seek((int)typeOffset, SeekOrigin.Begin);
                        uint nameOffset = FindResourceEntry(input, name);
                        if (!GetResourceTable(ref nameOffset, resourceOffset))
                            return result;

                        br.BaseStream.Seek((int)nameOffset, SeekOrigin.Begin);
                        uint leafOffset = FindResourceEntry(input, language);
                        if (leafOffset == 0 || GetResourceTable(ref leafOffset, resourceOffset))
                            return result;

                        // Virtual memory address and size of the resource data.
                        br.BaseStream.Seek(leafOffset, SeekOrigin.Begin);
                        uint dataAddress = br.ReadUInt32();
                        uint dataSize = br.ReadUInt32();

                        // ignore codepage and reserved word
                        uint dataOffset = sections.ToFileOffset(dataAddress);
                        if (dataOffset == 0)
                            return result;

                        result.Offset = dataOffset;
                        result.Size = dataSize;

                        return result;
                    }
                }
                catch
                {
                    return result;
                }
            }

            public static bool GetFileVersionInternal(Stream input)
            {
                Resource res = FindResource(input, (uint)ResourceId.NameVersionInfo, (uint)ResourceId.TypeVersion);
                if (!res)
                    return false;

                return SkipToFixedFileInfo<ushort>(input, res.Offset, 6);
            }
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

        public enum BinaryType : ushort
        {
            UnknownBinary = 0,
            DOSMagic = 0x5a4d, // "MZ"
            OS2Magic = 0x454E, // "NE"
            VXDMagic = 0x454C, // "LE"
            PEMagic = 0x4550, // "PE"
            PEMagic2 = 0x0000, // "\0\0"
        }

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
        public static Resource FindResource(Stream input, uint name, uint type = (uint)ResourceId.TypeData, uint language = (uint)ResourceId.Default)
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

        public static BinaryType DetermineBinaryType(Stream input)
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

        public static bool SkipToFixedFileInfo<T>(Stream input, uint resourceOffset, uint offset)
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
