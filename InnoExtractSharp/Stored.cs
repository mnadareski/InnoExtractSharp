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

using InnoExtractSharp.Setup;
using InnoExtractSharp.Streams;

namespace InnoExtractSharp
{
    public class Stored
    {
        #region ComponentEntry

        public static ComponentEntry.NamedFlags ComponentFlags0 =
            ComponentEntry.NamedFlags.Fixed
            | ComponentEntry.NamedFlags.Restart
            | ComponentEntry.NamedFlags.DisableNoUninstallWarning;

        public static ComponentEntry.NamedFlags ComponentFlags1 =
            ComponentEntry.NamedFlags.Fixed
            | ComponentEntry.NamedFlags.Restart
            | ComponentEntry.NamedFlags.DisableNoUninstallWarning
            | ComponentEntry.NamedFlags.Exclusive;

        public static ComponentEntry.NamedFlags ComponentFlags2 =
            ComponentEntry.NamedFlags.Fixed
            | ComponentEntry.NamedFlags.Restart
            | ComponentEntry.NamedFlags.DisableNoUninstallWarning
            | ComponentEntry.NamedFlags.Exclusive
            | ComponentEntry.NamedFlags.DontInheritCheck;

        #endregion

        #region DeleteEntry

        public static DeleteEntry.TargetType[] DeleteTargetTypes =
        {
            DeleteEntry.TargetType.Files,
            DeleteEntry.TargetType.FilesAndSubdirs,
            DeleteEntry.TargetType.DirIfEmpty
        };

        #endregion

        #region DirectoryEntry

        public static DirectoryEntry.Flags InnoDirectoryOptions0 =
            DirectoryEntry.Flags.NeverUninstall
            | DirectoryEntry.Flags.DeleteAfterInstall
            | DirectoryEntry.Flags.AlwaysUninstall;

        // Starting with version 5.2.0
        public static DirectoryEntry.Flags InnoDirectoryOptions1 =
            DirectoryEntry.Flags.NeverUninstall
            | DirectoryEntry.Flags.DeleteAfterInstall
            | DirectoryEntry.Flags.AlwaysUninstall
            | DirectoryEntry.Flags.SetNtfsCompression
            | DirectoryEntry.Flags.UnsetNtfsCompression;

        #endregion

        #region FileEntry

        public static FileEntry.FileCopyMode[] FileCopyModes =
        {
            FileEntry.FileCopyMode.cmNormal,
            FileEntry.FileCopyMode.cmIfDoesntExist,
            FileEntry.FileCopyMode.cmAlwaysOverwrite,
            FileEntry.FileCopyMode.cmAlwaysSkipIfSameOrOlder
        };

        public static FileEntry.FileType[] FileTypes0 =
        {
            FileEntry.FileType.UserFile,
            FileEntry.FileType.UninstExe
        };

        // win32, before 5.0.0
        public static FileEntry.FileType[] FileTypes1 =
        {
            FileEntry.FileType.UserFile,
            FileEntry.FileType.UninstExe,
            FileEntry.FileType.RegSvrExe
        };

        #endregion

        #region Header

        public static Header.ArchitectureTypes ArchitectureTypes0 =
            Header.ArchitectureTypes.ArchitectureUnknown
            | Header.ArchitectureTypes.X86
            | Header.ArchitectureTypes.Amd64
            | Header.ArchitectureTypes.IA64;

         public static Header.ArchitectureTypes ArchitectureTypes1 =
            Header.ArchitectureTypes.ArchitectureUnknown
            | Header.ArchitectureTypes.X86
            | Header.ArchitectureTypes.Amd64
            | Header.ArchitectureTypes.IA64
            | Header.ArchitectureTypes.ARM64;

        public static Header.ArchitectureTypes ArchitectureTypesAll =
            Header.ArchitectureTypes.ArchitectureUnknown
            | Header.ArchitectureTypes.X86
            | Header.ArchitectureTypes.Amd64
            | Header.ArchitectureTypes.IA64
            | Header.ArchitectureTypes.ARM64;

        public static Header.AlphaFormat[] AlphaFormats =
        {
            Header.AlphaFormat.AlphaIgnored,
            Header.AlphaFormat.AlphaDefined,
            Header.AlphaFormat.AlphaPremultiplied
        };

        public static Header.InstallVerbosity[] InstallVerbosities =
        {
            Header.InstallVerbosity.NormalInstallMode,
            Header.InstallVerbosity.SilentInstallMode,
            Header.InstallVerbosity.VerySilentInstallMode
        };

        public static Header.LogMode[] LogModes =
        {
            Header.LogMode.AppendLog,
            Header.LogMode.NewLog,
            Header.LogMode.OverwriteLog
        };

        public static Header.Style[] SetupStyles =
        {
            Header.Style.ClassicStyle,
            Header.Style.ModernStyle
        };

        public static Header.AutoBool[] AutoBoolAutoNoYes =
        {
            Header.AutoBool.Auto,
            Header.AutoBool.No,
            Header.AutoBool.Yes
        };

        public static Header.AutoBool[] AutoBoolYesNoAuto =
        {
            Header.AutoBool.Yes,
            Header.AutoBool.No,
            Header.AutoBool.Auto
        };

        // pre-5.3.7
        public static Header.PrivilegeLevel[] PrivilegeLevels0 =
        {
            Header.PrivilegeLevel.NoPrivileges,
            Header.PrivilegeLevel.PowerUserPrivileges,
            Header.PrivilegeLevel.AdminPriviliges
        };

        // post-5.3.7
        public static Header.PrivilegeLevel[] PrivilegeLevels1 =
        {
            Header.PrivilegeLevel.NoPrivileges,
            Header.PrivilegeLevel.PowerUserPrivileges,
            Header.PrivilegeLevel.AdminPriviliges,
            Header.PrivilegeLevel.LowestPrivileges
        };

        public static Header.LanguageDetectionMethod[] LanguageDetectionMethods =
        {
            Header.LanguageDetectionMethod.UILanguage,
            Header.LanguageDetectionMethod.LocaleLanguage,
            Header.LanguageDetectionMethod.NoLanguageDetection
        };

        // pre-4.2.5
        public static CompressionMethod[] CompressionMethods0 =
        {
            CompressionMethod.Zlib,
            CompressionMethod.BZip2,
            CompressionMethod.LZMA1
        };

        // 4.2.5
        public static CompressionMethod[] CompressionMethods1 =
        {
            CompressionMethod.Stored,
            CompressionMethod.BZip2,
            CompressionMethod.LZMA1
        };

        // [4.2.6 5.3.9)
        public static CompressionMethod[] CompressionMethods2 =
        {
            CompressionMethod.Stored,
            CompressionMethod.Zlib,
            CompressionMethod.BZip2,
            CompressionMethod.LZMA1
        };

        // 5.3.9+
        public static CompressionMethod[] CompressionMethods3 =
        {
            CompressionMethod.Stored,
            CompressionMethod.Zlib,
            CompressionMethod.BZip2,
            CompressionMethod.LZMA1,
            CompressionMethod.LZMA2
        };

        #endregion

        #region IconEntry

        public static IconEntry.CloseSetting[] CloseSettings =
        {
            IconEntry.CloseSetting.NoSetting,
            IconEntry.CloseSetting.CloseOnExit,
            IconEntry.CloseSetting.DontCloseOnExit
        };

        #endregion

        #region IniEntry

        public static IniEntry.Flags IniFlags =
            IniEntry.Flags.CreateKeyIfDoesntExist
            | IniEntry.Flags.UninsDeleteEntry
            | IniEntry.Flags.UninsDeleteEntireSection
            | IniEntry.Flags.UninsDeleteSectionIfEmpty
            | IniEntry.Flags.HasValue;

        #endregion

        #region RegistryEntry

        // 16-bit
        public static RegistryEntry.ValueType[] RegistryEntryTypes0 =
        {
            RegistryEntry.ValueType.None,
            RegistryEntry.ValueType.String
        };

        public static RegistryEntry.ValueType[] RegistryEntryTypes1 =
        {
            RegistryEntry.ValueType.None,
            RegistryEntry.ValueType.String,
            RegistryEntry.ValueType.ExpandString,
            RegistryEntry.ValueType.DWord,
            RegistryEntry.ValueType.Binary,
            RegistryEntry.ValueType.MultiString
        };

        // Starting with version 5.2.5
        public static RegistryEntry.ValueType[] RegistryEntryTypes2 =
        {
            RegistryEntry.ValueType.None,
            RegistryEntry.ValueType.String,
            RegistryEntry.ValueType.ExpandString,
            RegistryEntry.ValueType.DWord,
            RegistryEntry.ValueType.Binary,
            RegistryEntry.ValueType.MultiString,
            RegistryEntry.ValueType.QWord
        };

        #endregion

        #region RunEntry

        public static RunEntry.WaitCondition[] RunWaitConditions =
        {
            RunEntry.WaitCondition.WaitUntilTerminated,
            RunEntry.WaitCondition.NoWait,
            RunEntry.WaitCondition.WaitUntilIdle
        };

        #endregion

        #region TypeEntry

        public static TypeEntry.TypeFlags TypeFlags =
            TypeEntry.TypeFlags.CustomSetupType;

        public static TypeEntry.SetupType[] SetupTypes =
        {
            TypeEntry.SetupType.User,
            TypeEntry.SetupType.DefaultFull,
            TypeEntry.SetupType.DefaultCompact,
            TypeEntry.SetupType.DefaultCustom
        };

        #endregion
    }
}
