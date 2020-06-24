﻿/*
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

using System.Runtime.InteropServices;

namespace InnoExtractSharp.Util
{
    /// <summary>
    /// functions for dealing with different endiannesses.
    /// </summary>
    public partial class Utility
    {
        /// <summary>
        /// Get the alignment of a type.
        /// </summary>
        public static int AlignmentOf(object o)
        {
            return Marshal.SizeOf(o);
        }

        /// <summary>
        /// Check if a pointer has aparticular alignment.
        /// </summary>
        public static bool IsAlignedOn(byte[] p, int alignment)
        {
            return alignment == 1
                || (IsPowerOf2(alignment)
                    ? ModPowerOf2(p.Length, alignment) == 0
                    : (p.Length % alignment) == 0);
        }

        /// <summary>
        /// Check if a pointer is aligned for a specific type.
        /// </summary>
        public static bool IsAligned(byte[] p, object o)
        {
            return IsAlignedOn(p, AlignmentOf(o));
        }
    }
}