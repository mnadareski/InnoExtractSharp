/*
 * Copyright (C) 2011-2017 Daniel Scharrer
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
using System.Linq;

namespace InnoExtractSharp.Util
{
    /// <summary>
    /// functions for dealing with different endiannesses.
    /// </summary>
    public partial class Utility
    {
        /// <summary>
        /// Load a bool value
        /// </summary>
        public static bool LoadBool(Stream input)
        {
            return Endianness<bool>.Load(input, new LittleEndian<bool>());
        }

        /// <summary>
        /// Discard a number of bytes from a non-seekable input stream or stream-like object
        /// </summary>
        /// <param name="input">The stream to "seek"</param>
        /// <param name="bytes">Number of bytes to skip ahead<param>
        public static void Discard(Stream input, uint bytes)
        {
            byte[] buf = new byte[1024];
            while (bytes != 0)
            {
                int n = (int)Math.Min(bytes, (uint)buf.Length);
                input.Read(buf, 0, n);
                bytes -= (uint)n;
            }
        }

        #region GetBits<T>(T number, uint first, uint last)

        /// <summary>
        /// Get the number represented by a specific range of bits of another number
        /// All other bits are masked and the requested bits are shifted to position 0
        /// </summary>
        /// <param name="number">The number containing the required bits</param>
        /// <param name="first">Index of the first desired bit</param>
        /// <param name="last">Index of the last desired bit (inclusive)</param>
        /// <returns></returns>
        public static byte GetBits(byte number, int first, int last)
        {
            number = (byte)(number >> first); last -= first;
            byte mask = (byte)((last + 1 == sizeof(byte) ? 0 : (byte)(1 << (last + 1))) - 1);
            return (byte)(number & mask);
        }

        /// <summary>
        /// Get the number represented by a specific range of bits of another number
        /// All other bits are masked and the requested bits are shifted to position 0
        /// </summary>
        /// <param name="number">The number containing the required bits</param>
        /// <param name="first">Index of the first desired bit</param>
        /// <param name="last">Index of the last desired bit (inclusive)</param>
        /// <returns></returns>
        public static sbyte GetBits(sbyte number, int first, int last)
        {
            number = (sbyte)(number >> first); last -= first;
            sbyte mask = (sbyte)((last + 1 == sizeof(sbyte) ? 0 : (sbyte)(1 << (last + 1))) - 1);
            return (sbyte)(number & mask);
        }

        /// <summary>
        /// Get the number represented by a specific range of bits of another number
        /// All other bits are masked and the requested bits are shifted to position 0
        /// </summary>
        /// <param name="number">The number containing the required bits</param>
        /// <param name="first">Index of the first desired bit</param>
        /// <param name="last">Index of the last desired bit (inclusive)</param>
        /// <returns></returns>
        public static ushort GetBits(ushort number, int first, int last)
        {
            number = (ushort)(number >> first); last -= first;
            ushort mask = (ushort)((last + 1 == sizeof(ushort) ? 0 : (ushort)(1 << (last + 1))) - 1);
            return (ushort)(number & mask);
        }

        /// <summary>
        /// Get the number represented by a specific range of bits of another number
        /// All other bits are masked and the requested bits are shifted to position 0
        /// </summary>
        /// <param name="number">The number containing the required bits</param>
        /// <param name="first">Index of the first desired bit</param>
        /// <param name="last">Index of the last desired bit (inclusive)</param>
        /// <returns></returns>
        public static short GetBits(short number, int first, int last)
        {
            number = (short)(number >> first); last -= first;
            short mask = (short)((last + 1 == sizeof(short) ? 0 : (short)(1 << (last + 1))) - 1);
            return (short)(number & mask);
        }

        /// <summary>
        /// Get the number represented by a specific range of bits of another number
        /// All other bits are masked and the requested bits are shifted to position 0
        /// </summary>
        /// <param name="number">The number containing the required bits</param>
        /// <param name="first">Index of the first desired bit</param>
        /// <param name="last">Index of the last desired bit (inclusive)</param>
        /// <returns></returns>
        public static uint GetBits(uint number, int first, int last)
        {
            number = (number >> first); last -= first;
            uint mask = (last + 1 == sizeof(uint) ? 0 : (uint)(1 << (last + 1))) - 1;
            return (number & mask);
        }

        /// <summary>
        /// Get the number represented by a specific range of bits of another number
        /// All other bits are masked and the requested bits are shifted to position 0
        /// </summary>
        /// <param name="number">The number containing the required bits</param>
        /// <param name="first">Index of the first desired bit</param>
        /// <param name="last">Index of the last desired bit (inclusive)</param>
        /// <returns></returns>
        public static int GetBits(int number, int first, int last)
        {
            number = (number >> first); last -= first;
            int mask = (last + 1 == sizeof(int) ? 0 : (int)(1 << (last + 1))) - 1;
            return (number & mask);
        }

        /// <summary>
        /// Get the number represented by a specific range of bits of another number
        /// All other bits are masked and the requested bits are shifted to position 0
        /// </summary>
        /// <param name="number">The number containing the required bits</param>
        /// <param name="first">Index of the first desired bit</param>
        /// <param name="last">Index of the last desired bit (inclusive)</param>
        /// <returns></returns>
        public static ulong GetBits(ulong number, int first, int last)
        {
            number = (number >> first); last -= first;
            ulong mask = (last + 1 == sizeof(ulong) ? 0 : (ulong)(1 << (last + 1))) - 1;
            return (number & mask);
        }

        /// <summary>
        /// Get the number represented by a specific range of bits of another number
        /// All other bits are masked and the requested bits are shifted to position 0
        /// </summary>
        /// <param name="number">The number containing the required bits</param>
        /// <param name="first">Index of the first desired bit</param>
        /// <param name="last">Index of the last desired bit (inclusive)</param>
        /// <returns></returns>
        public static long GetBits(long number, int first, int last)
        {
            number = (number >> first); last -= first;
            long mask = (last + 1 == sizeof(long) ? 0 : (long)(1 << (last + 1))) - 1;
            return (number & mask);
        }

        #endregion

        /// <summary>
        /// Parse an ASCII representation of an unsigned integer
        /// </summary>
        public static ulong ToUnsigned(string chars, int count)
        {
            byte[] converted = chars.ToCharArray().Select(c => (byte)c).ToArray();
            if (count == 1 && converted.Length >= 1)
                return converted[0];
            else if (count == 2 && converted.Length >= 2)
                return BitConverter.ToUInt16(converted, 0);
            else if (count == 4 && converted.Length >= 4)
                return BitConverter.ToUInt32(converted, 0);
            else if (count == 8 && converted.Length >= 8)
                return BitConverter.ToUInt64(converted, 0);

            return default(ulong);
        }
    }
}
