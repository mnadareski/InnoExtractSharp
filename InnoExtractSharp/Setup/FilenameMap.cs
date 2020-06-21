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
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace InnoExtractSharp.Setup
{
    /// <summary>
    /// Map for converting between stored filenames and output filenames.
    /// </summary>
    public class FilenameMap : Dictionary<string, string>
    {
        public bool Lowercase;
        public bool Expand;

        public FilenameMap()
        {
            Lowercase = false;
            Expand = false;
        }

        public string Lookup(string key)
        {
            return (this.ContainsKey(key) ? this[key] : key);
        }

        public string ExpandVariables(string str, int begin, int end, bool close = false)
        {
            string result = string.Empty;

            while (begin != end)
            {
                // Flush everything before the next bracket
                int pos = begin;
                while (pos != end && str[pos] != '{' && str[pos] != '}')
                    ++pos;

                result += str.Substring(begin, pos);
                begin = pos;

                // No more variables or escape sequences
                if (pos == end)
                    break;

                begin++;

                // Current context closed
                if (close && str[pos] == '}')
                    break;

                // literal '}' character
                if (!close && str[pos] == '}')
                {
                    result += '}';
                    continue;
                }

                // '{{' escape sequence
                if (begin != end && str[begin] == '{')
                {
                    result += '{';
                    begin++;
                    continue;
                }

                // Recursively expand variables until we reach the end of this context
                result += Lookup(ExpandVariables(str, begin, end, true));
            }

            return result;
        }

        public string ShortenPath(string path)
        {
            string result = string.Empty;

            int begin = 0;
            int end = path.Length;

            while (begin != end)
            {
                int s_begin = begin;
                int s_end = path.IndexOfAny(new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, begin, end - begin);
                begin = (s_end == end) ? end : (s_end + 1);

                int segmentLength = s_end - s_begin;

                // Emtpy segment - ignore
                if (segmentLength == 0)
                    continue;

                // '.' segment - ignore
                if (segmentLength == 1 && path[s_begin] == '.')
                    continue;

                // '..' segment - backtrace in result path
                if (segmentLength == 2 && path[s_begin] == '.' && path[s_begin + 1] == '.')
                {
                    int lastSep = result.LastIndexOf(Path.DirectorySeparatorChar);
                    if (lastSep == -1)
                        lastSep = 0;
                    char[] resultArray = result.ToCharArray();
                    Array.Resize(ref resultArray, lastSep);
                    result = new string(resultArray);
                    continue;
                }

                // Normal segment - append to the result path
                if (!String.IsNullOrWhiteSpace(result))
                    result += Path.DirectorySeparatorChar;

                result += path.Substring(s_begin, s_end - s_begin);
            }

            return result;
        }

        public string Convert(string path)
        {
            // Convert paths to lower-case if requested
            if (Lowercase)
                path = path.ToLower();

            // Don't expand variables if requested
            if (!Expand)
                return path;

            string expanded = ExpandVariables(path, 0, path.Length);
            return ShortenPath(expanded);
        }

        /// <summary>
        /// Set if paths should be converted to lower-case.
        /// </summary>
        public void SetLowercase(bool enable)
        {
            Lowercase = enable;
        }

        /// <summary>
        /// Get if paths are be converted to lower-case
        /// </summary>
        public bool IsLowercase()
        {
            return Lowercase;
        }

        /// <summary>
        /// Set if variables should be expanded and path separators converted.
        /// </summary>
        public void SetExpand(bool enable)
        {
            Expand = enable;
        }

        /// <summary>
        /// Check for separators in input paths.
        /// </summary>
        public static bool IsPathSeparator(char c)
        {
            return (c == Path.DirectorySeparatorChar || c == Path.AltDirectorySeparatorChar);
        }

        public static bool IsUnsafePathChar(char c)
        {
            if (c < 32)
            {
                return true;
            }
            switch (c)
            {
                case '<': return true;
                case '>': return true;
                case ':': return true;
                case '"': return true;
                case '|': return true;
                case '?': return true;
                case '*': return true;
                default: return false;
            }
        }

        public static string ReplaceUnsafeChars(string str)
        {
            return new string(str.Select(c => IsUnsafePathChar(c) ? '$' : c).ToArray());
        }
    }
}
