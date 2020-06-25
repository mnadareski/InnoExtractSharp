﻿/*
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

namespace InnoExtractSharp.Setup
{
    public class KnownVersion
    {
        public char[] Name = new char[64];

        public uint Version;
        public InnoVersion.Flags Variant;

        public KnownVersion(string name, uint version, InnoVersion.Flags variant)
        {
            Name = name.Substring(0, Math.Min(name.Length, 64)).PadRight(64, ' ').ToCharArray();
            Version = version;
            Variant = variant;
        }

        public static implicit operator uint(KnownVersion version)
        {
            return version.Version;
        }
    }
}
