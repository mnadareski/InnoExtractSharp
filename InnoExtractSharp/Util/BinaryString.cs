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

using System;
using System.IO;
using System.Text;

namespace InnoExtractSharp.Util
{
    /// <summary>
    /// Wrapper to load a length-prefixed string from an input stream into a std::string.
    /// The string length is stored as 32-bit integer.
    /// 
    /// Use \ref encoded_string to also convert the string to UTF-8.
    /// </summary>
    public class BinaryString
    {
        public string Data;

        /// <param name="target">The std::string object to receive the loaded string.</param>
        public BinaryString(string target)
        {
            Data = target;
        }

        /// <summary>
        /// Load a length-prefixed string
        /// </summary>
        public static void Load(Stream input, out string target)
        {
            try
            {
                using (BinaryReader br = new BinaryReader(input, Encoding.Default, true))
                {
                    uint length = br.ReadUInt32();

                    target = string.Empty;

                    while (length > 0)
                    {
                        char[] buffer = new char[10 * 1024];
                        int bufSize = (int)Math.Min(length, buffer.Length);
                        buffer = br.ReadChars(bufSize);
                        target += new string(buffer);
                        length -= (uint)bufSize;
                    }
                }
            }
            catch
            {
                target = null;
                return;
            }
        }

        public static void Skip(Stream input)
        {
            try
            {
                using (BinaryReader br = new BinaryReader(input, Encoding.Default, true))
                {
                    uint length = br.ReadUInt32();
                    Utility.Discard(input, length);
                }
            }
            catch
            {
                return;
            }
        }

        /// <summary>
        /// Load a length-prefixed string
        /// </summary>
        public static string Load(Stream input)
        {
            Load(input, out string target);
            return target;
        }
    }
}
