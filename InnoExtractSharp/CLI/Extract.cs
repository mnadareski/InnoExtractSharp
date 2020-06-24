/*
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using InnoExtractSharp.Crypto;
using InnoExtractSharp.Loader;
using InnoExtractSharp.Setup;
using InnoExtractSharp.Streams;
using InnoExtractSharp.Util;

namespace InnoExtractSharp.CLI
{
    public enum CollisionAction
    {
        OverwriteCollisions,
        RenameCollisions,
        RenameAllCollisions,
        ErrorOnCollisions,
    }

    public class Extract
    {
        internal static void PrintFilterInfo(Item item, bool temp)
        {
            bool first = true;

            if (!string.IsNullOrEmpty(item.Languages))
            {
                Console.Write($" [{item.Languages}");
                first = false;
            }

            if (temp)
            {
                Console.Write($"{(first ? " [" : ", ")}{temp}");
                first = false;
            }

            if (!first)
                Console.Write("]");
        }

        internal static void PrintFilterInfo(FileEntry file)
        {
            bool isTemp = (file.Options & FileEntry.Flags.DeleteAfterInstall) != 0;
            PrintFilterInfo(file, isTemp);
        }

        internal static void PrintFilterInfo(DirectoryEntry dir)
        {
            bool isTemp = (dir.Options & DirectoryEntry.Flags.DeleteAfterInstall) != 0;
            PrintFilterInfo(dir, isTemp);
        }

        internal static void PrintSizeInfo(InnoFile file, ulong size)
        {
            if (Logger.Debug)
                Console.Write($" @ {file.Offset:x}");

            Console.Write($" ({(size != 0 ? size : file.Size)})");
        }

        internal static void PrintChecksumInfo(InnoFile file, Checksum checksum)
        {
            if (checksum == null || checksum.Type == ChecksumType.None)
                checksum = file.Checksum;

            Console.Write(checksum.GetChecksum());
        }

        internal static bool PromptOverwrite()
        {
            return true; // TODO the user always overwrites
        }

        internal static string HandleCollision(FileEntry oldfile, DataEntry olddata, FileEntry newfile, DataEntry newdata)
        {
            bool allowTimestamp = true;

            if ((newfile.Options & FileEntry.Flags.IgnoreVersion) == 0)
            {
                bool versionInfoValid = (newdata.Options & DataEntry.Flags.VersionInfoValid) != 0;

                if ((olddata.Options & DataEntry.Flags.VersionInfoValid) != 0)
                {
                    allowTimestamp = false;

                    if (!versionInfoValid || olddata.FileVersion > newdata.FileVersion)
                    {
                        if (((newfile.Options & FileEntry.Flags.PromptIfOlder) == 0) || !PromptOverwrite())
                            return "old version";
                    }
                    else if (newdata.FileVersion == olddata.FileVersion && (newfile.Options & FileEntry.Flags.OverwriteSameVersion) == 0)
                    {
                        if ((newfile.Options & FileEntry.Flags.ReplaceSameVersionIfContentsDiffer) != 0 && olddata.File.Checksum == newdata.File.Checksum)
                            return "duplicate (checksum)";

                        if ((newfile.Options & FileEntry.Flags.CompareTimeStamp) == 0)
                            return "duplicate (version)";

                        allowTimestamp = true;
                    }
                }
                else if (versionInfoValid)
                {
                    allowTimestamp = false;
                }
            }

            if (allowTimestamp && (newfile.Options & FileEntry.Flags.CompareTimeStamp) != 0)
            {
                if (newdata.Timestamp == olddata.Timestamp && newdata.TimestampNsec == olddata.TimestampNsec)
                    return "duplicate (modification time)";

                if (newdata.Timestamp < olddata.Timestamp
                    || (newdata.Timestamp == olddata.Timestamp && newdata.TimestampNsec < olddata.TimestampNsec))
                {
                    if (((newfile.Options & FileEntry.Flags.PromptIfOlder) == 0) || !PromptOverwrite())
                        return "old version (modification time)";
                }
            }

            if (((newfile.Options & FileEntry.Flags.ConfirmOverwrite) != 0) && !PromptOverwrite())
                return "user chose not to overwrite";

            if (oldfile.Attributes != UInt32.MaxValue && ((oldfile.Attributes & FileEntry.Flags.ReadOnly) != 0))
            {
                if (((newfile.Options & FileEntry.Flags.OverwriteReadOnly) == 0) && !PromptOverwrite())
                    return "user chose not to overwrite read-only file";
            }

            return null; // overwrite old file
        }

        internal static string ParentDir(string path)
        {
            int pos = path.LastIndexOf(Setup.PathSep);
            if (pos == -1)
                return string.Empty;

            return path.Substring(0, pos);
        }

        internal static bool InsertDirs(DirectoriesMap processedDirectories, PathFilter includes, string internalPath, string path, bool implied)
        {
            string dir = ParentDir(path);
            string internalDir = ParentDir(internalPath);

            if (string.IsNullOrEmpty(internalDir))
                return false;

            if (implied || includes.Match(internalDir))
            {
                bool existingSecond = processedDirectories.ContainsKey(internalDir);
                if (!existingSecond)
                    processedDirectories[internalDir] = new ProcessedDirectory(dir);

                ProcessedDirectory existingFirst = processedDirectories[internalDir];

                if (implied)
                    existingFirst.SetImplied(true); // TODO: existing.first->second.set_implied(true);

                if (existingSecond)
                {
                    if (existingFirst.Path() != dir)
                    {
                        // Existing dir case differs, fix path
                        if (existingFirst.Path().Length == dir.Length)
                            path.Replace(dir, existingFirst.Path());
                        else
                            path = existingFirst.Path() + path.Substring(dir.Length);

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                implied = true;
            }

            int oldLength = dir.Length;
            if (InsertDirs(processedDirectories, includes, internalDir, dir, implied))
            {
                // Existing dir case differs, fix path
                if (dir.Length == oldLength)
                    path = dir + path.Substring(dir.Length);
                else
                    path = dir + path.Substring(oldLength);

                // Also fix previously inserted directory
                bool inserted = processedDirectories.ContainsKey(internalDir);
                if (inserted)
                    processedDirectories[internalDir].SetPath(dir);

                return true;
            }

            return false;
        }

        internal static bool RenameCollision(ExtractOptions o, FilesMap processedFiles, string path, ProcessedFile other, bool commonComponent, bool commonLanguage, bool commonArch, bool first)
        {
            FileEntry file = other.Entry();

            bool requireNumberSuffix = !first || (o.Collisions == CollisionAction.RenameAllCollisions);
            string oss = string.Empty;
            FileEntry.Flags archFlags = FileEntry.Flags.Bits32 | FileEntry.Flags.Bits64;

            if (!commonComponent && !string.IsNullOrEmpty(file.Components))
            {
                if (Setup.IsSimpleExpression(file.Components))
                {
                    requireNumberSuffix = false;
                    oss += $"#{file.Components}";
                }
            }

            if (!commonLanguage && !string.IsNullOrEmpty(file.Languages))
            {
                if (Setup.IsSimpleExpression(file.Languages))
                {
                    requireNumberSuffix = false;
                    oss += $"@{file.Languages}";
                }
            }

            if (!commonArch && (file.Options & archFlags) == FileEntry.Flags.Bits32)
            {
                requireNumberSuffix = false;
                oss += "@32bit";
            }
            else if (!commonArch && (file.Options & archFlags) == FileEntry.Flags.Bits64)
            {
                requireNumberSuffix = false;
                oss += "@64bit";
            }

            int i = 0;
            string suffix = oss;
            if (requireNumberSuffix)
                oss += $"${i++}";

            for(; ; )
            {
                bool insertionSecond = processedFiles.ContainsKey(path + oss);
                if (!insertionSecond)
                    processedFiles[path + oss] = new ProcessedFile(other.Path() + oss, file);

                ProcessedFile insertionFirst = processedFiles[path + oss];

                if (!insertionSecond)
                {
                    // Found an available name and inserted
                    return true;
                }
                
                if (insertionFirst.Entry() == file)
                {
                    // File already has the desired name, abort
                    return false;
                }

                oss = suffix;
                oss += $"${i++}";
            }
        }

        internal static void RenameCollisions(ExtractOptions o, FilesMap processedFiles, CollisionMap collisions)
        {
            foreach (var collision in collisions)
            {
                string path = collision.Key;

                if (!processedFiles.TryGetValue(path, out ProcessedFile baseFile))
                    continue;

                FileEntry file = baseFile.Entry();
                FileEntry.Flags archFlags = FileEntry.Flags.Bits32 | FileEntry.Flags.Bits64;

                bool commonComponent = true;
                bool commonLanguage = true;
                bool commonArch = true;
                foreach (ProcessedFile other in collision.Value)
                {
                    commonComponent = commonComponent && other.Entry().Components == file.Components;
                    commonLanguage = commonLanguage && other.Entry().Languages == file.Languages;
                    commonArch = commonArch && (other.Entry().Options & archFlags) == (file.Options & archFlags);
                }

                bool ignoreComponent = commonComponent || o.Collisions != CollisionAction.RenameAllCollisions;
                if (RenameCollision(o, processedFiles, path, baseFile, ignoreComponent, commonLanguage, commonArch, true))
                    processedFiles.Remove(path);

                foreach (ProcessedFile other in collision.Value)
                {
                    RenameCollision(o, processedFiles, path, other, commonComponent, commonLanguage, commonArch, false);
                }
            }
        }

        internal static bool PrintFileInfo(ExtractOptions o, Info info)
        {
            if (!o.Quiet)
            {
                string name = string.IsNullOrEmpty(info.Header.AppVersionedName) ? info.Header.AppName : info.Header.AppVersionedName;
                string verb = "Inspecting";
                if (o.Extract)
                    verb = "Extracting";
                else if (o.Test)
                    verb = "Testing";
                else if (o.List)
                    verb = "Listing";

                Console.WriteLine($"{verb} \"{name}\" - setup data version {info.Version}");
            }

            if (Logger.Debug)
            {
                Console.WriteLine();
                Debug.PrintInfo(info);
                Console.WriteLine();
            }

            bool multipleSections = (o.ListLanguages ? 1 : 0) + (o.GogGameId ? 1 : 0) + (o.List ? 1 : 0) + (o.ShowPassword ? 1 : 0) > 1;
            if (!o.Quiet && multipleSections)
                Console.WriteLine();

            if (o.ListLanguages)
            {
                if (o.Silent)
                {
                    foreach (LanguageEntry language in info.Languages)
                    {
                        Console.WriteLine($"{language.Name} {language.LanguageName}");
                    }
                }
                else
                {
                    if (multipleSections)
                        Console.WriteLine("Languages:");

                    foreach (LanguageEntry language in info.Languages)
                    {
                        Console.Write($" - {language.Name}");
                        if (!string.IsNullOrEmpty(language.LanguageName))
                            Console.Write($": {language.LanguageName}");

                        Console.WriteLine();
                    }

                    if (info.Languages.Count == 0)
                        Console.WriteLine(" (none)\n");
                }

                if ((o.Silent || !o.Quiet) && multipleSections)
                    Console.WriteLine();
            }

            if (o.GogGameId)
            {
                string id = Gog.GetGameId(info);
                if (string.IsNullOrEmpty(id))
                {
                    if (!o.Quiet)
                        Console.WriteLine("No GOG.com game ID found!");
                }
                else if (!o.Silent)
                {
                    Console.WriteLine($"GOG.com game ID is {id}");
                }
                else
                {
                    Console.WriteLine(id);
                }

                if ((o.Silent || !o.Quiet) && multipleSections)
                    Console.WriteLine();
            }

            if (o.ShowPassword)
            {
                if ((info.Header.Options & Header.Flags.Password) != 0)
                {
                    if (o.Silent)
                        Console.WriteLine(info.Header.Password);
                    else
                        Console.WriteLine($"Password hash: {info.Header.Password}");

                    if (o.Silent)
                    {
                        Console.WriteLine(info.Header.PasswordSalt);
                    }
                    else if (!string.IsNullOrEmpty(info.Header.PasswordSalt))
                    {
                        Console.Write($"Password salt: {info.Header.PasswordSalt}");
                        if (!o.Quiet)
                            Console.Write(" (hex bytes, prepended to password)");

                        Console.WriteLine();
                    }

                    if (o.Silent)
                        Console.WriteLine(Utility.EncodingName(info.Codepage));
                    else
                        Console.WriteLine($"Password encoding: {Utility.EncodingName(info.Codepage)}");
                }
                else if (!o.Quiet)
                {
                    Console.WriteLine("Setup is not passworded!");
                }

                if ((o.Silent || !o.Quiet) && multipleSections)
                    Console.WriteLine();
            }

            return multipleSections;
        }

        internal static ProcessedEntries FilterEntries(ExtractOptions o, Info info)
        {
            ProcessedEntries processed = new ProcessedEntries();
            CollisionMap collisions = new CollisionMap();
            PathFilter includes = new PathFilter(o);

            // Filter the directories to be created
            foreach (DirectoryEntry directory in info.Directories)
            {
                if (!o.ExtractTemp && (directory.Options & DirectoryEntry.Flags.DeleteAfterInstall) != 0)
                    continue; // Ignore temporary dirs

                if (!string.IsNullOrEmpty(directory.Languages))
                {
                    if (!string.IsNullOrEmpty(o.Language) && !Setup.ExpressionMatch(o.Language, directory.Languages))
                        continue; // Ignore other languages
                }
                else if (o.LanguageOnly)
                {
                    continue; // Ignore language-agnostic dirs
                }

                string path = o.Filenames.Convert(directory.Name);
                if (string.IsNullOrEmpty(path))
                    continue; // Don't know what to do with this

                string internalPath = path.ToLowerInvariant();

                bool pathIncluded = includes.Match(internalPath);

                InsertDirs(processed.Directories, includes, internalPath, path, pathIncluded);

                ProcessedDirectory existingFirst;
                if (pathIncluded)
                {
                    bool existingSecond = processed.Directories.ContainsKey(internalPath);
                    if (!existingSecond)
                        processed.Directories[internalPath] = new ProcessedDirectory(path);

                    existingFirst = processed.Directories[internalPath];
                }
                else
                {
                    bool existingSecond = processed.Directories.ContainsKey(internalPath);
                    if (!existingSecond)
                        continue;

                    existingFirst = processed.Directories[internalPath];
                }

                existingFirst.SetEntry(directory);
            }

            // Filter the files to be extracted
            foreach (FileEntry file in info.Files)
            {
                if (file.Location >= info.DataEntries.Count)
                    continue; // Ignore external files (copy commands)

                if (!o.ExtractTemp && (file.Options & FileEntry.Flags.DeleteAfterInstall) != 0)
                    continue; // Ignore temporary files

                if (!string.IsNullOrEmpty(file.Languages))
                {
                    if (!string.IsNullOrEmpty(o.Language) && !Setup.ExpressionMatch(o.Language, file.Languages))
                        continue; // Ignore other languages
                }
                else if (o.LanguageOnly)
                {
                    continue; // Ignore language-agnostic files
                }

                string path = o.Filenames.Convert(file.Destination);
                if (string.IsNullOrEmpty(path))
                    continue; // Internal file, not extracted

                string internalPath = path.ToLowerInvariant();

                bool pathIncluded = includes.Match(internalPath);

                InsertDirs(processed.Directories, includes, internalPath, path, pathIncluded);

                if (!pathIncluded)
                    continue; // Ignore excluded file

                bool insertionSecond = processed.Files.ContainsKey(internalPath);
                if (!insertionSecond)
                    processed.Files[internalPath] = new ProcessedFile(file, path);

                if (insertionSecond)
                {
                    // Collision!
                    ProcessedFile existing = processed.Files[internalPath];
                
                    if (o.Collisions == CollisionAction.ErrorOnCollisions)
                    {
                        throw new ArgumentException($"Collision: {path}");
                    }
                    else if (o.Collisions == CollisionAction.RenameAllCollisions)
                    {
                        collisions[internalPath].Add(new ProcessedFile(file, path));
                    }
                    else
                    {
                        DataEntry newdata = info.DataEntries[(int)file.Location];
                        DataEntry olddata = info.DataEntries[(int)existing.Entry().Location];
                        string skip = HandleCollision(existing.Entry(), olddata, file, newdata);

                        if (!string.IsNullOrEmpty(o.DefaultLanguage))
                        {
                            bool oldlang = Setup.ExpressionMatch(o.DefaultLanguage, file.Languages);
                            bool newlang = Setup.ExpressionMatch(o.DefaultLanguage, existing.Entry().Languages);
                            if (oldlang && !newlang)
                                skip = null;
                            else if (!oldlang && newlang)
                                skip = "overwritten";
                        }

                        if (o.Collisions == CollisionAction.RenameCollisions)
                        {
                            FileEntry clobberedfile = skip != null ? file : existing.Entry();
                            string clobberedpath = skip != null ? path : existing.Path();
                            collisions[internalPath].Add(new ProcessedFile(clobberedfile, clobberedpath));
                        }
                        else if (!o.Silent)
                        {
                            Console.Write(" - ");
                            string clobberedpath = skip != null ? path : existing.Path();
                            Console.Write($"\"{clobberedpath}\"");
                            PrintFilterInfo(skip != null ? file : existing.Entry());
                            if (o.ListSizes)
                                PrintSizeInfo(skip != null ? newdata.File : olddata.File, skip != null ? file.Size : existing.Entry().Size);
                            if (o.ListChecksums)
                            {
                                Console.Write(" ");
                                PrintChecksumInfo(skip != null ? newdata.File : olddata.File, skip != null ? file.Checksum : existing.Entry().Checksum);
                            }

                            Console.WriteLine($" - {(skip != null ? skip : "overwritten")}");
                        }

                        if (skip == null)
                        {
                            existing.SetEntry(file);
                            if (file.Type != FileEntry.FileType.UninstExe)
                            {
                                // Old file is "deleted" first → use case from new file
                                existing.SetPath(path);
                            }
                        }
                    }
                }
            }

            if (o.Collisions == CollisionAction.RenameCollisions || o.Collisions == CollisionAction.RenameAllCollisions)
                RenameCollisions(o, processed.Files, collisions);

            return processed;
        }

        internal static void CreateOutputDirectory(ExtractOptions o)
        {
            try
            {
                if (!string.IsNullOrEmpty(o.OutputDir) && !Directory.Exists(o.OutputDir))
                    Directory.CreateDirectory(o.OutputDir);
            }
            catch
            {
                throw new Exception($"Could not create output directory \"{o.OutputDir}\"");
            }
        }

        public static void ProcessFile(string installer, ExtractOptions o)
        {
            bool isDirectory;
            try
            {
                isDirectory = Directory.Exists(installer);
            }
            catch
            {
                throw new Exception($"Could not open file \"{installer}\": access denied");
            }

            if (isDirectory)
                throw new Exception($"Input file \"{installer}\" is a directory!");

            FileStream ifs;
            try
            {
                ifs = new FileStream(installer, FileMode.Open, FileAccess.Read);
                if (!ifs.CanRead)
                    throw new Exception();
            }
            catch
            {
                throw new Exception($"Could not open file \"{installer}\"");
            }

            Offsets offsets = new Offsets();
            offsets.Load(ifs);

            if (o.DataVersion)
            {
                InnoVersion version = new InnoVersion();
                ifs.Seek(offsets.HeaderOffset, SeekOrigin.Begin);
                version.Load(ifs);
                if (o.Silent)
                    Console.WriteLine(version);
                else
                    Console.WriteLine(version); // Had color data before

                return;
            }

            if (o.DumpHeaders)
            {
                CreateOutputDirectory(o);
                return;
            }

            Info.EntryTypes entries = 0;
            if (o.List || o.Test || o.Extract || (o.GogGalaxy && o.ListLanguages))
            {
                entries |= Info.EntryTypes.Files;
                entries |= Info.EntryTypes.Directories;
                entries |= Info.EntryTypes.DataEntries;
            }

            if (o.ListLanguages)
                entries |= Info.EntryTypes.Languages;

            if (o.GogGameId || o.Gog)
                entries |= Info.EntryTypes.RegistryEntries;

            if (!o.ExtractUnknown)
                entries |= Info.EntryTypes.NoUnknownVersion;

            ifs.Seek(offsets.HeaderOffset, SeekOrigin.Begin);
            Info info = new Info();
            try
            {
                info.Load(ifs, entries, o.Codepage);
            }
            catch (VersionError)
            {
                string headerfile = installer;
                Path.ChangeExtension(headerfile, ".0");
                if (offsets.HeaderOffset == 0 && headerfile != installer && File.Exists(headerfile))
                {
                    Console.Write($"Opening \"{headerfile}\"");
                    ProcessFile(headerfile, o);
                    return;
                }

                if (offsets.FoundMagic)
                {
                    if (offsets.HeaderOffset == 0)
                        throw new FormatError("Could not determine location of setup headers!");
                    else
                        throw new FormatError("Could not determine setup data version!");
                }

                throw;
            }
            catch (Exception e)
            {
                string oss = string.Empty;
                oss += "Stream error while parsing setup headers!\n";
                oss += $" ├─ detected setup version: {info.Version}\n";
                oss += $" └─ error reason: {e.Message}";
                throw new FormatError(oss);
            }

            if (o.GogGalaxy && (o.List || o.Test || o.Extract || o.ListLanguages))
                GogGalaxy.ParseGalaxyFiles(info, o.Gog);

            bool multipleSections = PrintFileInfo(o, info);

            string password = string.Empty;
            if (string.IsNullOrEmpty(o.Password))
            {
                if (!o.Quiet && (o.List || o.Test || o.Extract) && (info.Header.Options & Header.Flags.EncryptionUsed) != 0)
                    Console.WriteLine("Setup contains encrypted files, use the --password option to extract them");
            }
            else
            {
                password = Utility.FromUtf8(o.Password, (KnownCodepage)info.Codepage);
                if ((info.Header.Options & Header.Flags.Password) != 0)
                {
                    Hasher checksum = new Hasher(info.Header.Password.Type);
                    checksum.Update(info.Header.PasswordSalt.ToCharArray().Select(c => (byte)c).ToArray(), 0, info.Header.PasswordSalt.Length);
                    checksum.Update(password.ToCharArray().Select(c => (byte)c).ToArray(), 0, password.Length);
                    if (checksum.Finalize() != info.Header.Password)
                    {
                        if (o.CheckPassword)
                            throw new ArgumentException("Incorrect password provided");

                        Console.WriteLine("Incorrect password provided");
                        password = string.Empty;
                    }
                }
            }

            if (!o.List && !o.Test && !o.Extract)
                return;

            if (!o.Silent && multipleSections)
                Console.WriteLine("Files:");

            ProcessedEntries processed = FilterEntries(o, info);

            if (o.Extract)
                CreateOutputDirectory(o);

            if (o.List || o.Extract)
            {
                foreach (var i in processed.Directories)
                {
                    string path = i.Value.Path();

                    if (o.List && !i.Value.Implied())
                    {
                        if (!o.Silent)
                        {
                            Console.Write(" - ");
                            Console.Write($"\"{path}{Setup.PathSep}\"");
                            if (i.Value.HasEntry())
                                PrintFilterInfo(i.Value.Entry());

                            Console.WriteLine();
                        }
                        else
                        {
                            Console.WriteLine($"{path}{Setup.PathSep}");
                        }
                    }

                    if (o.Extract)
                    {
                        string dir = Path.Combine(o.OutputDir, path);
                        try
                        {
                            Directory.CreateDirectory(dir);
                        }
                        catch
                        {
                            throw new Exception($"Could not create directory \"{dir}\"");
                        }
                    }
                }
            }

            List<List<OutputLocation>> filesForLocation = new List<List<OutputLocation>>(info.DataEntries.Count);
            foreach (var i in processed.Files)
            {
                ProcessedFile file = i.Value;
                filesForLocation[(int)file.Entry().Location].Add(new OutputLocation(file, 0));
                if (o.Test || o.Extract)
                {
                    ulong offset = info.DataEntries[(int)file.Entry().Location].UncompressedSize;
                    uint sortSlice = info.DataEntries[(int)file.Entry().Location].Chunk.FirstSlice;
                    uint sortOffset = info.DataEntries[(int)file.Entry().Location].Chunk.SortOffset;
                    foreach (uint location in file.Entry().AdditionalLocations)
                    {
                        DataEntry data = info.DataEntries[(int)location];
                        filesForLocation[(int)location].Add(new OutputLocation(file, offset));
                        offset += data.UncompressedSize;
                        if (data.Chunk.FirstSlice > sortSlice
                            || (data.Chunk.FirstSlice == sortSlice && data.Chunk.SortOffset > sortOffset))
                        {
                            sortSlice = data.Chunk.FirstSlice;
                            sortOffset = data.Chunk.SortOffset;
                        }
                        else if (data.Chunk.FirstSlice == sortSlice && data.Chunk.SortOffset == data.Chunk.Offset)
                        {
                            data.Chunk.SortOffset = ++sortOffset;
                        }
                        else
                        {
                            // Could not reorder chunk - no point in trying to reordder the remaining chunks
                            sortSlice = UInt32.MaxValue;
                        }
                    }
                }
            }

            ulong totalSize = 0;

            Chunks chunks = new Chunks();
            for (int i = 0; i < info.DataEntries.Count; i++)
            {
                if (filesForLocation[i].Count != 0)
                {
                    DataEntry location = info.DataEntries[i];
                    if (chunks[location.Chunk] == null)
                        chunks[location.Chunk] = new Files();

                    chunks[location.Chunk][location.File] = i;
                    totalSize += location.UncompressedSize;
                }
            }

            SliceReader sliceReader;
            if (o.Extract || o.Test)
            {
                if (offsets.DataOffset != 0)
                {
                    sliceReader = new SliceReader(ifs, offsets.DataOffset);
                }
                else
                {
                    string dir = Path.GetDirectoryName(installer);
                    string basename = Path.GetFileNameWithoutExtension(installer);
                    string basename2 = info.Header.BaseFilename;

                    // Prevent access to unexpected files
                    basename2 = basename.Replace('/', '_').Replace('\\', '_');

                    // Older Inno Setup versions used the basename stored in the headers, change our default accordingly
                    if (info.Version < InnoVersion.INNO_VERSION(4, 1, 7) && !string.IsNullOrEmpty(basename2))
                    {
                        string temp = basename; basename = basename2; basename2 = temp;
                    }

                    sliceReader = new SliceReader(dir, basename, basename2, info.Header.SlicesPerDisk);
                }
            }

            MultiPartOutputs multiOutputs = new MultiPartOutputs();

            foreach (var chunk in chunks)
            {
                Console.WriteLine($"[starting {chunk.Key.Compression} chunk @ slice {chunk.Key.FirstSlice} + {offsets.DataOffset:x} + {chunk.Key.Offset:x}]");

                ChunkReader chunkSource = new ChunkReader();
                if ((o.Extract || o.Test) && (chunk.Key.Encryption == EncryptionMethod.Plaintext || !string.IsNullOrEmpty(password)))
                    chunkSource = ChunkReader.Get(sliceReader, chunk.Key, password);

                ulong offset = 0;

                foreach (var location in chunk.Value)
                {
                    InnoFile file = location.Key;
                    List<OutputLocation> outputLocations = filesForLocation[location.Value];

                    if (file.Offset > offset)
                    {
                        Console.WriteLine($"discarding {file.Offset - offset:x} @ {offset:x}");
                        if (chunkSource.Get())
                            Utility.Discard(chunkSource, (uint)(file.Offset - offset));
                    }

                    // Print filename and size
                    if (o.List)
                    {
                        if (!o.Silent)
                        {
                            bool named = false;
                            ulong size = 0;
                            Checksum localChecksum = null;
                            foreach (OutputLocation output in outputLocations)
                            {
                                if (output.Second != 0)
                                    continue;

                                if (output.First.Entry().Size != 0)
                                {
                                    if (size != 0 && size != output.First.Entry().Size)
                                        Console.WriteLine("Mismatched output sizes");

                                    size = output.First.Entry().Size;
                                }

                                if (output.First.Entry().Checksum.Type != ChecksumType.None)
                                {
                                    if (localChecksum != null && localChecksum != output.First.Entry().Checksum)
                                        Console.WriteLine("Mismatched output checksums");

                                    localChecksum = output.First.Entry().Checksum;
                                }

                                if (named)
                                {
                                    Console.Write(", ");
                                }
                                else
                                {
                                    Console.Write(" - ");
                                    named = true;
                                }

                                // TODO: Used to be a color difference
                                if (chunk.Key.Encryption != EncryptionMethod.Plaintext)
                                {
                                    if (string.IsNullOrEmpty(password))
                                        Console.Write($"\"{output.First.Path()}\"");
                                    else
                                        Console.Write($"\"{output.First.Path()}\"");
                                }
                                else
                                {
                                    Console.Write($"\"{output.First.Path()}\"");
                                }

                                PrintFilterInfo(output.First.Entry());
                            }

                            if (named)
                            {
                                if (o.ListSizes)
                                    PrintSizeInfo(file, size);

                                if (o.ListChecksums)
                                {
                                    Console.Write(" ");
                                    PrintChecksumInfo(file, localChecksum);
                                }

                                if (chunk.Key.Encryption != EncryptionMethod.Plaintext && string.IsNullOrEmpty(password))
                                    Console.Write(" - encrypted");

                                Console.WriteLine();
                            }
                        }
                        else
                        {
                            foreach (OutputLocation output in outputLocations)
                            {
                                if (output.Second == 0)
                                {
                                    ProcessedFile fileinfo = output.First;
                                    if (o.ListSizes)
                                    {
                                        ulong size = fileinfo.Entry().Size;
                                        Console.Write($"{(size != 0 ? size : file.Size)} ");
                                    }

                                    if (o.ListChecksums)
                                    {
                                        PrintChecksumInfo(file, fileinfo.Entry().Checksum);
                                        Console.Write(" ");
                                    }

                                    Console.WriteLine(fileinfo.Path());
                                }
                            }
                        }

                        if (o.Extract || o.Test)
                            Console.OpenStandardOutput().Flush();
                    }

                    // Seek to the correct position within the chunk
                    if (chunkSource.Get() && file.Offset < offset)
                    {
                        string oss = string.Empty;
                        oss += $"Bad offset while extracting files: file start ({file.Offset}) is before end of previous file ({offset})";
                        throw new FormatError(oss);
                    }

                    offset = file.Offset + file.Size;

                    if (!chunkSource.Get())
                        continue; // Not extracting/testing this file

                    Checksum checksum;

                    // Open input file
                    FileReader fileSource = FileReader.Get(chunkSource, file, checksum);

                    // Open output files
                    List<FileOutput> singleOutputs = new List<FileOutput>();
                    List<FileOutput> outputs = new List<FileOutput>();
                    foreach (OutputLocation outputLoc in outputLocations)
                    {
                        ProcessedFile fileinfo = outputLoc.First;
                        try
                        {
                            if (!o.Extract && fileinfo.Entry().Checksum.Type == ChecksumType.None)
                                continue;

                            // Re-use existing file output for multi-part files
                            FileOutput output = null;
                            if (fileinfo.IsMultipart())
                            {
                                bool it = multiOutputs.ContainsKey(fileinfo);
                                if (it)
                                    output = multiOutputs[fileinfo];
                            }

                            if (output == null)
                            {
                                output = new FileOutput(o.OutputDir, fileinfo, o.Extract);
                                if (fileinfo.IsMultipart())
                                    multiOutputs.Add(fileinfo, output);
                                else
                                    singleOutputs.Add(output);
                            }

                            outputs.Add(output);
                        }
                        catch
                        {
                            // should never happen
                            Environment.Exit(0);
                        }
                    }

                    // Copy data
                    ulong outputSize = 0;
                    while (!fileSource.EOF())
                    {
                        byte[] buffer = new byte[8192 * 10];
                        int bufferSize = buffer.Length;
                        int n = fileSource.Read(buffer, bufferSize).GCount();
                        if (n > 0)
                        {
                            foreach (FileOutput output in outputs)
                            {
                                bool success = output.Write(buffer, n);
                                if (!success)
                                    throw new Exception($"Error writing file \"{output.OutputPath()}\"");
                            }

                            outputSize += (ulong)n;
                        }
                    }

                    DataEntry data = info.DataEntries[location.Value];

                    if (outputSize != data.UncompressedSize)
                        Console.WriteLine($"Unexpected output file size: {outputSize} != {data.UncompressedSize}");

                    DateTime filetime = new DateTime((long)data.Timestamp);
                    if (o.Extract && o.PreserveFileTimes && o.LocalTimestamps && (data.Options & DataEntry.Flags.TimeStampInUTC) == 0)
                        filetime = filetime.ToUniversalTime();

                    foreach (FileOutput output in outputs)
                    {
                        if (output.File().IsMultipart() && !output.IsComplete())
                            continue;

                        // Verify output checksum if available
                        if (output.File().Entry().Checksum.Type != ChecksumType.None && output.CalculateChecksum())
                        {
                            Checksum outputChecksum = output.Checksum();
                            if (outputChecksum != output.File().Entry().Checksum)
                            {
                                Console.WriteLine($"Output checksum mismatch for {output.File().Path()}:");
                                Console.WriteLine($" ├─ actual:   {outputChecksum}");
                                Console.Write($" └─ expected: {output.File().Entry().Checksum}");

                                if (o.Test)
                                    throw new Exception("Integrity test failed!");
                            }
                        }

                        // Adjust file timestamps
                        if (o.Extract && o.PreserveFileTimes)
                        {
                            output.Close();
                            if (!Utility.SetFileTime(output.OutputPath(), filetime, data.TimestampNsec))
                                Console.Write($"Error setting timestamp on file {output.OutputPath()}");
                        }

                        if (output.File().IsMultipart())
                        {
                            Console.Write("[finalizing multi-part file]");
                            multiOutputs.Remove(output.File());
                        }
                    }

                    // Verify checksums
                    if (checksum != file.Checksum)
                    {
                        Console.WriteLine("Checksum mismatch:");
                        Console.WriteLine($" ├─ actual:   {checksum}");
                        Console.Write($" └─ expected: {file.Checksum}");

                        if (o.Test)
                            throw new Exception("Integrity test failed!");
                    }
                }

                if (offset < chunk.Key.Size)
                    Console.WriteLine($"discarding {chunk.Key.Size - offset} bytes at the end of chunk @ {offset:x}");
            }

            if (multiOutputs.Count != 0)
                Console.WriteLine("Incomplete multi-part files");

            if (o.WarnUnused || o.Gog)
                Gog.ProbeBinFiles(o, info, installer, offsets.DataOffset == 0);
        }
    }
}
