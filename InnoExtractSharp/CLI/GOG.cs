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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using InnoExtractSharp.Crypto;
using InnoExtractSharp.Loader;
using InnoExtractSharp.Setup;
using InnoExtractSharp.Streams;
using InnoExtractSharp.Util;

namespace InnoExtractSharp.CLI
{
    public class Gog
    {
        /// <returns>the GOG.com game ID for this installer or an empty string</returns>
        public static string GetGameId(Info info)
        {
            string id = string.Empty;

            string prefix = "SOFTWARE\\GOG.com\\Games\\";
            int prefixLength = prefix.Length;

            foreach (RegistryEntry entry in info.RegistryEntries)
            {
                if (!entry.Key.StartsWith(prefix))
                    continue;

                if (!entry.Key.Substring(prefixLength).Contains("\\"))
                    continue;

                if (string.Equals(entry.Name, "gameID", StringComparison.OrdinalIgnoreCase))
                {
                    id = Utility.ToUtf8(entry.Value, (KnownCodepage)info.Codepage);
                    break;
                }

                if (string.IsNullOrEmpty(id))
                    id = entry.Key.Substring(prefixLength);
            }

            return id;
        }

        internal static string GetVerb(ExtractOptions o)
        {
            string verb = "inspect";
            if (o.Extract)
                verb = "extract";
            else if (o.Test)
                verb = "test";
            else if (o.List)
                verb = "list the contents of";

            return verb;
        }

        internal static bool ProcessFileUnrar(string file, ExtractOptions o, string password)
        {
            List<string> args = new List<string>();
            string program = "unrar";

            if (o.Extract)
                args.Add("x");
            else if (o.Test)
                args.Add("t");
            else if (o.Silent)
                args.Add("lb");
            else
                args.Add("l");

            args.Add("-p-");
            if (!string.IsNullOrEmpty(password))
                args.Add($"-p{password}");

            args.Add("-idc"); // Disable copyright header

            //if (!Progress.IsEnabled())
            //    args.Add("-idp"); // Disable progress display

            if (o.Filenames.IsLowercase())
                args.Add("-cl"); // Connvert filenames to lowercase

            if (!o.List)
                args.Add("-idq"); // Disable file list

            args.Add("-o+"); // Overwrite existing files

            if (o.PreserveFileTimes)
                args.Add("-tsmca"); // Restore file times
            else
                args.Add("-tsm0c0a0"); // Don't restore file times

            args.Add("-y"); // Enable batch mode

            args.Add("--");

            args.Add(file);

            string dir = o.OutputDir;
            if (!string.IsNullOrEmpty(dir))
            {
                if (!dir.EndsWith("/") && !dir.EndsWith("\\"))
                    dir += Path.DirectorySeparatorChar;

                args.Add(dir);
            }

            // args.Add(null);

            Process process = Process.Start(program, string.Join(" ", args));
            process.WaitForExit();
            if (process.ExitCode < 0)
            {
                process = Process.Start("rar", string.Join(" ", args));
                process.WaitForExit();
                if (process.ExitCode < 0)
                    return false;
            }

            if (process.ExitCode > 0)
                throw new Exception($"Could not {GetVerb(o)} \"{file}\": unrar failed");   

            return true;
        }

        internal static bool ProcessFileUnar(string file, ExtractOptions o, string password)
        {
            string dir = o.OutputDir;

            string program = string.Empty;
            List<string> args = new List<string>();
            if (o.Extract)
            {
                program = "unar";

                args.Add("-f"); // Overwrite existing files

                args.Add("-D"); // Don't create directory

                if (!string.IsNullOrEmpty(dir))
                {
                    args.Add("-o");
                    args.Add(dir);
                }

                if (!o.List)
                    args.Add("-q"); // Disable file list
            }
            else
            {
                program = "lsar";

                if (o.Test)
                    args.Add("-t");
            }

            if (!string.IsNullOrEmpty(password))
            {
                args.Add("-p");
                args.Add(password);
            }

            args.Add("--");

            args.Add(file);

            //args.Add(null);

            Process process = Process.Start(program, string.Join(" ", args));
            process.WaitForExit();
            if (process.ExitCode < 0)
                return false;

            if (process.ExitCode > 0)
                throw new Exception($"Could not {GetVerb(o)} \"{file}\": unar failed");

            return true;
        }

        internal static bool ProcessRarFile(string file, ExtractOptions o, string password)
        {
            return ProcessFileUnrar(file, o, password) || ProcessFileUnar(file, o, password);
        }

        internal static char HexChar(int c)
        {
            if (c < 10)
                return (char)('0' + c);
            else
                return (char)('a' + (c - 10));
        }

        internal static void ProcessRarFiles(List<string> files, ExtractOptions o, Info info)
        {
            if ((!o.List || !o.Test || !o.Extract) || files.Count == 0)
                return;

            // Calculate password from the GOG.com game ID
            string password = GetGameId(info);
            if (!string.IsNullOrEmpty(password))
            {
                MD5 md5 = new MD5();
                md5.Init();
                md5.Update(password.ToCharArray().Select(c => (byte)c).ToArray(), 0, password.Length);
                byte[] hash = md5.Finalize();
                char[] passwordArr = new char[hash.Length * 2];
                for (int i = 0; i < hash.Length; i++)
                {
                    passwordArr[2 * i + 0] = HexChar(hash[i] / 16);
                    passwordArr[2 * i + 1] = HexChar(hash[i] % 16);
                }
            }

            if ((!o.Extract && !o.Test && o.List) && files.Count == 1)
            {
                // When listing contents or for single-file archives, pass the bin file to unrar
                bool ok = true;
                foreach(string file in files)
                {
                    if (!ProcessRarFile(file, o, password))
                        ok = false;
                }

                if (ok)
                    return;
            }
            else
            {
                /*
                * When extracting multi-part archives we need to create symlinks with special
                * names so that unrar will find all the parts of the archive.
                */

                TemporaryDirectory tmpdir = new TemporaryDirectory(o.OutputDir);

                string firstFile = string.Empty;
                try
                {
                    string here = Environment.CurrentDirectory;

                    string basename = files[0];
                    if (basename.EndsWith("-1"))
                        basename = basename.Substring(0, basename.Length - 2);

                    int i = 0;
                    string oss = string.Empty;
                    foreach (string file in files)
                    {
                        oss = string.Empty;
                        oss = $"{basename}.r{i.ToString().PadLeft(2, '0')}";
                        string symlink = Path.Combine(tmpdir.Get(), oss);

                        if (!Path.IsPathRooted(file))
                            CreateSymbolicLink(Path.Combine(here, file), symlink, SymbolicLink.File);
                        else
                            CreateSymbolicLink(file, symlink, SymbolicLink.File);

                        if (i == 0)
                            firstFile = symlink;

                        i++;
                    }
                }
                catch
                {
                    throw new Exception($"Could not {GetVerb(o)} \"{files[0]}\": unable to create .r?? symlinks");
                }

                if (ProcessRarFile(firstFile, o, password))
                    return;
            }

            throw new Exception($"Could not {GetVerb(o)} \"{files[0]}\": install `unrar` or `unar`");
        }

        internal static void ProcessBinFiles(List<string> files, ExtractOptions o, Info info)
        {
            FileStream ifs = new FileStream(files[0], FileMode.Open, FileAccess.Read);
            if (!ifs.CanRead)
                throw new Exception($"Could not open file \"{files[0]}\"");

            byte[] magic = new byte[4];
            if (ifs.Read(magic, 0, magic.Length) == 0)
            {
                string magicString = new string(magic.Select(b => (char)b).ToArray());
                if (magicString.StartsWith("Rar!"))
                {
                    ifs.Close();
                    ProcessRarFiles(files, o, info);
                    return;
                }

                if (magicString.StartsWith("MZ"))
                {
                    Offsets offsets = new Offsets();
                    offsets.Load(ifs);
                    if (offsets.HeaderOffset != 0)
                    {
                        ifs.Close();
                        ExtractOptions newOptions = o;
                        newOptions.Gog = false;
                        newOptions.WarnUnused = false;
                        Console.WriteLine();
                        Extract.ProcessFile(files[0], newOptions);
                        return;
                    }
                }
            }

            throw new Exception($"Could not {GetVerb(o)} \"{files[0]}\": unknown filetype");
        }

        internal static int ProbeBinFileSeries(ExtractOptions o, Info info, string dir, string basename, int format = 0, int start = 0)
        {
            int count = 0;

            List<string> files = new List<string>();

            for (int i = start; ; i++)
            {
                string file = string.Empty;
                if (format == 0)
                    file = Path.Combine(dir, basename);
                else
                    file = Path.Combine(dir, SliceReader.SliceFilename(basename, i, format));

                try
                {
                    if (!File.Exists(file))
                        break;
                }
                catch
                {
                    break;
                }

                if (o.Gog)
                {
                    files.Add(file);
                }
                else
                {
                    Console.WriteLine($"{Path.GetFileName(file)} is not part of the installer!");
                    count++;
                }

                if (format == 0)
                    break;
            }

            if (files.Count != 0)
                ProcessBinFiles(files, o, info);

            return count;
        }

        public static void ProbeBinFiles(ExtractOptions o, Info info, string setupFile, bool external)
        {
            string dir = Path.GetDirectoryName(setupFile);
            string basename = Path.GetFileNameWithoutExtension(setupFile);

            int binCount = 0;
            binCount += ProbeBinFileSeries(o, info, dir, basename + ".bin");
            binCount += ProbeBinFileSeries(o, info, dir, basename + "-0" + ".bin");

            uint maxSlice = 0;
            if (external)
            {
                foreach (DataEntry location in info.DataEntries)
                {
                    maxSlice = Math.Max(maxSlice, location.Chunk.FirstSlice);
                    maxSlice = Math.Max(maxSlice, location.Chunk.LastSlice);
                }
            }

            int slice = 0;
            int format = 1;
            if (external && info.Header.SlicesPerDisk == 1)
                slice = (int)maxSlice + 1;

            binCount += ProbeBinFileSeries(o, info, dir, basename, format, slice);

            slice = 0;
            format = 2;
            if (external && info.Header.SlicesPerDisk != 1)
            {
                slice = (int)maxSlice + 1;
                format = info.Header.SlicesPerDisk;
            }

            binCount += ProbeBinFileSeries(o, info, dir, basename, format, slice);

            if (binCount != 0)
            {
                string verb = "inspecting";
                if (o.Extract)
                    verb = "extracting";
                else if (o.Test)
                    verb = "testing";
                else if (o.List)
                    verb = "listing the contents of";

                Console.WriteLine($"Use the --gog option to try {verb} {(binCount > 1 ? "these files" : "this file")}.");
            }
        }

        #region External

        // https://stackoverflow.com/questions/11156754/what-the-c-sharp-equivalent-of-mklink-j
        [DllImport("kernel32.dll")]
        private static extern bool CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, SymbolicLink dwFlags);

        private enum SymbolicLink
        {
            File = 0,
            Directory = 1
        }

        #endregion
    }
}
