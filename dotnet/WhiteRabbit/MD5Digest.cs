using System.Runtime.CompilerServices;

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint[] Compute(Phrase input)
        {
            var chunk0 = GetChunk(input, 0);
            var chunk1 = GetChunk(input, 1);
            var chunk2 = GetChunk(input, 2);
            var chunk3 = GetChunk(input, 3);
            var chunk4 = GetChunk(input, 4);
            var chunk5 = GetChunk(input, 5);
            var chunk6 = GetChunk(input, 6);
            var chunk7 = GetChunk(input, 7);

            var a = A0;
            var b = B0;
            var c = C0;
            var d = D0;

            a = b + LeftRotate(K1 + a + Blend(d, c, b) + chunk0, 7);
            d = a + LeftRotate(K2 + d + Blend(c, b, a) + chunk1, 12);
            c = d + LeftRotate(K3 + c + Blend(b, a, d) + chunk2, 17);
            b = c + LeftRotate(K4 + b + Blend(a, d, c) + chunk3, 22);
            a = b + LeftRotate(K5 + a + Blend(d, c, b) + chunk4, 7);
            d = a + LeftRotate(K6 + d + Blend(c, b, a) + chunk5, 12);
            c = d + LeftRotate(K7 + c + Blend(b, a, d) + chunk6, 17);
            b = c + LeftRotate(K8 + b + Blend(a, d, c), 22);
            a = b + LeftRotate(K9 + a + Blend(d, c, b), 7);
            d = a + LeftRotate(K10 + d + Blend(c, b, a), 12);
            c = d + LeftRotate(K11 + c + Blend(b, a, d), 17);
            b = c + LeftRotate(K12 + b + Blend(a, d, c), 22);
            a = b + LeftRotate(K13 + a + Blend(d, c, b), 7);
            d = a + LeftRotate(K14 + d + Blend(c, b, a), 12);
            c = d + LeftRotate(K15 + c + Blend(b, a, d) + chunk7, 17);
            b = c + LeftRotate(K16 + b + Blend(a, d, c), 22);

            a = b + LeftRotate(K17 + a + Blend(c, b, d) + chunk1, 5);
            d = a + LeftRotate(K18 + d + Blend(b, a, c) + chunk6, 9);
            c = d + LeftRotate(K19 + c + Blend(a, d, b), 14);
            b = c + LeftRotate(K20 + b + Blend(d, c, a) + chunk0, 20);
            a = b + LeftRotate(K21 + a + Blend(c, b, d) + chunk5, 5);
            d = a + LeftRotate(K22 + d + Blend(b, a, c), 9);
            c = d + LeftRotate(K23 + c + Blend(a, d, b), 14);
            b = c + LeftRotate(K24 + b + Blend(d, c, a) + chunk4, 20);
            a = b + LeftRotate(K25 + a + Blend(c, b, d), 5);
            d = a + LeftRotate(K26 + d + Blend(b, a, c) + chunk7, 9);
            c = d + LeftRotate(K27 + c + Blend(a, d, b) + chunk3, 14);
            b = c + LeftRotate(K28 + b + Blend(d, c, a), 20);
            a = b + LeftRotate(K29 + a + Blend(c, b, d), 5);
            d = a + LeftRotate(K30 + d + Blend(b, a, c) + chunk2, 9);
            c = d + LeftRotate(K31 + c + Blend(a, d, b), 14);
            b = c + LeftRotate(K32 + b + Blend(d, c, a), 20);

            a = b + LeftRotate(K33 + a + Xor(b, c, d) + chunk5, 4);
            d = a + LeftRotate(K34 + d + Xor(a, b, c), 11);
            c = d + LeftRotate(K35 + c + Xor(d, a, b), 16);
            b = c + LeftRotate(K36 + b + Xor(c, d, a) + chunk7, 23);
            a = b + LeftRotate(K37 + a + Xor(b, c, d) + chunk1, 4);
            d = a + LeftRotate(K38 + d + Xor(a, b, c) + chunk4, 11);
            c = d + LeftRotate(K39 + c + Xor(d, a, b), 16);
            b = c + LeftRotate(K40 + b + Xor(c, d, a), 23);
            a = b + LeftRotate(K41 + a + Xor(b, c, d), 4);
            d = a + LeftRotate(K42 + d + Xor(a, b, c) + chunk0, 11);
            c = d + LeftRotate(K43 + c + Xor(d, a, b) + chunk3, 16);
            b = c + LeftRotate(K44 + b + Xor(c, d, a) + chunk6, 23);
            a = b + LeftRotate(K45 + a + Xor(b, c, d), 4);
            d = a + LeftRotate(K46 + d + Xor(a, b, c), 11);
            c = d + LeftRotate(K47 + c + Xor(d, a, b), 16);
            b = c + LeftRotate(K48 + b + Xor(c, d, a) + chunk2, 23);

            a = b + LeftRotate(K49 + a + I(c, b, d) + chunk0, 6);
            d = a + LeftRotate(K50 + d + I(b, a, c), 10);
            c = d + LeftRotate(K51 + c + I(a, d, b) + chunk7, 15);
            b = c + LeftRotate(K52 + b + I(d, c, a) + chunk5, 21);
            a = b + LeftRotate(K53 + a + I(c, b, d), 6);
            d = a + LeftRotate(K54 + d + I(b, a, c) + chunk3, 10);
            c = d + LeftRotate(K55 + c + I(a, d, b), 15);
            b = c + LeftRotate(K56 + b + I(d, c, a) + chunk1, 21);
            a = b + LeftRotate(K57 + a + I(c, b, d), 6);
            d = a + LeftRotate(K58 + d + I(b, a, c), 10);
            c = d + LeftRotate(K59 + c + I(a, d, b) + chunk6, 15);
            b = c + LeftRotate(K60 + b + I(d, c, a), 21);
            a = b + LeftRotate(K61 + a + I(c, b, d) + chunk4, 6);
            d = a + LeftRotate(K62 + d + I(b, a, c), 10);
            c = d + LeftRotate(K63 + c + I(a, d, b) + chunk2, 15);
            b = c + LeftRotate(K64 + b + I(d, c, a), 21);

            a += A0;
            b += B0;
            c += C0;
            d += D0;

            return new[]
            {
                a[0],
                b[0],
                c[0],
                d[0],
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector<uint> Blend(Vector<uint> a, Vector<uint> b, Vector<uint> x)
        {
            return (x & b) | (~x & a);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector<uint> Xor(Vector<uint> a, Vector<uint> b, Vector<uint> c)
        {
            return a ^ b ^ c;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector<uint> I(Vector<uint> a, Vector<uint> b, Vector<uint> c)
        {
            return a ^ (b | ~c);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint LeftRotate(uint x, int left)
        {
            return (x << left) | (x >> 32 - left);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector<uint> LeftRotate(Vector<uint> x, int left)
        {
            return new Vector<uint>(new[]
            {
                LeftRotate(x[0], left),
                LeftRotate(x[1], left),
                LeftRotate(x[2], left),
                LeftRotate(x[3], left),
            });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector<uint> GetChunk(Phrase phrase, int offset)
        {
            return new Vector<uint>(new[]
            {
                phrase.Buffer[offset],
                phrase.Buffer[offset],
                phrase.Buffer[offset],
                phrase.Buffer[offset],
            });
        }
    }
}
