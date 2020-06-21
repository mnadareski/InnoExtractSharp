/*
 * Copyright (C) 2011-2019 Daniel Scharrer
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
    // Introduced in 2.0.0
    public class ComponentEntry : Entry
    {
        [Flags]
        public enum NamedFlags
        {
            Fixed = 1 << 0,
            Restart = 1 << 1,
            DisableNoUninstallWarning = 1 << 2,
            Exclusive = 1 << 3,
            DontInheritCheck = 1 << 4,
        }

        public string Name;
        public string Description;
        public string Types;
        public string Languages;
        public string Check;

        public ulong ExtraDiskSpaceRequired;

        public int Level;
        public bool Used;

        public WindowsVersionRange Winver;

        public NamedFlags Options;

        public ulong Size;

        public override void Load(Stream input, Info i)
        {
            using (BinaryReader br = new BinaryReader(input, Encoding.GetEncoding((int)version.Codepage()), true))
            {
                EncodedString.Load(input, out Name, (int)i.Codepage());

                EncodedString.Load(input, out Description, (int)version.Codepage());

                EncodedString.Load(input, out Types, (int)version.Codepage());

                if (version >= InnoVersion.INNO_VERSION(4, 0, 1))
                    EncodedString.Load(input, out Languages, (int)version.Codepage());
                else
                    Languages = string.Empty;

                if ()

                if (version >= InnoVersion.INNO_VERSION_EXT(3, 0, 6, 1))
                    EncodedString.Load(input, out Check, (int)version.Codepage());
                else
                    Check = string.Empty;

                if (version >= InnoVersion.INNO_VERSION(4, 0, 0))
                    ExtraDiskSpaceRequired = br.ReadUInt64();
                else
                    ExtraDiskSpaceRequired = br.ReadUInt32();

                if (version >= InnoVersion.INNO_VERSION_EXT(3, 0, 6, 1))
                {
                    Level = br.ReadInt32();
                    Used = br.ReadBoolean();
                }
                else
                {
                    Level = 0;
                    Used = true;
                }

                Winver.Load(input, version);

                if (version >= InnoVersion.INNO_VERSION(4, 2, 3))
                    Options = (NamedFlags)(br.ReadUInt32() & (uint)Stored.ComponentFlags2);
                else if (version >= InnoVersion.INNO_VERSION_EXT(3, 0, 6, 1))
                    Options = (NamedFlags)(br.ReadUInt32() & (uint)Stored.ComponentFlags1);
                else
                    Options = (NamedFlags)(br.ReadUInt32() & (uint)Stored.ComponentFlags0);

                if (version >= InnoVersion.INNO_VERSION(4, 0, 0))
                    Size = br.ReadUInt64();
                else
                    Size = br.ReadUInt32();
            }
        }
    }
}
