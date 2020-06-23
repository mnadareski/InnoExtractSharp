/*
 * Copyright (C) 2014-2020 Daniel Scharrer
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

using System.Collections.Generic;
using InnoExtractSharp.Setup;

namespace InnoExtractSharp.CLI
{
    public class ExtractOptions
    {
		public bool Quiet;
		public bool Silent;

		public bool WarnUnused; //!< Warn if there are unused files

		public bool ListSizes; //!< Show size information for files
		public bool ListChecksums; //!< Show checksum information for files

		public bool DataVersion; //!< Print the data version
		public bool DumpHeaders; //!< Dump setup headers
		public bool List; //!< List files
		public bool Test; //!< Test files (but don't extract)
		public bool Extract; //!< Extract files
		public bool ListLanguages; //!< List available languages
		public bool GogGameId; //!< Show the GOG.com game id
		public bool ShowPassword; //!< Show password check information
		public bool CheckPassword; //!< Abort if the provided password is incorrect

		public bool PreserveFileTimes; //!< Set timestamps of extracted files
		public bool LocalTimestamps; //!< Use local timezone for setting timestamps

		public bool Gog; //!< Try to extract additional archives used in GOG.com installers
		public bool GogGalaxy; //!< Try to re-assemble GOG Galaxy files

		public bool ExtractUnknown; //!< Try to extract unknown Inno Setup versions

		public bool ExtractTemp; //!< Extract temporary files
		public bool LanguageOnly; //!< Extract files not associated with any language
		public string Language; //!< Extract only files for this language
		public List<string> Include; //!< Extract only files matching these patterns

		public uint Codepage;

		public FilenameMap Filenames;
		public CollisionAction Collisions;
		public string DefaultLanguage;

		public string Password;

		public string OutputDir;

		public ExtractOptions()
		{
			Quiet = false;
			Silent = false;
			WarnUnused = false;
			ListSizes = false;
			ListChecksums = false;
			DataVersion = false;
			List = false;
			Test = false;
			Extract = false;
			ListLanguages = false;
			GogGameId = false;
			ShowPassword = false;
			CheckPassword = false;
			PreserveFileTimes = false;
			LocalTimestamps = false;
			Gog = false;
			GogGalaxy = false;
			ExtractUnknown = false;
			ExtractTemp = false;
			LanguageOnly = false;
			Collisions = CollisionAction.OverwriteCollisions;
		}
	}
}
