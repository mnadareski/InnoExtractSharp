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
    public class TypeEntry : Entry
    {
        // introduced in 2.0.0

        public enum SetupType
        {
            User,
            DefaultFull,
            DefaultCompact,
            DefaultCustom,
        }

        [Flags]
        public enum TypeFlags
        {
            CustomSetupType = 1 << 0,
        }

        public string Name;
        public string Description;
        public string Languages;
        public string Check;

        public WindowsVersionRange Winver;

        public bool CustomType;

        public SetupType Type;

        public ulong Size;

        public override void Load(Stream input, InnoVersion version, List<Entry> entries = null)
        {
            using (BinaryReader br = new BinaryReader(input, Encoding.Default, true))
            {
                EncodedString.Load(input, out Name, (int)version.Codepage());

                EncodedString.Load(input, out Description, (int)version.Codepage());
                if (version >= InnoVersion.INNO_VERSION(4, 0, 1))
                    EncodedString.Load(input, out Languages, (int)version.Codepage());
                else
                    Languages = string.Empty;
                if (version >= InnoVersion.INNO_VERSION_EXT(3, 0, 6, 1))
                    EncodedString.Load(input, out Check, (int)version.Codepage());
                else
                    Check = string.Empty;

                Winver.Load(input, version);

                TypeFlags options = (TypeFlags)(br.ReadUInt32() & (uint)Stored.TypeFlags);
                CustomType = ((options & TypeFlags.CustomSetupType) != 0);

                if (version >= InnoVersion.INNO_VERSION(4, 0, 3))
                    Type = Stored.SetupTypes.TryGetValue(br.ReadByte(), SetupType.User);
                else
                    Type = SetupType.User;

                if (version >= InnoVersion.INNO_VERSION(4, 0, 0))
                    Size = br.ReadUInt64();
                else
                    Size = br.ReadUInt32();
            }
        }
    }
}
