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
using System.Text;
using InnoExtractSharp.Util;

namespace InnoExtractSharp.Setup
{
    public class DirectoryEntry : Item
    {
        [Flags]
        public enum Flags
        {
            NeverUninstall = 1 << 0,
            DeleteAfterInstall = 1 << 1,
            AlwaysUninstall = 1 << 2,
            SetNtfsCompression = 1 << 3,
            UnsetNtfsCompression = 1 << 4,
        }

        public string Name;
        public string Permissions;

        public uint Attributes;

        /// <summary>
        /// Index into the permission entry list
        /// </summary>
        public short Permission;

        public Flags Options;

        public override void Load(Stream input, InnoVersion version, List<Entry> entries = null)
        {
            using (BinaryReader br = new BinaryReader(input, Encoding.Default, true))
            {
                if (version < InnoVersion.INNO_VERSION(1, 3, 21))
                    br.ReadUInt32(); // uncompressed size of the entry

                EncodedString.Load(input, out Name, (int)version.Codepage());

                LoadConditionData(input, version);

                if (version >= InnoVersion.INNO_VERSION(4, 0, 11) && version < InnoVersion.INNO_VERSION(4, 1, 0))
                    EncodedString.Load(input, out Permissions, (int)version.Codepage());
                else
                    Permissions = string.Empty;

                if (version >= InnoVersion.INNO_VERSION(2, 0, 11))
                    Attributes = br.ReadUInt32();
                else
                    Attributes = 0;

                LoadVersionData(input, version);

                if (version >= InnoVersion.INNO_VERSION(4, 1, 0))
                    Permission = br.ReadInt16();
                else
                    Permission = -1;

                if (version >= InnoVersion.INNO_VERSION(5, 2, 0))
                    Options = (Flags)(br.ReadUInt32() & (uint)Stored.InnoDirectoryOptions1);
                else
                    Options = (Flags)(br.ReadUInt32() & (uint)Stored.InnoDirectoryOptions0);
            }
        }
    }
}
