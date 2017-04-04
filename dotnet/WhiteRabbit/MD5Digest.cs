using System.Numerics;
using System.Runtime.CompilerServices;
using WhiteRabbitUnmanagedBridge;

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
        public static unsafe Vector<uint> Compute(Phrase input)
        {
            var result = stackalloc uint[4];
            MD5Unmanaged.ComputeMD5(input.Buffer, result);
            return new Vector<uint>(new[] {
                result[0],
                result[1],
                result[2],
                result[3],
            });
        }
    }
}
