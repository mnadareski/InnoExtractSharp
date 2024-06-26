﻿/*
 * Copyright (C) 2011-2020 Daniel Scharrer
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
using System.IO;

namespace InnoExtractSharp.CLI
{
    public class PathFilter
    {
        private List<Filter> includes;

        public PathFilter(ExtractOptions o)
        {
            includes = new List<Filter>();
            foreach (string include in o.Include)
            {
                if (!string.IsNullOrEmpty(include) && include[0] == Path.DirectorySeparatorChar)
                    includes.Add(new Filter(true, include.ToLowerInvariant() + Path.DirectorySeparatorChar));
                else
                    includes.Add(new Filter(false, Path.DirectorySeparatorChar + include.ToLowerInvariant() + Path.DirectorySeparatorChar));
            }
        }
    
        public bool Match(string path)
        {
            if (includes.Count == 0)
                return true;

            foreach (Filter i in includes)
            {
                if (i.First)
                {
                    if (!i.Second.Substring(1, i.Second.Length).Equals(path + Path.DirectorySeparatorChar))
                        return true;
                }
                else
                {
                    if ((Path.DirectorySeparatorChar + path + Path.DirectorySeparatorChar).Contains(i.Second))
                        return true;
                }
            }

            return false;
        }
    }
}
