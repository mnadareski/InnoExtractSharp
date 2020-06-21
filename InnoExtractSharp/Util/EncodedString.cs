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

using System.IO;
using System.Text;

namespace InnoExtractSharp.Util
{
    /// <summary>
    /// Wrapper to load a length-prefixed string with a specified encoding from an input stream
    /// into a UTF-8 encoded std::string.
    /// The string length is stored as 32-bit integer.
    /// </summary>
    public class EncodedString
    {
        public string Data;
        public int Codepage;

        public EncodedString(string target, int codepage)
        {
            Data = target;
            Codepage = codepage;
        }

        /// <summary>
        /// Load and convert a length-prefixed string
        /// </summary>
        public static void Load(Stream input, out string target, int codepage)
        {
            try
            {
                using (BinaryReader br = new BinaryReader(input, Encoding.GetEncoding(codepage), true))
                {
                    target = br.ReadString();
                }
            }
            catch
            {
                target = null;
                return;
            }
        }

        /// <summary>
        /// Load and convert a length-prefixed string
        /// </summary>
        public static string Load(Stream input, int codepage)
        {
            Load(input, out string target, codepage);
            return target;
        }
    }
}
