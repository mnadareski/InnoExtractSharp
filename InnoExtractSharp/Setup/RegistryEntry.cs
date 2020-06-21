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
    public class RegistryEntry : Item
    {
        [Flags]
        public enum Flags
        {
            CreateValueIfDoesntExist = 1 << 0,
            UninsDeleteValue = 1 << 1,
            UninsClearValue = 1 << 2,
            UninsDeleteEntireKey = 1 << 3,
            UninsDeleteEntireKeyIfEmpty = 1 << 4,
            PreserveStringType = 1 << 5,
            DeleteKey = 1 << 6,
            DeleteValue = 1 << 7,
            NoError = 1 << 8,
            DontCreateKey = 1 << 9,
            Bits32 = 1 << 10,
            Bits64 = 1 << 11,
        }

        public enum HiveName
        {
            HKCR,
            HKCU,
            HKLM,
            HKU,
            HKPD,
            HKCC,
            HKDD,
            Unset,
        }

        public enum ValueType
        {
            None,
            String,
            ExpandString,
            DWord,
            Binary,
            MultiString,
            QWord,
        }

        public string Key;
        public string Name; // empty string means (Default) key
        public string Value;

        public string Permissions;

        public HiveName Hive;

        public int Permission; // index into the permission entry list

        public ValueType Type;

        public Flags Options;

        public override void Load(Stream input, InnoVersion version, List<Entry> entries = null)
        {
            using (BinaryReader br = new BinaryReader(input, Encoding.Default, true))
            {
                if (version < InnoVersion.INNO_VERSION(1, 3, 21))
                    br.ReadUInt32(); // uncompressed size of the entry

                EncodedString.Load(input, out Key, (int)version.Codepage());
                if (version.Bits != 16)
                    EncodedString.Load(input, out Name, (int)version.Codepage());
                else
                    Name = "(Default)";

                EncodedString.Load(input, out Value, (int)version.Codepage());

                LoadConditionData(input, version);

                if (version >= InnoVersion.INNO_VERSION(4, 0, 11) && version < InnoVersion.INNO_VERSION(4, 1, 0))
                    EncodedString.Load(input, out Permissions, (int)version.Codepage());
                else
                    Permissions = string.Empty;

                LoadVersionData(input, version);

                if (version.Bits != 16)
                    Hive = (HiveName)(br.ReadUInt32() & ~0x80000000);
                else
                    Hive = HiveName.Unset;

                if (version >= InnoVersion.INNO_VERSION(4, 1, 0))
                    Permission = br.ReadInt16();
                else
                    Permission = -1;

                if (version >= InnoVersion.INNO_VERSION(5, 2, 5))
                    Type = Stored.RegistryEntryTypes2.TryGetValue(br.ReadByte(), ValueType.None);
                else if (version.Bits != 16)
                    Type = Stored.RegistryEntryTypes1.TryGetValue(br.ReadByte(), ValueType.None);
                else
                    Type = Stored.RegistryEntryTypes0.TryGetValue(br.ReadByte(), ValueType.None);

                Flags flagreader = 0;

                if (version.Bits != 16)
                {
                    flagreader |= Flags.CreateValueIfDoesntExist;
                    flagreader |= Flags.UninsDeleteValue;
                }
                flagreader |= Flags.UninsClearValue;
                flagreader |= Flags.UninsDeleteEntireKey;
                flagreader |= Flags.UninsDeleteEntireKeyIfEmpty;
                flagreader |= Flags.PreserveStringType;
                if (version >= InnoVersion.INNO_VERSION(1, 3, 21))
                {
                    flagreader |= Flags.DeleteKey;
                    flagreader |= Flags.DeleteValue;
                    flagreader |= Flags.NoError;
                    flagreader |= Flags.DontCreateKey;
                }
                if (version >= InnoVersion.INNO_VERSION(5, 1, 0))
                {
                    flagreader |= Flags.Bits32;
                    flagreader |= Flags.Bits64;
                }

                Options = flagreader;
            }
        }
    }
}
