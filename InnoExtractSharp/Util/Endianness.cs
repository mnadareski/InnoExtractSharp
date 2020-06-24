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
using System.Runtime.InteropServices;

namespace InnoExtractSharp.Util
{
    /// <summary>
    /// Load/store functions for a specific endianness.
    /// </summary>
    public abstract class Endianness<T>
    {
        /// <summary>
        /// Load a value of type T that is stored with a specific endianness.
        /// </summary>
        public static T Load(Stream input, Endianness<T> endianness)
        {
            byte[] buffer = new byte[Marshal.SizeOf(default(T))];
            input.Read(buffer, 0, buffer.Length);
            return endianness.Load(buffer, 0);
        }

        /// <summary>
        /// Load a value of type T that is stored as little endian.
        /// </summary>
        public static T LoadLittleEndian(Stream input)
        {
            return Load(input, new LittleEndian<T>());
        }

        /// <summary>
        /// Load a value of type T that is stored with a specific endianness.
        /// </summary>
        /// <param name="input">Input stream to load from.</param>
        /// <param name="bits">The number of bits used to store the number.</param>
        public static T Load(Stream input, Endianness<T> endianness, int bits)
        {
            T def = default(T);
            if (bits == 8 && (def is byte || def is sbyte))
                return Load(input, endianness);
            if (bits == 16 && (def is ushort || def is short))
                return Load(input, endianness);
            if (bits == 32 && (def is uint || def is int))
                return Load(input, endianness);
            if (bits == 64 && (def is ulong || def is long))
                return Load(input, endianness);

            return def;
        }

        /// <summary>
        /// Load a value of type T that is stored as little endian.
        /// </summary>
        /// <param name="input">Input stream to load from.</param>
        /// <param name="bits">The number of bits used to store the number.</param>
        public static T LoadLittleEndian(Stream input, int bits)
        {
            return Load(input, new LittleEndian<T>(), bits);
        }

        /// <summary>
        /// Load a single integer.
        /// </summary>
        /// <param name="buffer">Memory location containing the integer. Will read sizeof(T) bytes.</param>
        /// <returns>the loaded integer.</returns>
        public T Load(byte[] buffer, int bufferPtr)
        {
            if (Native())
            {
                T value = default(T);
                IntPtr ptr = new IntPtr();
                Marshal.StructureToPtr<T>(value, ptr, false);
                Marshal.Copy(buffer, bufferPtr, ptr, Marshal.SizeOf(value));
                return value;
            }
            else
            {
                return LoadAlien(buffer, bufferPtr);
            }
        }

        /// <summary>
        /// Load an array of integers.
        /// </summary>
        /// <param name="buffer">
        /// Memory location containing the integers (without padding).
        /// Will read <code>sizeof(T) * count</code> bytes.
        /// </param>
        /// <param name="values">Output array for the loaded integers.</param>
        /// <param name="count">How many integers to load.</param>
        public void Load(byte[] buffer, int bufferPtr, ref T[] values, int count)
        {
            int valueSize = Marshal.SizeOf(default(T));
            if (Native() || valueSize == 1)
            {
                IntPtr ptr = new IntPtr();
                Marshal.StructureToPtr<T[]>(values, ptr, false);
                Marshal.Copy(buffer, bufferPtr, ptr, valueSize * count);
            }
            else
            {
                for (int i = 0; i < count; i++, bufferPtr += valueSize)
                {
                    values[i] = LoadAlien(buffer, bufferPtr);
                }
            }
        }

        /// <summary>
        /// Store a single integer.
        /// </summary>
        /// <param name="value">The integer to store.</param>
        /// <param name="buffer">Memory location to receive the integer. Will write sizeof(T) bytes.</param>
        public void Store(T value, ref byte[] buffer, int bufferPtr)
        {
            int valueSize = Marshal.SizeOf(default(T));
            if (Native())
            {
                IntPtr ptr = new IntPtr();
                Marshal.StructureToPtr<T>(value, ptr, false);
                Marshal.Copy(ptr, buffer, bufferPtr, valueSize);
            }
            else
            {
                StoreAlien(value, ref buffer, bufferPtr);
            }
        }

        /// <summary>
        /// Store an array of integers.
        /// </summary>
        /// <param name="values">The integers to store.</param>
        /// <param name="count">How many integers to store.</param>
        /// <param name="buffer">
        /// Memory location to receive the integers (without padding).
        /// Will write <code>sizeof(T) * count</code> bytes.
        /// </param>
        public void Store(T[] values, int count, ref byte[] buffer, int bufferPtr)
        {
            int valueSize = Marshal.SizeOf(default(T));
            if (Native || valueSize == 1)
            {
                IntPtr ptr = new IntPtr();
                Marshal.StructureToPtr<T[]>(values, ptr, false);
                Marshal.Copy(ptr, buffer, bufferPtr, valueSize * count);
            }
            else
            {
                for (int i = 0; i < count; i++, bufferPtr += valueSize)
                {
                    StoreAlien<T>(values[i], buffer, bufferPtr);
                }
            }
        }
    
        protected virtual bool Reversed() { return false; }

        protected abstract bool Native();

        protected abstract T Decode(byte[] buffer);

        protected abstract void Encode(T value, byte[] buffer, int bufferPtr);

        private T LoadAlien(byte[] buffer, int bufferPtr)
        {
            if (Reversed())
            {
                T value = default(T);
                IntPtr ptr = new IntPtr();
                Marshal.StructureToPtr<T>(value, ptr, false);
                Marshal.Copy(buffer, bufferPtr, ptr, Marshal.SizeOf(value));
                return Byteswap(value);
            }
            else
            {
                return Decode(buffer, bufferPtr);
            }
        }

        private void StoreAlien(T value, ref byte[] buffer, int bufferPtr)
        {
            if (Reversed())
            {
                IntPtr ptr = new IntPtr();
                Marshal.StructureToPtr<T>(Byteswap(value), ptr, false);
                Marshal.Copy(ptr, buffer, bufferPtr, Marshal.SizeOf(value));
            }
            else
            {
                Encode(value, buffer, bufferPtr);
            }
        }

        internal static bool IsLittleEndian()
        {
            return BitConverter.IsLittleEndian;
        }

        internal static bool IsBigEndian()
        {
            return !BitConverter.IsLittleEndian;
        }

        private T Byteswap(T value)
        {
            // Int8
            if (value is byte)
            {
                byte newValue = (value as byte?).Value;
                byte swapped = Utility.Byteswap(newValue);
                return (T)Convert.ChangeType(swapped, typeof(T));
            }
            if (value is sbyte)
            {
                sbyte newValue = (value as sbyte?).Value;
                sbyte swapped = Utility.Byteswap(newValue);
                return (T)Convert.ChangeType(swapped, typeof(T));
            }

            // Int16
            if (value is ushort)
            {
                ushort newValue = (value as ushort?).Value;
                ushort swapped = Utility.Byteswap(newValue);
                return (T)Convert.ChangeType(swapped, typeof(T));
            }
            if (value is short)
            {
                short newValue = (value as short?).Value;
                short swapped = Utility.Byteswap(newValue);
                return (T)Convert.ChangeType(swapped, typeof(T));
            }

            // Int32
            if (value is uint)
            {
                uint newValue = (value as uint?).Value;
                uint swapped = Utility.Byteswap(newValue);
                return (T)Convert.ChangeType(swapped, typeof(T));
            }
            if (value is int)
            {
                int newValue = (value as int?).Value;
                int swapped = Utility.Byteswap(newValue);
                return (T)Convert.ChangeType(swapped, typeof(T));
            }

            // Int64
            if (value is ulong)
            {
                ulong newValue = (value as ulong?).Value;
                ulong swapped = Utility.Byteswap(newValue);
                return (T)Convert.ChangeType(swapped, typeof(T));
            }
            if (value is long)
            {
                long newValue = (value as long?).Value;
                long swapped = Utility.Byteswap(newValue);
                return (T)Convert.ChangeType(swapped, typeof(T));
            }

            return default(T);
        }
    }
}
