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

namespace InnoExtractSharp.Crypto
{
    public class SHA1 : Checksum
    {
        System.Security.Cryptography.SHA1 self;

        public SHA1()
        {
            Type = ChecksumType.SHA1;
            self = System.Security.Cryptography.SHA1.Create();
        }

        public void Init()
        {
            self.Initialize();
        }

        public override void Update(byte[] data, int dataPtr, int length)
        {
            self.TransformBlock(data, dataPtr, length, null, 0);
        }

        public byte[] Finalize()
        {
            return self.TransformFinalBlock(null, 0, 0);
        }
    }
}
