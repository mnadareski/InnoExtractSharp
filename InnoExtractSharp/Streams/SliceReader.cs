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

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

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
    public class SliceReader
    {
        // Information for reading embedded setup data
        public readonly uint DataOffset;

        // Information for eading external setup data
        public string Dir;          //!< Slice directory specified at construction.
        public string BaseFile;     //!< Base file name for slices.
        public string BaseFile2;    //!< Fallback base filename for slices.
        public int SlicesPerDisk;   //!< Number of slices grouped into each disk (for names).

        // Information about the current slice
        public int CurrentSlice;    //!< Number of the currently opened slice.
        public uint SliceSize;      //!< Size in bytes of the currently opened slice.

        // Streams
        public Stream Ifs;          //!< File input stream used when reading from external slices.
        public Stream Input;        //!< Input stream to read from.

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
        /// <param name="input">
        /// A seekable input stream for the setup executable.
        /// The initial read position of the stream is ignored.
        /// </param>
        /// <param name="offset">
        /// The offset within the given stream where the setup data starts.
        /// This offset is given by \ref loader::offsets::data_offset.
        /// </param>
        public SliceReader(Stream input, uint offset)
        {
            DataOffset = offset;
            SlicesPerDisk = 1;
            CurrentSlice = 0;
            SliceSize = 0;
            Input = input;

            int maxSize = Int32.MaxValue;
            long fileSize = input.Length;

            SliceSize = (uint)Math.Min(fileSize, maxSize);
            if (DataOffset > fileSize)
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
            DataOffset = 0;
            Dir = dirname;
            BaseFile = basename;
            BaseFile2 = basename2;
            SlicesPerDisk = diskSliceCount;
            CurrentSlice = 0;
            SliceSize = 0;
            Input = Ifs;
        }

        public void Seek(int slice)
        {
            if (slice == CurrentSlice && IsOpen())
                return;

            if (DataOffset != 0)
                throw new SliceError("cannot change slices in single-file setup");

            Open(slice);
        }

        public bool OpenFile(string file)
        {
            if (!File.Exists(file))
                return false;

            try
            {
                Ifs.Close();

                Ifs = File.OpenRead(file);
                if (Ifs == null)
                    return false;

                long fileSize = Ifs.Length;
                Ifs.Seek(0, SeekOrigin.Begin);

                byte[] magic = new byte[8];
                Ifs.Read(magic, 0, 8);
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
                    Ifs.Close();
                    throw new SliceError($"bad slice magic number in \"{file}\"");
                }

                byte[] sliceSizeArr = new byte[4];
                if (Ifs.Read(sliceSizeArr, 0, 4) == 0)
                {
                    Ifs.Close();
                    throw new SliceError($"could not read slice size in \"{file}\"");
                }

                SliceSize = BitConverter.ToUInt32(sliceSizeArr, 0);
                if (SliceSize > fileSize)
                {
                    Ifs.Close();
                    throw new SliceError($"bad slice size in {file}: {SliceSize} > {fileSize}");
                }
                else if (SliceSize < Ifs.Position)
                {
                    Ifs.Close();
                    throw new SliceError($"bad slice size in {file}: {SliceSize} < {Ifs.Position}");
                }

                return true;
            }
            catch
            {
                Ifs.Close();
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
                if (string.Equals(actualFilename, filename, StringComparison.OrdinalIgnoreCase) && OpenFile(path))
                    return true;
            }

            return false;
        }

        public void Open(int slice)
        {
            CurrentSlice = slice;
            Input = Ifs;
            // Ifs.Close();

            string sliceFile = SliceFilename(BaseFile, slice, SlicesPerDisk);
            if (OpenFile(Path.Combine(Dir, sliceFile)))
                return;

            string sliceFile2 = SliceFilename(BaseFile2, slice, SlicesPerDisk);
            if (!string.IsNullOrWhiteSpace(BaseFile2) && sliceFile != sliceFile2 && OpenFile(Path.Combine(Dir, sliceFile2)))
                return;

            if (OpenFileCaseInsensitive(Dir, sliceFile))
                return;

            if (!string.IsNullOrWhiteSpace(BaseFile2) && sliceFile2 != sliceFile && OpenFileCaseInsensitive(Dir, sliceFile2))
                return;

            string oss = $"could not open slice {slice}: {sliceFile}";
            if (!string.IsNullOrWhiteSpace(BaseFile2) && sliceFile2 != sliceFile)
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

            offset += DataOffset;

            if (offset > SliceSize)
                return false;

            if (offset >= Input.Length)
                return false;

            Input.Seek(offset, SeekOrigin.Begin);
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
        public int Read(ref char[] buffer, ref int bufferPtr, int bytes)
        {
            using (BinaryReader br = new BinaryReader(Input, Encoding.Default, true))
            {
                Seek(CurrentSlice);

                int nread = 0;
                while (bytes > 0)
                {
                    long readPos = Input.Position;
                    if (readPos > SliceSize)
                        break;

                    long remaining = SliceSize - readPos;
                    if (remaining == 0)
                    {
                        Seek(CurrentSlice + 1);
                        readPos = Input.Position;
                        if (readPos > SliceSize)
                            break;

                        remaining = SliceSize - readPos;
                    }

                    ulong toread = (ulong)Math.Min(remaining, bytes);
                    toread = Math.Min(toread, Int32.MaxValue);
                    byte[] byteBuffer = new byte[(int)toread];
                    int read = Input.Read(byteBuffer, bufferPtr, (int)toread);
                    if (read == 0)
                        break;

                    Array.Copy(byteBuffer.Select(b => (char)b).ToArray(), 0, buffer, bufferPtr, (int)toread);
                    nread += read; bufferPtr += read; bytes -= read;
                }

                return (nread != 0 || bytes == 0) ? nread : -1;
            }
        }

        /// <returns>the number currently opened slice.</returns>
        public int Slice()
        {
            return CurrentSlice;
        }

        /// <returns>true a slice is currently open.</returns>
        public bool IsOpen()
        {
            return Input != Ifs || Ifs.CanRead;
        }
    }
}
