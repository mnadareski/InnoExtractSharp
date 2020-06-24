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
using System.Runtime.InteropServices;

namespace InnoExtractSharp.Util
{
    /// <summary>
    /// Load and store big-endian integers.
    /// </summary>
    public class BigEndian<T> : Endianness<T>
    {
        /// <returns>true if we are running on a big-endian machine.</returns>
        protected override bool Native()
        {
            return IsBigEndian();
        }

        protected override bool Reversed()
        {
            return IsLittleEndian();
        }

        protected override T Decode(byte[] buffer)
        {
            T value = default(T);
            for (int i = 0; i < Marshal.SizeOf(value); i++)
            {
                value = DecodeStep(ref value, buffer, i);
            }

            return value;
        }

        private T DecodeStep(ref T value, byte[] buffer, int index)
        {
            int size = Marshal.SizeOf(value);

            // Int8
            if (value is byte)
            {
                byte newValue = (value as byte?).Value;
                byte bufferItem = buffer[index];
                newValue = (byte)(newValue | (bufferItem << ((size - index - 1) * 8)));
                return (T)Convert.ChangeType(newValue, typeof(T));
            }
            if (value is sbyte)
            {
                sbyte newValue = (value as sbyte?).Value;
                sbyte bufferItem = (sbyte)buffer[index];
                newValue = (sbyte)(newValue | (bufferItem << ((size - index - 1) * 8)));
                return (T)Convert.ChangeType(newValue, typeof(T));
            }

            // Int16
            if (value is ushort)
            {
                ushort newValue = (value as ushort?).Value;
                ushort bufferItem = BitConverter.ToUInt16(buffer, index);
                newValue = (ushort)(newValue | (bufferItem << ((size - index - 1) * 8)));
                return (T)Convert.ChangeType(newValue, typeof(T));
            }
            if (value is short)
            {
                short newValue = (value as short?).Value;
                short bufferItem = BitConverter.ToInt16(buffer, index);
                newValue = (short)(newValue | (bufferItem << ((size - index - 1) * 8)));
                return (T)Convert.ChangeType(newValue, typeof(T));
            }

            // Int32
            if (value is uint)
            {
                uint newValue = (value as uint?).Value;
                uint bufferItem = BitConverter.ToUInt32(buffer, index);
                newValue = (uint)(newValue | (bufferItem << ((size - index - 1) * 8)));
                return (T)Convert.ChangeType(newValue, typeof(T));
            }
            if (value is int)
            {
                int newValue = (value as int?).Value;
                int bufferItem = BitConverter.ToInt32(buffer, index);
                newValue = (int)(newValue | (bufferItem << ((size - index - 1) * 8)));
                return (T)Convert.ChangeType(newValue, typeof(T));
            }

            // Int64
            if (value is ulong)
            {
                ulong newValue = (value as ulong?).Value;
                ulong bufferItem = BitConverter.ToUInt64(buffer, index);
                newValue = (ulong)(newValue | (bufferItem << ((size - index - 1) * 8)));
                return (T)Convert.ChangeType(newValue, typeof(T));
            }
            if (value is long)
            {
                long newValue = (value as long?).Value;
                long bufferItem = BitConverter.ToInt64(buffer, index);
                newValue = (long)(newValue | (bufferItem << ((size - index - 1) * 8)));
                return (T)Convert.ChangeType(newValue, typeof(T));
            }

            return default(T);
        }

        protected override void Encode(T value, byte[] buffer, int bufferPtr)
        {
            for (int i = 0; i < Marshal.SizeOf(value); i++)
            {
                buffer[i] = EncodeStep(value, i);
            }
        }

        private byte EncodeStep(T value, int index)
        {
            int size = Marshal.SizeOf(value);

            // Int8
            if (value is byte)
            {
                byte newValue = (value as byte?).Value;
                return (byte)((newValue >> ((size - index - 1) * 8)) & 0xff);
            }
            if (value is sbyte)
            {
                sbyte newValue = (value as sbyte?).Value;
                return (byte)((newValue >> ((size - index - 1) * 8)) & 0xff);
            }

            // Int16
            if (value is ushort)
            {
                ushort newValue = (value as ushort?).Value;
                return (byte)((newValue >> ((size - index - 1) * 8)) & 0xff);
            }
            if (value is short)
            {
                short newValue = (value as short?).Value;
                return (byte)((newValue >> ((size - index - 1) * 8)) & 0xff);
            }

            // Int32
            if (value is uint)
            {
                uint newValue = (value as uint?).Value;
                return (byte)((newValue >> ((size - index - 1) * 8)) & 0xff);
            }
            if (value is int)
            {
                int newValue = (value as int?).Value;
                return (byte)((newValue >> ((size - index - 1) * 8)) & 0xff);
            }

            // Int64
            if (value is ulong)
            {
                ulong newValue = (value as ulong?).Value;
                return (byte)((newValue >> ((size - index - 1) * 8)) & 0xff);
            }
            if (value is long)
            {
                long newValue = (value as long?).Value;
                return (byte)((newValue >> ((size - index - 1) * 8)) & 0xff);
            }

            return default(byte);
        }
    }
}
