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

using System.Collections.Generic;
using System.IO;
using System.Text;
using InnoExtractSharp.Util;

namespace InnoExtractSharp.Setup
{
    public class DeleteEntry : Item
    {
        public enum TargetType
        {
            Files = 1 << 0,
            FilesAndSubdirs = 1 << 1,
            DirIfEmpty = 1 << 2,
        }

        public string Name;

        public TargetType Type;

        public override void Load(Stream input, InnoVersion version, List<Entry> entries = null)
        {
            using (BinaryReader br = new BinaryReader(input, Encoding.Default, true))
            {
                if (version < InnoVersion.INNO_VERSION(1, 3, 21))
                    br.ReadUInt32(); // uncompressed size of the entry

                EncodedString.Load(input, out Name, (int)version.Codepage());

                LoadConditionData(input, version);

                LoadVersionData(input, version);

                Type = Stored.DeleteTargetTypes.TryGetValue(br.ReadByte(), TargetType.Files);
            }
        }
    }
}
