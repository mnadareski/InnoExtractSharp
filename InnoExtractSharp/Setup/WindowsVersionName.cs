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

namespace InnoExtractSharp.Setup
{
    public class WindowsVersionName
    {
        public string Name;
        public WindowsVersion.Data Version;

        public static WindowsVersionName[] Names =
        {
            new WindowsVersionName { Name = "Windows 1.0", Version = new WindowsVersion.Data { Major = 1, Minor = 4, Build = 0 }},
            new WindowsVersionName { Name = "Windows 2.0", Version = new WindowsVersion.Data { Major = 2, Minor = 11, Build = 0 }},
            new WindowsVersionName { Name = "Windows 3.0", Version = new WindowsVersion.Data { Major = 3, Minor = 0, Build = 0 }},
            new WindowsVersionName { Name = "Windows for Workgroups 3.11", Version = new WindowsVersion.Data { Major = 3, Minor = 11, Build = 0 }},
            new WindowsVersionName { Name = "Windows 95", Version = new WindowsVersion.Data { Major = 4, Minor = 0, Build = 950 }},
            new WindowsVersionName { Name = "Windows 98", Version = new WindowsVersion.Data { Major = 4, Minor = 1, Build = 1998 }},
            new WindowsVersionName { Name = "Windows 98 Second Edition", Version = new WindowsVersion.Data { Major = 4, Minor = 1, Build = 2222 }},
            new WindowsVersionName { Name = "Windows ME", Version = new WindowsVersion.Data { Major = 4, Minor = 90, Build = 3000 }},
        };
        public static WindowsVersionName[] NtNames =
        {
            new WindowsVersionName { Name = "Windows NT Workstation 3.5", Version = new WindowsVersion.Data { Major = 3, Minor = 5, Build = 807 }},
            new WindowsVersionName { Name = "Windows NT 3.1", Version = new WindowsVersion.Data { Major = 3, Minor = 10, Build = 528 }},
            new WindowsVersionName { Name = "Windows NT Workstation 3.51", Version = new WindowsVersion.Data { Major = 3, Minor = 51, Build = 1057 }},
            new WindowsVersionName { Name = "Windows NT Workstation 4.0", Version = new WindowsVersion.Data { Major = 4, Minor = 0, Build = 1381 }},
            new WindowsVersionName { Name = "Windows 2000", Version = new WindowsVersion.Data { Major = 5, Minor = 0, Build = 2195 }},
            new WindowsVersionName { Name = "Windows XP", Version = new WindowsVersion.Data { Major = 5, Minor = 1, Build = 2600 }},
            new WindowsVersionName { Name = "Windows XP x64", Version = new WindowsVersion.Data { Major = 5, Minor = 2, Build = 3790 }},
            new WindowsVersionName { Name = "Windows Vista", Version = new WindowsVersion.Data { Major = 6, Minor = 0, Build = 6000 }},
            new WindowsVersionName { Name = "Windows 7", Version = new WindowsVersion.Data { Major = 6, Minor = 1, Build = 7600 }},
            new WindowsVersionName { Name = "Windows 8", Version = new WindowsVersion.Data { Major = 6, Minor = 2, Build = 0 }},
            new WindowsVersionName { Name = "Windows 8.1", Version = new WindowsVersion.Data { Major = 6, Minor = 3, Build = 0 }},
            new WindowsVersionName { Name = "Windows 10", Version = new WindowsVersion.Data { Major = 10, Minor = 0, Build = 0 }},
        };

        public static string GetVersionName(WindowsVersion.Data version, bool nt = false)
        {
            WindowsVersionName[] names;
            if (nt)
                names = NtNames;
            else
                names = Names;

            foreach (var name in names)
            {
                if (name.Version.Major == version.Major && name.Version.Minor == version.Minor)
                    return name.Name;
            }

            return null;
        }
    }
}
