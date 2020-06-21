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

using System.IO;
using System.Linq;
using System.Text;
using InnoExtractSharp.Util;

namespace InnoExtractSharp.Crypto
{
    public enum ChecksumType
    {
        None,
        Adler32,
        CRC32,
        MD5,
        SHA1,
    }

    public abstract class Checksum
    {
        public uint Adler32;
        public uint CRC32;
        public byte[] MD5 = new byte[16];
        public byte[] SHA1 = new byte[20];

        public ChecksumType Type;

        public static bool operator ==(Checksum c1, Checksum c2)
        {
            if (c1 == null && c2 != null)
                return false;
            else if (c1 != null && c2 == null)
                return false;

            if (c1.Type != c2.Type)
                return false;

            switch (c1.Type)
            {
                case ChecksumType.None:
                    return true;
                case ChecksumType.Adler32:
                    return (c1.Adler32 == c2.Adler32);
                case ChecksumType.CRC32:
                    return (c1.CRC32 == c2.CRC32);
                case ChecksumType.MD5:
                    return (c1.MD5.SequenceEqual(c2.MD5));
                case ChecksumType.SHA1:
                    return (c1.SHA1.SequenceEqual(c2.SHA1));
                default:
                    return false;
            }
        }
        public static bool operator !=(Checksum c1, Checksum c2)
        {
            return !(c1 == c2);
        }
        public override bool Equals(object obj)
        {
            return (this == (obj as Checksum));
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public abstract void Update(byte[] data, int dataPtr, int length);

        public string GetChecksum()
        {
            string checksum = $"{this.Type} ";
            switch (Type)
            {
                case ChecksumType.None:
                    checksum += "(no checksum)";
                    break;
                case ChecksumType.Adler32:
                    checksum += $"0x{this.Adler32:x}";
                    break;
                case ChecksumType.CRC32:
                    checksum += $"0x{this.CRC32:x}";
                    break;
                case ChecksumType.MD5:
                    for (int i = 0; i < this.MD5.Length; i++)
                    {
                        checksum += $"{this.MD5[i]:x}";
                    }

                    break;
                case ChecksumType.SHA1:
                    for (int i = 0; i < this.SHA1.Length; i++)
                    {
                        checksum += $"{this.SHA1[i]:x}";
                    }

                    break;
            }

            return checksum;
        }

        /// <summary>
        /// Load the data and process it
        /// Data is processed as-is and then converted according to Endianness
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public uint LoadUInt32(Stream input, Endianness end = null)
        {
            if (end == null)
                end = new LittleEndian();

            byte[] buffer = new byte[sizeof(uint)];
            using (BinaryReader br = new BinaryReader(input, Encoding.Default, true))
            {
                buffer = br.ReadBytes(buffer.Length);
                this.Update(buffer, 0, buffer.Length);
                return end.LoadUInt32(buffer, 0);
            }
        }

        /// <summary>
        /// Load the data and process it
        /// Data is processed as-is and then converted according to Endianness
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public byte LoadByte(Stream input, Endianness end = null)
        {
            if (end == null)
                end = new LittleEndian();

            byte[] buffer = new byte[sizeof(byte)];
            using (BinaryReader br = new BinaryReader(input, Encoding.Default, true))
            {
                buffer = br.ReadBytes(buffer.Length);
                this.Update(buffer, 0, buffer.Length);
                return end.LoadByte(buffer, 0);
            }
        }
    }

    public static class ChecksumTypeExtensions
    {
        public static string Name(this ChecksumType type)
        {
            switch (type)
            {
                case ChecksumType.None:
                    return "None";
                case ChecksumType.Adler32:
                    return "Adler32";
                case ChecksumType.CRC32:
                    return "CRC32";
                case ChecksumType.MD5:
                    return "MD5";
                case ChecksumType.SHA1:
                    return "SHA-1";
                default:
                    return string.Empty;
            }
        }
    }
}
