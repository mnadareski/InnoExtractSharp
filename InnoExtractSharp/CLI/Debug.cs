using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using InnoExtractSharp.Loader;
using InnoExtractSharp.Setup;

namespace InnoExtractSharp.CLI
{
    public class Debug
    {
        public void PrintOffsets(Offsets offsets)
        {
            Console.WriteLine("Loaded offsets:");
            if (offsets.ExeOffset != 0)
            {
                Console.Write($"- exe: @ {offsets.ExeOffset:x}");
                if (offsets.ExeCompressedSize != 0)
                    Console.Write($"  compressed: {offsets.ExeCompressedSize:x}");

                Console.Write($"  uncompressed: {offsets.ExeUncompressedSize:x}");
                Console.Write($"  checksum: {offsets.ExeChecksum}");
                Console.WriteLine();
            }

            if (offsets.MessageOffset != 0)
                Console.WriteLine($"- message offset: {offsets.MessageOffset:x}");

            Console.WriteLine($"- header offset: {offsets.HeaderOffset:x}");
            Console.WriteLine($"- data offset: {offsets.DataOffset:x}");
        }

        public static void Print(Stream os, WindowsVersionRange winver, Header header)
        {
            var def = header.Winver;

            using (StreamWriter writer = new StreamWriter(os, Encoding.Default, 1024 * 1024, true))
            {
                if (winver.Begin != def.Begin)
                    writer.WriteLine($"  Min version: ");  // os << if_not_equal("  Min version", winver.begin, def.begin);

                if (winver.End != def.End)
                    writer.WriteLine($"  Only below version: "); // os << if_not_equal("  Only below version", winver.end, def.end);
            }
        }
    
        public static void Print(Stream os, Item item, Header header)
        {
            using (StreamWriter writer = new StreamWriter(os, Encoding.Default, 1024 * 1024, true))
            {
                if (!string.IsNullOrEmpty(item.Components))
                    writer.WriteLine($"  Components: {item.Components}");
                if (!string.IsNullOrEmpty(item.Tasks))
                    writer.WriteLine($"  Tasks: {item.Tasks}");
                if (!string.IsNullOrEmpty(item.Languages))
                    writer.WriteLine($"  Languages: {item.Languages}");
                if (!string.IsNullOrEmpty(item.Check))
                    writer.WriteLine($"  Check: {item.Check}");
                if (!string.IsNullOrEmpty(item.Check))
                    writer.WriteLine($"  After install: {item.AfterInstall}");
                if (!string.IsNullOrEmpty(item.Check))
                    writer.WriteLine($"  Before install: {item.BeforeInstall}");

                Print(os, item.Winver, header);
            }
        }
    
        public static void PrintEntry(Info info, long i, LanguageEntry entry)
        {
            Console.WriteLine($" - \"{entry.Name}\":");
            if (!string.IsNullOrEmpty(entry.LanguageName))
                Console.WriteLine($"  Language name: {entry.LanguageName}");
            if (!string.IsNullOrEmpty(entry.DialogFont))
                Console.WriteLine($"  Dialog font: {entry.DialogFont}");
            if (!string.IsNullOrEmpty(entry.TitleFont))
                Console.WriteLine($"  Title font: {entry.TitleFont}");
            if (!string.IsNullOrEmpty(entry.WelcomeFont))
                Console.WriteLine($"  Welcome font: {entry.WelcomeFont}");
            if (!string.IsNullOrEmpty(entry.CopyrightFont))
                Console.WriteLine($"  Copyright font: {entry.CopyrightFont}");
            if (!string.IsNullOrEmpty(entry.Data))
                Console.WriteLine($"  Data: {entry.Data}");
            if (!string.IsNullOrEmpty(entry.LicenseText))
                Console.WriteLine($"  License: {entry.LicenseText}");
            if (!string.IsNullOrEmpty(entry.InfoBefore))
                Console.WriteLine($"  Info before text: {entry.InfoBefore}");
            if (!string.IsNullOrEmpty(entry.InfoAfter))
                Console.WriteLine($"  Info after text: {entry.InfoAfter}");

            Console.WriteLine($"  Language id: {entry.LanguageId:x}");

            if (entry.Codepage != 0)
                Console.WriteLine($"  Codepage: {entry.Codepage}");
            if (entry.DialogFontSize != 0)
                Console.WriteLine($"  Dialog font size: {entry.DialogFontSize}");
            if (entry.DialogFontStandardHeight != 0)
                Console.WriteLine($"  Dialog font standard height: {entry.DialogFontStandardHeight}");
            if (entry.TitleFontSize != 0)
                Console.WriteLine($"  Title font size: {entry.TitleFontSize}");
            if (entry.WelcomeFontSize != 0)
                Console.WriteLine($"  Welcome font size: {entry.WelcomeFontSize}");
            if (entry.CopyrightFontSize != 0)
                Console.WriteLine($"  Copyright font size: {entry.CopyrightFontSize}");
            if (entry.RightToLeft)
                Console.WriteLine($"  Right to left: {entry.RightToLeft}");
        }
    
        public static void PrintEntry(Info info, long i, MessageEntry entry)
        {
            Console.Write($" - \"{entry.Name}\"");
            if (entry.Language < 0)
                Console.Write(" (default) = ");
            else
                Console.Write($" ({(info.Languages[entry.Language] as LanguageEntry).Name}) = ");

            Console.WriteLine();
        }
    
        public static void PrintEntry(Info info, long i, PermissionEntry entry)
        {
            Console.WriteLine($" - {entry.Permissions.Length}");
        }

        public static void PrintEntry(Info info, long i, TypeEntry entry)
        {
            Console.WriteLine($" - \"{entry.Name}\":");
            if (!string.IsNullOrEmpty(entry.Description))
                Console.WriteLine($"  Description: {entry.Description}");
            if (!string.IsNullOrEmpty(entry.Languages))
                Console.WriteLine($"  Languages: {entry.Languages}");
            if (!string.IsNullOrEmpty(entry.Check))
                Console.WriteLine($"  Check: {entry.Check}");

            Print(Console.OpenStandardOutput(), entry.Winver, info.Header);

            if (entry.CustomType)
                Console.WriteLine($"  Custom setup type: {entry.CustomType}");
            if (entry.Type != TypeEntry.SetupType.User)
                Console.WriteLine($"  Type: {entry.Type}");
            if (entry.Size != 0)
                Console.WriteLine($"  Size: {entry.Size}");
        }
    
        public static void PrintEntry(Info info, long i, ComponentEntry entry)
        {
            Console.WriteLine($" - \"{entry.Name}\":");
            if (!string.IsNullOrEmpty(entry.Types))
                Console.WriteLine($"  Types: {entry.Types}");
            if (!string.IsNullOrEmpty(entry.Description))
                Console.WriteLine($"  Description: {entry.Description}");
            if (!string.IsNullOrEmpty(entry.Languages))
                Console.WriteLine($"  Languages: {entry.Languages}");
            if (!string.IsNullOrEmpty(entry.Check))
                Console.WriteLine($"  Check: {entry.Check}");
            if (entry.ExtraDiskSpaceRequired != 0)
                Console.WriteLine($"  Extra disk space required: {entry.ExtraDiskSpaceRequired}");
            if (entry.Level != 0)
                Console.WriteLine($"  Level: {entry.Level}");
            if (!entry.Used)
                Console.WriteLine($"  Used: {entry.Used}");

            Print(Console.OpenStandardOutput(), entry.Winver, info.Header);

            if (entry.Options != 0)
                Console.WriteLine($"  Options: {entry.Options}");
            if (entry.Size != 0)
                Console.WriteLine($"  Size: {entry.Size}");
        }
    
        public static void PrintEntry(Info info, long i, TaskEntry entry)
        {
            Console.WriteLine($" - \"{entry.Name}\":");
            if (!string.IsNullOrEmpty(entry.Description))
                Console.WriteLine($"  Description: {entry.Description}");
            if (!string.IsNullOrEmpty(entry.GroupDescription))
                Console.WriteLine($"  Group description: {entry.GroupDescription}");
            if (!string.IsNullOrEmpty(entry.Components))
                Console.WriteLine($"  Components: {entry.Components}");
            if (!string.IsNullOrEmpty(entry.Languages))
                Console.WriteLine($"  Languages: {entry.Languages}");
            if (!string.IsNullOrEmpty(entry.Check))
                Console.WriteLine($"  Check: {entry.Check}");
            if (entry.Level != 0)
                Console.WriteLine($"  Level: {entry.Level}");
            if (!entry.Used)
                Console.WriteLine($"  Used: {entry.Used}");

            Print(Console.OpenStandardOutput(), entry.WinVer, info.Header);

            if (entry.Options != 0)
                Console.WriteLine($"  Types: {entry.Options}");
        }
    
        public static void PrintEntry(Info info, long i, DirectoryEntry entry)
        {

        }
    }
}
