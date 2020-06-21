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

using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace InnoExtractSharp.Util
{
    // loosely based on Qflags from Qt

    public class EnumSize
    {
        public static int Value;
    }

    /// <summary>
    /// A typesafe way to define flags as a combination of enum values.
    /// 
    /// This type should not be used directly, only through DECLARE_FLAGS.
    /// </summary>
    public class Flags<T> where T: struct, IConvertible
    {
        public static int Bits;

        private bool[] _flags;

        private Flags(bool[] flag)
        {
            _flags = flag;
        }

        public Flags()
        {
            _flags = null;
        }

        public Flags(T flag)
        {
            _flags = new bool[Marshal.SizeOf(flag)];
        }

        public static Flags<T> Load(bool[] flags)
        {
            return new Flags<T>(flags);
        }

        /// <summary>
        /// Test if a specific flag is set.
        /// </summary>
        public bool Has(T flag)
        {
            return _flags[Marshal.SizeOf(flag)];
        }

        /// <summary>
        /// Test if a collection of flags are all set.
        /// </summary>
        public bool HasAll(Flags<T> o)
        {
            return _flags.SequenceEqual(o._flags);
        }

        public static Flags<T> operator ~(Flags<T> flags)
        {
            return new Flags<T>() { _flags = flags._flags.Select(b => !b).ToArray() };
        }

        public static bool operator !(Flags<T> flags)
        {
            return flags._flags.All(b => b == false);
        }

        public static Flags<T> operator &(Flags<T> a, Flags<T> b)
        {
            bool[] newflags = new bool[Math.Min(a._flags.Length, b._flags.Length)];
            for (int i = 0; i < Math.Min(a._flags.Length, b._flags.Length); i++)
                newflags[i] = a._flags[i] & b._flags[i];
            return new Flags<T>() { _flags = newflags };
        }

        public static Flags<T> operator |(Flags<T> a, Flags<T> b)
        {
            bool[] newflags = new bool[Math.Min(a._flags.Length, b._flags.Length)];
            for (int i = 0; i < Math.Min(a._flags.Length, b._flags.Length); i++)
                newflags[i] = a._flags[i] | b._flags[i];
            return new Flags<T>() { _flags = newflags };
        }

        public static Flags<T> operator ^(Flags<T> a, Flags<T> b)
        {
            bool[] newflags = new bool[Math.Min(a._flags.Length, b._flags.Length)];
            for (int i = 0; i < Math.Min(a._flags.Length, b._flags.Length); i++)
                newflags[i] = a._flags[i] ^ b._flags[i];
            return new Flags<T>() { _flags = newflags };
        }

        public static Flags<T> operator &(Flags<T> a, T flag)
        {
            return a & new Flags<T>(flag);
        }

        public static Flags<T> operator |(Flags<T> a, T flag)
        {
            return a | new Flags<T>(flag);
        }

        public static Flags<T> operator ^(Flags<T> a, T flag)
        {
            return a ^ new Flags<T>(flag);
        }

        /// <summary>
        /// Get a set of flags with all possible values set.
        /// </summary>
        public static Flags<T> All()
        {
            bool[] allset = new bool[Bits];
            for (int i = 0; i < allset.Length; i++)
                allset[i] = true;
            return new Flags<T>(allset);
        }
    }
}
