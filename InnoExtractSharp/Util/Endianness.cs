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

namespace InnoExtractSharp.Util
{
    public class Endianness
    {
        public bool Native()
        {
            return false;
        }

        protected bool Reversed()
        {
            return false;
        }

        protected bool IsLittleEndian()
        {
            return BitConverter.IsLittleEndian;
        }

        protected bool IsBigEndian()
        {
            return !BitConverter.IsLittleEndian;
        }

        #region Byteswap<T>(T value)

        public byte Byteswap(byte value)
        {
            return value;
        }

        public ushort Byteswap(ushort value)
        {
            return ((ushort)(((0xFF00 & value) >> 8) | ((0x00FF & value) << 8)));
        }

        public short Byteswap(short value)
        {
            return ((short)(((0xFF00 & value) >> 8) | ((0x00FF & value) << 8)));
        }

        public uint Byteswap(uint value)
        {
            return ((uint)(((0xFF000000 & value) >> 24) | ((0x00FF0000 & value) >> 8) | ((0x0000FF00 & value) << 8) | ((0x000000FF & value) << 24)));
        }

        public int Byteswap(int value)
        {
            return ((int)(((0xFF000000 & value) >> 24) | ((0x00FF0000 & value) >> 8) | ((0x0000FF00 & value) << 8) | ((0x000000FF & value) << 24)));
        }

        public ulong Byteswap(ulong value)
        {
            return ((0x00000000000000FF) & (value >> 56)
               | (0x000000000000FF00) & (value >> 40)
               | (0x0000000000FF0000) & (value >> 24)
               | (0x00000000FF000000) & (value >> 8)
               | (0x000000FF00000000) & (value << 8)
               | (0x0000FF0000000000) & (value << 24)
               | (0x00FF000000000000) & (value << 40)
               | (0xFF00000000000000) & (value << 56));
        }

        public long Byteswap(long value)
        {
            unchecked
            {
                return (((0x00000000000000FF) & (value >> 56)
                   | (0x000000000000FF00) & (value >> 40)
                   | (0x0000000000FF0000) & (value >> 24)
                   | (0x00000000FF000000) & (value >> 8)
                   | (0x000000FF00000000) & (value << 8)
                   | (0x0000FF0000000000) & (value << 24)
                   | (0x00FF000000000000) & (value << 40)
                   | (long)(0xFF00000000000000) & (value << 56)));
            }
        }

        #endregion

        #region Load<T>(byte[] buffer, int bufferPtr)

        /// <summary>
        /// Load a single byte
        /// </summary>
        public byte LoadByte(byte[] buffer, int bufferPtr)
        {
            return buffer[bufferPtr];
        }

        /// <summary>
        /// Load a single unsigned short
        /// </summary>
        public ushort LoadUInt16(byte[] buffer, int bufferPtr)
        {
            if (this.Native())
                return BitConverter.ToUInt16(buffer, bufferPtr);
            else
                return LoadAlienUInt16(buffer, bufferPtr);
        }

        /// <summary>
        /// Load a single short
        /// </summary>
        public short LoadInt16(byte[] buffer, int bufferPtr)
        {
            if (this.Native())
                return BitConverter.ToInt16(buffer, bufferPtr);
            else
                return LoadAlienInt16(buffer, bufferPtr);
        }

        /// <summary>
        /// Load a single unsigned integer
        /// </summary>
        public uint LoadUInt32(byte[] buffer, int bufferPtr)
        {
            if (this.Native())
                return BitConverter.ToUInt32(buffer, bufferPtr);
            else
                return LoadAlienUInt32(buffer, bufferPtr);
        }

        /// <summary>
        /// Load a single integer
        /// </summary>
        public int LoadInt32(byte[] buffer, int bufferPtr)
        {
            if (this.Native())
                return BitConverter.ToInt32(buffer, bufferPtr);
            else
                return LoadAlienInt32(buffer, bufferPtr);
        }

        /// <summary>
        /// Load a single unsigned long
        /// </summary>
        public ulong LoadUInt64(byte[] buffer, int bufferPtr)
        {
            if (this.Native())
                return BitConverter.ToUInt64(buffer, bufferPtr);
            else
                return LoadAlienUInt64(buffer, bufferPtr);
        }

        /// <summary>
        /// Load a single long
        /// </summary>
        public long LoadInt64(byte[] buffer, int bufferPtr)
        {
            if (this.Native())
                return BitConverter.ToInt64(buffer, bufferPtr);
            else
                return LoadAlienInt64(buffer, bufferPtr);
        }

        #endregion

        #region Load<T>(byte[] buffer, int bufferPtr, ref T[] values, int count)

        /// <summary>
        /// Load an array of bytes
        /// </summary>
        public void LoadByte(byte[] buffer, int bufferPtr, ref byte[] values, int count)
        {
            if (this.Native() || values.Length == 1)
                Array.Copy(buffer, bufferPtr, values, 0, count);
            else
            {
                for (int i = 0; i < count; i++)
                    values[i] = LoadAlienByte(buffer, bufferPtr);
            }
        }

        /// <summary>
        /// Load an array of unsigned shorts
        /// </summary>
        public void LoadUInt16(byte[] buffer, int bufferPtr, ref ushort[] values, int count)
        {
            if (this.Native() || values.Length == 1)
            {
                for (int i = bufferPtr; i < count; i += sizeof(ushort))
                    values[i] = BitConverter.ToUInt16(buffer, i);
            }
            else
            {
                for (int i = 0; i < count; i++)
                    values[i] = LoadAlienUInt16(buffer, bufferPtr);
            }
        }

        /// <summary>
        /// Load an array of shorts
        /// </summary>
        public void LoadInt16(byte[] buffer, int bufferPtr, ref short[] values, int count)
        {
            if (this.Native() || values.Length == 1)
            {
                for (int i = bufferPtr; i < count; i += sizeof(ushort))
                    values[i] = BitConverter.ToInt16(buffer, i);
            }
            else
            {
                for (int i = 0; i < count; i++)
                    values[i] = LoadAlienInt16(buffer, bufferPtr);
            }
        }

        /// <summary>
        /// Load an array of unsigned integers
        /// </summary>
        public void LoadUInt32(byte[] buffer, int bufferPtr, ref uint[] values, int count)
        {
            if (this.Native() || values.Length == 1)
            {
                for (int i = bufferPtr; i < count; i += sizeof(ushort))
                    values[i] = BitConverter.ToUInt32(buffer, i);
            }
            else
            {
                for (int i = 0; i < count; i++)
                    values[i] = LoadAlienUInt32(buffer, bufferPtr);
            }
        }

        /// <summary>
        /// Load an array of integers
        /// </summary>
        public void LoadInt32(byte[] buffer, int bufferPtr, ref int[] values, int count)
        {
            if (this.Native() || values.Length == 1)
            {
                for (int i = bufferPtr; i < count; i += sizeof(ushort))
                    values[i] = BitConverter.ToInt32(buffer, i);
            }
            else
            {
                for (int i = 0; i < count; i++)
                    values[i] = LoadAlienInt32(buffer, bufferPtr);
            }
        }

        /// <summary>
        /// Load an array of unsigned longs
        /// </summary>
        public void LoadUInt64(byte[] buffer, int bufferPtr, ref ulong[] values, int count)
        {
            if (this.Native() || values.Length == 1)
            {
                for (int i = bufferPtr; i < count; i += sizeof(ushort))
                    values[i] = BitConverter.ToUInt64(buffer, i);
            }
            else
            {
                for (int i = 0; i < count; i++)
                    values[i] = LoadAlienUInt64(buffer, bufferPtr);
            }
        }

        /// <summary>
        /// Load an array of longs
        /// </summary>
        public void LoadInt64(byte[] buffer, int bufferPtr, ref long[] values, int count)
        {
            if (this.Native() || values.Length == 1)
            {
                for (int i = bufferPtr; i < count; i += sizeof(ushort))
                    values[i] = BitConverter.ToInt64(buffer, i);
            }
            else
            {
                for (int i = 0; i < count; i++)
                    values[i] = LoadAlienInt64(buffer, bufferPtr);
            }
        }

        #endregion

        #region Store<T>(T value, ref byte[] buffer, int bufferPtr)

        /// <summary>
        /// Store a single byte
        /// </summary>
        public void StoreByte(byte value, ref byte[] buffer, int bufferPtr)
        {
            if (this.Native())
                buffer[bufferPtr] = value;
            else
                StoreAlienByte(value, ref buffer, ref bufferPtr);
        }

        /// <summary>
        /// Store a single unsigned short
        /// </summary>
        public void StoreUInt16(ushort value, ref byte[] buffer, int bufferPtr)
        {
            if (this.Native())
            {
                foreach (byte b in BitConverter.GetBytes(value))
                    buffer[bufferPtr++] = b;
            }
            else
                StoreAlienUInt16(value, ref buffer, ref bufferPtr);
        }

        /// <summary>
        /// Store a single short
        /// </summary>
        public void StoreInt16(short value, ref byte[] buffer, int bufferPtr)
        {
            if (this.Native())
            {
                foreach (byte b in BitConverter.GetBytes(value))
                    buffer[bufferPtr++] = b;
            }
            else
                StoreAlienInt16(value, ref buffer, ref bufferPtr);
        }

        /// <summary>
        /// Store a single unsigned integer
        /// </summary>
        public void StoreUInt32(uint value, ref byte[] buffer, int bufferPtr)
        {
            if (this.Native())
            {
                foreach (byte b in BitConverter.GetBytes(value))
                    buffer[bufferPtr++] = b;
            }
            else
                StoreAlienUInt32(value, ref buffer, ref bufferPtr);
        }

        /// <summary>
        /// Store a single integer
        /// </summary>
        public void StoreInt32(int value, ref byte[] buffer, int bufferPtr)
        {
            if (this.Native())
            {
                foreach (byte b in BitConverter.GetBytes(value))
                    buffer[bufferPtr++] = b;
            }
            else
                StoreAlienInt32(value, ref buffer, ref bufferPtr);
        }

        /// <summary>
        /// Store a single unsigned long
        /// </summary>
        public void StoreUInt64(ulong value, ref byte[] buffer, int bufferPtr)
        {
            if (this.Native())
            {
                foreach (byte b in BitConverter.GetBytes(value))
                    buffer[bufferPtr++] = b;
            }
            else
                StoreAlienUInt64(value, ref buffer, ref bufferPtr);
        }

        /// <summary>
        /// Store a single long
        /// </summary>
        public void StoreInt64(long value, ref byte[] buffer, int bufferPtr)
        {
            if (this.Native())
            {
                foreach (byte b in BitConverter.GetBytes(value))
                    buffer[bufferPtr++] = b;
            }
            else
                StoreAlienInt64(value, ref buffer, ref bufferPtr);
        }

        #endregion

        #region Store<T>(T[] values, int count, byte[] buffer, int bufferPtr)

        /// <summary>
        /// Store an array of bytes
        /// </summary>
        public void StoreByte(byte[] values, int count, ref byte[] buffer, int bufferPtr)
        {
            foreach (byte value in values)
            {
                if (this.Native())
                    buffer[bufferPtr++] = value;
                else
                    StoreAlienByte(value, ref buffer, ref bufferPtr);
            }
        }

        /// <summary>
        /// Store an array of unsigned shorts
        /// </summary>
        public void StoreUInt16(ushort[] values, int count, ref byte[] buffer, int bufferPtr)
        {
            foreach (ushort value in values)
            {
                if (this.Native())
                {
                    foreach (byte b in BitConverter.GetBytes(value))
                        buffer[bufferPtr++] = b;
                }
                else
                    StoreAlienUInt16(value, ref buffer, ref bufferPtr);
            }
        }

        /// <summary>
        /// Store an array of shorts
        /// </summary>
        public void StoreInt16(short[] values, int count, ref byte[] buffer, int bufferPtr)
        {
            foreach (short value in values)
            {
                if (this.Native())
                {
                    foreach (byte b in BitConverter.GetBytes(value))
                        buffer[bufferPtr++] = b;
                }
                else
                    StoreAlienInt16(value, ref buffer, ref bufferPtr);
            }
        }

        /// <summary>
        /// Store an array of unsigned integers
        /// </summary>
        public void StoreUInt32(uint[] values, int count, ref byte[] buffer, int bufferPtr)
        {
            foreach (uint value in values)
            {
                if (this.Native())
                {
                    foreach (byte b in BitConverter.GetBytes(value))
                        buffer[bufferPtr++] = b;
                }
                else
                    StoreAlienUInt32(value, ref buffer, ref bufferPtr);
            }
        }

        /// <summary>
        /// Store an array of integers
        /// </summary>
        public void StoreInt32(int[] values, int count, ref byte[] buffer, int bufferPtr)
        {
            foreach (int value in values)
            {
                if (this.Native())
                {
                    foreach (byte b in BitConverter.GetBytes(value))
                        buffer[bufferPtr++] = b;
                }
                else
                    StoreAlienInt32(value, ref buffer, ref bufferPtr);
            }
        }

        /// <summary>
        /// Store an array of unsigned longs
        /// </summary>
        public void StoreUInt64(ulong[] values, int count, ref byte[] buffer, int bufferPtr)
        {
            foreach (ulong value in values)
            {
                if (this.Native())
                {
                    foreach (byte b in BitConverter.GetBytes(value))
                        buffer[bufferPtr++] = b;
                }
                else
                    StoreAlienUInt64(value, ref buffer, ref bufferPtr);
            }
        }

        /// <summary>
        /// Store an array of longs
        /// </summary>
        public void StoreInt64(long[] values, int count, ref byte[] buffer, int bufferPtr)
        {
            foreach (long value in values)
            {
                if (this.Native())
                {
                    foreach (byte b in BitConverter.GetBytes(value))
                        buffer[bufferPtr++] = b;
                }
                else
                    StoreAlienInt64(value, ref buffer, ref bufferPtr);
            }
        }

        #endregion

        #region LoadAlien<T>(byte[] buffer, int bufferPtr)

        public byte LoadAlienByte(byte[] buffer, int bufferPtr)
        {
            if (Reversed())
                return Byteswap(buffer[bufferPtr]);
            else
                return DecodeByte(buffer, bufferPtr);
        }

        public ushort LoadAlienUInt16(byte[] buffer, int bufferPtr)
        {
            if (Reversed())
                return Byteswap(BitConverter.ToUInt16(buffer, bufferPtr));
            else
                return DecodeUInt16(buffer, bufferPtr);
        }

        public short LoadAlienInt16(byte[] buffer, int bufferPtr)
        {
            if (Reversed())
                return Byteswap(BitConverter.ToInt16(buffer, bufferPtr));
            else
                return DecodeInt16(buffer, bufferPtr);
        }

        public uint LoadAlienUInt32(byte[] buffer, int bufferPtr)
        {
            if (Reversed())
                return Byteswap(BitConverter.ToUInt32(buffer, bufferPtr));
            else
                return DecodeUInt32(buffer, bufferPtr);
        }

        public int LoadAlienInt32(byte[] buffer, int bufferPtr)
        {
            if (Reversed())
                return Byteswap(BitConverter.ToInt32(buffer, bufferPtr));
            else
                return DecodeInt32(buffer, bufferPtr);
        }

        public ulong LoadAlienUInt64(byte[] buffer, int bufferPtr)
        {
            if (Reversed())
                return Byteswap(BitConverter.ToUInt64(buffer, bufferPtr));
            else
                return DecodeUInt64(buffer, bufferPtr);
        }

        public long LoadAlienInt64(byte[] buffer, int bufferPtr)
        {
            if (Reversed())
                return Byteswap(BitConverter.ToInt64(buffer, bufferPtr));
            else
                return DecodeInt64(buffer, bufferPtr);
        }

        #endregion

        #region StoreAlien<T>(T value, byte[] buffer, int bufferPtr)

        public void StoreAlienByte(byte value, ref byte[] buffer, ref int bufferPtr)
        {
            if (Reversed())
                buffer[bufferPtr] = Byteswap(value);
            else
                EncodeByte(value, ref buffer, ref bufferPtr);
        }

        public void StoreAlienUInt16(ushort value, ref byte[] buffer, ref int bufferPtr)
        {
            if (Reversed())
            {
                foreach (byte b in BitConverter.GetBytes(Byteswap(value)))
                    buffer[bufferPtr++] = b;
            }
            else
                EncodeUInt16(value, ref buffer, ref bufferPtr);
        }

        public void StoreAlienInt16(short value, ref byte[] buffer, ref int bufferPtr)
        {
            if (Reversed())
            {
                foreach (byte b in BitConverter.GetBytes(Byteswap(value)))
                    buffer[bufferPtr++] = b;
            }
            else
                EncodeInt16(value, ref buffer, ref bufferPtr);
        }

        public void StoreAlienUInt32(uint value, ref byte[] buffer, ref int bufferPtr)
        {
            if (Reversed())
            {
                foreach (byte b in BitConverter.GetBytes(Byteswap(value)))
                    buffer[bufferPtr++] = b;
            }
            else
                EncodeUInt32(value, ref buffer, ref bufferPtr);
        }

        public void StoreAlienInt32(int value, ref byte[] buffer, ref int bufferPtr)
        {
            if (Reversed())
            {
                foreach (byte b in BitConverter.GetBytes(Byteswap(value)))
                    buffer[bufferPtr++] = b;
            }
            else
                EncodeInt32(value, ref buffer, ref bufferPtr);
        }

        public void StoreAlienUInt64(ulong value, ref byte[] buffer, ref int bufferPtr)
        {
            if (Reversed())
            {
                foreach (byte b in BitConverter.GetBytes(Byteswap(value)))
                    buffer[bufferPtr++] = b;
            }
            else
                EncodeUInt64(value, ref buffer, ref bufferPtr);
        }

        public void StoreAlienInt64(long value, ref byte[] buffer, ref int bufferPtr)
        {
            if (Reversed())
            {
                foreach (byte b in BitConverter.GetBytes(Byteswap(value)))
                    buffer[bufferPtr++] = b;
            }
            else
                EncodeInt64(value, ref buffer, ref bufferPtr);
        }

        #endregion

        #region Decode<T>(byte[] buffer, int bufferPtr)

        protected byte DecodeByte(byte[] buffer, int bufferPtr)
        {
            return default(byte);
        }

        protected ushort DecodeUInt16(byte[] buffer, int bufferPtr)
        {
            return default(ushort);
        }

        protected short DecodeInt16(byte[] buffer, int bufferPtr)
        {
            return default(short);
        }

        protected uint DecodeUInt32(byte[] buffer, int bufferPtr)
        {
            return default(uint);
        }

        protected int DecodeInt32(byte[] buffer, int bufferPtr)
        {
            return default(int);
        }

        protected ulong DecodeUInt64(byte[] buffer, int bufferPtr)
        {
            return default(ulong);
        }

        protected long DecodeInt64(byte[] buffer, int bufferPtr)
        {
            return default(long);
        }

        #endregion

        #region Encode<T>(T value, ref byte[] buffer, ref int bufferPtr)

        protected void EncodeByte(byte value, ref byte[] buffer, ref int bufferPtr)
        {
        }

        protected void EncodeUInt16(ushort value, ref byte[] buffer, ref int bufferPtr)
        {
        }

        protected void EncodeInt16(short value, ref byte[] buffer, ref int bufferPtr)
        {
        }

        protected void EncodeUInt32(uint value, ref byte[] buffer, ref int bufferPtr)
        {
        }

        protected void EncodeInt32(int value, ref byte[] buffer, ref int bufferPtr)
        {
        }

        protected void EncodeUInt64(ulong value, ref byte[] buffer, ref int bufferPtr)
        {
        }

        protected void EncodeInt64(long value, ref byte[] buffer, ref int bufferPtr)
        {
        }

        #endregion
    }

    public class LittleEndian : Endianness
    {
        public new bool Native()
        {
            return IsLittleEndian();
        }

        protected new bool Reversed()
        {
            return IsBigEndian();
        }

        #region Decode<T>(byte[] buffer, int bufferPtr)

        protected new byte DecodeByte(byte[] buffer, int bufferPtr)
        {
            byte value = 0;
            for (int i = 0; i < sizeof(byte); i++)
                value = (byte)(value | ((byte)(buffer[bufferPtr + i]) << (i * 8)));
            return value;
        }

        protected new ushort DecodeUInt16(byte[] buffer, int bufferPtr)
        {
            ushort value = 0;
            for (int i = 0; i < sizeof(ushort); i++)
                value = (ushort)(value | ((ushort)(buffer[bufferPtr + i]) << (i * 8)));
            return value;
        }

        protected new short DecodeInt16(byte[] buffer, int bufferPtr)
        {
            short value = 0;
            for (int i = 0; i < sizeof(short); i++)
                value = (short)(value | ((short)(buffer[bufferPtr + i]) << (i * 8)));
            return value;
        }

        protected new uint DecodeUInt32(byte[] buffer, int bufferPtr)
        {
            uint value = 0;
            for (int i = 0; i < sizeof(uint); i++)
                value = (uint)(value | ((uint)(buffer[bufferPtr + i]) << (i * 8)));
            return value;
        }

        protected new int DecodeInt32(byte[] buffer, int bufferPtr)
        {
            int value = 0;
            for (int i = 0; i < sizeof(int); i++)
                value = (int)(value | ((int)(buffer[bufferPtr + i]) << (i * 8)));
            return value;
        }

        protected new ulong DecodeUInt64(byte[] buffer, int bufferPtr)
        {
            ulong value = 0;
            for (int i = 0; i < sizeof(ulong); i++)
                value = (ulong)(value | ((ulong)(buffer[bufferPtr + i]) << (i * 8)));
            return value;
        }

        protected new long DecodeInt64(byte[] buffer, int bufferPtr)
        {
            long value = 0;
            for (int i = 0; i < sizeof(long); i++)
                value = (long)(value | ((long)(buffer[bufferPtr + i]) << (i * 8)));
            return value;
        }

        #endregion

        #region Encode<T>(T value, ref byte[] buffer, ref int bufferPtr)

        protected new void EncodeByte(byte value, ref byte[] buffer, ref int bufferPtr)
        {
            for (int i = 0; i < sizeof(byte); i++)
                buffer[i] = (byte)((value >> (i * 8)) & 0xff);
        }

        protected new void EncodeUInt16(ushort value, ref byte[] buffer, ref int bufferPtr)
        {
            for (int i = 0; i < sizeof(ushort); i++)
                buffer[i] = (byte)((value >> (i * 8)) & 0xff);
        }

        protected new void EncodeInt16(short value, ref byte[] buffer, ref int bufferPtr)
        {
            for (int i = 0; i < sizeof(short); i++)
                buffer[i] = (byte)((value >> (i * 8)) & 0xff);
        }

        protected new void EncodeUInt32(uint value, ref byte[] buffer, ref int bufferPtr)
        {
            for (int i = 0; i < sizeof(uint); i++)
                buffer[i] = (byte)((value >> (i * 8)) & 0xff);
        }

        protected new void EncodeInt32(int value, ref byte[] buffer, ref int bufferPtr)
        {
            for (int i = 0; i < sizeof(int); i++)
                buffer[i] = (byte)((value >> (i * 8)) & 0xff);
        }

        protected new void EncodeUInt64(ulong value, ref byte[] buffer, ref int bufferPtr)
        {
            for (int i = 0; i < sizeof(ulong); i++)
                buffer[i] = (byte)((value >> (i * 8)) & 0xff);
        }

        protected new void EncodeInt64(long value, ref byte[] buffer, ref int bufferPtr)
        {
            for (int i = 0; i < sizeof(long); i++)
                buffer[i] = (byte)((value >> (i * 8)) & 0xff);
        }

        #endregion
    }

    public class BigEndian : Endianness
    {
        public new bool Native()
        {
            return IsBigEndian();
        }

        protected new bool Reversed()
        {
            return IsLittleEndian();
        }

        #region Decode<T>(byte[] buffer, int bufferPtr)

        protected new byte DecodeByte(byte[] buffer, int bufferPtr)
        {
            byte value = 0;
            for (int i = 0; i < sizeof(byte); i++)
                value = (byte)(value | (byte)(buffer[i]) << ((sizeof(byte) - i - 1) * 8));
            return value;
        }

        protected new ushort DecodeUInt16(byte[] buffer, int bufferPtr)
        {
            ushort value = 0;
            for (int i = 0; i < sizeof(ushort); i++)
                value = (ushort)(value | (ushort)(buffer[i]) << ((sizeof(ushort) - i - 1) * 8));
            return value;
        }

        protected new short DecodeInt16(byte[] buffer, int bufferPtr)
        {
            short value = 0;
            for (int i = 0; i < sizeof(short); i++)
                value = (short)(value | (short)(buffer[i]) << ((sizeof(short) - i - 1) * 8));
            return value;
        }

        protected new uint DecodeUInt32(byte[] buffer, int bufferPtr)
        {
            uint value = 0;
            for (int i = 0; i < sizeof(uint); i++)
                value = (uint)(value | (uint)(buffer[i]) << ((sizeof(uint) - i - 1) * 8));
            return value;
        }

        protected new int DecodeInt32(byte[] buffer, int bufferPtr)
        {
            int value = 0;
            for (int i = 0; i < sizeof(int); i++)
                value = (int)(value | (int)(buffer[i]) << ((sizeof(int) - i - 1) * 8));
            return value;
        }

        protected new ulong DecodeUInt64(byte[] buffer, int bufferPtr)
        {
            ulong value = 0;
            for (int i = 0; i < sizeof(ulong); i++)
                value = (ulong)(value | (ulong)(buffer[i]) << ((sizeof(ulong) - i - 1) * 8));
            return value;
        }

        protected new long DecodeInt64(byte[] buffer, int bufferPtr)
        {
            long value = 0;
            for (int i = 0; i < sizeof(long); i++)
                value = (long)(value | (long)(buffer[i]) << ((sizeof(long) - i - 1) * 8));
            return value;
        }

        #endregion

        #region Encode<T>(T value, ref byte[] buffer, ref int bufferPtr)

        protected new void EncodeByte(byte value, ref byte[] buffer, ref int bufferPtr)
        {
            for (int i = 0; i < sizeof(byte); i++)
                buffer[i] = (byte)((value >> ((sizeof(byte) - i - 1) * 8)) & 0xff);
        }

        protected new void EncodeUInt16(ushort value, ref byte[] buffer, ref int bufferPtr)
        {
            for (int i = 0; i < sizeof(ushort); i++)
                buffer[i] = (byte)((value >> ((sizeof(ushort) - i - 1) * 8)) & 0xff);
        }

        protected new void EncodeInt16(short value, ref byte[] buffer, ref int bufferPtr)
        {
            for (int i = 0; i < sizeof(short); i++)
                buffer[i] = (byte)((value >> ((sizeof(short) - i - 1) * 8)) & 0xff);
        }

        protected new void EncodeUInt32(uint value, ref byte[] buffer, ref int bufferPtr)
        {
            for (int i = 0; i < sizeof(uint); i++)
                buffer[i] = (byte)((value >> ((sizeof(uint) - i - 1) * 8)) & 0xff);
        }

        protected new void EncodeInt32(int value, ref byte[] buffer, ref int bufferPtr)
        {
            for (int i = 0; i < sizeof(int); i++)
                buffer[i] = (byte)((value >> ((sizeof(int) - i - 1) * 8)) & 0xff);
        }

        protected new void EncodeUInt64(ulong value, ref byte[] buffer, ref int bufferPtr)
        {
            for (int i = 0; i < sizeof(ulong); i++)
                buffer[i] = (byte)((value >> ((sizeof(ulong) - i - 1) * 8)) & 0xff);
        }

        protected new void EncodeInt64(long value, ref byte[] buffer, ref int bufferPtr)
        {
            for (int i = 0; i < sizeof(long); i++)
                buffer[i] = (byte)((value >> ((sizeof(long) - i - 1) * 8)) & 0xff);
        }

        #endregion
    }
}
