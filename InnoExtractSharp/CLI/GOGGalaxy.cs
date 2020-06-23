/*
 * Copyright (C) 2018-2019 Daniel Scharrer
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
using System.Linq;
using InnoExtractSharp.Crypto;
using InnoExtractSharp.Setup;
using InnoExtractSharp.Streams;

namespace InnoExtractSharp.CLI
{
    public class GogGalaxy
    {
        internal static List<string> ParseFunctionCall(string code, string name)
        {
            List<string> arguments = new List<string>();
            if (string.IsNullOrEmpty(code))
                return arguments;

            char[] whitespace = new char[] { ' ', '\t', '\r', '\n' };
            char[] separator = new char[] { ' ', '\t', '\r', '\n', '(', ')', ',', '\'', '"' };

            int start = FindFirstNotOf(code, whitespace);
            if (start == -1)
                return arguments;

            int end = FindFirstOf(code, separator, start);
            if (end == -1)
                return arguments;

            int parenthesis = FindFirstNotOf(code, whitespace, end);
            if (parenthesis == -1 || code[parenthesis] != '(')
                return arguments;

            if (end - start != name.Length || code.CompareTo(name.Substring(start, end - start)) != 0)
                return arguments;

            int p = parenthesis + 1;
            while (true)
            {
                p = FindFirstNotOf(code, whitespace, p);
                if (p == -1)
                {
                    Console.WriteLine($"Error parsing function call: {code}");
                    return arguments;
                }

                arguments.Add(string.Empty); // arguments.resize(arguments.size() + 1);

                if (code[p] == '\'')
                {
                    p++;
                    while (true)
                    {
                        int stringEnd = code.IndexOf('\'', p);
                        arguments[arguments.Count - 1] += code.Substring(p, stringEnd - p);
                        if (stringEnd == -1 || stringEnd + 1 == code.Length)
                        {
                            Console.WriteLine($"Error parsing function call: {code}");
                            return arguments;
                        }

                        p = stringEnd + 1;
                        if (code[p] == '\'')
                        {
                            arguments[arguments.Count - 1] += '\'';
                            p++;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                else
                {
                    int tokenEnd = FindFirstOf(code, separator, p);
                    arguments[arguments.Count - 1] = code.Substring(p, tokenEnd - p);
                    if (tokenEnd == -1 || tokenEnd == code.Length)
                    {
                        Console.WriteLine($"Error parsing function call: {code}");
                        return arguments;
                    }

                    p = tokenEnd;
                }

                p = FindFirstNotOf(code, whitespace, p);
                if (p == -1)
                {
                    Console.WriteLine($"Error parsing function call: {code}");
                    return arguments;
                }

                if (code[p] == ')')
                {
                    break;
                }
                else if (code[p] == ',')
                {
                    p++;
                }
                else
                {
                    Console.WriteLine($"Error parsing function call: {code}");
                    return arguments;
                }
            }

            p++;
            if (p != code.Length)
            {
                p = FindFirstNotOf(code, whitespace, p);
                if (p != -1)
                {
                    if (code[p] != ';' || FindFirstNotOf(code, whitespace, p + 1) != -1)
                        Console.WriteLine($"Error parsing function call: {code}");
                }
            }

            return arguments;
        }

        private static int FindFirstOf(string s, char[] chars, int start = 0)
        {
            int pos = -1;
            for(int i = start; i < s.Length; i++)
            {
                if (chars.Contains(s[i]))
                {
                    pos = i;
                    break;
                }
            }

            return pos;
        }

        private static int FindFirstNotOf(string s, char[] chars, int start = 0)
        {
            int pos = -1;
            for (int i = start; i < s.Length; i++)
            {
                if (!chars.Contains(s[i]))
                {
                    pos = i;
                    break;
                }
            }

            return pos;
        }

        internal static int ParseHex(char c)
        {
            if (c >= '0' && c <= '9')
                return c - '0';
            else if (c >= 'a' && c <= 'f')
                return c - 'a' + 10;
            else if (c >= 'A' && c <= 'F')
                return c - 'a' + 10;
            else
                return -1;
        }

        internal static Checksum ParseChecksum(string str)
        {
            Checksum checksum = new MD5();

            if (str.Length != 32)
            {
                // Unknown checksum type
                checksum.Type = ChecksumType.None;
                return checksum;
            }

            for (int i = 0; i < 16; i++)
            {
                int a = ParseHex(str[2 * i]);
                int b = ParseHex(str[2 * i + 1]);
                if (a < 0 || b < 0)
                {
                    checksum.Type = ChecksumType.None;
                    break;
                }

                checksum.MD5[i] = (byte)((a << 4) | b);
            }

            return checksum;
        }

        internal static List<Constraint> ParseConstraints(string input)
        {
            List<Constraint> result = new List<Constraint>();

            int start = 0;
            while (start < input.Length)
            {
                start = FindFirstNotOf(input, new char[] { ' ', '\t', '\r', '\n' }, start);
                if (start == -1)
                    break;

                bool negated = false;
                if (input[start] == '!')
                {
                    negated = true;
                    start++;
                }

                int end = input.IndexOf('#', start);
                if (end == -1)
                    end = input.Length;

                if (end != start)
                {
                    string token = input.Substring(start, end - start);
                    token = token.Trim();
                    result.Add(new Constraint(token, negated));
                }

                if (end == -1)
                    end = input.Length;

                start = end + 1;
            }

            return result;
        }

        internal static string CreateConstraintExpression(List<Constraint> constraints)
        {
            string result = string.Empty;

            foreach (Constraint entry in constraints)
            {
                if (!string.IsNullOrEmpty(result))
                    result += " or ";

                if (entry.Negated)
                    result += " not ";

                result += entry.Name;
            }

            return result;
        }

        /// <summary>
        /// For some GOG installers, some application files are shipped in GOG Galaxy format:
        /// Thse files are split into one or more parts and then individually compressed.
        /// The parts are decompressed and reassembled by pre-/post-install scripts.
        /// This function parses the arguments to those scripts so that we can re-assemble them ourselves.
        /// 
        /// The first part of a multi-part file has a before_install script that configures the output filename
        /// as well as the number of parts in the file and a checksum for the whole file.
        /// 
        /// Each part (including the first) has an after_install script with a checksum for the decompressed
        /// part as well as compressed and decompressed sizes.
        /// 
        /// Additionally, language constrained are also parsed from check scripts and added to the language list.
        /// </summary>
        public void ParseGalaxyFiles(Info info, bool force)
        {
            if (!force)
            {
                bool isGog = info.Header.AppPublisher.Contains("GOG.com");
                isGog = isGog || info.Header.AppPublisherUrl.Contains("www.gog.com");
                isGog = isGog || info.Header.AppSupportUrl.Contains("www.gog.com");
                isGog = isGog || info.Header.AppUpdatesUrl.Contains("www.gog.com");
                if (!isGog)
                    return;
            }

            FileEntry fileStart = null;
            int remainingParts = 0;

            bool hasLanguageConstratints = false;
            HashSet<string> allLanguages = new HashSet<string>();

            for (int i = 0; i < info.Files.Count; i++)
            {
                FileEntry file = info.Files[i];

                // Multi-part file info: file checksum, filename, part count
                List<string> startInfo = ParseFunctionCall(file.BeforeInstall, "before_install");
                if (startInfo.Count == 0)
                    startInfo = ParseFunctionCall(file.BeforeInstall, "before_install_dependency");

                if (startInfo.Count != 0)
                {
                    if (remainingParts != 0)
                    {
                        Console.WriteLine($"Incomplete GOG Galaxy file {fileStart.Destination}");
                        remainingParts = 0;
                    }

                    // Recover the original filename - parts are named after the MD5 hash of their contents
                    if (startInfo.Count >= 2 && !string.IsNullOrEmpty(startInfo[1]))
                        file.Destination = startInfo[1];

                    file.Checksum = ParseChecksum(startInfo[0]);
                    file.Size = 0;
                    if (file.Checksum.Type == ChecksumType.None)
                        Console.WriteLine($"Could not parse checksum for GOG Galaxy file {file.Destination}: {startInfo[0]}");

                    if (startInfo.Count < 3)
                    {
                        Console.WriteLine($"Missing part count for GOG Galaxy file {file.Destination}");
                        remainingParts = 1;
                    }
                    else
                    {
                        try
                        {
                            if (!Int32.TryParse(startInfo[2], out remainingParts) || remainingParts == 0)
                                remainingParts = 1;

                            fileStart = file;
                        }
                        catch
                        {
                            Console.WriteLine($"Could not parse part count for GOG Galaxy file {file.Destination}: {startInfo[2]}");
                        }
                    }
                }

                // File part ifo: part checksum, compressed part size, uncompressed part size
                List<string> partInfo = ParseFunctionCall(file.AfterInstall, "after_install");
                if (partInfo.Count == 0)
                    partInfo = ParseFunctionCall(file.AfterInstall, "after_install_dependency");

                if (partInfo.Count != 0)
                {
                    if (remainingParts == 0)
                    {
                        Console.WriteLine($"Missing file start for GOG Galaxy file part {file.Destination}");
                    }
                    else if (file.Location > info.DataEntries.Count)
                    {
                        Console.WriteLine($"Invalid data location for GOG Galaxy file part {file.Destination}");
                        remainingParts = 0;
                    }
                    else if (partInfo.Count < 3)
                    {
                        Console.WriteLine($"Mising size for GOG Galaxy file part {file.Destination}");
                        remainingParts = 0;
                    }
                    else
                    {
                        remainingParts--;

                        DataEntry data = info.DataEntries[(int)file.Location];

                        // Ignore file part MD5 checksum, setup already contains a better one for the deflated data
                        try
                        {
                            UInt64.TryParse(partInfo[1], out ulong compressedSize);
                            if (data.File.Size != compressedSize)
                                Console.WriteLine($"Unexpected compressed size for GOG Galaxy file part {file.Destination}: {compressedSize} != {data.File.Size}");
                        }
                        catch
                        {
                            Console.WriteLine($"Could not parse compressed size for GOG Galaxy file part {file.Destination}: {partInfo[1]}");
                        }

                        try
                        {
                            // GOG Galaxy file parts are deflated, inflate them while extracting
                            UInt64.TryParse(partInfo[2], out data.UncompressedSize);
                            data.File.Filter = CompressionFilter.ZlibFilter;

                            fileStart.Size += data.UncompressedSize;
                            if (file != fileStart)
                            {
                                // Ignore this file entry and instead add the data location to the start file
                                file.Destination = string.Empty;
                                fileStart.AdditionalLocations.Add(file.Location);

                                if (file.Components != fileStart.Components
                                    || file.Tasks != fileStart.Tasks
                                    || file.Languages != fileStart.Languages
                                    || file.Check != fileStart.Check
                                    || file.Options != fileStart.Options)
                                {
                                    Console.WriteLine($"Mismatched constraints for different parts of GOG Galaxy file {fileStart.Destination}: {file.Destination}");
                                }
                            }
                        }
                        catch
                        {
                            Console.WriteLine($"Could not parse size for GOG Galaxy file part {file.Destination}: {partInfo[1]}");
                            remainingParts = 0;
                        }
                    }
                }
                else if (startInfo.Count != 0)
                {
                    Console.WriteLine($"Missing part info for GOG Galaxy file {file.Destination}");
                    remainingParts = 0;
                }
                else if (remainingParts != 0)
                {
                    Console.WriteLine($"Incomplete GOG Galaxy file {fileStart.Destination}");
                    remainingParts = 0;
                }

                if (!string.IsNullOrEmpty(file.Destination))
                {
                    // languages, architectures, winversions
                    List<string> check = ParseFunctionCall(file.Check, "check_if_install");
                    if (check.Count != 0 && !string.IsNullOrEmpty(check[0]))
                    {
                        List<Constraint> languages = ParseConstraints(check[0]);
                        foreach (Constraint language in languages)
                        {
                            allLanguages.Add(language.Name);
                        }
                    }
                }

                hasLanguageConstratints = hasLanguageConstratints || !string.IsNullOrEmpty(file.Languages);
            }

            if (remainingParts != 0)
                Console.WriteLine($"Incomplete GOG Galaxy file {fileStart.Destination}");

            /*
            * GOG Galaxy multi-part files also have their own constraints, convert these to standard
            * Inno Setup ones.
            *
            * Do this in a separate loop to not break constraint checks above.
            */
            foreach (FileEntry file in info.Files)
            {
                if (string.IsNullOrEmpty(file.Destination))
                    continue;

                // languages, architectures, winversions
                List<string> check = ParseFunctionCall(file.Check, "check_if_install");
                if (check.Count != 0)
                {
                    if (!string.IsNullOrEmpty(check[0]))
                    {
                        List<Constraint> languages = ParseConstraints(check[0]);

                        // Ignore constraints that just contain all languages
                        bool hasAllLanguages = false;
                        if (languages.Count >= allLanguages.Count && allLanguages.Count > 1)
                        {
                            hasAllLanguages = true;
                            foreach (string knownLanguage in allLanguages)
                            {
                                bool hasLanguage = false;
                                foreach (Constraint language in languages)
                                {
                                    if (!language.Negated && language.Name == knownLanguage)
                                    {
                                        hasLanguage = true;
                                        break;
                                    }
                                }

                                if (!hasLanguage)
                                {
                                    hasAllLanguages = false;
                                    break;
                                }
                            }
                        }

                        if (languages.Count != 0 && !hasAllLanguages)
                        {
                            if (!string.IsNullOrEmpty(file.Languages))
                                Console.WriteLine($"Overwriting language constraints for GOG Galaxy file {file.Destination}");

                            file.Languages = CreateConstraintExpression(languages);
                        }
                    }

                    if (check.Count >= 2 && !string.IsNullOrEmpty(check[1]))
                    {
                        FileEntry.Flags allArch = FileEntry.Flags.Bits32 | FileEntry.Flags.Bits64;
                        FileEntry.Flags arch = 0;
                        if (check[1] != "32#64#")
                        {
                            List<Constraint> architectures = ParseConstraints(check[1]);
                            foreach (Constraint architecture in architectures)
                            {
                                if (architecture.Negated && architectures.Count > 1)
                                    Console.WriteLine($"Ignoring architecture for GOG Galaxy file {file.Destination}: {architecture.Name}");
                                else if (architecture.Name == "32")
                                    arch |= FileEntry.Flags.Bits32;
                                else if (architecture.Name == "64")
                                    arch |= FileEntry.Flags.Bits64;
                                else
                                    Console.WriteLine($"Unknown architecture for GOG Galaxy file {file.Destination}: {architecture.Name}");

                                if (architecture.Negated && architectures.Count <= 1)
                                    arch = allArch & ~arch;
                            }

                            if (arch == allArch)
                                arch = 0;
                        }

                        if ((file.Options & allArch) != 0 && (file.Options & allArch) != arch)
                            Console.WriteLine($"Overwriting architecture constraints for GOG Galaxy file {file.Destination}");

                        file.Options = (file.Options & ~allArch) | arch;
                    }

                    if (check.Count >= 3 && !string.IsNullOrEmpty(check[2]))
                        Console.WriteLine($"Ignoring OS constraint for GOG Galaxy file {file.Destination}: {check[2]}");

                    if (string.IsNullOrEmpty(file.Components))
                        file.Components = "game";
                }

                // component id, ?
                List<string> dependency = ParseFunctionCall(file.Check, "check_if_install_dependency");
                if (dependency.Count != 0)
                {
                    if (string.IsNullOrEmpty(file.Components) && !string.IsNullOrEmpty(dependency[0]))
                        file.Components = dependency[0];
                }
            }

            if (allLanguages.Count != 0)
            {
                if (!hasLanguageConstratints)
                    info.Languages.Clear();

                // info.languages.reserve(all_languages.size());
                foreach (string name in allLanguages)
                {
                    LanguageEntry language = new LanguageEntry();
                    language.Name = name;
                    language.DialogFontSize = 0;
                    language.DialogFontStandardHeight = 0;
                    language.TitleFontSize = 0;
                    language.WelcomeFontSize = 0;
                    language.CopyrightFontSize = 0;
                    language.RightToLeft = false;
                    info.Languages.Add(language);
                }
            }
        }
    }
}
