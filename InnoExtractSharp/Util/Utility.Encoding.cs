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
// Parts based on:
////////////////////////////////////////////////////////////
//
// SFML - Simple and Fast Multimedia Library
// Copyright (C) 2007-2009 Laurent Gomila (laurent.gom@gmail.com)
//
// This software is provided 'as-is', without any express or implied warranty.
// In no event will the authors be held liable for any damages arising from the
// use of this software.
//
// Permission is granted to anyone to use this software for any purpose,
// including commercial applications, and to alter it and redistribute it freely,
// subject to the following restrictions:
//
// 1. The origin of this software must not be misrepresented;
//    you must not claim that you wrote the original software.
//    If you use this software in a product, an acknowledgment
//    in the product documentation would be appreciated but is not required.
//
// 2. Altered source versions must be plainly marked as such,
//    and must not be misrepresented as being the original software.
//
// 3. This notice may not be removed or altered from any source distribution.
//
////////////////////////////////////////////////////////////
//
// This code has been taken from SFML and altered to fit the project's needs.
//
////////////////////////////////////////////////////////////

using System.Text;

namespace InnoExtractSharp.Util
{
    public enum KnownCodepage : uint
    {
        cp_dos708 = 708, // arabic
        cp_windows874 = 874, // thai
        cp_shift_jis = 932, // japanese
        cp_gbk = 936, // chinese
        cp_uhc = 949, // korean
        cp_big5 = 950, // chinese
        cp_big5_hkscs = 951, // chinese
        cp_utf16le = 1200,
        cp_utf16be = 1201,
        cp_windows1250 = 1250, // latin
        cp_windows1251 = 1251, // cyrillic
        cp_windows1252 = 1252, // latin
        cp_windows1253 = 1253, // greek
        cp_windows1254 = 1254, // turkish
        cp_windows1255 = 1255, // hebrew
        cp_windows1256 = 1256, // arabic
        cp_windows1257 = 1257, // baltic
        cp_windows1258 = 1258, // vietnamese
        cp_windows1270 = 1270, // sami
        cp_johab = 1361, // korean
        cp_macroman = 10000, // latin
        cp_macjapanese = 10001, // japanese
        cp_macchinese1 = 10002, // chinese
        cp_mackorean = 10003, // korean
        cp_macarabic = 10004, // arabic
        cp_machebrew = 10005, // hebrew
        cp_macgreek = 10006, // greek
        cp_maccyrillic = 10007, // cyrillic
        cp_macchinese2 = 10008, // chinese
        cp_macromania = 10010, // latin
        cp_macukraine = 10017, // cyrillic
        cp_macthai = 10021, // thai
        cp_macroman2 = 10029, // latin
        cp_maciceland = 10079, // latin
        cp_macturkish = 10081, // turkish
        cp_maccroatian = 10082, // latin
        cp_utf32le = 12000,
        cp_utf32be = 12001,
        cp_cns = 20000, // chinese
        cp_big5_eten = 20002, // chinese
        cp_ia5 = 20105, // latin
        cp_ia5_de = 20106, // latin
        cp_ia5_se2 = 20107, // latin
        cp_ia5_no2 = 20108, // latin
        cp_ascii = 20127, // latin
        cp_t61 = 20261, // latin
        cp_iso_6937 = 20269, // latin
        cp_ibm273 = 20273, // latin
        cp_ibm277 = 20277, // latin
        cp_ibm278 = 20278, // latin
        cp_ibm280 = 20280, // latin
        cp_ibm284 = 20284, // latin
        cp_ibm285 = 20285, // latin
        cp_ibm290 = 20290, // japanese
        cp_ibm297 = 20297, // latin
        cp_ibm420 = 20420, // arabic
        cp_ibm423 = 20423, // greek
        cp_ibm424 = 20424, // hebrew
        cp_ibm833 = 20833, // korean
        cp_ibm838 = 20838, // thai
        cp_koi8_r = 20866, // cyrillic
        cp_ibm871 = 20871, // latin
        cp_ibm880 = 20880, // cyrillic
        cp_ibm905 = 20905, // turkish
        cp_ibm924 = 20924, // latin
        cp_euc_jp_ms = 20932, // japanese
        cp_gb2312_80 = 20936, // chinese
        cp_wansung = 20949, // korean
        cp_ibm1025 = 21025, // cyrillic
        cp_koi8_u = 21866, // cyrillic
        cp_iso_8859_1 = 28591, // latin
        cp_iso_8859_2 = 28592, // latin
        cp_iso_8859_3 = 28593, // latin
        cp_iso_8859_4 = 28594, // latin
        cp_iso_8859_5 = 28595, // cyrillic
        cp_iso_8859_6 = 28596, // arabic
        cp_iso_8859_7 = 28597, // greek
        cp_iso_8859_8 = 28598, // hebrew
        cp_iso_8859_9 = 28599, // turkish
        cp_iso_8859_10 = 28600, // latin
        cp_iso_8859_11 = 28601, // thai
        cp_iso_8859_13 = 28603, // baltic
        cp_iso_8859_14 = 28604, // celtic
        cp_iso_8859_15 = 28605, // latin
        cp_europa3 = 29001, // latin
        cp_iso_8859_6i = 38596, // hebrew
        cp_iso_8859_8i = 38598, // hebrew
        cp_iso_2022_jp = 50220, // japanese
        cp_iso_2022_jp2 = 50221, // japanese
        cp_iso_2022_jp3 = 50222, // japanese
        cp_iso_2022_kr = 50225, // korean
        cp_iso_2022_cn = 50227, // chinese
        cp_iso_2022_cn2 = 50229, // chinese
        cp_ibm930 = 50930, // japanese
        cp_ibm931 = 50931, // japanese
        cp_ibm933 = 50933, // korean
        cp_ibm935 = 50935, // chinese
        cp_ibm936 = 50936, // chinese
        cp_ibm937 = 50937, // chinese
        cp_ibm939 = 50939, // japanese
        cp_euc_jp = 51932, // japanese
        cp_euc_cn = 51936, // chinese
        cp_euc_kr = 51949, // korean
        cp_euc_tw = 51950, // chinese
        cp_gb2312_hz = 52936, // chinese
        cp_gb18030 = 54936, // chinese
        cp_utf7 = 65000,
        cp_utf8 = 65001,
    }

    public partial class Utility
    {
        /// <summary>
        /// Convert a string in place to UTF-8 from a specified encoding.
        /// </summary>
        /// <param name="data">The input string to convert.</param>
        /// <param name="codepage">The Windows codepage number for the input string encoding.</param>
        /// <remarks>This function is not thread-safe.</remarks>
        public static string ToUtf8(string data, KnownCodepage codepage = KnownCodepage.cp_windows1252)
        {
            Encoding srcEncoding = Encoding.GetEncoding((int)codepage);
            byte[] idBytes = srcEncoding.GetBytes(data);
            idBytes = Encoding.Convert(srcEncoding, Encoding.UTF8, idBytes);
            return Encoding.UTF8.GetString(idBytes);
        }

        /// <summary>
        /// Convert a string from UTF-8 to a specified encoding.
        /// </summary>
        /// <param name="data">The input string to convert.</param>
        /// <param name="codepage">The Windows codepage number for the input string encoding.</param>
        /// <remarks>This function is not thread-safe.</remarks>
        public static string FromUtf8(string data, KnownCodepage codepage = KnownCodepage.cp_windows1252)
        {
            Encoding destEncoding = Encoding.GetEncoding((int)codepage);
            byte[] idBytes = Encoding.UTF8.GetBytes(data);
            idBytes = Encoding.Convert(Encoding.UTF8, destEncoding, idBytes);
            return destEncoding.GetString(idBytes);
        }

        /// <summary>
        /// Get names for encodings where iconv doesn't have the codepage alias
        /// </summary>
        internal static string EncodingName(KnownCodepage codepage)
        {
            switch (codepage)
            {
                case KnownCodepage.cp_ascii: return "US-ASCII";
                case KnownCodepage.cp_big5: return "BIG5";
                case KnownCodepage.cp_big5_eten: return "BIG5";
                case KnownCodepage.cp_big5_hkscs: return "BIG5-HKSCS";
                case KnownCodepage.cp_cns: return "EUC-TW";
                case KnownCodepage.cp_dos708: return "ISO-8859-6";
                case KnownCodepage.cp_euc_cn: return "EUC-CN";
                case KnownCodepage.cp_euc_jp: return "EUC-JP";
                case KnownCodepage.cp_euc_jp_ms: return "EUC-JP-MS";
                case KnownCodepage.cp_euc_kr: return "EUC-KR";
                case KnownCodepage.cp_euc_tw: return "EUC-TW";
                case KnownCodepage.cp_gb2312_80: return "GB2312";
                case KnownCodepage.cp_gb2312_hz: return "GB2312";
                case KnownCodepage.cp_gb18030: return "GB18030";
                case KnownCodepage.cp_gbk: return "GBK";
                case KnownCodepage.cp_ia5: return "ISO_646.IRV:1991";
                case KnownCodepage.cp_ia5_de: return "ISO646-DE";
                case KnownCodepage.cp_ia5_no2: return "ISO646-NO2";
                case KnownCodepage.cp_ia5_se2: return "ISO646-SE2";
                case KnownCodepage.cp_ibm273: return "IBM273";
                case KnownCodepage.cp_ibm277: return "IBM277";
                case KnownCodepage.cp_ibm278: return "IBM278";
                case KnownCodepage.cp_ibm280: return "IBM280";
                case KnownCodepage.cp_ibm284: return "IBM284";
                case KnownCodepage.cp_ibm285: return "IBM285";
                case KnownCodepage.cp_ibm290: return "IBM290";
                case KnownCodepage.cp_ibm297: return "IBM297";
                case KnownCodepage.cp_ibm420: return "IBM420";
                case KnownCodepage.cp_ibm423: return "IBM423";
                case KnownCodepage.cp_ibm424: return "IBM424";
                case KnownCodepage.cp_ibm833: return "IBM833";
                case KnownCodepage.cp_ibm838: return "IBM1160";
                case KnownCodepage.cp_ibm871: return "IBM871";
                case KnownCodepage.cp_ibm880: return "IBM880";
                case KnownCodepage.cp_ibm905: return "IBM905";
                case KnownCodepage.cp_ibm924: return "IBM1047";
                case KnownCodepage.cp_ibm930: return "IBM930";
                case KnownCodepage.cp_ibm931: return "IBM931";
                case KnownCodepage.cp_ibm933: return "IBM933";
                case KnownCodepage.cp_ibm935: return "IBM935";
                case KnownCodepage.cp_ibm936: return "IBM936";
                case KnownCodepage.cp_ibm937: return "IBM937";
                case KnownCodepage.cp_ibm939: return "IBM939";
                case KnownCodepage.cp_ibm1025: return "IBM1025";
                case KnownCodepage.cp_iso_2022_cn: return "ISO-2022-CN";
                case KnownCodepage.cp_iso_2022_cn2: return "ISO-2022-CN-EXT";
                case KnownCodepage.cp_iso_2022_jp: return "ISO-2022-JP";
                case KnownCodepage.cp_iso_2022_jp2: return "ISO-2022-JP-2";
                case KnownCodepage.cp_iso_2022_jp3: return "ISO-2022-JP-3";
                case KnownCodepage.cp_iso_2022_kr: return "ISO-2022-KR";
                case KnownCodepage.cp_iso_6937: return "ISO_6937";
                case KnownCodepage.cp_iso_8859_10: return "ISO-8859-10";
                case KnownCodepage.cp_iso_8859_11: return "ISO-8859-11";
                case KnownCodepage.cp_iso_8859_13: return "ISO-8859-13";
                case KnownCodepage.cp_iso_8859_14: return "ISO-8859-14";
                case KnownCodepage.cp_iso_8859_15: return "ISO-8859-15";
                case KnownCodepage.cp_iso_8859_1: return "ISO-8859-1";
                case KnownCodepage.cp_iso_8859_2: return "ISO-8859-2";
                case KnownCodepage.cp_iso_8859_3: return "ISO-8859-3";
                case KnownCodepage.cp_iso_8859_4: return "ISO-8859-4";
                case KnownCodepage.cp_iso_8859_5: return "ISO-8859-5";
                case KnownCodepage.cp_iso_8859_6: return "ISO-8859-6";
                case KnownCodepage.cp_iso_8859_6i: return "ISO-8859-6";
                case KnownCodepage.cp_iso_8859_7: return "ISO-8859-7";
                case KnownCodepage.cp_iso_8859_8: return "ISO-8859-8";
                case KnownCodepage.cp_iso_8859_8i: return "ISO-8859-8";
                case KnownCodepage.cp_iso_8859_9: return "ISO-8859-9";
                case KnownCodepage.cp_johab: return "JOHAB";
                case KnownCodepage.cp_koi8_r: return "KOI8-R";
                case KnownCodepage.cp_koi8_u: return "KOI8-U";
                case KnownCodepage.cp_macarabic: return "MACARABIC";
                case KnownCodepage.cp_macchinese1: return "BIG5";
                case KnownCodepage.cp_macchinese2: return "EUC-CN";
                case KnownCodepage.cp_maccroatian: return "MACCROATIAN";
                case KnownCodepage.cp_maccyrillic: return "MACCYRILLIC";
                case KnownCodepage.cp_macgreek: return "MACGREEK";
                case KnownCodepage.cp_machebrew: return "MACHEBREW";
                case KnownCodepage.cp_maciceland: return "MACICELAND";
                case KnownCodepage.cp_macjapanese: return "SHIFT-JIS";
                case KnownCodepage.cp_mackorean: return "EUC-KR";
                case KnownCodepage.cp_macroman2: return "MACCENTRALEUROPE";
                case KnownCodepage.cp_macroman: return "MACINTOSH";
                case KnownCodepage.cp_macromania: return "MACROMANIA";
                case KnownCodepage.cp_macthai: return "MACTHAI";
                case KnownCodepage.cp_macturkish: return "MACTURKISH";
                case KnownCodepage.cp_macukraine: return "MACUKRAINE";
                case KnownCodepage.cp_shift_jis: return "SHIFT-JIS";
                case KnownCodepage.cp_t61: return "T.61";
                case KnownCodepage.cp_uhc: return "UHC";
                case KnownCodepage.cp_utf7: return "UTF-7";
                case KnownCodepage.cp_utf8: return "UTF-8";
                case KnownCodepage.cp_utf16be: return "UTF-16BE";
                case KnownCodepage.cp_utf16le: return "UTF-16LE"; // "UTF-16" is platform-dependent without a BOM
                case KnownCodepage.cp_utf32be: return "UTF-32BE";
                case KnownCodepage.cp_utf32le: return "UTF-32LE";
                case KnownCodepage.cp_wansung: return "EUC-KR";
                case KnownCodepage.cp_windows1250: return "MS-EE";
                case KnownCodepage.cp_windows1251: return "MS-CYRL";
                case KnownCodepage.cp_windows1252: return "MS-ANSI";
                case KnownCodepage.cp_windows1253: return "MS-GREEK";
                case KnownCodepage.cp_windows1254: return "MS-TURK";
                case KnownCodepage.cp_windows1255: return "MS-HEBR";
                case KnownCodepage.cp_windows1256: return "MS-ARAB";
                default: return null;
            }
        }
    }
}
