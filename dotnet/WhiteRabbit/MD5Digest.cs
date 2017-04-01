namespace WhiteRabbit
{
    using System.Numerics;

    /**
     * Code taken from BouncyCastle and optimized for specific constraints (e.g. input is always larger than 4 bytes and smaller than 52 bytes).
     * Further optimization: input could be assumed to be smaller than 27 bytes (original phrase contains 18 letters, so that allows anagrams of 9 words)
     * base implementation of MD4 family style digest as outlined in
     * "Handbook of Applied Cryptography", pages 344 - 347.
     * implementation of MD5 as outlined in "Handbook of Applied Cryptography", pages 346 - 347.
     */
    internal static class MD5Digest
    {
        private static readonly Vector<uint> K1 = new Vector<uint>(0xd76aa478);
        private static readonly Vector<uint> K2 = new Vector<uint>(0xe8c7b756);
        private static readonly Vector<uint> K3 = new Vector<uint>(0x242070db);
        private static readonly Vector<uint> K4 = new Vector<uint>(0xc1bdceee);
        private static readonly Vector<uint> K5 = new Vector<uint>(0xf57c0faf);
        private static readonly Vector<uint> K6 = new Vector<uint>(0x4787c62a);
        private static readonly Vector<uint> K7 = new Vector<uint>(0xa8304613);
        private static readonly Vector<uint> K8 = new Vector<uint>(0xfd469501);
        private static readonly Vector<uint> K9 = new Vector<uint>(0x698098d8);
        private static readonly Vector<uint> K10 = new Vector<uint>(0x8b44f7af);
        private static readonly Vector<uint> K11 = new Vector<uint>(0xffff5bb1);
        private static readonly Vector<uint> K12 = new Vector<uint>(0x895cd7be);
        private static readonly Vector<uint> K13 = new Vector<uint>(0x6b901122);
        private static readonly Vector<uint> K14 = new Vector<uint>(0xfd987193);
        private static readonly Vector<uint> K15 = new Vector<uint>(0xa679438e);
        private static readonly Vector<uint> K16 = new Vector<uint>(0x49b40821);

        private static readonly Vector<uint> K17 = new Vector<uint>(0xf61e2562);
        private static readonly Vector<uint> K18 = new Vector<uint>(0xc040b340);
        private static readonly Vector<uint> K19 = new Vector<uint>(0x265e5a51);
        private static readonly Vector<uint> K20 = new Vector<uint>(0xe9b6c7aa);
        private static readonly Vector<uint> K21 = new Vector<uint>(0xd62f105d);
        private static readonly Vector<uint> K22 = new Vector<uint>(0x2441453);
        private static readonly Vector<uint> K23 = new Vector<uint>(0xd8a1e681);
        private static readonly Vector<uint> K24 = new Vector<uint>(0xe7d3fbc8);
        private static readonly Vector<uint> K25 = new Vector<uint>(0x21e1cde6);
        private static readonly Vector<uint> K26 = new Vector<uint>(0xc33707d6);
        private static readonly Vector<uint> K27 = new Vector<uint>(0xf4d50d87);
        private static readonly Vector<uint> K28 = new Vector<uint>(0x455a14ed);
        private static readonly Vector<uint> K29 = new Vector<uint>(0xa9e3e905);
        private static readonly Vector<uint> K30 = new Vector<uint>(0xfcefa3f8);
        private static readonly Vector<uint> K31 = new Vector<uint>(0x676f02d9);
        private static readonly Vector<uint> K32 = new Vector<uint>(0x8d2a4c8a);

        private static readonly Vector<uint> K33 = new Vector<uint>(0xfffa3942);
        private static readonly Vector<uint> K34 = new Vector<uint>(0x8771f681);
        private static readonly Vector<uint> K35 = new Vector<uint>(0x6d9d6122);
        private static readonly Vector<uint> K36 = new Vector<uint>(0xfde5380c);
        private static readonly Vector<uint> K37 = new Vector<uint>(0xa4beea44);
        private static readonly Vector<uint> K38 = new Vector<uint>(0x4bdecfa9);
        private static readonly Vector<uint> K39 = new Vector<uint>(0xf6bb4b60);
        private static readonly Vector<uint> K40 = new Vector<uint>(0xbebfbc70);
        private static readonly Vector<uint> K41 = new Vector<uint>(0x289b7ec6);
        private static readonly Vector<uint> K42 = new Vector<uint>(0xeaa127fa);
        private static readonly Vector<uint> K43 = new Vector<uint>(0xd4ef3085);
        private static readonly Vector<uint> K44 = new Vector<uint>(0x4881d05);
        private static readonly Vector<uint> K45 = new Vector<uint>(0xd9d4d039);
        private static readonly Vector<uint> K46 = new Vector<uint>(0xe6db99e5);
        private static readonly Vector<uint> K47 = new Vector<uint>(0x1fa27cf8);
        private static readonly Vector<uint> K48 = new Vector<uint>(0xc4ac5665);

        private static readonly Vector<uint> K49 = new Vector<uint>(0xf4292244);
        private static readonly Vector<uint> K50 = new Vector<uint>(0x432aff97);
        private static readonly Vector<uint> K51 = new Vector<uint>(0xab9423a7);
        private static readonly Vector<uint> K52 = new Vector<uint>(0xfc93a039);
        private static readonly Vector<uint> K53 = new Vector<uint>(0x655b59c3);
        private static readonly Vector<uint> K54 = new Vector<uint>(0x8f0ccc92);
        private static readonly Vector<uint> K55 = new Vector<uint>(0xffeff47d);
        private static readonly Vector<uint> K56 = new Vector<uint>(0x85845dd1);
        private static readonly Vector<uint> K57 = new Vector<uint>(0x6fa87e4f);
        private static readonly Vector<uint> K58 = new Vector<uint>(0xfe2ce6e0);
        private static readonly Vector<uint> K59 = new Vector<uint>(0xa3014314);
        private static readonly Vector<uint> K60 = new Vector<uint>(0x4e0811a1);
        private static readonly Vector<uint> K61 = new Vector<uint>(0xf7537e82);
        private static readonly Vector<uint> K62 = new Vector<uint>(0xbd3af235);
        private static readonly Vector<uint> K63 = new Vector<uint>(0x2ad7d2bb);
        private static readonly Vector<uint> K64 = new Vector<uint>(0xeb86d391);

        private static readonly Vector<uint> A0 = new Vector<uint>(0x67452301);
        private static readonly Vector<uint> B0 = new Vector<uint>(0xefcdab89);
        private static readonly Vector<uint> C0 = new Vector<uint>(0x98badcfe);
        private static readonly Vector<uint> D0 = new Vector<uint>(0x10325476);

        public static unsafe PhrasesHashesChunk Compute(PhrasesChunk input)
        {
            var initialUints = stackalloc uint[32];
            *(long*)(initialUints + 0 * 2) = *(long*)(input.Buffers + 0 * 8);
            *(long*)(initialUints + 1 * 2) = *(long*)(input.Buffers + 1 * 8);
            *(long*)(initialUints + 2 * 2) = *(long*)(input.Buffers + 2 * 8);
            *(long*)(initialUints + 3 * 2) = *(long*)(input.Buffers + 3 * 8);
            *(long*)(initialUints + 4 * 2) = *(long*)(input.Buffers + 4 * 8);
            *(long*)(initialUints + 5 * 2) = *(long*)(input.Buffers + 5 * 8);
            *(long*)(initialUints + 6 * 2) = *(long*)(input.Buffers + 6 * 8);
            *(long*)(initialUints + 7 * 2) = *(long*)(input.Buffers + 7 * 8);
            *(long*)(initialUints + 8 * 2) = *(long*)(input.Buffers + 8 * 8);
            *(long*)(initialUints + 9 * 2) = *(long*)(input.Buffers + 9 * 8);
            *(long*)(initialUints + 10 * 2) = *(long*)(input.Buffers + 10 * 8);
            *(long*)(initialUints + 11 * 2) = *(long*)(input.Buffers + 11 * 8);
            *(long*)(initialUints + 12 * 2) = *(long*)(input.Buffers + 12 * 8);
            *(long*)(initialUints + 13 * 2) = *(long*)(input.Buffers + 13 * 8);
            *(long*)(initialUints + 14 * 2) = *(long*)(input.Buffers + 14 * 8);
            *(long*)(initialUints + 15 * 2) = *(long*)(input.Buffers + 15 * 8);

            ((byte*)initialUints)[31 + 0 * 32] = 0;
            ((byte*)initialUints)[31 + 1 * 32] = 0;
            ((byte*)initialUints)[31 + 2 * 32] = 0;
            ((byte*)initialUints)[31 + 3 * 32] = 0;
            ((byte*)initialUints)[0 * 32 + input.Buffers[31 + 0 * 32]] = 128;
            ((byte*)initialUints)[1 * 32 + input.Buffers[31 + 1 * 32]] = 128;
            ((byte*)initialUints)[2 * 32 + input.Buffers[31 + 2 * 32]] = 128;
            ((byte*)initialUints)[3 * 32 + input.Buffers[31 + 3 * 32]] = 128;
            initialUints[7 + 0 * 8] = (uint)(input.Buffers[31 + 0 * 32] << 3);
            initialUints[7 + 1 * 8] = (uint)(input.Buffers[31 + 1 * 32] << 3);
            initialUints[7 + 2 * 8] = (uint)(input.Buffers[31 + 2 * 32] << 3);
            initialUints[7 + 3 * 8] = (uint)(input.Buffers[31 + 3 * 32] << 3);

            var message0 = new Vector<uint>(new uint[] {
                initialUints[0 + 0 * 8],
                initialUints[0 + 1 * 8],
                initialUints[0 + 2 * 8],
                initialUints[0 + 3 * 8],
            });

            var message1 = new Vector<uint>(new uint[] {
                initialUints[1 + 0 * 8],
                initialUints[1 + 1 * 8],
                initialUints[1 + 2 * 8],
                initialUints[1 + 3 * 8],
            });

            var message2 = new Vector<uint>(new uint[] {
                initialUints[2 + 0 * 8],
                initialUints[2 + 1 * 8],
                initialUints[2 + 2 * 8],
                initialUints[2 + 3 * 8],
            });

            var message3 = new Vector<uint>(new uint[] {
                initialUints[3 + 0 * 8],
                initialUints[3 + 1 * 8],
                initialUints[3 + 2 * 8],
                initialUints[3 + 3 * 8],
            });

            var message4 = new Vector<uint>(new uint[] {
                initialUints[4 + 0 * 8],
                initialUints[4 + 1 * 8],
                initialUints[4 + 2 * 8],
                initialUints[4 + 3 * 8],
            });

            var message5 = new Vector<uint>(new uint[] {
                initialUints[5 + 0 * 8],
                initialUints[5 + 1 * 8],
                initialUints[5 + 2 * 8],
                initialUints[5 + 3 * 8],
            });

            var message6 = new Vector<uint>(new uint[] {
                initialUints[6 + 0 * 8],
                initialUints[6 + 1 * 8],
                initialUints[6 + 2 * 8],
                initialUints[6 + 3 * 8],
            });

            var message7 = new Vector<uint>(new uint[] {
                initialUints[7 + 0 * 8],
                initialUints[7 + 1 * 8],
                initialUints[7 + 2 * 8],
                initialUints[7 + 3 * 8],
            });

            var a = A0;
            var b = B0;
            var c = C0;
            var d = D0;

            a = LeftRotate(message0 + K1 + a + ((b & c) | (~b & d)), 7, 32 - 7) + b;
            d = LeftRotate(message1 + K2 + d + ((a & b) | (~a & c)), 12, 32 - 12) + a;
            c = LeftRotate(message2 + K3 + c + ((d & a) | (~d & b)), 17, 32 - 17) + d;
            b = LeftRotate(message3 + K4 + b + ((c & d) | (~c & a)), 22, 32 - 22) + c;
            a = LeftRotate(message4 + K5 + a + ((b & c) | (~b & d)), 7, 32 - 7) + b;
            d = LeftRotate(message5 + K6 + d + ((a & b) | (~a & c)), 12, 32 - 12) + a;
            c = LeftRotate(message6 + K7 + c + ((d & a) | (~d & b)), 17, 32 - 17) + d;
            b = LeftRotate(K8 + b + ((c & d) | (~c & a)), 22, 32 - 22) + c;
            a = LeftRotate(K9 + a + ((b & c) | (~b & d)), 7, 32 - 7) + b;
            d = LeftRotate(K10 + d + ((a & b) | (~a & c)), 12, 32 - 12) + a;
            c = LeftRotate(K11 + c + ((d & a) | (~d & b)), 17, 32 - 17) + d;
            b = LeftRotate(K12 + b + ((c & d) | (~c & a)), 22, 32 - 22) + c;
            a = LeftRotate(K13 + a + ((b & c) | (~b & d)), 7, 32 - 7) + b;
            d = LeftRotate(K14 + d + ((a & b) | (~a & c)), 12, 32 - 12) + a;
            c = LeftRotate(message7 + K15 + c + ((d & a) | (~d & b)), 17, 32 - 17) + d;
            b = LeftRotate(K16 + b + ((c & d) | (~c & a)), 22, 32 - 22) + c;

            a = LeftRotate(message1 + K17 + a + ((b & d) | (c & ~d)), 5, 32 - 5) + b;
            d = LeftRotate(message6 + K18 + d + ((a & c) | (b & ~c)), 9, 32 - 9) + a;
            c = LeftRotate(K19 + c + ((d & b) | (a & ~b)), 14, 32 - 14) + d;
            b = LeftRotate(message0 + K20 + b + ((c & a) | (d & ~a)), 20, 32 - 20) + c;
            a = LeftRotate(message5 + K21 + a + ((b & d) | (c & ~d)), 5, 32 - 5) + b;
            d = LeftRotate(K22 + d + ((a & c) | (b & ~c)), 9, 32 - 9) + a;
            c = LeftRotate(K23 + c + ((d & b) | (a & ~b)), 14, 32 - 14) + d;
            b = LeftRotate(message4 + K24 + b + ((c & a) | (d & ~a)), 20, 32 - 20) + c;
            a = LeftRotate(K25 + a + ((b & d) | (c & ~d)), 5, 32 - 5) + b;
            d = LeftRotate(message7 + K26 + d + ((a & c) | (b & ~c)), 9, 32 - 9) + a;
            c = LeftRotate(message3 + K27 + c + ((d & b) | (a & ~b)), 14, 32 - 14) + d;
            b = LeftRotate(K28 + b + ((c & a) | (d & ~a)), 20, 32 - 20) + c;
            a = LeftRotate(K29 + a + ((b & d) | (c & ~d)), 5, 32 - 5) + b;
            d = LeftRotate(message2 + K30 + d + ((a & c) | (b & ~c)), 9, 32 - 9) + a;
            c = LeftRotate(K31 + c + ((d & b) | (a & ~b)), 14, 32 - 14) + d;
            b = LeftRotate(K32 + b + ((c & a) | (d & ~a)), 20, 32 - 20) + c;

            a = LeftRotate(message5 + K33 + a + (b ^ c ^ d), 4, 32 - 4) + b;
            d = LeftRotate(K34 + d + (a ^ b ^ c), 11, 32 - 11) + a;
            c = LeftRotate(K35 + c + (d ^ a ^ b), 16, 32 - 16) + d;
            b = LeftRotate(message7 + K36 + b + (c ^ d ^ a), 23, 32 - 23) + c;
            a = LeftRotate(message1 + K37 + a + (b ^ c ^ d), 4, 32 - 4) + b;
            d = LeftRotate(message4 + K38 + d + (a ^ b ^ c), 11, 32 - 11) + a;
            c = LeftRotate(K39 + c + (d ^ a ^ b), 16, 32 - 16) + d;
            b = LeftRotate(K40 + b + (c ^ d ^ a), 23, 32 - 23) + c;
            a = LeftRotate(K41 + a + (b ^ c ^ d), 4, 32 - 4) + b;
            d = LeftRotate(message0 + K42 + d + (a ^ b ^ c), 11, 32 - 11) + a;
            c = LeftRotate(message3 + K43 + c + (d ^ a ^ b), 16, 32 - 16) + d;
            b = LeftRotate(message6 + K44 + b + (c ^ d ^ a), 23, 32 - 23) + c;
            a = LeftRotate(K45 + a + (b ^ c ^ d), 4, 32 - 4) + b;
            d = LeftRotate(K46 + d + (a ^ b ^ c), 11, 32 - 11) + a;
            c = LeftRotate(K47 + c + (d ^ a ^ b), 16, 32 - 16) + d;
            b = LeftRotate(message2 + K48 + b + (c ^ d ^ a), 23, 32 - 23) + c;

            a = LeftRotate(message0 + K49 + a + (c ^ (b | ~d)), 6, 32 - 6) + b;
            d = LeftRotate(K50 + d + (b ^ (a | ~c)), 10, 32 - 10) + a;
            c = LeftRotate(message7 + K51 + c + (a ^ (d | ~b)), 15, 32 - 15) + d;
            b = LeftRotate(message5 + K52 + b + (d ^ (c | ~a)), 21, 32 - 21) + c;
            a = LeftRotate(K53 + a + (c ^ (b | ~d)), 6, 32 - 6) + b;
            d = LeftRotate(message3 + K54 + d + (b ^ (a | ~c)), 10, 32 - 10) + a;
            c = LeftRotate(K55 + c + (a ^ (d | ~b)), 15, 32 - 15) + d;
            b = LeftRotate(message1 + K56 + b + (d ^ (c | ~a)), 21, 32 - 21) + c;
            a = LeftRotate(K57 + a + (c ^ (b | ~d)), 6, 32 - 6) + b;
            d = LeftRotate(K58 + d + (b ^ (a | ~c)), 10, 32 - 10) + a;
            c = LeftRotate(message6 + K59 + c + (a ^ (d | ~b)), 15, 32 - 15) + d;
            b = LeftRotate(K60 + b + (d ^ (c | ~a)), 21, 32 - 21) + c;
            a = LeftRotate(message4 + K61 + a + (c ^ (b | ~d)), 6, 32 - 6) + b;
            d = LeftRotate(K62 + d + (b ^ (a | ~c)), 10, 32 - 10) + a;
            c = LeftRotate(message2 + K63 + c + (a ^ (d | ~b)), 15, 32 - 15) + d;
            b = LeftRotate(K64 + b + (d ^ (c | ~a)), 21, 32 - 21) + c;

            return new[]
            {
                0x67452301 + a,
                0xefcdab89 + b,
                0x98badcfe + c,
                0x10325476 + d,
            };
        }

        private static uint LeftRotate(Vector<uint> x, int left, int right)
        {
            return (x << left) | (x >> right);
        }
    }
}
