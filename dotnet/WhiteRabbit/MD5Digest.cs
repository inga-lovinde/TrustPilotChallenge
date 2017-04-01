using System.Runtime.CompilerServices;

namespace WhiteRabbit
{
    /**
     * Code taken from BouncyCastle and optimized for specific constraints (e.g. input is always larger than 4 bytes and smaller than 52 bytes).
     * Further optimization: input could be assumed to be smaller than 27 bytes (original phrase contains 18 letters, so that allows anagrams of 9 words)
     * base implementation of MD4 family style digest as outlined in
     * "Handbook of Applied Cryptography", pages 344 - 347.
     * implementation of MD5 as outlined in "Handbook of Applied Cryptography", pages 346 - 347.
     */
    internal static class MD5Digest
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe uint[] Compute(Phrase input)
        {
            uint a = 0x67452301;
            uint b = 0xefcdab89;
            uint c = 0x98badcfe;
            uint d = 0x10325476;

            a = b + LeftRotate(0xd76aa478 + a + Blend(d, c, b) + input.Buffer[0], 7);
            d = a + LeftRotate(0xe8c7b756 + d + Blend(c, b, a) + input.Buffer[1], 12);
            c = d + LeftRotate(0x242070db + c + Blend(b, a, d) + input.Buffer[2], 17);
            b = c + LeftRotate(0xc1bdceee + b + Blend(a, d, c) + input.Buffer[3], 22);
            a = b + LeftRotate(0xf57c0faf + a + Blend(d, c, b) + input.Buffer[4], 7);
            d = a + LeftRotate(0x4787c62a + d + Blend(c, b, a) + input.Buffer[5], 12);
            c = d + LeftRotate(0xa8304613 + c + Blend(b, a, d) + input.Buffer[6], 17);
            b = c + LeftRotate(0xfd469501 + b + Blend(a, d, c), 22);
            a = b + LeftRotate(0x698098d8 + a + Blend(d, c, b), 7);
            d = a + LeftRotate(0x8b44f7af + d + Blend(c, b, a), 12);
            c = d + LeftRotate(0xffff5bb1 + c + Blend(b, a, d), 17);
            b = c + LeftRotate(0x895cd7be + b + Blend(a, d, c), 22);
            a = b + LeftRotate(0x6b901122 + a + Blend(d, c, b), 7);
            d = a + LeftRotate(0xfd987193 + d + Blend(c, b, a), 12);
            c = d + LeftRotate(0xa679438e + c + Blend(b, a, d) + input.Buffer[7], 17);
            b = c + LeftRotate(0x49b40821 + b + Blend(a, d, c), 22);

            a = b + LeftRotate(0xf61e2562 + a + Blend(c, b, d) + input.Buffer[1], 5);
            d = a + LeftRotate(0xc040b340 + d + Blend(b, a, c) + input.Buffer[6], 9);
            c = d + LeftRotate(0x265e5a51 + c + Blend(a, d, b), 14);
            b = c + LeftRotate(0xe9b6c7aa + b + Blend(d, c, a) + input.Buffer[0], 20);
            a = b + LeftRotate(0xd62f105d + a + Blend(c, b, d) + input.Buffer[5], 5);
            d = a + LeftRotate(0x02441453 + d + Blend(b, a, c), 9);
            c = d + LeftRotate(0xd8a1e681 + c + Blend(a, d, b), 14);
            b = c + LeftRotate(0xe7d3fbc8 + b + Blend(d, c, a) + input.Buffer[4], 20);
            a = b + LeftRotate(0x21e1cde6 + a + Blend(c, b, d), 5);
            d = a + LeftRotate(0xc33707d6 + d + Blend(b, a, c) + input.Buffer[7], 9);
            c = d + LeftRotate(0xf4d50d87 + c + Blend(a, d, b) + input.Buffer[3], 14);
            b = c + LeftRotate(0x455a14ed + b + Blend(d, c, a), 20);
            a = b + LeftRotate(0xa9e3e905 + a + Blend(c, b, d), 5);
            d = a + LeftRotate(0xfcefa3f8 + d + Blend(b, a, c) + input.Buffer[2], 9);
            c = d + LeftRotate(0x676f02d9 + c + Blend(a, d, b), 14);
            b = c + LeftRotate(0x8d2a4c8a + b + Blend(d, c, a), 20);

            a = b + LeftRotate(0xfffa3942 + a + Xor(b, c, d) + input.Buffer[5], 4);
            d = a + LeftRotate(0x8771f681 + d + Xor(a, b, c), 11);
            c = d + LeftRotate(0x6d9d6122 + c + Xor(d, a, b), 16);
            b = c + LeftRotate(0xfde5380c + b + Xor(c, d, a) + input.Buffer[7], 23);
            a = b + LeftRotate(0xa4beea44 + a + Xor(b, c, d) + input.Buffer[1], 4);
            d = a + LeftRotate(0x4bdecfa9 + d + Xor(a, b, c) + input.Buffer[4], 11);
            c = d + LeftRotate(0xf6bb4b60 + c + Xor(d, a, b), 16);
            b = c + LeftRotate(0xbebfbc70 + b + Xor(c, d, a), 23);
            a = b + LeftRotate(0x289b7ec6 + a + Xor(b, c, d), 4);
            d = a + LeftRotate(0xeaa127fa + d + Xor(a, b, c) + input.Buffer[0], 11);
            c = d + LeftRotate(0xd4ef3085 + c + Xor(d, a, b) + input.Buffer[3], 16);
            b = c + LeftRotate(0x04881d05 + b + Xor(c, d, a) + input.Buffer[6], 23);
            a = b + LeftRotate(0xd9d4d039 + a + Xor(b, c, d), 4);
            d = a + LeftRotate(0xe6db99e5 + d + Xor(a, b, c), 11);
            c = d + LeftRotate(0x1fa27cf8 + c + Xor(d, a, b), 16);
            b = c + LeftRotate(0xc4ac5665 + b + Xor(c, d, a) + input.Buffer[2], 23);

            a = b + LeftRotate(0xf4292244 + a + I(c, b, d) + input.Buffer[0], 6);
            d = a + LeftRotate(0x432aff97 + d + I(b, a, c), 10);
            c = d + LeftRotate(0xab9423a7 + c + I(a, d, b) + input.Buffer[7], 15);
            b = c + LeftRotate(0xfc93a039 + b + I(d, c, a) + input.Buffer[5], 21);
            a = b + LeftRotate(0x655b59c3 + a + I(c, b, d), 6);
            d = a + LeftRotate(0x8f0ccc92 + d + I(b, a, c) + input.Buffer[3], 10);
            c = d + LeftRotate(0xffeff47d + c + I(a, d, b), 15);
            b = c + LeftRotate(0x85845dd1 + b + I(d, c, a) + input.Buffer[1], 21);
            a = b + LeftRotate(0x6fa87e4f + a + I(c, b, d), 6);
            d = a + LeftRotate(0xfe2ce6e0 + d + I(b, a, c), 10);
            c = d + LeftRotate(0xa3014314 + c + I(a, d, b) + input.Buffer[6], 15);
            b = c + LeftRotate(0x4e0811a1 + b + I(d, c, a), 21);
            a = b + LeftRotate(0xf7537e82 + a + I(c, b, d) + input.Buffer[4], 6);
            d = a + LeftRotate(0xbd3af235 + d + I(b, a, c), 10);
            c = d + LeftRotate(0x2ad7d2bb + c + I(a, d, b) + input.Buffer[2], 15);
            b = c + LeftRotate(0xeb86d391 + b + I(d, c, a), 21);

            return new[]
            {
                0x67452301 + a,
                0xefcdab89 + b,
                0x98badcfe + c,
                0x10325476 + d,
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint Blend(uint a, uint b, uint x)
        {
            return (x & b) | (~x & a);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint Xor(uint a, uint b, uint c)
        {
            return a ^ b ^ c;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint I(uint a, uint b, uint c)
        {
            return a ^ (b | ~c);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint LeftRotate(uint x, int left)
        {
            return (x << left) | (x >> 32 - left);
        }
    }
}
