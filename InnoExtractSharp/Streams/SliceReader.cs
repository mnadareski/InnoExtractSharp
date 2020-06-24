/*
 * Copyright (C) 2011-2019 Daniel Scharrer
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

namespace InnoExtractSharp.Streams
{
    /// <summary>
    /// Abstraction for reading either data embedded inside the setup executable or from
    /// multiple external slices.
    /// 
    /// Setup data contained in the executable is located by a non-zeore
    /// \ref loader::offsets::data_offset.
    /// 
    /// The contained data is made up of one or more \ref chunk "chunks"
    /// (read by \ref chunk_reader), which in turn contain one or more  \ref file "files"
    /// (read by \ref file_reader).
    /// </summary>
    public class SliceReader : Stream
    {
        // Information for reading embedded setup data
        private uint dataOffset;

        // Information for eading external setup data
        private string dir;          //!< Slice directory specified at construction.
        private string baseFile;     //!< Base file name for slices.
        private string baseFile2;    //!< Fallback base filename for slices.
        private int slicesPerDisk;   //!< Number of slices grouped into each disk (for names).

        // Information about the current slice
        private int currentSlice;    //!< Number of the currently opened slice.
        private uint sliceSize;      //!< Size in bytes of the currently opened slice.

        // Streams
        private FileStream ifs;      //!< File input stream used when reading from external slices.
        private Stream input;        //!< Input stream to read from.

        private static string[] sliceIds =
        {
            "idska16" + (char)0x1a,
            "idska32" + (char)0x1a,
        };

        /// <summary>
        /// Construct a \ref slice_reader to read from data inside the setup file.
        /// Seeking to anything except the zeroeth slice is not allowed.
        /// 
        /// The constructed reader will allow reading the byte range [data_offset, file end)
        /// from the setup executable and provide this as the range [0, file end - data_offset).
        /// </summary>
        /// <param name="istream">
        /// A seekable input stream for the setup executable.
        /// The initial read position of the stream is ignored.
        /// </param>
        /// <param name="offset">
        /// The offset within the given stream where the setup data starts.
        /// This offset is given by \ref loader::offsets::data_offset.
        /// </param>
        public SliceReader(Stream istream, uint offset)
        {
            dataOffset = offset;
            slicesPerDisk = 1;
            currentSlice = 0;
            sliceSize = 0;
            input = istream;

            long fileSize = istream.Length;

            sliceSize = (uint)Math.Min(fileSize, Int32.MaxValue);
            if (dataOffset > fileSize)
                throw new SliceError("could not seek to data");
        }

        /// <summary>
        /// Construct a \ref slice_reader to read from external data slices (aka disks).
        /// 
        /// Slice files must be located at \c $dir/$base_file-$disk.bin
        /// or \c $dir/$base_file-$disk$sliceletter.bin if \c slices_per_disk is greater
        /// than \c 1.
        /// 
        /// The disk number is given by \code slice / slices_per_disk + 1 \endcode while
        /// the sliceletter is the ASCII char \code 'a' + (slice % slices_per_disk) \endcode.
        /// </summary>
        /// <param name="dirname">The directory containing the slice files.</param>
        /// <param name="basename">The base name for slice files.</param>
        /// <param name="basename2">Alternative base name for slice files.</param>
        /// <param name="diskSliceCount">How many slices are grouped into one disk. Must not be \c 0.</param>
        public SliceReader(string dirname, string basename, string basename2, int diskSliceCount)
        {
            dataOffset = 0;
            dir = dirname;
            baseFile = basename;
            baseFile2 = basename2;
            slicesPerDisk = diskSliceCount;
            currentSlice = 0;
            sliceSize = 0;
            input = ifs;
        }

        public void Seek(int slice)
        {
            if (slice == currentSlice && IsOpen())
                return;

            if (dataOffset != 0)
                throw new SliceError("cannot change slices in single-file setup");

            Open(slice);
        }

        public bool OpenFile(string file)
        {
            if (!File.Exists(file))
                return false;

            Console.WriteLine($"Opening \"{file}\"");

            try
            {
                ifs?.Close();

                ifs = File.OpenRead(file);
                if (ifs == null)
                    return false;

                long fileSize = ifs.Length;
                ifs.Seek(0, SeekOrigin.Begin);

                byte[] magic = new byte[8];
                int magicRead = ifs.Read(magic, 0, 8);
                if (magicRead < 8)
                {
                    ifs.Close();
                    throw new SliceError($"could not read slice magic number in \"{file}\"");
                }

                string magicString = new string(magic.Select(b => (char)b).ToArray());

                bool found = false;
                for (int i = 0; i < sliceIds.Length; i++)
                {
                    if (string.Equals(magicString, sliceIds[i]))
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    ifs.Close();
                    throw new SliceError($"bad slice magic number in \"{file}\"");
                }

                sliceSize = Endianness<uint>.LoadLittleEndian(ifs);
                if (sliceSize == 0)
                {
                    ifs.Close();
                    throw new SliceError($"could not read slice size in \"{file}\"");
                }
                else if (sliceSize > fileSize)
                {
                    ifs.Close();
                    throw new SliceError($"bad slice size in {file}: {sliceSize} > {fileSize}");
                }
                else if (sliceSize < ifs.Position)
                {
                    ifs.Close();
                    throw new SliceError($"bad slice size in {file}: {sliceSize} < {ifs.Position}");
                }

                return true;
            }
            catch
            {
                ifs.Close();
                throw new SliceError("Error reading from slice");
            }
        }

        public static string SliceFilename(string basename, int slice, int slicesPerDisk = 1)
        {
            string oss = $"{basename}-";

            if (slicesPerDisk == 0)
                throw new SliceError("slices per disk must not be zero");

            if (slicesPerDisk == 1)
            {
                oss += (slice + 1);
            }
            else
            {
                int major = (slice / slicesPerDisk) + 1;
                int minor = slice % slicesPerDisk;
                oss += $"{major}{(byte)0xa}{minor}";
            }

            oss += ".bin";
            return oss;
        }

        public bool OpenFileCaseInsensitive(string dirname, string filename)
        {
            if (!Directory.Exists(dirname))
                throw new SliceError($"{dirname} is not a valid directory");

            foreach (string path in Directory.EnumerateFiles(dirname, "*", SearchOption.AllDirectories))
            {
                string actualFilename = Path.GetFileName(path);
                if (string.Equals(actualFilename, filename, StringComparison.OrdinalIgnoreCase) && OpenFile(Path.Combine(dirname, actualFilename)))
                    return true;
            }

            return false;
        }

        public void Open(int slice)
        {
            currentSlice = slice;
            input = ifs;
            ifs = null;

            string sliceFile = SliceFilename(baseFile, slice, slicesPerDisk);
            if (OpenFile(Path.Combine(dir, sliceFile)))
                return;

            string sliceFile2 = SliceFilename(baseFile2, slice, slicesPerDisk);
            if (!string.IsNullOrWhiteSpace(baseFile2) && sliceFile != sliceFile2 && OpenFile(Path.Combine(dir, sliceFile2)))
                return;

            if (OpenFileCaseInsensitive(dir, sliceFile))
                return;

            if (!string.IsNullOrWhiteSpace(baseFile2) && sliceFile2 != sliceFile && OpenFileCaseInsensitive(dir, sliceFile2))
                return;

            string oss = $"could not open slice {slice}: {sliceFile}";
            if (!string.IsNullOrWhiteSpace(baseFile2) && sliceFile2 != sliceFile)
                oss += $" or {sliceFile2}";

            throw new SliceError(oss);
        }

        /// <summary>
        /// Attempt to seek to an offset within a slice.
        /// </summary>
        /// <param name="slice">The slice to seek to.</param>
        /// <param name="offset">The byte offset to seek to within the given slice.</param>
        /// <returns>
        /// \c false if the requested slice could not be opened, or if the requested
        /// offset is not a valid position in that slice - \c true otherwise.
        /// </returns>
        public bool Seek(int slice, uint offset)
        {
            Seek(slice);

            offset += dataOffset;

            if (offset > sliceSize)
                return false;

            if (offset >= input.Length)
                return false;

            input.Seek(offset, SeekOrigin.Begin);
            return true;
        }

        /// <summary>
        /// Read a number of bytes starting at the current slice and offset within that slice.
        /// 
        /// The current offset will be advanced by the number of bytes read. It is not an error
        /// to read past the end of the current slice (unless it is the last slice). Doing so
        /// will automatically seek to the start of the next slice and continue reading from
        /// there.
        /// </summary>
        /// <param name="buffer">Buffer to receive the bytes read.</param>
        /// <param name="bytes">Number of bytes to read.</param>
        /// <returns>
        /// The number of bytes read or \c -1 if there was an error. Unless we are at the
        /// end of the last slice, this function blocks until the number of requested
        /// bytes have been read.
        /// </returns>
        public override int Read(byte[] buffer, int offset, int bytes)
        {
            Seek(currentSlice);

            int nread = 0;
            while (bytes > 0)
            {
                long readPos = input.Position;
                if (readPos > sliceSize)
                    break;

                long remaining = sliceSize - readPos;
                if (remaining == 0)
                {
                    Seek(currentSlice + 1);
                    readPos = input.Position;
                    if (readPos > sliceSize)
                        break;

                    remaining = sliceSize - readPos;
                }

                int toread = (int)Math.Min(remaining, bytes);
                toread = Math.Min(toread, Int32.MaxValue);
                int read = input.Read(buffer, offset, toread);
                if (read == 0)
                    break;

                nread += read; offset += read; bytes -= read;
            }

            return (nread != 0 || bytes == 0) ? nread : -1;
        }

        /// <returns>the number currently opened slice.</returns>
        public int Slice()
        {
            return currentSlice;
        }

        /// <returns>true a slice is currently open.</returns>
        public bool IsOpen()
        {
            return input != ifs || ifs.CanRead;
        }

        #region Stream Overrides

        public override bool CanRead => input.CanRead;

        public override bool CanSeek => input.CanSeek;

        public override bool CanWrite => input.CanWrite;

        public override long Length => input.Length;

        public override long Position { get => input.Position; set => input.Position = value; }

        public override void Flush()
        {
            input.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return input.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            input.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            input.Write(buffer, offset, count);
        }

        #endregion
    }
}
