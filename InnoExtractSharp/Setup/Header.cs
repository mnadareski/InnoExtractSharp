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
using System.IO;
using System.Text;
using InnoExtractSharp.Crypto;
using InnoExtractSharp.Streams;
using InnoExtractSharp.Util;

namespace InnoExtractSharp.Setup
{
    /// <summary>
    /// Structures for the main setup header in Inno Setup files.
    /// </summary>
    public class Header
    {
        [Flags]
        public enum Flags : long
        {
            DisableStartupPrompt = 1L << 0,
            CreateAppDir = 1L << 1,
            AllowNoIcons = 1L << 2,
            AlwaysRestart = 1L << 3,
            AlwaysUsePersonalGroup = 1L << 4,
            WindowVisible = 1L << 5,
            WindowShowCaption = 1L << 6,
            WindowResizable = 1L << 7,
            WindowStartMaximized = 1L << 8,
            EnableDirDoesntExistWarning = 1L << 9,
            Password = 1L << 10,
            AllowRootDirectory = 1L << 11,
            DisableFinishedPage = 1L << 12,
            ChangesAssociations = 1L << 13,
            UsePreviousAppDir = 1L << 14,
            BackColorHorizontal = 1L << 15,
            UsePreviousGroup = 1L << 16,
            UpdateUninstallLogAppName = 1L << 17,
            UsePreviousSetupType = 1L << 18,
            DisableReadyMemo = 1L << 19,
            AlwaysShowComponentsList = 1L << 20,
            FlatComponentsList = 1L << 21,
            ShowComponentSizes = 1L << 22,
            UsePreviousTasks = 1L << 23,
            DisableReadyPage = 1L << 24,
            AlwaysShowDirOnReadyPage = 1L << 25,
            AlwaysShowGroupOnReadyPage = 1L << 26,
            AllowUNCPath = 1L << 27,
            UserInfoPage = 1L << 28,
            UsePreviousUserInfo = 1L << 29,
            UninstallRestartComputer = 1L << 30,
            RestartIfNeededByRun = 1L << 31,
            ShowTasksTreeLines = 1L << 32,
            AllowCancelDuringInstall = 1L << 33,
            WizardImageStretch = 1L << 34,
            AppendDefaultDirName = 1L << 35,
            AppendDefaultGroupName = 1L << 36,
            EncryptionUsed = 1L << 37,
            ChangesEnvironment = 1L << 38,
            ShowUndisplayableLanguages = 1L << 39,
            SetupLogging = 1L << 40,
            SignedUninstaller = 1L << 41,
            UsePreviousLanguage = 1L << 42,
            DisableWelcomePage = 1L << 43,
            CloseApplications = 1L << 44,
            RestartApplications = 1L << 45,
            AllowNetworkDrive = 1L << 46,
            ForceCloseApplications = 1L << 47,

            // Obsolete flags
            Uninstallable = 1L << 48,
            DisableDirPage = 1L << 49,
            DisableProgramGroupPage = 1L << 50,
            DisableAppendDir = 1L << 51,
            AdminPrivilegesRequired = 1L << 52,
            AlwaysCreateUninstallIcon = 1L << 53,
            CreateUninstallRegKey = 1L << 54,
            BzipUsed = 1L << 55,
            ShowLanguageDialog = 1L << 56,
            DetectLanguageUsingLocale = 1L << 57,
            DisableDirExistsWarning = 1L << 58,
            BackSolid = 1L << 59,
            OverwriteUninstRegEntries = 1L << 60,
        }

        [Flags]
        public enum ArchitectureTypes
        {
            ArchitectureUnknown = 1 << 0,
            X86 = 1 << 1,
            Amd64 = 1 << 2,
            IA64 = 1 << 3,
            ARM64 = 1 << 4,
        }

        public string AppName;
        public string AppVersionedName;
        public string AppId;
        public string AppCopyright;
        public string AppPublisher;
        public string AppPublisherUrl;
        public string AppSupportPhone;
        public string AppSupportUrl;
        public string AppUpdatesUrl;
        public string AppVersion;
        public string DefaultDirName;
        public string DefaultGroupName;
        public string UninstallIconName;
        public string BaseFilename;
        public string UninstallFilesDir;
        public string UninstallName;
        public string UninstallIcon;
        public string AppMutex;
        public string DefaultUserName;
        public string DefaultUserOrganization;
        public string DefaultSerial;
        public string AppReadmeFile;
        public string AppContact;
        public string AppComments;
        public string AppModifyPath;
        public string CreateUninstallRegistryKey;
        public string Uninstallable;
        public string CloseApplicationsFilter;
        public string SetupMutex;
        public string LicenseText;
        public string InfoBefore;
        public string InfoAfter;
        public string UninstallerSignature;
        public string CompiledCode;

        public bool[] LeadBytes = new bool[256]; // bitset<256>

        public int LanguageCount;
        public int MessageCount;
        public int PermissionCount;
        public int TypeCount;
        public int ComponentCount;
        public int TaskCount;
        public int DirectoryCount;
        public int FileCount;
        public int DataEntryCount;
        public int IconCount;
        public int IniEntryCount;
        public int RegistryEntryCount;
        public int DeleteEntryCount;
        public int UninstallDeleteEntryCount;
        public int RunEntryCount;
        public int UninstallRunEntryCount;

        public WindowsVersionRange Winver;

        public uint BackColor;
        public uint BackColor2;
        public uint ImageBackColor;
        public uint SmallImageBackColor;

        public enum AlphaFormat
        {
            AlphaIgnored,
            AlphaDefined,
            AlphaPremultiplied,
        }

        public AlphaFormat ImageAlphaFormat;

        public Checksum Password;
        public string PasswordSalt;

        public long ExtraDiskSpaceRequired;
        public int SlicesPerDisk;

        public enum InstallVerbosity
        {
            NormalInstallMode,
            SilentInstallMode,
            VerySilentInstallMode,
        }

        public InstallVerbosity InstallMode;

        public enum LogMode
        {
            AppendLog,
            NewLog,
            OverwriteLog,
        }

        public LogMode UninstallLogMode;

        public enum Style
        {
            ClassicStyle,
            ModernStyle,
        }

        public Style UninstallStyle;

        public enum AutoBool
        {
            Auto,
            No,
            Yes,
        }

        public AutoBool DirExistsWarning;

        public enum PrivilegeLevel
        {
            NoPrivileges,
            PowerUserPrivileges,
            AdminPriviliges,
            LowestPrivileges,
        }

        public PrivilegeLevel PrivilegesRequired;

        public AutoBool ShowLanguageDialog;

        public enum LanguageDetectionMethod
        {
            UILanguage,
            LocaleLanguage,
            NoLanguageDetection,
        }

        public LanguageDetectionMethod LanguageDetection;

        public CompressionMethod Compression;

        public ArchitectureTypes ArchitecturesAllowed;
        public ArchitectureTypes ArchitecturesInstalledIn64bitMode;

        public uint SignedUninstallerOriginalSize;
        public uint SignedUninstallerHeaderChecksum;

        public AutoBool DisableDirPage;
        public AutoBool DisableProgramGroupPage;

        public ulong UninstallDisplaySize;

        public Flags Options;

        public void Load(Stream input, InnoVersion version)
        {
            using (BinaryReader br = new BinaryReader(input, Encoding.Default, true))
            {
                Options = 0;

                if (version < InnoVersion.INNO_VERSION(1, 3, 21))
                    br.ReadUInt32(); // uncompressed size of the setup header

                EncodedString.Load(input, out AppName, (int)version.Codepage());

                EncodedString.Load(input, out AppVersionedName, (int)version.Codepage());
                if (version >= InnoVersion.INNO_VERSION(1, 3, 21))
                    EncodedString.Load(input, out AppId, (int)version.Codepage());

                EncodedString.Load(input, out AppCopyright, (int)version.Codepage());
                if (version >= InnoVersion.INNO_VERSION(1, 3, 21))
                {
                    EncodedString.Load(input, out AppPublisher, (int)version.Codepage());
                    EncodedString.Load(input, out AppPublisherUrl, (int)version.Codepage());
                }
                else
                {
                    AppPublisher = string.Empty;
                    AppPublisherUrl = string.Empty;
                }
                if (version >= InnoVersion.INNO_VERSION(5, 1, 13))
                    EncodedString.Load(input, out AppSupportPhone, (int)version.Codepage());
                else
                    AppSupportPhone = string.Empty;
                if (version >= InnoVersion.INNO_VERSION(1, 3, 21))
                {
                    EncodedString.Load(input, out AppSupportUrl, (int)version.Codepage());
                    EncodedString.Load(input, out AppUpdatesUrl, (int)version.Codepage());
                    EncodedString.Load(input, out AppVersion, (int)version.Codepage());
                }
                else
                {
                    AppSupportUrl = string.Empty;
                    AppUpdatesUrl = string.Empty;
                    AppVersion = string.Empty;
                }

                EncodedString.Load(input, out DefaultDirName, (int)version.Codepage());
                EncodedString.Load(input, out DefaultGroupName, (int)version.Codepage());
                if (version < InnoVersion.INNO_VERSION(3, 0, 0))
                    AnsiString.Load(input, out UninstallIconName);
                else
                    UninstallIconName = string.Empty;

                EncodedString.Load(input, out BaseFilename, (int)version.Codepage());
                if (version >= InnoVersion.INNO_VERSION(1, 3, 21))
                {
                    if (version < InnoVersion.INNO_VERSION(5, 2, 5))
                    {
                        AnsiString.Load(input, out LicenseText);
                        AnsiString.Load(input, out InfoBefore);
                        AnsiString.Load(input, out InfoAfter);
                    }
                    EncodedString.Load(input, out UninstallFilesDir, (int)version.Codepage());
                    EncodedString.Load(input, out UninstallName, (int)version.Codepage());
                    EncodedString.Load(input, out UninstallIcon, (int)version.Codepage());
                    EncodedString.Load(input, out AppMutex, (int)version.Codepage());
                }
                else
                {
                    LicenseText = string.Empty;
                    InfoBefore = string.Empty;
                    InfoAfter = string.Empty;
                    UninstallFilesDir = string.Empty;
                    UninstallName = string.Empty;
                    UninstallIcon = string.Empty;
                    AppMutex = string.Empty;
                }
                if (version >= InnoVersion.INNO_VERSION(3, 0, 0))
                {
                    EncodedString.Load(input, out DefaultUserName, (int)version.Codepage());
                    EncodedString.Load(input, out DefaultUserOrganization, (int)version.Codepage());
                }
                else
                {
                    DefaultUserName = string.Empty;
                    DefaultUserOrganization = string.Empty;
                }
                if (version >= InnoVersion.INNO_VERSION_EXT(3, 0, 6, 1))
                {
                    EncodedString.Load(input, out DefaultSerial, (int)version.Codepage());
                    if (version < InnoVersion.INNO_VERSION(5, 2, 5))
                        BinaryString.Load(input, out CompiledCode);
                }
                else
                {
                    DefaultSerial = string.Empty;
                    CompiledCode = string.Empty;
                }
                if (version >= InnoVersion.INNO_VERSION(4, 2, 4))
                {
                    EncodedString.Load(input, out AppReadmeFile, (int)version.Codepage());
                    EncodedString.Load(input, out AppContact, (int)version.Codepage());
                    EncodedString.Load(input, out AppComments, (int)version.Codepage());
                    EncodedString.Load(input, out AppModifyPath, (int)version.Codepage());
                }
                else
                {
                    AppReadmeFile = string.Empty;
                    AppContact = string.Empty;
                    AppComments = string.Empty;
                    AppModifyPath = string.Empty;
                }
                if (version >= InnoVersion.INNO_VERSION(5, 3, 8))
                    EncodedString.Load(input, out CreateUninstallRegistryKey, (int)version.Codepage());
                else
                    CreateUninstallRegistryKey = string.Empty;
                if (version >= InnoVersion.INNO_VERSION(5, 3, 10))
                    EncodedString.Load(input, out Uninstallable, (int)version.Codepage());
                else
                    Uninstallable = string.Empty;
                if (version >= InnoVersion.INNO_VERSION(5, 5, 0))
                    EncodedString.Load(input, out CloseApplicationsFilter, (int)version.Codepage());
                else
                    CloseApplicationsFilter = string.Empty;
                if (version >= InnoVersion.INNO_VERSION(5, 5, 6))
                    EncodedString.Load(input, out SetupMutex, (int)version.Codepage());
                else
                    SetupMutex = string.Empty;
                if (version >= InnoVersion.INNO_VERSION(5, 2, 5))
                {
                    AnsiString.Load(input, out LicenseText);
                    AnsiString.Load(input, out InfoBefore);
                    AnsiString.Load(input, out InfoAfter);
                }
                if (version >= InnoVersion.INNO_VERSION(5, 2, 1) && version < InnoVersion.INNO_VERSION(5, 3, 10))
                    BinaryString.Load(input, out UninstallerSignature);
                else
                    UninstallerSignature = string.Empty;
                if (version >= InnoVersion.INNO_VERSION(5, 2, 5))
                    BinaryString.Load(input, out CompiledCode);

                if (version >= InnoVersion.INNO_VERSION(2, 0, 6) && !version.Unicode)
                {
                    uint lead = br.ReadUInt32();
                    for (int i = 0; i < 256; i++)
                    {
                        LeadBytes[i] = (lead % 2 == 0 ? false : true);
                        lead /= 2;
                    }
                }
                else
                    LeadBytes = null;

                if (version >= InnoVersion.INNO_VERSION(4, 0, 0))
                    LanguageCount = (int)br.ReadUInt32();
                else if (version >= InnoVersion.INNO_VERSION(2, 0, 1))
                    LanguageCount = 1;
                else
                    LanguageCount = 0;

                if (version >= InnoVersion.INNO_VERSION(4, 2, 1))
                    MessageCount = (int)br.ReadUInt32();
                else
                    MessageCount = 0;

                if (version >= InnoVersion.INNO_VERSION(4, 1, 0))
                    PermissionCount = (int)br.ReadUInt32();
                else
                    PermissionCount = 0;

                if (version >= InnoVersion.INNO_VERSION(2, 0, 0))
                {
                    TypeCount = (int)br.ReadUInt32();
                    ComponentCount = (int)br.ReadUInt32();
                    TaskCount = (int)br.ReadUInt32();
                }
                else
                {
                    TypeCount = 0;
                    ComponentCount = 0;
                    TaskCount = 0;
                }

                DirectoryCount = (int)(version.Bits == 16 ? br.ReadUInt16() : br.ReadUInt32());
                FileCount = (int)(version.Bits == 16 ? br.ReadUInt16() : br.ReadUInt32());
                DataEntryCount = (int)(version.Bits == 16 ? br.ReadUInt16() : br.ReadUInt32());
                IconCount = (int)(version.Bits == 16 ? br.ReadUInt16() : br.ReadUInt32());
                IniEntryCount = (int)(version.Bits == 16 ? br.ReadUInt16() : br.ReadUInt32());
                RegistryEntryCount = (int)(version.Bits == 16 ? br.ReadUInt16() : br.ReadUInt32());
                DeleteEntryCount = (int)(version.Bits == 16 ? br.ReadUInt16() : br.ReadUInt32());
                UninstallDeleteEntryCount = (int)(version.Bits == 16 ? br.ReadUInt16() : br.ReadUInt32());
                RunEntryCount = (int)(version.Bits == 16 ? br.ReadUInt16() : br.ReadUInt32());
                UninstallRunEntryCount = (int)(version.Bits == 16 ? br.ReadUInt16() : br.ReadUInt32());

                int licenseSize = 0;
                int infoBeforeSize = 0;
                int infoAfterSize = 0;
                if (version < InnoVersion.INNO_VERSION(1, 3, 21))
                {
                    licenseSize = (int)(version.Bits == 16 ? br.ReadUInt16() : br.ReadUInt32());
                    infoBeforeSize = (int)(version.Bits == 16 ? br.ReadUInt16() : br.ReadUInt32());
                    infoAfterSize = (int)(version.Bits == 16 ? br.ReadUInt16() : br.ReadUInt32());
                }

                Winver.Load(input, version);

                BackColor = br.ReadUInt32();
                if (version >= InnoVersion.INNO_VERSION(1, 3, 21))
                    BackColor2 = br.ReadUInt32();
                else
                    BackColor2 = 0;
                if (version < InnoVersion.INNO_VERSION(5, 5, 7))
                    ImageBackColor = br.ReadUInt32();
                else
                    ImageBackColor = 0;
                if (version >= InnoVersion.INNO_VERSION(2, 0, 0) && version < InnoVersion.INNO_VERSION(5, 0, 4))
                    SmallImageBackColor = br.ReadUInt32();
                else
                    SmallImageBackColor = 0;

                if (version >= InnoVersion.INNO_VERSION(5, 5, 7))
                    ImageAlphaFormat = Stored.AlphaFormats.TryGetValue(br.ReadByte(), AlphaFormat.AlphaIgnored);
                else
                    ImageAlphaFormat = AlphaFormat.AlphaIgnored;

                if (version < InnoVersion.INNO_VERSION(4, 2, 0))
                {
                    Password.CRC32 = br.ReadUInt32();
                    Password.Type = ChecksumType.CRC32;
                }
                else if (version < InnoVersion.INNO_VERSION(5, 3, 9))
                {
                    Password.MD5 = br.ReadChars(Password.MD5.Length);
                    Password.Type = ChecksumType.MD5;
                }
                else
                {
                    Password.SHA1 = br.ReadChars(Password.SHA1.Length);
                    Password.Type = ChecksumType.SHA1;
                }
                if (version >= InnoVersion.INNO_VERSION(4, 2, 2))
                    PasswordSalt = "PasswordCheckHash" + new string(br.ReadChars(8));
                else
                    PasswordSalt = string.Empty;

                if (version >= InnoVersion.INNO_VERSION(4, 0, 0))
                {
                    ExtraDiskSpaceRequired = br.ReadInt64();
                    SlicesPerDisk = (int)br.ReadUInt32();
                }
                else
                {
                    ExtraDiskSpaceRequired = br.ReadInt32();
                    SlicesPerDisk = 1;
                }

                if (version >= InnoVersion.INNO_VERSION(2, 0, 0) && version < InnoVersion.INNO_VERSION(5, 0, 0))
                    InstallMode = Stored.InstallVerbosities.TryGetValue(br.ReadByte(), InstallVerbosity.NormalInstallMode);
                else
                    InstallMode = InstallVerbosity.NormalInstallMode;

                if (version >= InnoVersion.INNO_VERSION(1, 3, 21))
                    UninstallLogMode = Stored.LogModes.TryGetValue(br.ReadByte(), LogMode.AppendLog);
                else
                    UninstallLogMode = LogMode.AppendLog;

                if (version >= InnoVersion.INNO_VERSION(2, 0, 0) && version < InnoVersion.INNO_VERSION(5, 0, 0))
                    UninstallStyle = Stored.SetupStyles.TryGetValue(br.ReadByte(), Style.ClassicStyle);
                else
                    UninstallStyle = (version < InnoVersion.INNO_VERSION(5, 0, 0)) ? Style.ClassicStyle : Style.ModernStyle;

                if (version >= InnoVersion.INNO_VERSION(1, 3, 21))
                    DirExistsWarning = Stored.AutoBoolAutoNoYes.TryGetValue(br.ReadByte(), AutoBool.Auto);
                else
                {
                    DirExistsWarning = AutoBool.Auto;
                }

                if (version >= InnoVersion.INNO_VERSION(3, 0, 0) && version < InnoVersion.INNO_VERSION(3, 0, 3))
                {
                    AutoBool val = Stored.AutoBoolAutoNoYes.TryGetValue(br.ReadByte(), AutoBool.Auto);
                    switch (val)
                    {
                        case AutoBool.Yes: Options |= Flags.AlwaysRestart; break;
                        case AutoBool.Auto: Options |= Flags.RestartIfNeededByRun; break;
                        case AutoBool.No: break;
                    }
                }

                if (version >= InnoVersion.INNO_VERSION(5, 3, 7))
                    PrivilegesRequired = Stored.PrivilegeLevels1.TryGetValue(br.ReadByte(), PrivilegeLevel.NoPrivileges);
                else if (version >= InnoVersion.INNO_VERSION(3, 0, 4))
                    PrivilegesRequired = Stored.PrivilegeLevels0.TryGetValue(br.ReadByte(), PrivilegeLevel.NoPrivileges);

                if (version >= InnoVersion.INNO_VERSION(4, 0, 10))
                {
                    ShowLanguageDialog = Stored.AutoBoolYesNoAuto.TryGetValue(br.ReadByte(), AutoBool.Yes);
                    LanguageDetection = Stored.LanguageDetectionMethods.TryGetValue(br.ReadByte(), LanguageDetectionMethod.UILanguage);
                }

                if (version >= InnoVersion.INNO_VERSION(5, 3, 9))
                    Compression = Stored.CompressionMethods3.TryGetValue(br.ReadByte(), CompressionMethod.UnknownCompression);
                else if (version >= InnoVersion.INNO_VERSION(4, 2, 6))
                    Compression = Stored.CompressionMethods2.TryGetValue(br.ReadByte(), CompressionMethod.UnknownCompression);
                else if (version >= InnoVersion.INNO_VERSION(4, 2, 5))
                    Compression = Stored.CompressionMethods1.TryGetValue(br.ReadByte(), CompressionMethod.UnknownCompression);
                else if (version >= InnoVersion.INNO_VERSION(4, 1, 5))
                    Compression = Stored.CompressionMethods0.TryGetValue(br.ReadByte(), CompressionMethod.UnknownCompression);

                if (version >= InnoVersion.INNO_VERSION(5, 6, 0))
                {
                    ArchitecturesAllowed = (ArchitectureTypes)(br.ReadUInt32() & (uint)Stored.ArchitectureTypes0);
                    ArchitecturesInstalledIn64bitMode = (ArchitectureTypes)(br.ReadUInt32() & (uint)Stored.ArchitectureTypes0);
                }
                else if (version >= InnoVersion.INNO_VERSION(5, 1, 0))
                {
                    ArchitecturesAllowed = (ArchitectureTypes)(br.ReadUInt32() & (uint)Stored.ArchitectureTypes1);
                    ArchitecturesInstalledIn64bitMode = (ArchitectureTypes)(br.ReadUInt32() & (uint)Stored.ArchitectureTypes1);
                }
                else
                {
                    ArchitecturesAllowed = Stored.ArchitectureTypesAll;
                    ArchitecturesInstalledIn64bitMode = Stored.ArchitectureTypesAll;
                }

                if (version >= InnoVersion.INNO_VERSION(5, 2, 1) && version < InnoVersion.INNO_VERSION(5, 3, 10))
                {
                    SignedUninstallerOriginalSize = br.ReadUInt32();
                    SignedUninstallerHeaderChecksum = br.ReadUInt32();
                }
                else
                    SignedUninstallerOriginalSize = SignedUninstallerHeaderChecksum = 0;

                if (version >= InnoVersion.INNO_VERSION(5, 3, 3))
                {
                    DisableDirPage = Stored.AutoBoolAutoNoYes.TryGetValue(br.ReadByte(), AutoBool.Auto);
                    DisableProgramGroupPage = Stored.AutoBoolAutoNoYes.TryGetValue(br.ReadByte(), AutoBool.Auto);
                }

                if (version >= InnoVersion.INNO_VERSION(5, 5, 0))
                    UninstallDisplaySize = br.ReadUInt64();
                else if (version >= InnoVersion.INNO_VERSION(5, 3, 6))
                    UninstallDisplaySize = br.ReadUInt32();
                else
                    UninstallDisplaySize = 0;

                if (version == InnoVersion.INNO_VERSION_EXT(5, 5, 0, 1))
                {
                    /*
                     * This is needed to extract an Inno Setup variant (BlackBox v2?) that uses
                     * the 5.5.0 (unicode) data version string while the format differs:
                     * The language entries are off by one byte and the EncryptionUsed flag
                     * gets set while there is no decrypt_dll.
                     * I'm not sure where exactly this byte goes, but it's after the compression
                     * type and before EncryptionUsed flag.
                     * The other values/flags between here and there look sane (mostly default).
                     */
                    br.ReadByte();
                }

                Flags flagreader = 0;

                flagreader |= Flags.DisableStartupPrompt;
                if (version < InnoVersion.INNO_VERSION(5, 3, 10))
                    flagreader |= Flags.Uninstallable;
                flagreader |= Flags.CreateAppDir;
                if (version < InnoVersion.INNO_VERSION(5, 3, 3))
                    flagreader |= Flags.DisableDirPage;
                if (version < InnoVersion.INNO_VERSION(1, 3, 21))
                    flagreader |= Flags.DisableDirExistsWarning;
                if (version < InnoVersion.INNO_VERSION(5, 3, 3))
                    flagreader |= Flags.DisableProgramGroupPage;
                flagreader |= Flags.AllowNoIcons;
                if (version < InnoVersion.INNO_VERSION(3, 0, 0) || version >= InnoVersion.INNO_VERSION(3, 0, 3))
                    flagreader |= Flags.AlwaysRestart;
                if (version < InnoVersion.INNO_VERSION(1, 3, 21))
                    flagreader |= Flags.BackSolid;
                flagreader |= Flags.AlwaysUsePersonalGroup;
                flagreader |= Flags.WindowVisible;
                flagreader |= Flags.WindowShowCaption;
                flagreader |= Flags.WindowResizable;
                flagreader |= Flags.WindowStartMaximized;
                flagreader |= Flags.EnableDirDoesntExistWarning;
                if (version < InnoVersion.INNO_VERSION(4, 1, 2))
                    flagreader |= Flags.DisableAppendDir;
                flagreader |= Flags.Password;
                flagreader |= Flags.AllowRootDirectory;
                flagreader |= Flags.DisableFinishedPage;
                if (version.Bits != 16)
                {
                    if (version < InnoVersion.INNO_VERSION(3, 0, 4))
                        flagreader |= Flags.AdminPrivilegesRequired;
                    if (version < InnoVersion.INNO_VERSION(3, 0, 0))
                        flagreader |= Flags.AlwaysCreateUninstallIcon;
                    if (version < InnoVersion.INNO_VERSION(1, 3, 21))
                        flagreader |= Flags.OverwriteUninstRegEntries;
                    flagreader |= Flags.ChangesAssociations;
                }
                if (version >= InnoVersion.INNO_VERSION(1, 3, 21))
                {
                    if (version < InnoVersion.INNO_VERSION(5, 3, 8))
                        flagreader |= Flags.CreateUninstallRegKey;
                    flagreader |= Flags.UsePreviousAppDir;
                    flagreader |= Flags.BackColorHorizontal;
                    flagreader |= Flags.UsePreviousGroup;
                    flagreader |= Flags.UpdateUninstallLogAppName;
                }
                if (version >= InnoVersion.INNO_VERSION(2, 0, 0))
                {
                    flagreader |= Flags.UsePreviousSetupType;
                    flagreader |= Flags.DisableReadyMemo;
                    flagreader |= Flags.AlwaysShowComponentsList;
                    flagreader |= Flags.FlatComponentsList;
                    flagreader |= Flags.ShowComponentSizes;
                    flagreader |= Flags.UsePreviousTasks;
                    flagreader |= Flags.DisableReadyPage;
                }
                if (version >= InnoVersion.INNO_VERSION(2, 0, 7))
                {
                    flagreader |= Flags.AlwaysShowDirOnReadyPage;
                    flagreader |= Flags.AlwaysShowGroupOnReadyPage;
                }
                if (version >= InnoVersion.INNO_VERSION(2, 0, 17) && version < InnoVersion.INNO_VERSION(4, 1, 5))
                    flagreader |= Flags.BzipUsed;
                if (version >= InnoVersion.INNO_VERSION(2, 0, 18))
                    flagreader |= Flags.AllowUNCPath;
                if (version >= InnoVersion.INNO_VERSION(3, 0, 0))
                {
                    flagreader |= Flags.UserInfoPage;
                    flagreader |= Flags.UsePreviousUserInfo;
                }
                if (version >= InnoVersion.INNO_VERSION(3, 0, 1))
                    flagreader |= Flags.UninstallRestartComputer;
                if (version >= InnoVersion.INNO_VERSION(3, 0, 3))
                    flagreader |= Flags.RestartIfNeededByRun;
                if (version >= InnoVersion.INNO_VERSION_EXT(3, 0, 6, 1))
                    flagreader |= Flags.ShowTasksTreeLines;
                if (version >= InnoVersion.INNO_VERSION(4, 0, 0) && version < InnoVersion.INNO_VERSION(4, 0, 10))
                    flagreader |= Flags.ShowLanguageDialog;
                if (version >= InnoVersion.INNO_VERSION(4, 0, 1) && version < InnoVersion.INNO_VERSION(4, 0, 10))
                    flagreader |= Flags.DetectLanguageUsingLocale;
                if (version >= InnoVersion.INNO_VERSION(4, 0, 9))
                    flagreader |= Flags.AllowCancelDuringInstall;
                else
                    Options |= Flags.AllowCancelDuringInstall;
                if (version >= InnoVersion.INNO_VERSION(4, 1, 3))
                    flagreader |= Flags.WizardImageStretch;
                if (version >= InnoVersion.INNO_VERSION(4, 1, 8))
                {
                    flagreader |= Flags.AppendDefaultDirName;
                    flagreader |= Flags.AppendDefaultGroupName;
                }
                if (version >= InnoVersion.INNO_VERSION(4, 2, 2))
                    flagreader |= Flags.EncryptionUsed;
                if (version >= InnoVersion.INNO_VERSION(5, 0, 4))
                    flagreader |= Flags.ChangesEnvironment;
                if (version >= InnoVersion.INNO_VERSION(5, 1, 7) && !version.Unicode)
                    flagreader |= Flags.ShowUndisplayableLanguages;
                if (version >= InnoVersion.INNO_VERSION(5, 1, 13))
                    flagreader |= Flags.SetupLogging;
                if (version >= InnoVersion.INNO_VERSION(5, 2, 1))
                    flagreader |= Flags.SignedUninstaller;
                if (version >= InnoVersion.INNO_VERSION(5, 3, 8))
                    flagreader |= Flags.UsePreviousLanguage;
                if (version >= InnoVersion.INNO_VERSION(5, 3, 9))
                    flagreader |= Flags.DisableWelcomePage;
                if (version >= InnoVersion.INNO_VERSION(5, 5, 0))
                {
                    flagreader |= Flags.CloseApplications;
                    flagreader |= Flags.RestartApplications;
                    flagreader |= Flags.AllowNetworkDrive;
                }
                else
                    Options |= Flags.AllowNetworkDrive;
                if (version >= InnoVersion.INNO_VERSION(5, 5, 7))
                    flagreader |= Flags.ForceCloseApplications;

                Options |= flagreader;

                if (version < InnoVersion.INNO_VERSION(3, 0, 4))
                    PrivilegesRequired = (Options & Flags.AdminPrivilegesRequired) != 0 ? PrivilegeLevel.AdminPriviliges : PrivilegeLevel.NoPrivileges;

                if (version < InnoVersion.INNO_VERSION(4, 0, 10))
                {
                    ShowLanguageDialog = (Options & Flags.ShowLanguageDialog) != 0 ? AutoBool.Yes : AutoBool.No;
                    LanguageDetection = (Options & Flags.DetectLanguageUsingLocale) != 0 ? LanguageDetectionMethod.LocaleLanguage : LanguageDetectionMethod.UILanguage;
                }

                if (version < InnoVersion.INNO_VERSION(4, 1, 5))
                    Compression = (Options & Flags.BzipUsed) != 0 ? CompressionMethod.BZip2 : CompressionMethod.Zlib;

                if (version < InnoVersion.INNO_VERSION(5, 3, 3))
                {
                    DisableDirPage = (Options & Flags.DisableDirPage) != 0 ? AutoBool.Yes : AutoBool.No;
                    DisableProgramGroupPage = (Options & Flags.DisableProgramGroupPage) != 0 ? AutoBool.Yes : AutoBool.No;
                }

                if (version < InnoVersion.INNO_VERSION(1, 3, 21))
                {
                    if (licenseSize > 0)
                        LicenseText = new string(br.ReadChars(licenseSize));

                    if (infoBeforeSize > 0)
                        InfoBefore = new string(br.ReadChars(infoBeforeSize));

                    if (infoAfterSize > 0)
                        InfoAfter = new string(br.ReadChars(infoAfterSize));
                }
            }
        }
    }
}
