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
using InnoExtractSharp.Util;

namespace InnoExtractSharp.Setup
{
    public class Item : Entry
    {
        public string Components;
        public string Tasks;
        public string Languages;
        public string Check;

        public string AfterInstall;
        public string BeforeInstall;

        public WindowsVersionRange Winver;

        public override void Load(Stream input, InnoVersion version, List<Entry> entries = null)
        {
            // No-op, overriden in inheriting classes
        }

        protected void LoadConditionData(Stream input, InnoVersion version)
        {
            if (version >= InnoVersion.INNO_VERSION(2, 0, 0))
            {
                EncodedString.Load(input, out Components, (int)version.Codepage());
                EncodedString.Load(input, out Tasks, (int)version.Codepage());
            }
            else
            {
                Components = string.Empty;
                Tasks = string.Empty;
            }
            if (version >= InnoVersion.INNO_VERSION(4, 0, 1))
                EncodedString.Load(input, out Languages, (int)version.Codepage());
            else
                Languages = string.Empty;

            if (version >= InnoVersion.INNO_VERSION_EXT(3, 0, 6, 1))
                EncodedString.Load(input, out Check, (int)version.Codepage());
            else
                Check = string.Empty;

            if (version >= InnoVersion.INNO_VERSION(4, 1, 0))
            {
                EncodedString.Load(input, out AfterInstall, (int)version.Codepage());
                EncodedString.Load(input, out BeforeInstall, (int)version.Codepage());
            }
            else
            {
                AfterInstall = string.Empty;
                BeforeInstall = string.Empty;
            }
        }

        protected void LoadVersionData(Stream input, InnoVersion version)
        {
            Winver.Load(input, version);
        }
    }
}
