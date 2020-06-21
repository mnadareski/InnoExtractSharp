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
using System.Text;
using InnoExtractSharp.Streams;
using InnoExtractSharp.Util;

namespace InnoExtractSharp.Setup
{
    public class Info
    {
        [Flags]
        public enum EntryTypes
        {
            Components = 1 << 0,
            DataEntries = 1 << 1,
            DeleteEntries = 1 << 2,
            UninstallDeleteEntries = 1 << 3,
            Directories = 1 << 4,
            Files = 1 << 5,
            Icons = 1 << 6,
            IniEntries = 1 << 7,
            Languages = 1 << 8,
            Messages = 1 << 9,
            Permissions = 1 << 10,
            RegistryEntries = 1 << 11,
            RunEntries = 1 << 12,
            UninstallRunEntries = 1 << 13,
            Tasks  = 1 << 14,
            Types  = 1 << 15,
            WizardImages  = 1 << 16,
            DecompressorDll  = 1 << 17,
            DecryptDll  = 1 << 18,
            NoSkip  = 1 << 19,
            NoUnknownVersion  = 1 << 20,
        }

        public InnoVersion Version;

        public uint Codepage;

        public Header Header;

        public List<ComponentEntry> Components;
        public List<DataEntry> DataEntries;
        public List<DeleteEntry> DeleteEntries;
        public List<DeleteEntry> UninstallDeleteEntries;
        public List<DirectoryEntry> Directories;
        public List<FileEntry> Files;
        public List<IconEntry> Icons;
        public List<IniEntry> IniEntries;
        public List<LanguageEntry> Languages;
        public List<MessageEntry> Messages;
        public List<PermissionEntry> Permissions;
        public List<RegistryEntry> RegistryEntries;
        public List<RunEntry> RunEntries;
        public List<RunEntry> UninstallRunEntries;
        public List<TaskEntry> Tasks;
        public List<TypeEntry> Types;

        // Images displayed in the installer UI
        // Loading enabled by \c WizardImages
        public List<string> WizardImages;
        public List<string> WizardImagesSmall;

        // Contents of the helper DLL used to decompress setup data in some versions.
        // Loading enabled by \c DecompressorDll
        public string DecompressorDll;

        // Contents of the helper DLL used to decrypt setup data.
        // Loading enabled by \c DecryptDll
        public string DecryptDll;

        public Info() { }
        ~Info() { }

        /// <summary>
        /// Load setup headers.
        /// </summary>
        /// <param name="input">he input stream to load the setup headers from.
        ///     It must already be positioned at start of \ref setup::version
        ///     identifier whose position is given by
        ///     \ref loader::offsets::header_offset.</param>
        /// <param name="entries">What kinds of entries to load.</param>
        /// <param name="forceCodepage">Windows codepage to use for strings in ANSI installers.</param>
        public void Load(Stream input, EntryTypes entries, int forceCodepage = 0)
        {
            Version.Load(input);

            if (!Version.Known)
            {
                if ((entries & EntryTypes.NoUnknownVersion) != 0)
                {
                    throw new Exception(); // TOOD: Don't do this
                }
                // log_warning << "Unexpected setup data version: " << color::white << version << color::reset;
            }

            uint listedVersion = Version.Value;

            // Some setup versions didn't increment the data version number when they should have.
            // To work around this, we try to parse the headers for both data versions.
            bool ambiguous = Version.IsAmbiguous();
            if (ambiguous)
            {
                // Force parsing all headers so that we don't miss any errors.
                entries |= EntryTypes.NoSkip;
            }
            if (!Version.Known || ambiguous)
            {
                long start = input.Position;
                try
                {
                    Load(input, entries, Version);
                    return;
                }
                catch
                {
                    Version.Value = Version.Next();
                    if (Version == 0)
                    {
                        Version.Value = listedVersion;
                        throw;
                    }

                    input.Seek(start, SeekOrigin.Begin);
                }
            }

            try
            {
                Load(input, entries, Version);
            }
            catch
            {
                Version.Value = listedVersion;
                throw;
            }
        }

        /// <summary>
        /// Load setup headers.
        /// </summary>
        /// <param name="input">he input stream to load the setup headers from.
        ///     It must already be positioned at start of \ref setup::version
        ///     identifier whose position is given by
        ///     \ref loader::offsets::header_offset.</param>
        /// <param name="entries">What kinds of entries to load.</param>
        /// <param name="version">The setup data version of the headers.</param>
        public void Load(Stream input, EntryTypes entries, InnoVersion version)
        {
            if ((entries & (EntryTypes.Messages | EntryTypes.NoSkip)) != 0)
                entries |= EntryTypes.Languages;

            Stream reader = BlockReader.Get(input, version);

            Header.Load(reader, version);

            LoadEntries(reader, version, entries, Header.LanguageCount, Languages, EntryTypes.Languages);

            if (version < InnoVersion.INNO_VERSION(4, 0, 0))
                LoadWizardAndDecompressor(reader, version, Header, this, entries);

            LoadEntries(reader, version, entries, Header.MessageCount, Messages, EntryTypes.Messages, Languages);
            LoadEntries(reader, version, entries, Header.PermissionCount, Permissions, EntryTypes.Permissions);
            LoadEntries(reader, version, entries, Header.TypeCount, Types, EntryTypes.Types);
            LoadEntries(reader, version, entries, Header.ComponentCount, Components, EntryTypes.Components);
            LoadEntries(reader, version, entries, Header.TaskCount, Tasks, EntryTypes.Tasks);
            LoadEntries(reader, version, entries, Header.DirectoryCount, Directories, EntryTypes.Directories);
            LoadEntries(reader, version, entries, Header.FileCount, Files, EntryTypes.Files);
            LoadEntries(reader, version, entries, Header.IconCount, Icons, EntryTypes.Icons);
            LoadEntries(reader, version, entries, Header.IniEntryCount, IniEntries, EntryTypes.IniEntries);
            LoadEntries(reader, version, entries, Header.RegistryEntryCount, RegistryEntries, EntryTypes.RegistryEntries);
            LoadEntries(reader, version, entries, Header.DeleteEntryCount, DeleteEntries, EntryTypes.DeleteEntries);
            LoadEntries(reader, version, entries, Header.UninstallDeleteEntryCount, UninstallDeleteEntries, EntryTypes.UninstallDeleteEntries);
            LoadEntries(reader, version, entries, Header.RunEntryCount, RunEntries, EntryTypes.RunEntries);
            LoadEntries(reader, version, entries, Header.UninstallRunEntryCount, UninstallRunEntries, EntryTypes.UninstallRunEntries);

            if (version >= InnoVersion.INNO_VERSION(4, 0, 0))
                LoadWizardAndDecompressor(input, version, Header, this, entries);

            // restart the compression stream
            CheckIsEnd(input, "unknown data at end of primary header stream");
            reader = BlockReader.Get(input, version);

            LoadEntries(reader, version, entries, Header.DataEntryCount, DataEntries, EntryTypes.DataEntries);

            CheckIsEnd(input, "unknown data at end of secondary header stream");
        }

        private void LoadEntry(Stream input, InnoVersion version, Entry entity, List<Entry> args)
        {
            entity.Load(input, version, args);
        }

        /// <summary>
        /// Load setup headers for a specific version.
        /// </summary>
        /// <param name="input">The input stream to load the setup headers from.
        ///     It must already be positioned at start of the compressed headers.
        ///     The compressed headers start directly after the \ref setup::version
        ///     identifier whose position is given by
        ///     \ref loader::offsets::header_offset.</param>
        /// <param name="entries">What kinds of entries to load.</param>
        /// <param name="forceCodepage">Windows codepage to use for strings in ANSI installers.</param>
        private void TryLoad(Stream input, EntryTypes entries, uint forceCodepage)
        {
            if ((entries & (EntryTypes.Messages | EntryTypes.NoSkip)) != 0 || (!Version.IsUnicode() && forceCodepage == 0))
                entries |= EntryTypes.Languages;

            BlockReader reader = BlockReader.Get(input, Version);

            Header.Load(reader, Version);
            LoadEntries(reader, entries, Header.LanguageCount, Languages.Select(l => l as Entry).ToList(), EntryTypes.Languages);
            if (Version.IsUnicode())
            {
                // Unicode installers are always UTF16-LE, do not allow users to override that.
                Codepage = Utility.CP_UTF16LE;
            }
            else if (forceCodepage != 0)
            {
                Codepage = forceCodepage;
            }
            else if (Languages.Count == 0)
            {
                Codepage = Utility.CP_Windows1252;
            }
            else
            {
                // Non-Unicode installers do not have a defined codepage but instead just assume the
                // codepage of the system the installer is run on.
                // Look at the list of available languages to guess a suitable codepage.
                Codepage = Languages[0].Codepage;
                foreach (LanguageEntry language in Languages)
                {
                    if (language.Codepage == Utility.CP_Windows1252)
                    {
                        Codepage = Utility.CP_Windows1252;
                        break;
                    }
                }
            }

            Header.Decode(Codepage);
            foreach (LanguageEntry language in Languages)
            {
                language.Decode(Codepage);
            }

            if (Version < InnoVersion.INNO_VERSION(4, 0, 0))
                LoadWizardAndDecompressor(reader, Version, Header, this, entries);

            LoadEntries(reader, entries, Header.MessageCount, Messages.Select(e => e as Entry).ToList(), EntryTypes.Messages);
            LoadEntries(reader, entries, Header.PermissionCount, Permissions.Select(e => e as Entry).ToList(), EntryTypes.Permissions);
            LoadEntries(reader, entries, Header.TypeCount, Types.Select(e => e as Entry).ToList(), EntryTypes.Types);
            LoadEntries(reader, entries, Header.ComponentCount, Components.Select(e => e as Entry).ToList(), EntryTypes.Components);
            LoadEntries(reader, entries, Header.TaskCount, Tasks.Select(e => e as Entry).ToList(), EntryTypes.Tasks);
            LoadEntries(reader, entries, Header.DirectoryCount, Directories.Select(e => e as Entry).ToList(), EntryTypes.Directories);
            LoadEntries(reader, entries, Header.FileCount, Files.Select(e => e as Entry).ToList(), EntryTypes.Files);
            LoadEntries(reader, entries, Header.IconCount, Icons.Select(e => e as Entry).ToList(), EntryTypes.Icons);
            LoadEntries(reader, entries, Header.IniEntryCount, IniEntries.Select(e => e as Entry).ToList(), EntryTypes.IniEntries);
            LoadEntries(reader, entries, Header.RegistryEntryCount, RegistryEntries.Select(e => e as Entry).ToList(), EntryTypes.RegistryEntries);
            LoadEntries(reader, entries, Header.DeleteEntryCount, DeleteEntries.Select(e => e as Entry).ToList(), EntryTypes.DeleteEntries);
            LoadEntries(reader, entries, Header.UninstallDeleteEntryCount, UninstallDeleteEntries.Select(e => e as Entry).ToList(), EntryTypes.UninstallDeleteEntries);
            LoadEntries(reader, entries, Header.RunEntryCount, RunEntries.Select(e => e as Entry).ToList(), EntryTypes.RunEntries);
            LoadEntries(reader, entries, Header.UninstallRunEntryCount, UninstallRunEntries.Select(e => e as Entry).ToList(), EntryTypes.UninstallRunEntries);

            if (Version >= InnoVersion.INNO_VERSION(4, 0, 0))
                LoadWizardAndDecompressor(reader, Version, Header, this, entries);

            // restart the compression stream
            CheckIsEnd(input, "unknown data at end of primary header stream");
            reader = BlockReader.Get(input, Version);

            LoadEntries(reader)
        }

        private void LoadEntries(Stream input, EntryTypes entries, int count, List<Entry> result, EntryTypes entryType)
        {
            result.Clear();
            if ((entries & entryType) != 0)
            {
                result = new List<Entry>(count);
                for (int i = 0; i < count; i++)
                {
                    result[i].Load(input, this);
                }
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    Entry entry = new Entry();
                    entry.Load(input, this);
                }
            }
        }

        private void LoadWizardImages(Stream input, InnoVersion version, List<string> images, EntryTypes entries)
        {
            using (BinaryReader br = new BinaryReader(input, Encoding.Default, true))
            {
                int count = 1;
                if (version >= InnoVersion.INNO_VERSION(5, 6, 0))
                    count = (int)br.ReadUInt32();

                if ((entries & (EntryTypes.WizardImages | EntryTypes.NoSkip)) != 0)
                {
                    images = new List<string>(count);
                    for (int i = 0; i < count; i++)
                    {
                        BinaryString.Load(input, out string image);
                        images[i] = image;
                    }

                    if (version < InnoVersion.INNO_VERSION(5, 6, 0) && string.IsNullOrEmpty(images[0]))
                        images = new List<string>();
                }
                else
                {
                    for (int i = 0; i < count; i++)
                    {
                        BinaryString.Skip(input);
                    }
                }
            }
        }

        private void LoadWizardAndDecompressor(Stream input, InnoVersion version, Header header, Info info, EntryTypes entries)
        {
            info.WizardImages = new List<string>();
            info.WizardImagesSmall = new List<string>();

            LoadWizardImages(input, version, info.WizardImages, entries);

            if (version >= InnoVersion.INNO_VERSION(2, 0, 0) || version.IsIsx())
                LoadWizardImages(input, version, info.WizardImagesSmall, entries);

            info.DecompressorDll = string.Empty;
            if (header.Compression == CompressionMethod.BZip2
               || (header.Compression == CompressionMethod.LZMA1 && version == InnoVersion.INNO_VERSION(4, 1, 5))
               || (header.Compression == CompressionMethod.Zlib && version >= InnoVersion.INNO_VERSION(4, 2, 6)))
            {
                if ((entries & (EntryTypes.DecompressorDll | EntryTypes.NoSkip)) != 0)
                {
                    BinaryString.Load(input, out string dll);
                    info.DecompressorDll = dll;
                }
                else
                {
                    // decompressor dll - we don't need this
                    BinaryString.Skip(input);
                }
            }

            info.DecryptDll = string.Empty;
            if ((header.Options & Header.Flags.EncryptionUsed) != 0)
            {
                if ((entries & (EntryTypes.DecryptDll | EntryTypes.NoSkip)) != 0)
                {
                    BinaryString.Load(input, out string dll);
                    info.DecryptDll = dll;
                }
                else
                {
                    // decrypt dll - we don't need this
                    BinaryString.Skip(input);
                }
            }
        }

        private void CheckIsEnd(Stream input, string message)
        {
            try
            {
                input.Seek(1, SeekOrigin.Current);
                input.Seek(-1, SeekOrigin.Current);
            }
            catch
            {
                throw new Exception(message); // TODO: We shouldn't do this
            }
        }

        private static Entry FromEntryType(EntryTypes entryType)
        {
            switch (entryType)
            {
                case EntryTypes.Components:
                    return new ComponentEntry();
                case EntryTypes.DataEntries:
                    return new DataEntry();
                case EntryTypes.DeleteEntries:
                    return new DeleteEntry();
                case EntryTypes.UninstallDeleteEntries:
                    return new DeleteEntry();
                case EntryTypes.Directories:
                    return new DirectoryEntry();
                case EntryTypes.Files:
                    return new FileEntry();
                case EntryTypes.Icons:
                    return new IconEntry();
                case EntryTypes.IniEntries:
                    return new IniEntry();
                case EntryTypes.Languages:
                    return new LanguageEntry();
                case EntryTypes.Messages:
                    return new MessageEntry();
                case EntryTypes.Permissions:
                    return new PermissionEntry();
                case EntryTypes.RegistryEntries:
                    return new RegistryEntry();
                case EntryTypes.RunEntries:
                    return new RunEntry();
                case EntryTypes.UninstallRunEntries:
                    return new RunEntry();
                case EntryTypes.Tasks:
                    return new TaskEntry();
                case EntryTypes.Types:
                    return new TypeEntry();
                default:
                    return null;
            }
        }
    }
}
