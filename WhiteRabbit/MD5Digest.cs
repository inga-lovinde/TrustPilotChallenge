namespace WhiteRabbit
{
    using System;

    /**
     * Code taken from BouncyCastle and optimized for specific constraints (e.g. input is always larger than 4 bytes and smaller than 52 bytes).
     * base implementation of MD4 family style digest as outlined in
     * "Handbook of Applied Cryptography", pages 344 - 347.
     * implementation of MD5 as outlined in "Handbook of Applied Cryptography", pages 346 - 347.
     */
    internal static class MD5Digest
    {
        public static uint[] Compute(byte[] input)
        {
            var length = input.Length;

            var xBytes = new byte[4 * 14];
            Buffer.BlockCopy(input, 0, xBytes, 0, length);
            xBytes[length] = 128;

            var x0 = LE_To_UInt32(xBytes, 4 * 0);
            var x1 = LE_To_UInt32(xBytes, 4 * 1);
            var x2 = LE_To_UInt32(xBytes, 4 * 2);
            var x3 = LE_To_UInt32(xBytes, 4 * 3);
            var x4 = LE_To_UInt32(xBytes, 4 * 4);
            var x5 = LE_To_UInt32(xBytes, 4 * 5);
            var x6 = LE_To_UInt32(xBytes, 4 * 6);
            var x7 = LE_To_UInt32(xBytes, 4 * 7);
            var x8 = LE_To_UInt32(xBytes, 4 * 8);
            var x9 = LE_To_UInt32(xBytes, 4 * 9);
            var x10 = LE_To_UInt32(xBytes, 4 * 10);
            var x11 = LE_To_UInt32(xBytes, 4 * 11);
            var x12 = LE_To_UInt32(xBytes, 4 * 12);
            var x13 = LE_To_UInt32(xBytes, 4 * 13);
            var x14 = (uint)(length << 3);
            uint x15 = 0;

            uint a = 0x67452301;
            uint b = 0xefcdab89;
            uint c = 0x98badcfe;
            uint d = 0x10325476;

            a = LeftRotate(x0 + 0xd76aa478 + a + ((b & c) | (~b & d)), 7, 32 - 7) + b;
            d = LeftRotate(x1 + 0xe8c7b756 + d + ((a & b) | (~a & c)), 12, 32 - 12) + a;
            c = LeftRotate(x2 + 0x242070db + c + ((d & a) | (~d & b)), 17, 32 - 17) + d;
            b = LeftRotate(x3 + 0xc1bdceee + b + ((c & d) | (~c & a)), 22, 32 - 22) + c;
            a = LeftRotate(x4 + 0xf57c0faf + a + ((b & c) | (~b & d)), 7, 32 - 7) + b;
            d = LeftRotate(x5 + 0x4787c62a + d + ((a & b) | (~a & c)), 12, 32 - 12) + a;
            c = LeftRotate(x6 + 0xa8304613 + c + ((d & a) | (~d & b)), 17, 32 - 17) + d;
            b = LeftRotate(x7 + 0xfd469501 + b + ((c & d) | (~c & a)), 22, 32 - 22) + c;
            a = LeftRotate(x8 + 0x698098d8 + a + ((b & c) | (~b & d)), 7, 32 - 7) + b;
            d = LeftRotate(x9 + 0x8b44f7af + d + ((a & b) | (~a & c)), 12, 32 - 12) + a;
            c = LeftRotate(x10 + 0xffff5bb1 + c + ((d & a) | (~d & b)), 17, 32 - 17) + d;
            b = LeftRotate(x11 + 0x895cd7be + b + ((c & d) | (~c & a)), 22, 32 - 22) + c;
            a = LeftRotate(x12 + 0x6b901122 + a + ((b & c) | (~b & d)), 7, 32 - 7) + b;
            d = LeftRotate(x13 + 0xfd987193 + d + ((a & b) | (~a & c)), 12, 32 - 12) + a;
            c = LeftRotate(x14 + 0xa679438e + c + ((d & a) | (~d & b)), 17, 32 - 17) + d;
            b = LeftRotate(x15 + 0x49b40821 + b + ((c & d) | (~c & a)), 22, 32 - 22) + c;

            a = LeftRotate(x1 + 0xf61e2562 + a + ((b & d) | (c & ~d)), 5, 32 - 5) + b;
            d = LeftRotate(x6 + 0xc040b340 + d + ((a & c) | (b & ~c)), 9, 32 - 9) + a;
            c = LeftRotate(x11 + 0x265e5a51 + c + ((d & b) | (a & ~b)), 14, 32 - 14) + d;
            b = LeftRotate(x0 + 0xe9b6c7aa + b + ((c & a) | (d & ~a)), 20, 32 - 20) + c;
            a = LeftRotate(x5 + 0xd62f105d + a + ((b & d) | (c & ~d)), 5, 32 - 5) + b;
            d = LeftRotate(x10 + 0x2441453 + d + ((a & c) | (b & ~c)), 9, 32 - 9) + a;
            c = LeftRotate(x15 + 0xd8a1e681 + c + ((d & b) | (a & ~b)), 14, 32 - 14) + d;
            b = LeftRotate(x4 + 0xe7d3fbc8 + b + ((c & a) | (d & ~a)), 20, 32 - 20) + c;
            a = LeftRotate(x9 + 0x21e1cde6 + a + ((b & d) | (c & ~d)), 5, 32 - 5) + b;
            d = LeftRotate(x14 + 0xc33707d6 + d + ((a & c) | (b & ~c)), 9, 32 - 9) + a;
            c = LeftRotate(x3 + 0xf4d50d87 + c + ((d & b) | (a & ~b)), 14, 32 - 14) + d;
            b = LeftRotate(x8 + 0x455a14ed + b + ((c & a) | (d & ~a)), 20, 32 - 20) + c;
            a = LeftRotate(x13 + 0xa9e3e905 + a + ((b & d) | (c & ~d)), 5, 32 - 5) + b;
            d = LeftRotate(x2 + 0xfcefa3f8 + d + ((a & c) | (b & ~c)), 9, 32 - 9) + a;
            c = LeftRotate(x7 + 0x676f02d9 + c + ((d & b) | (a & ~b)), 14, 32 - 14) + d;
            b = LeftRotate(x12 + 0x8d2a4c8a + b + ((c & a) | (d & ~a)), 20, 32 - 20) + c;

            a = LeftRotate(x5 + 0xfffa3942 + a + (b ^ c ^ d), 4, 32 - 4) + b;
            d = LeftRotate(x8 + 0x8771f681 + d + (a ^ b ^ c), 11, 32 - 11) + a;
            c = LeftRotate(x11 + 0x6d9d6122 + c + (d ^ a ^ b), 16, 32 - 16) + d;
            b = LeftRotate(x14 + 0xfde5380c + b + (c ^ d ^ a), 23, 32 - 23) + c;
            a = LeftRotate(x1 + 0xa4beea44 + a + (b ^ c ^ d), 4, 32 - 4) + b;
            d = LeftRotate(x4 + 0x4bdecfa9 + d + (a ^ b ^ c), 11, 32 - 11) + a;
            c = LeftRotate(x7 + 0xf6bb4b60 + c + (d ^ a ^ b), 16, 32 - 16) + d;
            b = LeftRotate(x10 + 0xbebfbc70 + b + (c ^ d ^ a), 23, 32 - 23) + c;
            a = LeftRotate(x13 + 0x289b7ec6 + a + (b ^ c ^ d), 4, 32 - 4) + b;
            d = LeftRotate(x0 + 0xeaa127fa + d + (a ^ b ^ c), 11, 32 - 11) + a;
            c = LeftRotate(x3 + 0xd4ef3085 + c + (d ^ a ^ b), 16, 32 - 16) + d;
            b = LeftRotate(x6 + 0x4881d05 + b + (c ^ d ^ a), 23, 32 - 23) + c;
            a = LeftRotate(x9 + 0xd9d4d039 + a + (b ^ c ^ d), 4, 32 - 4) + b;
            d = LeftRotate(x12 + 0xe6db99e5 + d + (a ^ b ^ c), 11, 32 - 11) + a;
            c = LeftRotate(x15 + 0x1fa27cf8 + c + (d ^ a ^ b), 16, 32 - 16) + d;
            b = LeftRotate(x2 + 0xc4ac5665 + b + (c ^ d ^ a), 23, 32 - 23) + c;

            a = LeftRotate(x0 + 0xf4292244 + a + (c ^ (b | ~d)), 6, 32 - 6) + b;
            d = LeftRotate(x7 + 0x432aff97 + d + (b ^ (a | ~c)), 10, 32 - 10) + a;
            c = LeftRotate(x14 + 0xab9423a7 + c + (a ^ (d | ~b)), 15, 32 - 15) + d;
            b = LeftRotate(x5 + 0xfc93a039 + b + (d ^ (c | ~a)), 21, 32 - 21) + c;
            a = LeftRotate(x12 + 0x655b59c3 + a + (c ^ (b | ~d)), 6, 32 - 6) + b;
            d = LeftRotate(x3 + 0x8f0ccc92 + d + (b ^ (a | ~c)), 10, 32 - 10) + a;
            c = LeftRotate(x10 + 0xffeff47d + c + (a ^ (d | ~b)), 15, 32 - 15) + d;
            b = LeftRotate(x1 + 0x85845dd1 + b + (d ^ (c | ~a)), 21, 32 - 21) + c;
            a = LeftRotate(x8 + 0x6fa87e4f + a + (c ^ (b | ~d)), 6, 32 - 6) + b;
            d = LeftRotate(x15 + 0xfe2ce6e0 + d + (b ^ (a | ~c)), 10, 32 - 10) + a;
            c = LeftRotate(x6 + 0xa3014314 + c + (a ^ (d | ~b)), 15, 32 - 15) + d;
            b = LeftRotate(x13 + 0x4e0811a1 + b + (d ^ (c | ~a)), 21, 32 - 21) + c;
            a = LeftRotate(x4 + 0xf7537e82 + a + (c ^ (b | ~d)), 6, 32 - 6) + b;
            d = LeftRotate(x11 + 0xbd3af235 + d + (b ^ (a | ~c)), 10, 32 - 10) + a;
            c = LeftRotate(x2 + 0x2ad7d2bb + c + (a ^ (d | ~b)), 15, 32 - 15) + d;
            b = LeftRotate(x9 + 0xeb86d391 + b + (d ^ (c | ~a)), 21, 32 - 21) + c;

            return new[]
            {
                0x67452301 + a,
                0xefcdab89 + b,
                0x98badcfe + c,
                0x10325476 + d,
            };
        }

        private static uint LE_To_UInt32(byte[] bs, int off)
        {
            return (uint)bs[off]
                | (uint)bs[off + 1] << 8
                | (uint)bs[off + 2] << 16
                | (uint)bs[off + 3] << 24;
        }

        private static uint LeftRotate(uint x, int left, int right)
        {
            return (x << left) | (x >> right);
        }
    }
}
