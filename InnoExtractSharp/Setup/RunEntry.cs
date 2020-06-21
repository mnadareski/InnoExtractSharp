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
    public class RunEntry : Item
    {
        [Flags]
        public enum Flags
        {
            ShellExec = 1 << 0,
            SkipIfDoesntExist = 1 << 1,
            PostInstall = 1 << 2,
            Unchecked = 1 << 3,
            SkipIfSilent = 1 << 4,
            Skipif_not_equalSilent = 1 << 5,
            HideWizard = 1 << 6,
            Bits32 = 1 << 7,
            Bits64 = 1 << 8,
            RunAsOriginalUser = 1 << 9,
        }

        public enum WaitCondition
        {
            WaitUntilTerminated,
            NoWait,
            WaitUntilIdle,
        }

        public string Name;
        public string Parameters;
        public string WorkingDir;
        public string RunOnceId;
        public string StatusMessage;
        public string Verb;
        public string Description;

        public int ShowCommand;

        public WaitCondition Wait;

        public Flags Options;

        public override void Load(Stream input, InnoVersion version, List<Entry> entries = null)
        {
            using (BinaryReader br = new BinaryReader(input, Encoding.Default, true))
            {
                if (version < InnoVersion.INNO_VERSION(1, 3, 21))
                    br.ReadUInt32(); // uncompressed size of the entry

                EncodedString.Load(input, out Name, (int)version.Codepage());
                EncodedString.Load(input, out Parameters, (int)version.Codepage());
                EncodedString.Load(input, out WorkingDir, (int)version.Codepage());
                if (version >= InnoVersion.INNO_VERSION(1, 3, 21))
                    EncodedString.Load(input, out RunOnceId, (int)version.Codepage());
                else
                    RunOnceId = string.Empty;
                if (version >= InnoVersion.INNO_VERSION(2, 0, 2))
                    EncodedString.Load(input, out StatusMessage, (int)version.Codepage());
                else
                    StatusMessage = string.Empty;
                if (version >= InnoVersion.INNO_VERSION(5, 1, 13))
                    EncodedString.Load(input, out Verb, (int)version.Codepage());
                else
                    Verb = string.Empty;
                if (version >= InnoVersion.INNO_VERSION(2, 0, 0))
                    EncodedString.Load(input, out Description, (int)version.Codepage());

                LoadConditionData(input, version);

                LoadVersionData(input, version);

                if (version >= InnoVersion.INNO_VERSION(1, 3, 21))
                    ShowCommand = br.ReadInt32();
                else
                    ShowCommand = 0;

                Wait = Stored.RunWaitConditions.TryGetValue(br.ReadByte(), WaitCondition.WaitUntilTerminated);

                Flags flagreader = 0;

                flagreader |= Flags.ShellExec;
                if (version >= InnoVersion.INNO_VERSION(1, 3, 21))
                {
                    flagreader |= Flags.SkipIfDoesntExist;
                }
                if (version >= InnoVersion.INNO_VERSION(2, 0, 0))
                {
                    flagreader |= Flags.PostInstall;
                    flagreader |= Flags.Unchecked;
                    flagreader |= Flags.SkipIfSilent;
                    flagreader |= Flags.Skipif_not_equalSilent;
                }
                if (version >= InnoVersion.INNO_VERSION(2, 0, 8))
                {
                    flagreader |= Flags.HideWizard;
                }
                if (version >= InnoVersion.INNO_VERSION(5, 1, 10))
                {
                    flagreader |= Flags.Bits32;
                    flagreader |= Flags.Bits64;
                }
                if (version >= InnoVersion.INNO_VERSION(5, 2, 0))
                {
                    flagreader |= Flags.RunAsOriginalUser;
                }

                Options = flagreader;
            }
        }
    }
}
