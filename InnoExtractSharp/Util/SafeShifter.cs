/*
 * Copyright (C) 2011-2014 Daniel Scharrer
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
    public class SafeShifter
    {
        #region RightShift(T value, uint bits, bool overflow)

        public static byte RightShift(byte value, uint bits, bool overflow = false)
        {
            if (overflow)
                return 0;
            return (byte)(value >> (int)bits);
        }

        public static sbyte RightShift(sbyte value, uint bits, bool overflow = false)
        {
            if (overflow)
                return 0;
            return (sbyte)(value >> (int)bits);
        }

        public static ushort RightShift(ushort value, uint bits, bool overflow = false)
        {
            if (overflow)
                return 0;
            return (ushort)(value >> (int)bits);
        }

        public static short RightShift(short value, uint bits, bool overflow = false)
        {
            if (overflow)
                return 0;
            return (short)(value >> (int)bits);
        }

        public static uint RightShift(uint value, uint bits, bool overflow = false)
        {
            if (overflow)
                return 0;
            return (uint)(value >> (int)bits);
        }

        public static int RightShift(int value, uint bits, bool overflow = false)
        {
            if (overflow)
                return 0;
            return (int)(value >> (int)bits);
        }

        public static ulong RightShift(ulong value, uint bits, bool overflow = false)
        {
            if (overflow)
                return 0;
            return (ulong)(value >> (int)bits);
        }

        public static long RightShift(long value, uint bits, bool overflow = false)
        {
            if (overflow)
                return 0;
            return (long)(value >> (int)bits);
        }

        #endregion

        #region LeftShift(T value, uint bits, bool overflow)

        public static byte LeftShift(byte value, uint bits, bool overflow = false)
        {
            if (overflow)
                return 0;
            return (byte)(value << (int)bits);
        }

        public static sbyte LeftShift(sbyte value, uint bits, bool overflow = false)
        {
            if (overflow)
                return 0;
            return (sbyte)(value << (int)bits);
        }

        public static ushort LeftShift(ushort value, uint bits, bool overflow = false)
        {
            if (overflow)
                return 0;
            return (ushort)(value << (int)bits);
        }

        public static short LeftShift(short value, uint bits, bool overflow = false)
        {
            if (overflow)
                return 0;
            return (short)(value << (int)bits);
        }

        public static uint LeftShift(uint value, uint bits, bool overflow = false)
        {
            if (overflow)
                return 0;
            return (uint)(value << (int)bits);
        }

        public static int LeftShift(int value, uint bits, bool overflow = false)
        {
            if (overflow)
                return 0;
            return (int)(value << (int)bits);
        }

        public static ulong LeftShift(ulong value, uint bits, bool overflow = false)
        {
            if (overflow)
                return 0;
            return (ulong)(value << (int)bits);
        }

        public static long LeftShift(long value, uint bits, bool overflow = false)
        {
            if (overflow)
                return 0;
            return (long)(value << (int)bits);
        }

        #endregion
    }
}
