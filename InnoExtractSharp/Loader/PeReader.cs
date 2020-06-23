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

using System.Collections.Generic;
using System.IO;
using System.Text;

namespace InnoExtractSharp.Loader
{
    /// <summary>
    /// Reader for Win32 binaries
    /// </summary>
    public class PeReader : ExeReader
    {
        private class Header
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

        private class Section
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

        private class SectionTable
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

        private static bool GetResourceTable(ref uint entry, uint offset)
        {
            bool isTable = ((entry & (uint)1 << 31)) != 0;

            entry &= ~(1 << 31);
            entry += offset;

            return isTable;
        }

        private static uint FindResourceEntry(Stream input, uint id)
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
}
