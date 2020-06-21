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
    public class MessageEntry : Entry
    {
        // introduced in 4.2.1

        // UTF-8 encoded name.
        public string Name;

        // Value encoded in the codepage specified at language index.
        public string Value;

        // Index into the default language entry list or -1.
        public int Language;

        public override void Load(Stream input, InnoVersion version, List<Entry> languages)
        {
            using (BinaryReader br = new BinaryReader(input, Encoding.Default, true))
            {
                EncodedString.Load(input, out Name, (int)version.Codepage());
                string rawValue = BinaryString.Load(input);

                Language = br.ReadInt32();

                uint codepage;
                if (Language < 0)
                    codepage = version.Codepage();
                else if (Language >= languages.Count)
                {
                    Value = string.Empty;
                    return;
                }
                else
                    codepage = ((LanguageEntry)languages[Language]).Codepage;

                Value = rawValue; // UTF-8 encode
            }
        }
    }
}
