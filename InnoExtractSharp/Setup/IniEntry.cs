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
    public class IniEntry : Item
    {
        [Flags]
        public enum Flags
        {
            CreateKeyIfDoesntExist = 1 << 0,
            UninsDeleteEntry = 1 << 1,
            UninsDeleteEntireSection = 1 << 2,
            UninsDeleteSectionIfEmpty = 1 << 3,
            HasValue = 1 << 4,
        }

        public string Inifile;
        public string Section;
        public string Key;
        public string Value;

        public Flags Options;

        public override void Load(Stream input, InnoVersion version, List<Entry> entries)
        {
            using (BinaryReader br = new BinaryReader(input, Encoding.Default, true))
            {
                if (version < InnoVersion.INNO_VERSION(1, 3, 21))
                    br.ReadUInt32(); // uncompressed size of the entry

                EncodedString.Load(input, out Inifile, (int)version.Codepage());

                if (String.IsNullOrWhiteSpace(Inifile))
                    Inifile = "{windows}/WIN.INI";

                EncodedString.Load(input, out Section, (int)version.Codepage());
                EncodedString.Load(input, out Key, (int)version.Codepage());
                EncodedString.Load(input, out Value, (int)version.Codepage());

                LoadConditionData(input, version);

                LoadVersionData(input, version);

                if (version.Bits != 16)
                    Options = (Flags)(br.ReadUInt32() & (uint)Stored.IniFlags);
                else
                    Options = (Flags)(br.ReadUInt16() & (ushort)Stored.IniFlags);
            }
        }
    }
}
