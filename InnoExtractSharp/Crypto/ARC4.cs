/*
 * Copyright (C) 2018 Daniel Scharrer
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
    // Alledged RC4 en-/decryption calculation
    public class ARC4
    {
        private byte[] state = new byte[256];

        private int a;
        private int b;

        public void Init(string key, int length)
        {
            a = b = 0;

            for (int i = 0; i < state.Length; i++)
            {
                state[i] = (byte)i;
            }

            int j = 0;
            for (int i = 0; i < state.Length; i++)
            {
                j = (j + state[i] + (byte)(key[i % length])) % state.Length;
                byte temp = state[i]; state[i] = state[j]; state[j] = temp;
            }
        }

        public void Discard(int length)
        {
            for (int i = 0; i < length; i++)
            {
                Update();
            }
        }

        public void Crypt(char[] inArr, ref char[] outArr, int length)
        {
            for (int i = 0; i < length; i++)
            {
                Update();
                outArr[i] = (char)(state[(state[a] + state[b]) % state.Length] ^ (byte)inArr[i]);
            }
        }

        private void Update()
        {
            a = (a + 1) % state.Length;
            b = (b + state[a]) % state.Length;

            byte temp = state[a]; state[a] = state[b]; state[b] = temp;
        }
    }
}
