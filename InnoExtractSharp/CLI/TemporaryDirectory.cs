/*
 * Copyright (C) 2014-2019 Daniel Scharrer
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

namespace InnoExtractSharp.CLI
{
    public class TemporaryDirectory
    {
        private string parent;
        private string path;

        public TemporaryDirectory(string basePath)
        {
            try
            {
                if (!string.IsNullOrEmpty(basePath) && !File.Exists(basePath))
                {
                    Directory.CreateDirectory(basePath);
                    parent = basePath;
                }

                int tmpnum = 0;
                string oss = string.Empty;
                do
                {
                    oss += $"innoextract-tmp-{tmpnum++}";
                    path = Path.Combine(basePath, oss);
                } while (Directory.Exists(path));

                Directory.CreateDirectory(path);
            }
            catch
            {
                path = string.Empty;
                throw new Exception("Could not create temporary directory!");
            }
        }

        ~TemporaryDirectory()
        {
            if (!string.IsNullOrEmpty(path))
            {
                try
                {
                    Directory.Delete(path, true);
                    if (!string.IsNullOrEmpty(parent))
                        Directory.Delete(parent);
                }
                catch
                {
                    Console.WriteLine($"Could not remove temporary directory {path}!");
                }
            }
        }

        public string Get() { return path; }
    }
}
