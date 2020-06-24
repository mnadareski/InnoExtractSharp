/*
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

using System.Collections;
using System.IO;
using System.Text;

namespace InnoExtractSharp.Util
{
    /// <summary>
    /// Wrapper to load a length-prefixed string with a specified encoding from an input stream
    /// into a UTF-8 encoded std::string.
    /// The string length is stored as 32-bit integer.
    /// 
    /// You can also use the \ref ansi_string convenience wrapper for Windows-1252 strings.
    /// </summary>
    public class EncodedString
    {
        public string Data;
        public KnownCodepage Codepage;
        public BitArray LeadByteSet;

        /// <param name="target">The std::string object to receive the loaded UTF-8 string.</param>
        /// <param name="codepage">The Windows codepage for the encoding of the stored string.</param>
        public EncodedString(string target, KnownCodepage codepage)
        {
            Data = target;
            Codepage = codepage;
            LeadByteSet = null;
        }

        /// <param name="target">The std::string object to receive the loaded UTF-8 string.</param>
        /// <param name="codepage">The Windows codepage for the encoding of the stored string.</param>
        /// <param name="leadBytes">Preserve 0x5C path separators.</param>
        public EncodedString(string target, KnownCodepage codepage, BitArray leadBytes)
        {
            Data = target;
            Codepage = codepage;
            LeadByteSet = leadBytes;
        }

        /// <summary>
        /// Load and convert a length-prefixed string
        /// </summary>
        /// <remarks>This function is not thread-safe.</remarks>
        public static void Load(Stream input, out string target, KnownCodepage codepage, BitArray leadBytes = null)
        {
            BinaryString.Load(input, out target);
            Utility.ToUtf8(target, codepage);
        }

        /// <summary>
        /// Load and convert a length-prefixed string
        /// </summary>
        /// <remarks>This function is not thread-safe.</remarks>
        public static string Load(Stream input, KnownCodepage codepage, BitArray leadBytes = null)
        {
            Load(input, out string target, codepage, leadBytes);
            return target;
        }
    }
}
