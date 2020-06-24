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
    /// <summary>
    /// Math helper functions.
    /// </summary>
    public partial class Utility
    {
        #region CeilDiv<T>(T num, T denom)

        /// <summary>
        /// Divide by a number and round up the result.
        /// </summary>
        public static uint CeilDiv(uint num, uint denom)
        {
            return (num + (denom - (1))) / denom;
        }

        /// <summary>
        /// Divide by a number and round up the result.
        /// </summary>
        public static int CeilDiv(int num, int denom)
        {
            return (num + (denom - (1))) / denom;
        }

        /// <summary>
        /// Divide by a number and round up the result.
        /// </summary>
        public static ulong CeilDiv(ulong num, ulong denom)
        {
            return (num + (denom - (1))) / denom;
        }

        /// <summary>
        /// Divide by a number and round up the result.
        /// </summary>
        public static long CeilDiv(long num, long denom)
        {
            return (num + (denom - (1))) / denom;
        }

        #endregion

        #region IsPowerOf2<T>(T n)

        /// <summary>
        /// Check if a byte is a power of two.
        /// </summary>
        public static bool IsPowerOf2(byte n)
        {
            return n > 0 && (n & (n - 1)) == 0;
        }

        /// <summary>
        /// Check if an unsigned short is a power of two.
        /// </summary>
        public static bool IsPowerOf2(ushort n)
        {
            return n > 0 && (n & (n - 1)) == 0;
        }

        /// <summary>
        /// Check if a short is a power of two.
        /// </summary>
        public static bool IsPowerOf2(short n)
        {
            return n > 0 && (n & (n - 1)) == 0;
        }

        /// <summary>
        /// Check if an unsigned integer is a power of two.
        /// </summary>
        public static bool IsPowerOf2(uint n)
        {
            return n > 0 && (n & (n - 1)) == 0;
        }

        /// <summary>
        /// Check if an integer is a power of two.
        /// </summary>
        public static bool IsPowerOf2(int n)
        {
            return n > 0 && (n & (n - 1)) == 0;
        }

        /// <summary>
        /// Check if an unsigned long is a power of two.
        /// </summary>
        public static bool IsPowerOf2(ulong n)
        {
            return n > 0 && (n & (n - 1)) == 0;
        }

        /// <summary>
        /// Check if a long is a power of two.
        /// </summary>
        public static bool IsPowerOf2(long n)
        {
            return n > 0 && (n & (n - 1)) == 0;
        }

        #endregion

        #region ModPowerOf2<T1, T2>(T1 a, T2 b)

        /// <summary>
        /// Calculate <code>a % b</code> where b is always a power of two.
        /// </summary>
        public static long ModPowerOf2(long a, long b)
        {
            return a & (b - 1);
        }

        #endregion

        #region SafeRightShift(T value)

        public static byte SafeRightShift(byte value, uint bits)
        {
            return SafeShifter.RightShift(value, bits, (bits >= (8 * sizeof(byte))));
        }

        public static sbyte SafeRightShift(sbyte value, uint bits)
        {
            return SafeShifter.RightShift(value, bits, (bits >= (8 * sizeof(sbyte))));
        }

        public static ushort SafeRightShift(ushort value, uint bits)
        {
            return SafeShifter.RightShift(value, bits, (bits >= (8 * sizeof(ushort))));
        }

        public static short SafeRightShift(short value, uint bits)
        {
            return SafeShifter.RightShift(value, bits, (bits >= (8 * sizeof(short))));
        }

        public static uint SafeRightShift(uint value, uint bits)
        {
            return SafeShifter.RightShift(value, bits, (bits >= (8 * sizeof(uint))));
        }

        public static int SafeRightShift(int value, uint bits)
        {
            return SafeShifter.RightShift(value, bits, (bits >= (8 * sizeof(int))));
        }

        public static ulong SafeRightShift(ulong value, uint bits)
        {
            return SafeShifter.RightShift(value, bits, (bits >= (8 * sizeof(ulong))));
        }

        public static long SafeRightShift(long value, uint bits)
        {
            return SafeShifter.RightShift(value, bits, (bits >= (8 * sizeof(long))));
        }

        #endregion

        #region SafeLeftShift(T value)

        public static byte SafeLeftShift(byte value, uint bits)
        {
            return SafeShifter.LeftShift(value, bits, (bits >= (8 * sizeof(byte))));
        }

        public static sbyte SafeLeftShift(sbyte value, uint bits)
        {
            return SafeShifter.LeftShift(value, bits, (bits >= (8 * sizeof(sbyte))));
        }

        public static ushort SafeLeftShift(ushort value, uint bits)
        {
            return SafeShifter.LeftShift(value, bits, (bits >= (8 * sizeof(ushort))));
        }

        public static short SafeLeftShift(short value, uint bits)
        {
            return SafeShifter.LeftShift(value, bits, (bits >= (8 * sizeof(short))));
        }

        public static uint SafeLeftShift(uint value, uint bits)
        {
            return SafeShifter.LeftShift(value, bits, (bits >= (8 * sizeof(uint))));
        }

        public static int SafeLeftShift(int value, uint bits)
        {
            return SafeShifter.LeftShift(value, bits, (bits >= (8 * sizeof(int))));
        }

        public static ulong SafeLeftShift(ulong value, uint bits)
        {
            return SafeShifter.LeftShift(value, bits, (bits >= (8 * sizeof(ulong))));
        }

        public static long SafeLeftShift(long value, uint bits)
        {
            return SafeShifter.LeftShift(value, bits, (bits >= (8 * sizeof(long))));
        }

        #endregion

        #region RotlFixed(T x, uint y)

        public static byte RotlFixed(byte x, uint y)
        {
            return (byte)((x << (int)y) | (x >> (int)(sizeof(byte) * 8 - y)));
        }

        public static sbyte RotlFixed(sbyte x, uint y)
        {
            return (sbyte)((x << (int)y) | (x >> (int)(sizeof(sbyte) * 8 - y)));
        }

        public static ushort RotlFixed(ushort x, uint y)
        {
            return (ushort)((x << (int)y) | (x >> (int)(sizeof(ushort) * 8 - y)));
        }

        public static short RotlFixed(short x, uint y)
        {
            return (short)((x << (int)y) | (x >> (int)(sizeof(short) * 8 - y)));
        }

        public static uint RotlFixed(uint x, uint y)
        {
            return (uint)((x << (int)y) | (x >> (int)(sizeof(uint) * 8 - y)));
        }

        public static int RotlFixed(int x, uint y)
        {
            return (int)((x << (int)y) | (x >> (int)(sizeof(int) * 8 - y)));
        }

        public static ulong RotlFixed(ulong x, uint y)
        {
            return (ulong)((x << (int)y) | (x >> (int)(sizeof(ulong) * 8 - y)));
        }

        public static long RotlFixed(long x, uint y)
        {
            return (long)((x << (int)y) | (x >> (int)(sizeof(long) * 8 - y)));
        }

        #endregion
    }
}
