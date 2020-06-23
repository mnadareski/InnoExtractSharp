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

using System;
using System.IO;
using InnoExtractSharp.Crypto;
using InnoExtractSharp.Util;

namespace InnoExtractSharp.CLI
{
    public class FileOutput
    {
        private string path_;
        private ProcessedFile file_;
        private FStream stream_;

        private Hasher checksum_;
        private ulong checksumPosition_;

        private ulong position_;
        private ulong totalWritten_;

        private bool write_;

        public FileOutput(string dir, ProcessedFile f, bool write)
        {
            path_ = Path.Combine(dir, f.Path());
            file_ = f;
            checksum_ = new Hasher(f.Entry().Checksum.Type);
            checksumPosition_ = f.Entry().Checksum.Type == ChecksumType.None ? UInt64.MaxValue : 0;
            position_ = 0;
            totalWritten_ = 0;
            write_ = write;

            if (write_)
            {
                try
                {
                    stream_.Open(path_);
                    if (!stream_.IsOpen())
                        throw new Exception();
                }
                catch
                {
                    throw new ArgumentException($"Could not open output file \"{path_}\"");
                }
            }
        }
   
        public bool Write(byte[] data, int n)
        {
            if (write_)
                stream_.Write(data, n);

            if (checksumPosition_ == position_)
            {
                checksum_.Update(data, 0, n);
                checksumPosition_ += (ulong)n;
            }

            position_ += (ulong)n;
            totalWritten_ += (ulong)n;

            return !write_ || !stream_.Fail();
        }

        public void Seek(ulong newPosition)
        {
            if (newPosition == position_)
                return;

            if (!write_)
            {
                position_ = newPosition;
                return;
            }

            ulong max = UInt32.MaxValue / 4;

            if (newPosition <= max)
            {
                stream_.Seek(newPosition, SeekOrigin.Begin);
            }
            else
            {
                Fstream.OffType sign = (newPosition > position_) ? 1 : -1;
                ulong diff = (newPosition > position_) ? newPosition - position_ : position_ - newPosition;
                while (diff > 0)
                {
                    stream_.SeekP(sign * (Fstream.OffType)(Math.Min(diff, max)), SeekOrigin.Current);
                    diff -= Math.Min(diff, max);
                }
            }

            position_ = newPosition;
        }

        public void Close()
        {
            if (write_)
                stream_.Close();
        }

        public string Path() { return path_; }

        public ProcessedFile File() { return file_; }

        public bool IsComplete()
        {
            return totalWritten_ == file_.Entry().Size;
        }

        public bool HasChecksum()
        {
            return checksumPosition_ == file_.Entry().Size;
        }

        public bool CalculateChecksum()
        {
            if (HasChecksum())
                return true;

            if (!write_)
                return false;

            ulong max = UInt32.MaxValue / 4;
            ulong diff = checksumPosition_;
            stream_.SeekG((Fstream.OffType)Math.Min(diff, max), SeekOrigin.Begin);
            diff -= Math.Min(diff, max);
            while (diff > 0)
            {
                stream_.SeekG((Fstream.OffType)Math.Min(diff, max), SeekOrigin.Current);
                diff -= Math.Min(diff, max);
            }

            while (!stream_.EOF())
            {
                byte[] buffer = new byte[8192];
                int n = stream_.Read(buffer, 0, buffer.Length);
                checksum_.Update(buffer, 0, n);
                checksumPosition_ += (ulong)n;
            }

            if (!HasChecksum())
            {
                Console.WriteLine($"Could not read back {path_} to calculate output checksum for multi-part file");
                return false;
            }

            return true;
        }

        public Checksum Checksum()
        {
            return checksum_.Finalize();
        }
    }
}
