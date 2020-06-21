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
using InnoExtractSharp.Util;

namespace InnoExtractSharp.Setup
{
    public class IconEntry : Item
    {
        [Flags]
        public enum Flags
        {
            NeverUninstall = 1 << 0,
            CreateOnlyIfFileExists = 1 << 1,
            UseAppPaths = 1 << 2,
            FolderShortcut = 1 << 3,
            ExcludeFromShowInNewInstall = 1 << 4,
            PreventPinning = 1 << 5,

            // obsolete options:
            RunMinimized = 1 << 6,
        }

        public enum CloseSetting
        {
            NoSetting,
            CloseOnExit,
            DontCloseOnExit,
        }

        public string Name;
        public string Filename;
        public string Parameters;
        public string WorkingDir;
        public string IconFile;
        public string Comment;
        public string AppUserModelId;

        public int IconIndex;

        public int ShowCommand;

        public CloseSetting CloseOnExit;

        public ushort Hotkey;

        public Flags Options;

        public override void Load(Stream input, InnoVersion version, List<Entry> entries = null)
        {
            using (BinaryReader br = new BinaryReader(input, Encoding.Default, true))
            {
                if (version < InnoVersion.INNO_VERSION(1, 3, 21))
                    br.ReadUInt32(); // uncompressed size of the entry

                EncodedString.Load(input, out Name, (int)version.Codepage());

                EncodedString.Load(input, out Filename, (int)version.Codepage());

                EncodedString.Load(input, out Parameters, (int)version.Codepage());

                EncodedString.Load(input, out WorkingDir, (int)version.Codepage());

                EncodedString.Load(input, out IconFile, (int)version.Codepage());

                EncodedString.Load(input, out Comment, (int)version.Codepage());

                LoadConditionData(input, version);

                if (version >= InnoVersion.INNO_VERSION(5, 3, 5))
                    EncodedString.Load(input, out AppUserModelId, (int)version.Codepage());
                else
                    AppUserModelId = string.Empty;

                LoadVersionData(input, version);

                IconIndex = (int)(version.Bits == 16 ? br.ReadUInt16() : br.ReadUInt32());

                if (version >= InnoVersion.INNO_VERSION(1, 3, 21))
                {
                    ShowCommand = br.ReadInt32();
                    CloseOnExit = Stored.CloseSettings.TryGetValue(br.ReadByte(), CloseSetting.NoSetting);
                }
                else
                {
                    ShowCommand = 1;
                    CloseOnExit = CloseSetting.NoSetting;
                }

                if (version >= InnoVersion.INNO_VERSION(2, 0, 7))
                    Hotkey = br.ReadUInt16();
                else
                    Hotkey = 0;

                Flags flagreader = 0;

                flagreader |= Flags.NeverUninstall;
                if (version >= InnoVersion.INNO_VERSION(1, 3, 21))
                    flagreader |= Flags.RunMinimized;
                flagreader |= Flags.CreateOnlyIfFileExists;
                if (version.Bits != 16)
                    flagreader |= Flags.UseAppPaths;
                if (version >= InnoVersion.INNO_VERSION(5, 0, 3))
                    flagreader |= Flags.FolderShortcut;
                if (version >= InnoVersion.INNO_VERSION(5, 4, 2))
                    flagreader |= Flags.ExcludeFromShowInNewInstall;
                if (version >= InnoVersion.INNO_VERSION(5, 5, 0))
                    flagreader |= Flags.PreventPinning;

                Options = flagreader;
            }
        }
    }
}
