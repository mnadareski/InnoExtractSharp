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
using System.Linq;

namespace InnoExtractSharp.Util
{
    public abstract class AnsiConsoleParser
    {
        //! Character that started the current control sequence, or \c 0
        public byte InCommand;

        //! Buffer for control sequences if they span more than one flush
        public string Command;

        protected const char ESC = '\x1b';
        protected const char CSI = '['; //!< Control Sequence Indicator (preceded by \ref ESC)
        protected const char UTF8CSI0 = '\xc2'; //!< UTF-8 Control Sequence Indicator, first byte
        protected const char UTF8CSI1 = '\x9b'; //!< UTF-8 Control Sequence Indicator, second byte
        protected const char Separator = ';'; //! Separator for codes in CSI control sequences

        public class CommandType
        {
            private char[] validTypes = new char[]
            {
                'A', //!< Cursor Up
                'B', //!< Cursor Down
                'C', //!< Cursor Forward
                'D', //!< Cursor Back
                'E', //!< Cursor Next Line
                'F', //!< Cursor Previous Line
                'G', //!< Cursor Horizontal Absolute
                'H', //!< Cursor Position
                'J', //!< Erase Display
                'K', //!< Erase in Line
                'S', //!< Scroll Up
                'T', //!< Scroll Down
                'f', //!< Horizontal and Vertical Position
                'm', //!< Select Graphic Rendition
                'n', //!< Device Status Report
                's', //!< Save Cursor Position
                'u', //!< Restore Cursor Position
            };

            private string[] validTypeNames = new string[]
            {
                "CUU", //!< Cursor Up
                "CUD", //!< Cursor Down
                "CUF", //!< Cursor Forward
                "CUB", //!< Cursor Back
                "CNL", //!< Cursor Next Line
                "CPL", //!< Cursor Previous Line
                "CHA", //!< Cursor Horizontal Absolute
                "CUP", //!< Cursor Position
                "ED", //!< Erase Display
                "EL", //!< Erase in Line
                "SU", //!< Scroll Up
                "SD", //!< Scroll Down
                "HVP", //!< Horizontal and Vertical Position
                "SGR", //!< Select Graphic Rendition
                "DSR", //!< Device Status Report
                "SCP", //!< Save Cursor Position
                "RCP", //!< Restore Cursor Position
            };

            private char currentType;

            public CommandType(char cmd)
            {
                currentType = cmd;
            }

            public bool IsValid()
            {
                return currentType != default(char) && validTypes.Contains(currentType);
            }

            public string GetCommandString()
            {
                if (currentType != default(char) && validTypes.Contains(currentType))
                {
                    int index = validTypes.ToList().IndexOf(currentType);
                    return validTypeNames[index];
                }

                return null;
            }
        }

        /// <summary>
        /// Read one code form a command sequence
        /// 
        /// Each command sequence contains contains at least one code. Once there are no more
        /// commands in the command sequence, \c s will be set to \c NULL. After that has
        /// happened \ref read_code() should must not be called with the (\c s, \c end) pair.
        /// 
        /// The meaning of th returned code depends on the type of the command sequence.
        /// </summary>
        /// <param name="s">Command sequence</param>
        /// <param name="sPtr">Current position in the command sequence.</param>
        /// <param name="end">End of the command sequence.</param>
        /// <returns>the next code in the command sequence or unsigned(-1) if there was an error.</returns>
        protected ulong ReadCode(byte[] s, ref int sPtr, int end)
        {
            int sep = Array.IndexOf(s, (byte)Separator, sPtr, end - sPtr);
            ulong code = UInt64.MaxValue;
            switch (sep - sPtr)
            {
                case 0:
                    code = 0u;
                    break;
                case 1:
                    code = (byte)s[sPtr];
                    break;
                case 2:
                    code = BitConverter.ToUInt16(s, sPtr);
                    break;
                case 4:
                    code = BitConverter.ToUInt32(s, sPtr);
                    break;
                case 8:
                    code = BitConverter.ToUInt64(s, sPtr);
                    break;
            }

            if (sep == end)
                sPtr = -1;
            else
                sPtr = sep + 1;

            return code;
        }

        private bool IsStartChar(char c)
        {
            return (c == ESC /* escape */ || c == UTF8CSI0 /* first byte of UTF-8 CSI */);
        }

        private bool IsEndChar(char c)
        {
            return (c >= 64 && c < 127);
        }

        private int ReadCommand(byte[] s, ref int sPtr, int end, out byte[] code)
        {
            code = s;

            if (sPtr == end)
                return end; // Need to be able to read something

            if (Command.Length == 0 && s[sPtr] != (InCommand == ESC ? (byte)CSI : (byte)UTF8CSI1))
            {
                switch ((char)InCommand)
                {
                    case ESC: /* escaped char */ break;
                    default:
                        // char utf8[] = { in_command, *s };
                        List<byte> utf8List = new List<byte> { InCommand };
                        utf8List.AddRange(s.Skip(sPtr));
                        byte[] utf8 = utf8List.ToArray();

                        HandleText(utf8, 0, 2);
                        break;
                }

                return sPtr + 1; // Not a Control Sequence Initiator
            }

            int searchIndex = (string.IsNullOrEmpty(Command) ? sPtr + 1 : sPtr);
            int cmd = s.Take(end).Skip(searchIndex).First(b => IsEndChar((char)b));

            int csPtr = sPtr;
            int cePtr = cmd;

            if (!string.IsNullOrEmpty(Command) || cmd == end)
            {
                Command += new string(s.Take(cmd).Skip(sPtr).Select(b => (char)b).ToArray());
                code = Command.Select(c => (byte)c).ToArray();
                csPtr = 0;
                cePtr = csPtr + Command.Length;
            }

            if (cmd == end)
                return end; // Command not over yet

            // Extract the command type
            CommandType type = new CommandType((char)s[cmd]);

            // Skip starting character (part of the CSI sequence)
            csPtr++;

            HandleCommand(type, code, csPtr, cePtr);

            InCommand = 0;
            Command = string.Empty;

            return cmd + 1;
        }

        public AnsiConsoleParser()
        {
            InCommand = 0;
        }

        /// <summary>
        /// Will be called when an ANSI escape sequence has been found
        /// 
        /// Derived classes must override this.
        /// </summary>
        /// <param name="type">The type of command. This is the last character of the escape sequence.</param>
        /// <param name="codes">Code sequence</param>
        /// <param name="codesPtr">Start of the code sequence. Use \ref read_code() to read codes.</param>
        /// <param name="endPtr">End of the code sequence.</param>
        public abstract void HandleCommand(CommandType type, byte[] codes, int codesPtr, int endPtr);

        /// <summary>
        /// Will be called when plain text has been found
        /// 
        /// Derived classes must override this.
        /// </summary>
        /// <param name="s">Pointer to the text.</param>
        /// <param name="n">length of the text in bytes.</param>
        public abstract void HandleText(byte[] s, int sPtr, int n);

        /// <summary>
        /// Parse \c n characters from \c s
        /// 
        /// The string may contain multiple escape sequences and escape sequences may span
        /// multiple calls to write().
        /// 
        /// All escape sequences are passed to \ref handle_command() while plain text segments
        /// are passed to \ref handle_text().
        /// </summary>
        public int Write(byte[] s, int sPtr, int n)
        {
            int begin = sPtr;
            int end = sPtr + n;

            byte[] code = s;
            if (InCommand != (char)0)
                sPtr = ReadCommand(s, ref sPtr, end, out code);

            while (sPtr != end)
            {
                int cmd = code.Take(end).First(b => IsStartChar((char)b));

                // Output the non-escaped text
                HandleText(code, sPtr, cmd - sPtr);

                if (cmd == end)
                {
                    sPtr = end;
                    break;
                }

                // A command possibly starts here
                InCommand = code[cmd];
                cmd++;
                sPtr = ReadCommand(code, ref cmd, end, out code);
            }

            return sPtr - begin;
        }
    }
}
