/*
 * Copyright (C) 2011-2020 Daniel Scharrer
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
using System.Linq;
using System.Text;
using InnoExtractSharp.Crypto;
using InnoExtractSharp.Setup;

namespace InnoExtractSharp.Loader
{
    /// <summary>
    /// Bootstrap data for Inno Setup installers
    /// 
    /// This struct contains information used by the Inno Setup loader to bootstrap the installer.
    /// Some of these values are not available for all Inno Setup versions
    /// 
    /// Inno Setup versions before \c 5.1.5 simply stored a magic number and offset to this bootstrap
    /// data at a fixed position (\c 0x30) in the .exe file.
    /// 
    /// Alternatively, there is no stored bootstrap information and data is stored in external files
    /// while the main setup files contains only the version and headers(header_offset is \c 0).
    /// 
    /// Newer versions use a PE/COFF resource entry to store this bootstrap information.
    /// </summary>
    public class Offsets
    {
        private SetupLoaderVersion[] knownSetupLoaderVersions = new SetupLoaderVersion[]
        {
            new SetupLoaderVersion(new char[] { 'r', 'D', 'l', 'P', 't', 'S', '0', '2', (char)0x87, 'e', 'V', 'x' },    InnoVersion.INNO_VERSION(1, 2, 10)),
            new SetupLoaderVersion(new char[] { 'r', 'D', 'l', 'P', 't', 'S', '0', '4', (char)0x87, 'e', 'V', 'x' },    InnoVersion.INNO_VERSION(4, 0,  0)),
            new SetupLoaderVersion(new char[] { 'r', 'D', 'l', 'P', 't', 'S', '0', '5', (char)0x87, 'e', 'V', 'x' },    InnoVersion.INNO_VERSION(4, 0,  3)),
            new SetupLoaderVersion(new char[] { 'r', 'D', 'l', 'P', 't', 'S', '0', '6', (char)0x87, 'e', 'V', 'x' },    InnoVersion.INNO_VERSION(4, 0, 10)),
            new SetupLoaderVersion(new char[] { 'r', 'D', 'l', 'P', 't', 'S', '0', '7', (char)0x87, 'e', 'V', 'x' },    InnoVersion.INNO_VERSION(4, 1,  6)),
            new SetupLoaderVersion(new char[] { 'r', 'D', 'l', 'P', 't', 'S', (char)0xcd, (char)0xe6, (char)0xd7, '{', (char)0x0b, '*' }, InnoVersion.INNO_VERSION(5, 1,  5)),
            new SetupLoaderVersion(new char[] { 'n', 'S', '5', 'W', '7', 'd', 'T', (char)0x83, (char)0xaa, (char)0x1b, (char)0x0f, 'j' }, InnoVersion.INNO_VERSION(5, 1,  5)),
        };

        private const int ResourceNameInstaller = 11111;

        // TODO: Port this piece of info over to BurnOutSharp
        private const uint SetupLoaderHeaderOffset = 0x30;
        private const uint SetupLoaderHeaderMagic = 0x6f6e6e49; // "Inno"

        /// <summary>
        /// True if we have some indication that this is an Inno Setup file
        /// </summary>
        public bool FoundMagic;

        /// <summary>
        /// Offset of compressed `setup.e32` (the actual installer code)
        /// 
        ///  value of \c 0 means there is no setup.e32 embedded in this file
        /// </summary>
        public uint ExeOffset;

        /// <summary>
        /// Size of `setup.e32` after compression, in bytes
        /// 
        /// A value of \c 0 means the executable size is not known
        /// </summary>
        public uint ExeCompressedSize;

        /// <summary>
        /// Size of `setup.e32` before compression, in bytes
        /// </summary>
        public uint ExeUncompressedSize;

        /// <summary>
        /// Checksum of `setup.e32` before compression
        /// 
        /// Currently this is either a \ref crypto::CRC32 or \ref crypto::Adler32 checksum.
        /// </summary>
        public Checksum ExeChecksum;

        /// <summary>
        /// Offset of embedded setup messages
        /// </summary>
        public uint MessageOffset;

        /// <summary>
        /// Offset of embedded `setup-0.bin` data (the setup headers)
        /// 
        /// This points to a \ref setup::version followed by two compressed blocks of
        /// headers (see \ref stream::block_reader).
        /// 
        /// The headers are described by various structs in the \ref setup namespace.
        /// The first header is always \ref setup::header.
        /// </summary>
        public uint HeaderOffset;

        /// <summary>
        /// Offset of embedded `setup-1.bin` data
        /// 
        /// A value of \c 0 means that the setup data is stored in seprarate files.
        /// 
        /// /ref stream::slice_reader provides a uniform interface to this data, no matter if it
        /// is embedded or split into multiple external files (called slices).
        /// 
        /// The data is made up of one or more compressed or uncompressed chunks
        /// (read by \ref stream::chunk_reader) which in turn each contain the raw data for one or more file.
        /// 
        /// The layout of the chunks and files is stored in the \ref setup::data_entry headers
        /// while the \ref setup::file_entry headers provide the filenames and meta information.
        /// </summary>
        public uint DataOffset;

        /// <summary>
        /// \brief Find the setup loader offsets in a file
        /// Finding the headers always works - if there is no bootstrap information we assume that
        /// this is a file containing only the version and headers.
        /// </summary>
        /// <param name="input">A a seekable stream of the main installer file. This should be the file containing the headers, which is almost always the .exe file.</param>
        public void Load(Stream input)
        {
            FoundMagic = false;

            /*
	         * Try to load the offset table by following a pointer at a constant offset.
	         * This method of storing the offset table is used in versions before 5.1.5
	         */
            if (LoadFromExeFile(input))
                return;

            /*
             * Try to load an offset table located in a PE/COFF (.exe) resource entry.
             * This method of storing the offset table was introduced in version 5.1.5
             */
            if (LoadFromExeResource(input))
                return;

            /*
             * If no offset table has been found, this must be an external setup-0.bin file.
             * In that case, the setup headers start at the beginning of the file.
             */

            ExeCompressedSize = ExeUncompressedSize = ExeOffset = 0; // No embedded setup exe.

            MessageOffset = 0; // No embedded messages.

            HeaderOffset = 0; // Whole file contains just the setup headers.

            DataOffset = 0; // No embedded setup data.
        }

        private bool LoadFromExeFile(Stream input)
        {
            try
            {
                using (BinaryReader br = new BinaryReader(input, Encoding.Default, true))
                {
                    br.BaseStream.Seek(SetupLoaderHeaderOffset, SeekOrigin.Begin);

                    uint magic = br.ReadUInt32();
                    if (magic != SetupLoaderHeaderMagic)
                    {
                        input.Close();
                        return false;
                    }

                    FoundMagic = true;

                    uint offsetTableOffset = br.ReadUInt32();
                    uint notOffsetTableOffset = br.ReadUInt32();
                    if (offsetTableOffset != ~notOffsetTableOffset)
                    {
                        input.Close();
                        return false;
                    }

                    return LoadOffsetsAt(input, offsetTableOffset);
                }
            }
            catch
            {
                input.Close();
                return false;
            }
        }

        private bool LoadFromExeResource(Stream input)
        {
            ExeReader.Resource resource = ExeReader.FindResource(input, ResourceNameInstaller);
            if (!resource)
            {
                input.Close();
                return false;
            }

            FoundMagic = true;

            return LoadOffsetsAt(input, resource.Offset);
        }

        private bool LoadOffsetsAt(Stream input, uint pos)
        {
            try
            {
                using (BinaryReader br = new BinaryReader(input, Encoding.Default, true))
                {
                    br.BaseStream.Seek(pos, SeekOrigin.Begin);

                    byte[] magic = br.ReadBytes(12);

                    uint version = 0;
                    for (int i = 0; i < knownSetupLoaderVersions.Length; i++)
                    {
                        if (magic.Select(b => (char)b).SequenceEqual(knownSetupLoaderVersions[i].Magic))
                        {
                            version = knownSetupLoaderVersions[i].Version;
                            break;
                        }
                    }

                    if (version == 0)
                        version = UInt32.MaxValue;

                    CRC32 checksum = new CRC32();
                    checksum.Init();
                    checksum.Update(magic, 0, magic.Length);

                    if (version >= InnoVersion.INNO_VERSION(5, 1, 5))
                    {
                        uint revision = checksum.LoadUInt32(input);
                        // revision != 1 -> Unexpected revision
                    }

                    checksum.LoadUInt32(input);
                    ExeOffset = checksum.LoadUInt32(input);

                    if (version >= InnoVersion.INNO_VERSION(4, 1, 6))
                        ExeCompressedSize = 0;
                    else
                        ExeCompressedSize = checksum.LoadUInt32(input);

                    ExeUncompressedSize = checksum.LoadUInt32(input);

                    if (version >= InnoVersion.INNO_VERSION(4, 0, 3))
                    {
                        ExeChecksum.Type = ChecksumType.CRC32;
                        ExeChecksum.CRC32 = checksum.LoadUInt32(input);
                    }
                    else
                    {
                        ExeChecksum.Type = ChecksumType.Adler32;
                        ExeChecksum.Adler32 = checksum.LoadUInt32(input);
                    }

                    if (version >= InnoVersion.INNO_VERSION(4, 0, 0))
                        MessageOffset = 0;
                    else
                        MessageOffset = br.ReadUInt32();

                    HeaderOffset = checksum.LoadUInt32(input);
                    DataOffset = checksum.LoadUInt32(input);

                    if (version >= InnoVersion.INNO_VERSION(4, 0, 10))
                    {
                        uint expected = br.ReadUInt32();
                        // checksum.Finalize() != expected -> Checksum mismatch
                    }
                }
            }
            catch
            {
                input.Close();
                return false;
            }
           
            return true;
        }
    }
}
