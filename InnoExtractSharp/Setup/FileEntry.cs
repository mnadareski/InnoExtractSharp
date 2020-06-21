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
using System.Text;
using InnoExtractSharp.Crypto;
using InnoExtractSharp.Util;

namespace InnoExtractSharp.Setup
{
    public class FileEntry : Item
    {
        [Flags]
        public enum Flags : long
        {
            ConfirmOverwrite = 1L << 0,
            NeverUninstall = 1L << 1,
            RestartReplace = 1L << 2,
            DeleteAfterInstall = 1L << 3,
            RegisterServer = 1L << 4,
            RegisterTypeLib = 1L << 5,
            SharedFile = 1L << 6,
            CompareTimeStamp = 1L << 7,
            FontIsNotTrueType = 1L << 8,
            SkipIfSourceDoesntExist = 1L << 9,
            OverwriteReadOnly = 1L << 10,
            OverwriteSameVersion = 1L << 11,
            CustomDestName = 1L << 0,
            OnlyIfDestFileExists = 1L << 12,
            NoRegError = 1L << 13,
            UninsRestartDelete = 1L << 14,
            OnlyIfDoesntExist = 1L << 15,
            IgnoreVersion = 1L << 16,
            PromptIfOlder = 1L << 17,
            DontCopy = 1L << 18,
            UninsRemoveReadOnly = 1L << 19,
            RecurseSubDirsExternal = 1L << 20,
            ReplaceSameVersionIfContentsDiffer = 1L << 21,
            DontVerifyChecksum = 1L << 22,
            UninsNoSharedFilePrompt = 1L << 23,
            CreateAllSubDirs = 1L << 24,
            Bits32 = 1L << 25,
            Bits64 = 1L << 26,
            ExternalSizePreset = 1L << 27,
            SetNtfsCompression = 1L << 28,
            UnsetNtfsCompression = 1L << 29,
            GacInstall = 1L << 30,

            // obsolete options:
            IsReadmeFile = 1L << 31,
        }

        public enum FileType
        {
            UserFile,
            UninstExe,
            RegSvrExe,
        }

        public enum FileAttributes
        {
            ReadOnly = 0x1,
        }

        public enum FileCopyMode
        {
            cmNormal,
            cmIfDoesntExist,
            cmAlwaysOverwrite,
            cmAlwaysSkipIfSameOrOlder,
        }

        public string Source;
        public string Destination;
        public string InstallFontName;
        public string StrongAssemblyName;

        public uint Location; // index into the data entry list
        public uint Attributes;
        public ulong ExternalSize;

        public short Permission; // index into the permission entry list

        public Flags Options;

        public FileType Type;

        // Information about GOG Galaxy multi-part files
        // These are not used in normal Inno Setup installers
        List<uint> AdditionalLocations;
        public Checksum Checksum;
        public ulong Size;

        public override void Load(Stream input, InnoVersion version, List<Entry> entries = null)
        {
            using (BinaryReader br = new BinaryReader(input, Encoding.Default, true))
            {
                Options = 0;

                if (version < InnoVersion.INNO_VERSION(1, 3, 21))
                    br.ReadUInt32(); // uncompressed size of the entry

                EncodedString.Load(input, out Source, (int)version.Codepage());

                EncodedString.Load(input, out Destination, (int)version.Codepage());

                EncodedString.Load(input, out InstallFontName, (int)version.Codepage());

                if (version >= InnoVersion.INNO_VERSION(5, 2, 5))
                    EncodedString.Load(input, out StrongAssemblyName, (int)version.Codepage());
                else
                    StrongAssemblyName = string.Empty;

                LoadConditionData(input, version);

                LoadVersionData(input, version);

                Location = (version.Bits == 16 ? br.ReadUInt16() : br.ReadUInt32());
                Attributes = (version.Bits == 16 ? br.ReadUInt16() : br.ReadUInt32());
                ExternalSize = (version >= InnoVersion.INNO_VERSION(4, 0, 0) ? br.ReadUInt64() : br.ReadUInt32());

                if (version < InnoVersion.INNO_VERSION(3, 0, 5))
                {
                    FileCopyMode copyMode = Stored.FileCopyModes.TryGetValue(br.ReadByte(), FileCopyMode.cmNormal);
                    switch (copyMode)
                    {
                        case FileCopyMode.cmNormal: Options |= Flags.PromptIfOlder; break;
                        case FileCopyMode.cmIfDoesntExist: Options |= Flags.OnlyIfDoesntExist | Flags.PromptIfOlder; break;
                        case FileCopyMode.cmAlwaysOverwrite: Options |= Flags.IgnoreVersion | Flags.PromptIfOlder; break;
                        case FileCopyMode.cmAlwaysSkipIfSameOrOlder: break;
                    }
                }

                if (version >= InnoVersion.INNO_VERSION(4, 1, 0))
                    Permission = br.ReadInt16();
                else
                    Permission = -1;

                Flags flagreader = 0;
                flagreader |= Flags.ConfirmOverwrite;
                flagreader |= Flags.NeverUninstall;
                flagreader |= Flags.RestartReplace;
                flagreader |= Flags.DeleteAfterInstall;
                if (version.Bits != 16)
                {
                    flagreader |= Flags.RegisterServer;
                    flagreader |= Flags.RegisterTypeLib;
                    flagreader |= Flags.SharedFile;
                }
                if (version < InnoVersion.INNO_VERSION(2, 0, 0))
                    flagreader |= Flags.IsReadmeFile;
                flagreader |= Flags.CompareTimeStamp;
                flagreader |= Flags.FontIsNotTrueType;
                flagreader |= Flags.SkipIfSourceDoesntExist;
                flagreader |= Flags.OverwriteReadOnly;
                if (version >= InnoVersion.INNO_VERSION(1, 3, 21))
                {
                    flagreader |= Flags.OverwriteSameVersion;
                    flagreader |= Flags.CustomDestName;
                }
                if (version >= InnoVersion.INNO_VERSION(1, 3, 25))
                    flagreader |= Flags.OnlyIfDestFileExists;
                if (version >= InnoVersion.INNO_VERSION(2, 0, 5))
                    flagreader |= Flags.NoRegError;
                if (version >= InnoVersion.INNO_VERSION(3, 0, 1))
                    flagreader |= Flags.UninsRestartDelete;
                if (version >= InnoVersion.INNO_VERSION(3, 0, 5))
                {
                    flagreader |= Flags.OnlyIfDoesntExist;
                    flagreader |= Flags.IgnoreVersion;
                    flagreader |= Flags.PromptIfOlder;
                }
                if (version >= InnoVersion.INNO_VERSION_EXT(3, 0, 6, 1))
                    flagreader |= Flags.DontCopy;
                if (version >= InnoVersion.INNO_VERSION(4, 0, 5))
                    flagreader |= Flags.UninsRemoveReadOnly;
                if (version >= InnoVersion.INNO_VERSION(4, 1, 8))
                    flagreader |= Flags.RecurseSubDirsExternal;
                if (version >= InnoVersion.INNO_VERSION(4, 2, 1))
                    flagreader |= Flags.ReplaceSameVersionIfContentsDiffer;
                if (version >= InnoVersion.INNO_VERSION(4, 2, 5))
                    flagreader |= Flags.DontVerifyChecksum;
                if (version >= InnoVersion.INNO_VERSION(5, 0, 3))
                    flagreader |= Flags.UninsNoSharedFilePrompt;
                if (version >= InnoVersion.INNO_VERSION(5, 1, 0))
                    flagreader |= Flags.CreateAllSubDirs;
                if (version >= InnoVersion.INNO_VERSION(5, 1, 2))
                {
                    flagreader |= Flags.Bits32;
                    flagreader |= Flags.Bits64;
                }
                if (version >= InnoVersion.INNO_VERSION(5, 2, 0))
                {
                    flagreader |= Flags.ExternalSizePreset;
                    flagreader |= Flags.SetNtfsCompression;
                    flagreader |= Flags.UnsetNtfsCompression;
                }
                if (version >= InnoVersion.INNO_VERSION(5, 2, 5))
                    flagreader |= Flags.GacInstall;

                Options |= flagreader;

                if (version.Bits == 16 || version >= InnoVersion.INNO_VERSION(5, 0, 0))
                    Type = Stored.FileTypes0.TryGetValue(br.ReadByte(), FileType.UserFile);
                else
                    Type = Stored.FileTypes1.TryGetValue(br.ReadByte(), FileType.UserFile);

                AdditionalLocations = new List<uint>();
                Checksum.Type = ChecksumType.None;
                Size = 0;
            }
        }
    }
}
