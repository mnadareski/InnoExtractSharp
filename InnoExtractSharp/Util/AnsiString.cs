﻿/*
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

using System.IO;

namespace InnoExtractSharp.Util
{
    /// <summary>
    /// Convenience specialization of \ref encoded_string for loading Windows-1252 strings
    /// </summary>
    /// <remarks>This function is not thread-safe.</remarks>
    public class AnsiString : EncodedString
    {
        public AnsiString(string target)
            : base(target, KnownCodepage.cp_windows1252)
        {
        }

        /// <summary>
        /// Load and convert a length-prefixed string
        /// </summary>
        public static void Load(Stream input, out string target)
        {
            Load(input, out target, KnownCodepage.cp_windows1252);
        }
    }
}
