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
using System.Linq;
using System.Runtime.InteropServices;

namespace InnoExtractSharp.Util
{
    public class Utility
    {
        public class SafeShifter
        {
            public static int RightShift(int value, uint bits, bool overflow = false)
            {
                if (overflow)
                    return 0;
                return value >> (int)bits;
            }

            public static int LeftShift(int value, uint bits, bool overflow = false)
            {
                if (overflow)
                    return 0;
                return value << (int)bits;
            }
        }

        #region Alignment

        public static int AlignmentOf(object o)
        {
            return Marshal.SizeOf(o);
        }

        public static bool IsAlignedOn(byte[] p, int alignment)
        {
            return alignment == 1
                || (IsPowerOf2(alignment)
                    ? ModPowerOf2(p.Length, alignment) == 0
                    : (p.Length % alignment) == 0);
        }

        public static bool IsAligned(byte[] p, object o)
        {
            return IsAlignedOn(p, AlignmentOf(o));
        }

        #endregion

        #region Ansi

        public class AnsiConsoleParser
        {
            //! Character that started the current control sequence, or \c 0
            public char InCommand;

            //! Buffer for control sequences if they span more than one flush
            public List<char> Command;

            protected const char ESC = '\x1b';
            protected const char CSI = '['; //!< Control Sequence Indicator (preceded by \ref ESC)
            protected const char UTF8CSI0 = '\xc2'; //!< UTF-8 Control Sequence Indicator, first byte
            protected const char UTF8CSI1 = '\x9b'; //!< UTF-8 Control Sequence Indicator, second byte
            protected const char Separator = ';'; //! Separator for codes in CSI control sequences

            protected class CommandType
            {
                public const char CUU = 'A'; //!< Cursor Up
                public const char CUD = 'B'; //!< Cursor Down
                public const char CUF = 'C'; //!< Cursor Forward
                public const char CUB = 'D'; //!< Cursor Back
                public const char CNL = 'E'; //!< Cursor Next Line
                public const char CPL = 'F'; //!< Cursor Previous Line
                public const char CHA = 'G'; //!< Cursor Horizontal Absolute
                public const char CUP = 'H'; //!< Cursor Position
                public const char ED = 'J'; //!< Erase Display
                public const char EL = 'K'; //!< Erase in Line
                public const char SU = 'S'; //!< Scroll Up
		        public const char SD = 'T'; //!< Scroll Down
                public const char HVP = 'f'; //!< Horizontal and Vertical Position
                public const char SGR = 'm'; //!< Select Graphic Rendition
                public const char DSR = 'n'; //!< Device Status Report
                public const char SCP = 's'; //!< Save Cursor Position
                public const char RCP = 'u'; //!< Restore Cursor Position
            }

            /// <summary>
            /// Read one code form a command sequence
            /// 
            /// Each command sequence contains contains at least one code. Once there are no more
            /// commands in the command sequence, \c s will be set to \c NULL. After that has
            /// happened \ref read_code() should must not be called with the (\c s, \c end) pair.
            /// 
            /// The meaning of th returned code depends on the type of the command sequence.
            /// </summary>
            /// <param name="s">Command sequence</param>
            /// <param name="sPtr">Current position in the command sequence.</param>
            /// <param name="end">End of the command sequence.</param>
            /// <returns>the next code in the command sequence or unsigned(-1) if there was an error.</returns>
            protected uint ReadCode(char[] s, ref int sPtr, int end)
            {
                int sep = s.Skip(sPtr).First()
            }

            private bool IsStartChar(char c)
            {
                return (c == ESC /* escape */ || c == UTF8CSI0 /* first byte of UTF-8 CSI */);
            }

            private bool IsEndChar(char c)
            {
                return (c >= 64 && c < 127);
            }
        }

        #endregion

        #region Load

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
        public static ulong GetBits(ulong number, int first, int last)
        {
            number = (number >> first); last -= first;
            ulong mask = (last + 1 == sizeof(ulong) ? 0 : (ulong)(1 << (last + 1))) - 1;
            return (number & mask);
        }

        #endregion

        #endregion

        #region Math

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

        public static int SafeRightShift(int value, uint bits)
        {
            return SafeShifter.RightShift(value, bits, (bits >= (8 * sizeof(int))));
        }

        public static int SafeLeftShift(int value, uint bits)
        {
            return SafeShifter.LeftShift(value, bits, (bits >= (8 * sizeof(int))));
        }

        public static uint RotlFixed(uint x, uint y)
        {
            return (uint)(((int)x << (int)y) | ((int)x >> (int)(sizeof(int) * 8 - y)));
        }

        #endregion
    }
}
