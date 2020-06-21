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

namespace InnoExtractSharp.Crypto
{
    public class ChecksumUninitializedError { }

    public class Hasher
    {
        private Checksum self;

        public Hasher(ChecksumType type)
        {
            switch(type)
            {
                case ChecksumType.None:
                    break;
                case ChecksumType.Adler32:
                    self = new Adler32();
                    ((Adler32)self).Init();
                    break;
                case ChecksumType.CRC32:
                    self = new CRC32();
                    ((CRC32)self).Init();
                    break;
                case ChecksumType.MD5:
                    self = new MD5();
                    ((MD5)self).Init();
                    break;
                case ChecksumType.SHA1:
                    self = new SHA1();
                    ((SHA1)self).Init();
                    break;
            }
        }

        public void Update(byte[] data, int dataPtr, int size)
        {
            switch (self.Type)
            {
                case ChecksumType.None:
                    break;
                case ChecksumType.Adler32:
                    ((Adler32)self).Update(data, dataPtr, size);
                    break;
                case ChecksumType.CRC32:
                    ((CRC32)self).Update(data, dataPtr, size);
                    break;
                case ChecksumType.MD5:
                    ((MD5)self).Update(data, dataPtr, size);
                    break;
                case ChecksumType.SHA1:
                    ((SHA1)self).Update(data, dataPtr, size);
                    break;
            }
        }

        public Checksum Finalize()
        {
            switch (self.Type)
            {
                case ChecksumType.None:
                    break;
                case ChecksumType.Adler32:
                    self.Adler32 = ((Adler32)self).Finalize();
                    break;
                case ChecksumType.CRC32:
                    self.CRC32 = ((CRC32)self).Finalize();
                    break;
                case ChecksumType.MD5:
                    self.MD5 = ((MD5)self).Finalize();
                    break;
                case ChecksumType.SHA1:
                    self.SHA1 = ((SHA1)self).Finalize();
                    break;
            }

            return self;
        }
    }
}
