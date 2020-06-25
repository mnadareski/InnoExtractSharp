/*
 * Copyright (C) 2011-2019 Daniel Scharrer
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

using InnoExtractSharp.Util;
using System.IO;

namespace InnoExtractSharp.Setup
{
    public class WindowsVersion
    {
        public class Data
        {
            public uint Major;
            public uint Minor;
            public uint Build;

            public static bool operator ==(Data d1, Data d2)
            {
                return (d1.Build == d2.Build && d1.Major == d2.Major && d1.Minor == d2.Minor);
            }

            public static bool operator !=(Data d1, Data d2)
            {
                return !(d1 == d2);
            }

            public void Load(Stream input, InnoVersion version)
            {
                if (version >= InnoVersion.INNO_VERSION(1, 3, 21))
                    Build = Endianness<ushort>.LoadLittleEndian(input);
                else
                    Build = 0;

                Minor = Endianness<byte>.LoadLittleEndian(input);
                Major = Endianness<byte>.LoadLittleEndian(input);
            }
        }

        public Data WinVersion = new Data();
        public Data NtVersion = new Data();

        public class ServicePack
        {
            public uint Major;
            public uint Minor;

            public static bool operator ==(ServicePack sp1, ServicePack sp2)
            {
                return (sp1.Major == sp2.Major && sp1.Minor == sp2.Minor);
            }

            public static bool operator !=(ServicePack sp1, ServicePack sp2)
            {
                return !(sp1 == sp2);
            }
        }

        public ServicePack NtServicePack = new ServicePack();

        public readonly static WindowsVersion None = new WindowsVersion
        {
            WinVersion = new Data { Major = 0, Minor = 0, Build = 0 },
            NtVersion = new Data { Major = 0, Minor = 0, Build = 0 },
            NtServicePack = new ServicePack { Major = 0, Minor = 0 },
        };

        public void Load(Stream input, InnoVersion version)
        {
            WinVersion.Load(input, version);
            NtVersion.Load(input, version);

            if (version >= InnoVersion.INNO_VERSION(1, 3, 21))
            {
                NtServicePack.Minor = Endianness<byte>.LoadLittleEndian(input);
                NtServicePack.Major = Endianness<byte>.LoadLittleEndian(input);
            }
            else
            {
                NtServicePack.Minor = 0;
                NtServicePack.Major = 0;
            }
        }

        public static bool operator ==(WindowsVersion wv1, WindowsVersion wv2)
        {
            return (wv1.WinVersion == wv2.WinVersion
                && wv1.NtVersion == wv2.NtVersion
                && wv1.NtServicePack == wv2.NtServicePack);
        }

        public static bool operator !=(WindowsVersion wv1, WindowsVersion wv2)
        {
            return !(wv1 == wv2);
        }
    }
}
