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
    public class LanguageEntry : Entry
    {
        // introduced in 2.0.1

        public string Name;
        public string LanguageName;
        public string DialogFont;
        public string TitleFont;
        public string WelcomeFont;
        public string CopyrightFont;
        public string Data;
        public string LicenseText;
        public string InfoBefore;
        public string InfoAfter;

        public uint LanguageId;
        public uint Codepage;
        public int DialogFontSize;
        public int DialogFontStandardHeight;
        public int TitleFontSize;
        public int WelcomeFontSize;
        public int CopyrightFontSize;

        public bool RightToLeft;

        public override void Load(Stream input, InnoVersion version, List<Entry> entries = null)
        {
            using (BinaryReader br = new BinaryReader(input, Encoding.Default, true))
            {
                if (version >= InnoVersion.INNO_VERSION(4, 0, 0))
                    EncodedString.Load(input, out Name, (int)version.Codepage());
                else
                    Name = "default";

                EncodedString.Load(input, out LanguageName, (version >= InnoVersion.INNO_VERSION(4, 2, 2)) ? 1200 : 1252);

                EncodedString.Load(input, out DialogFont, (int)version.Codepage());
                EncodedString.Load(input, out TitleFont, (int)version.Codepage());
                EncodedString.Load(input, out WelcomeFont, (int)version.Codepage());
                EncodedString.Load(input, out CopyrightFont, (int)version.Codepage());

                if (version >= InnoVersion.INNO_VERSION(4, 0, 0))
                    BinaryString.Load(input, out Data);

                if (version >= InnoVersion.INNO_VERSION(4, 0, 1))
                {
                    AnsiString.Load(input, out LicenseText);
                    AnsiString.Load(input, out InfoBefore);
                    AnsiString.Load(input, out InfoAfter);
                }
                else
                {
                    LicenseText = string.Empty;
                    InfoBefore = string.Empty;
                    InfoAfter = string.Empty;
                }

                LanguageId = br.ReadUInt32();

                if (version >= InnoVersion.INNO_VERSION(4, 2, 2) && (version < InnoVersion.INNO_VERSION(5, 3, 0) || !version.Unicode))
                    Codepage = br.ReadUInt32();
                else
                    Codepage = 0;

                if (Codepage == 0)
                    Codepage = version.Codepage();

                DialogFontSize = (int)br.ReadUInt32();

                if (version < InnoVersion.INNO_VERSION(4, 1, 0))
                    DialogFontStandardHeight = (int)br.ReadUInt32();
                else
                    DialogFontStandardHeight = 0;

                TitleFontSize = (int)br.ReadUInt32();
                WelcomeFontSize = (int)br.ReadUInt32();
                CopyrightFontSize = (int)br.ReadUInt32();

                if (version >= InnoVersion.INNO_VERSION(5, 2, 3))
                    RightToLeft = br.ReadBoolean();
                else
                    RightToLeft = false;
            }
        }
    }
}
