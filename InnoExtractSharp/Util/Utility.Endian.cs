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

namespace InnoExtractSharp.Util
{
    /// <summary>
    /// functions for dealing with different endiannesses.
    /// </summary>
    public partial class Utility
    {
        #region Detail

        public static byte Byteswap(byte value)
        {
            return value;
        }

        public static sbyte Byteswap(sbyte value)
        {
            return (sbyte)Byteswap((byte)value);
        }

        public static ushort Byteswap(ushort value)
        {
            return (ushort)((byte)value << 8
                | (byte)(value >> 8));
        }

        public static short Byteswap(short value)
        {
            return (short)Byteswap((ushort)value);
        }

        public static uint Byteswap(uint value)
        {
            return (uint)((Byteswap((ushort)value)) << 16)
            | Byteswap((ushort)(value >> 16));
        }

        public static int Byteswap(int value)
        {
            return (int)Byteswap((uint)value);
        }

        public static ulong Byteswap(ulong value)
        {
            return (ulong)((Byteswap((uint)value)) << 32)
            | Byteswap((uint)(value >> 32));
        }

        public static long Byteswap(long value)
        {
            return (long)Byteswap((ulong)value);
        }

        #endregion


    }
}
