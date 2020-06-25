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

using InnoExtractSharp.Util;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace InnoExtractSharp.Setup
{

    public class InnoVersion
    {
        [Flags]
        public enum Flags
        {
            Bits16,
            Unicode,
            ISX,
        }

        public uint Value;

        public Flags Variant;

        public bool Known;

        public static KnownLegacyVersion[] LegacyVersions = new KnownLegacyVersion[]
        {
            new KnownLegacyVersion("i1.2.10--16\x1a", INNO_VERSION(1, 2, 10), Flags.Bits16),
            new KnownLegacyVersion("i1.2.10--32\x1a", INNO_VERSION(1, 2, 10), (Flags)0),
        };

        public static KnownVersion[] Versions = new KnownVersion[]
        {
            new KnownVersion("Inno Setup Setup Data (1.3.3)",                      INNO_VERSION_EXT(1, 3,  3, 0), (Flags)0),
            new KnownVersion("Inno Setup Setup Data (1.3.9)",                      INNO_VERSION_EXT(1, 3,  9, 0), (Flags)0),
            new KnownVersion("Inno Setup Setup Data (1.3.10)",                     INNO_VERSION_EXT(1, 3, 10, 0), (Flags)0),
            new KnownVersion("Inno Setup Setup Data (1.3.10) with ISX (1.3.10)",   INNO_VERSION_EXT(1, 3, 10, 0), Flags.ISX),
            new KnownVersion("Inno Setup Setup Data (1.3.12) with ISX (1.3.12.1)", INNO_VERSION_EXT(1, 3, 12, 1), Flags.ISX),
            new KnownVersion("Inno Setup Setup Data (1.3.21)",     /* ambiguous */ INNO_VERSION_EXT(1, 3, 21, 0), (Flags)0),
            new KnownVersion("Inno Setup Setup Data (1.3.21) with ISX (1.3.17)",   INNO_VERSION_EXT(1, 3, 21, 0), Flags.ISX),
            new KnownVersion("Inno Setup Setup Data (1.3.24)",                     INNO_VERSION_EXT(1, 3, 24, 0), (Flags)0),
            new KnownVersion("Inno Setup Setup Data (1.3.21) with ISX (1.3.24)",   INNO_VERSION_EXT(1, 3, 24, 0), Flags.ISX),
            new KnownVersion("Inno Setup Setup Data (1.3.25)",                     INNO_VERSION_EXT(1, 3, 25, 0), (Flags)0),
            new KnownVersion("Inno Setup Setup Data (1.3.25) with ISX (1.3.25)",   INNO_VERSION_EXT(1, 3, 25, 0), Flags.ISX),
            new KnownVersion("Inno Setup Setup Data (2.0.0)",                      INNO_VERSION_EXT(2, 0,  0, 0), (Flags)0),
            new KnownVersion("Inno Setup Setup Data (2.0.1)",      /* ambiguous */ INNO_VERSION_EXT(2, 0,  1, 0), (Flags)0),
            new KnownVersion("Inno Setup Setup Data (2.0.2)",                      INNO_VERSION_EXT(2, 0,  2, 0), (Flags)0),
            new KnownVersion("Inno Setup Setup Data (2.0.5)",                      INNO_VERSION_EXT(2, 0,  5, 0), (Flags)0),
            new KnownVersion("Inno Setup Setup Data (2.0.6a)",                     INNO_VERSION_EXT(2, 0,  6, 0), (Flags)0),
            new KnownVersion("Inno Setup Setup Data (2.0.6a) with ISX (2.0.3)",    INNO_VERSION_EXT(2, 0,  6, 0), Flags.ISX),
            new KnownVersion("Inno Setup Setup Data (2.0.7)",                      INNO_VERSION_EXT(2, 0,  7, 0), (Flags)0),
            new KnownVersion("Inno Setup Setup Data (2.0.8)",                      INNO_VERSION_EXT(2, 0,  8, 0), (Flags)0),
            new KnownVersion("Inno Setup Setup Data (2.0.8) with ISX (2.0.3)",     INNO_VERSION_EXT(2, 0,  8, 0), Flags.ISX),
            new KnownVersion("Inno Setup Setup Data (2.0.8) with ISX (2.0.10)",    INNO_VERSION_EXT(2, 0, 10, 0), Flags.ISX),
            new KnownVersion("Inno Setup Setup Data (2.0.11)",                     INNO_VERSION_EXT(2, 0, 11, 0), (Flags)0),
            new KnownVersion("Inno Setup Setup Data (2.0.11) with ISX (2.0.11)",   INNO_VERSION_EXT(2, 0, 11, 0), Flags.ISX),
            new KnownVersion("Inno Setup Setup Data (2.0.17)",                     INNO_VERSION_EXT(2, 0, 17, 0), (Flags)0),
            new KnownVersion("Inno Setup Setup Data (2.0.17) with ISX (2.0.11)",   INNO_VERSION_EXT(2, 0, 17, 0), Flags.ISX),
            new KnownVersion("Inno Setup Setup Data (2.0.18)",                     INNO_VERSION_EXT(2, 0, 18, 0), (Flags)0),
            new KnownVersion("Inno Setup Setup Data (2.0.18) with ISX (2.0.11)",   INNO_VERSION_EXT(2, 0, 18, 0), Flags.ISX),
            new KnownVersion("Inno Setup Setup Data (3.0.0a)",                     INNO_VERSION_EXT(3, 0,  0, 0), (Flags)0),
            new KnownVersion("Inno Setup Setup Data (3.0.1)",                      INNO_VERSION_EXT(3, 0,  1, 0), (Flags)0),
            new KnownVersion("Inno Setup Setup Data (3.0.1) with ISX (3.0.0)",     INNO_VERSION_EXT(3, 0,  1, 0), Flags.ISX),
            new KnownVersion("Inno Setup Setup Data (3.0.3)",      /* ambiguous */ INNO_VERSION_EXT(3, 0,  3, 0), (Flags)0),
            new KnownVersion("Inno Setup Setup Data (3.0.3) with ISX (3.0.3)",     INNO_VERSION_EXT(3, 0,  3, 0), Flags.ISX),
            new KnownVersion("Inno Setup Setup Data (3.0.4)",                      INNO_VERSION_EXT(3, 0,  4, 0), (Flags)0),
            new KnownVersion("My Inno Setup Extensions Setup Data (3.0.4)",        INNO_VERSION_EXT(3, 0,  4, 0), Flags.ISX),
            new KnownVersion("Inno Setup Setup Data (3.0.5)",                      INNO_VERSION_EXT(3, 0,  5, 0), (Flags)0),
            new KnownVersion("My Inno Setup Extensions Setup Data (3.0.6.1)",      INNO_VERSION_EXT(3, 0,  6, 1), Flags.ISX),
            new KnownVersion("Inno Setup Setup Data (4.0.0a)",                     INNO_VERSION_EXT(4, 0,  0, 0), (Flags)0),
            new KnownVersion("Inno Setup Setup Data (4.0.1)",                      INNO_VERSION_EXT(4, 0,  1, 0), (Flags)0),
            new KnownVersion("Inno Setup Setup Data (4.0.3)",                      INNO_VERSION_EXT(4, 0,  3, 0), (Flags)0),
            new KnownVersion("Inno Setup Setup Data (4.0.5)",                      INNO_VERSION_EXT(4, 0,  5, 0), (Flags)0),
            new KnownVersion("Inno Setup Setup Data (4.0.9)",                      INNO_VERSION_EXT(4, 0,  9, 0), (Flags)0),
            new KnownVersion("Inno Setup Setup Data (4.0.10)",                     INNO_VERSION_EXT(4, 0, 10, 0), (Flags)0),
            new KnownVersion("Inno Setup Setup Data (4.0.11)",                     INNO_VERSION_EXT(4, 0, 11, 0), (Flags)0),
            new KnownVersion("Inno Setup Setup Data (4.1.0)",                      INNO_VERSION_EXT(4, 1,  0, 0), (Flags)0),
            new KnownVersion("Inno Setup Setup Data (4.1.2)",                      INNO_VERSION_EXT(4, 1,  2, 0), (Flags)0),
            new KnownVersion("Inno Setup Setup Data (4.1.3)",                      INNO_VERSION_EXT(4, 1,  3, 0), (Flags)0),
            new KnownVersion("Inno Setup Setup Data (4.1.4)",                      INNO_VERSION_EXT(4, 1,  4, 0), (Flags)0),
            new KnownVersion("Inno Setup Setup Data (4.1.5)",                      INNO_VERSION_EXT(4, 1,  5, 0), (Flags)0),
            new KnownVersion("Inno Setup Setup Data (4.1.6)",                      INNO_VERSION_EXT(4, 1,  6, 0), (Flags)0),
            new KnownVersion("Inno Setup Setup Data (4.1.8)",                      INNO_VERSION_EXT(4, 1,  8, 0), (Flags)0),
            new KnownVersion("Inno Setup Setup Data (4.2.0)",                      INNO_VERSION_EXT(4, 2,  0, 0), (Flags)0),
            new KnownVersion("Inno Setup Setup Data (4.2.1)",                      INNO_VERSION_EXT(4, 2,  1, 0), (Flags)0),
            new KnownVersion("Inno Setup Setup Data (4.2.2)",                      INNO_VERSION_EXT(4, 2,  2, 0), (Flags)0),
            new KnownVersion("Inno Setup Setup Data (4.2.3)",      /* ambiguous */ INNO_VERSION_EXT(4, 2,  3, 0), (Flags)0),
            new KnownVersion("Inno Setup Setup Data (4.2.4)",                      INNO_VERSION_EXT(4, 2,  4, 0), (Flags)0),
            new KnownVersion("Inno Setup Setup Data (4.2.5)",                      INNO_VERSION_EXT(4, 2,  5, 0), (Flags)0),
            new KnownVersion("Inno Setup Setup Data (4.2.6)",                      INNO_VERSION_EXT(4, 2,  6, 0), (Flags)0),
            new KnownVersion("Inno Setup Setup Data (5.0.0)",                      INNO_VERSION_EXT(5, 0,  0, 0), (Flags)0),
            new KnownVersion("Inno Setup Setup Data (5.0.1)",                      INNO_VERSION_EXT(5, 0,  1, 0), (Flags)0),
            new KnownVersion("Inno Setup Setup Data (5.0.3)",                      INNO_VERSION_EXT(5, 0,  3, 0), (Flags)0),
            new KnownVersion("Inno Setup Setup Data (5.0.4)",                      INNO_VERSION_EXT(5, 0,  4, 0), (Flags)0),
            new KnownVersion("Inno Setup Setup Data (5.1.0)",                      INNO_VERSION_EXT(5, 1,  0, 0), (Flags)0),
            new KnownVersion("Inno Setup Setup Data (5.1.2)",                      INNO_VERSION_EXT(5, 1,  2, 0), (Flags)0),
            new KnownVersion("Inno Setup Setup Data (5.1.7)",                      INNO_VERSION_EXT(5, 1,  7, 0), (Flags)0),
            new KnownVersion("Inno Setup Setup Data (5.1.10)",                     INNO_VERSION_EXT(5, 1, 10, 0), (Flags)0),
            new KnownVersion("Inno Setup Setup Data (5.1.13)",                     INNO_VERSION_EXT(5, 1, 13, 0), (Flags)0),
            new KnownVersion("Inno Setup Setup Data (5.2.0)",                      INNO_VERSION_EXT(5, 2,  0, 0), (Flags)0),
            new KnownVersion("Inno Setup Setup Data (5.2.1)",                      INNO_VERSION_EXT(5, 2,  1, 0), (Flags)0),
            new KnownVersion("Inno Setup Setup Data (5.2.3)",                      INNO_VERSION_EXT(5, 2,  3, 0), (Flags)0),
            new KnownVersion("Inno Setup Setup Data (5.2.5)",                      INNO_VERSION_EXT(5, 2,  5, 0), (Flags)0),
            new KnownVersion("Inno Setup Setup Data (5.2.5) (u)",                  INNO_VERSION_EXT(5, 2,  5, 0), Flags.Unicode),
            new KnownVersion("Inno Setup Setup Data (5.3.0)",                      INNO_VERSION_EXT(5, 3,  0, 0), (Flags)0),
            new KnownVersion("Inno Setup Setup Data (5.3.0) (u)",                  INNO_VERSION_EXT(5, 3,  0, 0), Flags.Unicode),
            new KnownVersion("Inno Setup Setup Data (5.3.3)",                      INNO_VERSION_EXT(5, 3,  3, 0), (Flags)0),
            new KnownVersion("Inno Setup Setup Data (5.3.3) (u)",                  INNO_VERSION_EXT(5, 3,  3, 0), Flags.Unicode),
            new KnownVersion("Inno Setup Setup Data (5.3.5)",                      INNO_VERSION_EXT(5, 3,  5, 0), (Flags)0),
            new KnownVersion("Inno Setup Setup Data (5.3.5) (u)",                  INNO_VERSION_EXT(5, 3,  5, 0), Flags.Unicode),
            new KnownVersion("Inno Setup Setup Data (5.3.6)",                      INNO_VERSION_EXT(5, 3,  6, 0), (Flags)0),
            new KnownVersion("Inno Setup Setup Data (5.3.6) (u)",                  INNO_VERSION_EXT(5, 3,  6, 0), Flags.Unicode),
            new KnownVersion("Inno Setup Setup Data (5.3.7)",                      INNO_VERSION_EXT(5, 3,  7, 0), (Flags)0),
            new KnownVersion("Inno Setup Setup Data (5.3.7) (u)",                  INNO_VERSION_EXT(5, 3,  7, 0), Flags.Unicode),
            new KnownVersion("Inno Setup Setup Data (5.3.8)",                      INNO_VERSION_EXT(5, 3,  8, 0), (Flags)0),
            new KnownVersion("Inno Setup Setup Data (5.3.8) (u)",                  INNO_VERSION_EXT(5, 3,  8, 0), Flags.Unicode),
            new KnownVersion("Inno Setup Setup Data (5.3.9)",                      INNO_VERSION_EXT(5, 3,  9, 0), (Flags)0),
            new KnownVersion("Inno Setup Setup Data (5.3.9) (u)",                  INNO_VERSION_EXT(5, 3,  9, 0), Flags.Unicode),
            new KnownVersion("Inno Setup Setup Data (5.3.10)",                     INNO_VERSION_EXT(5, 3, 10, 0), (Flags)0),
            new KnownVersion("Inno Setup Setup Data (5.3.10) (u)",                 INNO_VERSION_EXT(5, 3, 10, 0), Flags.Unicode),
            new KnownVersion("Inno Setup Setup Data (5.4.2)",                      INNO_VERSION_EXT(5, 4,  2, 0), (Flags)0),
            new KnownVersion("Inno Setup Setup Data (5.4.2) (u)",  /* ambiguous */ INNO_VERSION_EXT(5, 4,  2, 0), Flags.Unicode),
            new KnownVersion("" /* BlackBox v1? */,                                INNO_VERSION_EXT(5, 4,  2, 1), (Flags)0),
            new KnownVersion("" /* BlackBox v1? */,                                INNO_VERSION_EXT(5, 4,  2, 1), Flags.Unicode),
            new KnownVersion("Inno Setup Setup Data (5.5.0)",                      INNO_VERSION_EXT(5, 5,  0, 0), (Flags)0),
            new KnownVersion("Inno Setup Setup Data (5.5.0) (u)",  /* ambiguous */ INNO_VERSION_EXT(5, 5,  0, 0), Flags.Unicode),
            new KnownVersion("" /* BlackBox v2 / Inno Setup Ultra */,              INNO_VERSION_EXT(5, 5,  0, 1), (Flags)0),
            new KnownVersion("" /* BlackBox v2 / Inno Setup Ultra */,              INNO_VERSION_EXT(5, 5,  0, 1), Flags.Unicode),
            new KnownVersion("Inno Setup Setup Data (5.5.6)",                      INNO_VERSION_EXT(5, 5,  6, 0), (Flags)0),
            new KnownVersion("Inno Setup Setup Data (5.5.6) (u)",                  INNO_VERSION_EXT(5, 5,  6, 0), Flags.Unicode),
            new KnownVersion("Inno Setup Setup Data (5.5.7)",      /* ambiguous */ INNO_VERSION_EXT(5, 5,  7, 0), (Flags)0),
            new KnownVersion("Inno Setup Setup Data (5.5.7) (u)",  /* ambiguous */ INNO_VERSION_EXT(5, 5,  7, 0), Flags.Unicode),
            new KnownVersion("Inno Setup Setup Data (5.5.7) (U)",  /* ambiguous */ INNO_VERSION_EXT(5, 5,  7, 0), Flags.Unicode),
            new KnownVersion("Inno Setup Setup Data (5.5.8) (u)", /* unofficial */ INNO_VERSION_EXT(5, 5,  7, 0), Flags.Unicode),
            new KnownVersion("" /* unknown 5.5.7 (u) variant */,   /* ambiguous */ INNO_VERSION_EXT(5, 5,  7, 1), (Flags)0),
            new KnownVersion("" /* unknown 5.5.7 (u) variant */,   /* ambiguous */ INNO_VERSION_EXT(5, 5,  7, 1), Flags.Unicode),
            new KnownVersion("Inno Setup Setup Data (5.6.0)",                      INNO_VERSION_EXT(5, 6,  0, 0), (Flags)0),
            new KnownVersion("Inno Setup Setup Data (5.6.0) (u)",                  INNO_VERSION_EXT(5, 6,  0, 0), Flags.Unicode),
            new KnownVersion("Inno Setup Setup Data (5.6.2)",     /* prerelease */ INNO_VERSION_EXT(5, 6,  2, 0), (Flags)0),
            new KnownVersion("Inno Setup Setup Data (5.6.2) (u)", /* prerelease */ INNO_VERSION_EXT(5, 6,  2, 0), Flags.Unicode),
            new KnownVersion("Inno Setup Setup Data (6.0.0) (u)",                  INNO_VERSION_EXT(6, 0,  0, 0), Flags.Unicode),
        };

        public InnoVersion()
        {
            Value = 0;
            Variant = 0;
            Known = false;
        }

        public InnoVersion(uint v, Flags type = 0, bool isKnown = false)
        {
            Value = v;
            Variant = type;
            Known = isKnown;
        }

        public InnoVersion(byte a, byte b, byte c, byte d = 0, Flags type = 0, bool isKnown = false)
        {
            Value = INNO_VERSION_EXT(a, b, c, d);
            Variant = type;
            Known = isKnown;
        }

        public uint A() { return Value >> 24; }
        public uint B() { return (Value >> 16) & 0xff; }
        public uint C() { return (Value >> 8) & 0xff; }
        public uint D() { return Value & 0xff; }

        public void Load(Stream input)
        {
            char[] digits = "0123456789".ToCharArray();

            using (BinaryReader br = new BinaryReader(input, Encoding.Default, true))
            {
                char[] legacyVersion = new char[12];
                legacyVersion = br.ReadChars(legacyVersion.Length);

                if (legacyVersion[0] == 'i' && legacyVersion[legacyVersion.Length - 1] == '\x1a')
                {
                    for (int i = 0; i < LegacyVersions.Length; i++)
                    {
                        if (legacyVersion.SequenceEqual(LegacyVersions[i].Name))
                        {
                            Value = LegacyVersions[i].Version;
                            Variant = LegacyVersions[i].Variant;
                            Known = true;
                            Console.WriteLine($"known legacy version: \"{Versions[i].Name}\"");
                            return;
                        }
                    }

                    Console.WriteLine($"unknown legacy version: {new string(legacyVersion)}");

                    if (legacyVersion[0] != 'i' || legacyVersion[2] != '.' || legacyVersion[4] != '.' || legacyVersion[7] != '-' || legacyVersion[8] != '-')
                        throw new VersionError();

                    if (legacyVersion[9] == '1' && legacyVersion[10] == '6')
                        Variant = Flags.Bits16;
                    else if (legacyVersion[9] == '3' && legacyVersion[10] == '2')
                        Variant = 0;
                    else
                        throw new VersionError();

                    string versionStrLegacy = new string(legacyVersion);

                    try
                    {
                        int a = Array.IndexOf(digits, versionStrLegacy[1]);
                        int b = Array.IndexOf(digits, versionStrLegacy[3]);
                        int c = (Array.IndexOf(digits, versionStrLegacy[5]) * 10) + Array.IndexOf(digits, versionStrLegacy[6]);
                        Value = INNO_VERSION(a, b, c);
                    }
                    catch
                    {
                        throw new VersionError();
                    }

                    Known = false;

                    return;
                }

                char[] version = new char[64];
                Array.Copy(legacyVersion, version, legacyVersion.Length);
                char[] versionExtra = br.ReadChars(version.Length - legacyVersion.Length);
                Array.Copy(versionExtra, 0, version, legacyVersion.Length, versionExtra.Length);

                for (int i = 0; i < Versions.Length; i++)
                {
                    if (version.SequenceEqual(Versions[i].Name))
                    {
                        Value = Versions[i].Version;
                        Variant = Versions[i].Variant;
                        Known = true;
                        Console.WriteLine($"known version: \"{Versions[i].Name}\"");
                        return;
                    }
                }

                int end = Array.IndexOf(version, '\0');
                string versionStr = new string(version.Take(end).ToArray());
                Console.WriteLine($"unknown version: {versionStr}");
                if (!versionStr.Contains("Inno Setup"))
                    throw new VersionError();

                Value = 0;
                int bracket = versionStr.IndexOf('(');
                for (; bracket != -1; bracket = versionStr.IndexOf('(', bracket + 1))
                {
                    if (versionStr.Length - bracket < 6)
                        continue;

                    try
                    {
                        int a_start = bracket + 1;
                        int a_end = FindFirstNotOf(versionStr, digits, a_start);
                        if (a_end == -1 || versionStr[a_end] != '.')
                            continue;

                        uint a = (uint)Utility.ToUnsigned(versionStr.Substring(a_start), a_end - a_start);

                        int b_start = a_end + 1;
                        int b_end = FindFirstNotOf(versionStr, digits, b_start);
                        if (b_end == -1 || versionStr[b_end] != '.')
                            continue;

                        uint b = (uint)Utility.ToUnsigned(versionStr.Substring(b_start), b_end - b_start);

                        int c_start = b_end + 1;
                        int c_end = FindFirstNotOf(versionStr, digits, c_start);
                        if (c_end == -1 || versionStr[c_end] != '.')
                            continue;

                        uint c = (uint)Utility.ToUnsigned(versionStr.Substring(c_start), c_end - c_start);

                        int d_start = c_end;
                        if (versionStr[d_start] == 'a')
                        {
                            if (d_start + 1 >= versionStr.Length)
                                continue;

                            d_start++;
                        }

                        uint d = 0;
                        if (versionStr[d_start] == '.')
                        {
                            d_start++;
                            int d_end = FindFirstNotOf(versionStr, digits, d_start);
                            if (d_end != -1 && d_end != d_start)

                                d = (uint)Utility.ToUnsigned(versionStr.Substring(c_start), c_end - c_start);
                        }

                        Value = Math.Max(Value, INNO_VERSION_EXT(a, b, c, d));
                    }
                    catch
                    {
                        continue;
                    }
                }

                if (Value == 0)
                    throw new VersionError(); //TODO: We really shouldn't do this

                Variant = 0;
                if (versionStr.Contains("(u)") || versionStr.Contains("(U)"))
                    Variant |= Flags.Unicode;
                if (versionStr.Contains("My Inno Setup Extensions") || versionStr.Contains("with ISX"))
                    Variant |= Flags.ISX;

                Known = false;
            }
        }

        public ushort Bits() { return (Variant & Flags.Bits16) != 0 ? (ushort)16 : (ushort)32; }
        public bool IsUnicode() { return (Variant & Flags.Unicode) != 0; }
        public bool IsIsx() { return (Variant & Flags.ISX) != 0; }

        /// <returns>true if the version stored might not be correct</returns>
        public bool IsAmbiguous()
        {
            if (Value == INNO_VERSION(2, 0, 1))
            {
                // might be either 2.0.1 or 2.0.2
                return true;
            }

            if (Value == INNO_VERSION(3, 0, 3))
            {
                // might be either 3.0.3 or 3.0.4
                return true;
            }

            if (Value == INNO_VERSION(4, 2, 3))
            {
                // might be either 4.2.3 or 4.2.4
                return true;
            }

            if (Value == INNO_VERSION(5, 5, 0))
            {
                // might be either 5.5.0 or 5.5.0.1
                return true;
            }

            if (Value == INNO_VERSION(5, 5, 7))
            {
                // might be either 5.5.7 or 5.6.0
                return true;
            }

            return false;
        }

        public uint Next()
        {
            KnownLegacyVersion legacyEnd = LegacyVersions.Last();
            KnownLegacyVersion legacyVersion;
            legacyVersion = LegacyVersions.Where(v => v.Version > Value).FirstOrDefault();
            if (legacyVersion != null && legacyVersion != legacyEnd)
                return legacyVersion.Version;

            KnownVersion end = Versions.Last();
            KnownVersion version;
            version = Versions.Where(v => v.Version > Value).FirstOrDefault();
            if (version != null && version != end)
                return version.Version;

            return 0;
        }

        public static implicit operator uint(InnoVersion version)
        {
            return version.Value;
        }

        public static uint INNO_VERSION(int a, int b, int c)
        {
            return INNO_VERSION_EXT(a, b, c, 0);
        }
        public static uint INNO_VERSION(uint a, uint b, uint c)
        {
            return INNO_VERSION_EXT(a, b, c, 0);
        }
        public static uint INNO_VERSION_EXT(int a, int b, int c, int d)
        {
            return (uint)((a << 24) | (b << 16) | (c << 8) | (d << 0));
        }
        public static uint INNO_VERSION_EXT(uint a, uint b, uint c, uint d)
        {
            return (a << 24) | (b << 16) | (c << 8) | (d << 0);
        }

        /// <summary>
        /// https://stackoverflow.com/questions/4498176/c-sharp-equivalent-of-c-stdstring-find-first-not-of-and-find-last-not-of
        /// </summary>
        private static int FindFirstNotOf(string str, char[] array, int pos)
        {
            return str.Select((x, j) => new { Val = x, Index = (int?)j })
                .Where(x => Array.IndexOf(array, x.Val, pos) == -1)
                .Select(x => x.Index)
                .FirstOrDefault() ?? -1;
        }
    }
}
