namespace WhiteRabbit
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Numerics;
    using System.Text;
    using Org.BouncyCastle.Crypto.Digests;

    /// <summary>
    /// Main class
    /// </summary>
    public static class Program
    {
        private const string SourcePhrase = "poultry outwits ants";

        private const int MaxWordsInPhrase = 4;

        /// <summary>
        /// Main entry point
        /// </summary>
        public static void Main()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var expectedHashes = new[]
            {
                "e4820b45d2277f3844eac66c903e84be",
                "23170acc097c24edb98fc5488ab033fe",
                "665e5bcb0c20062fe8abaaf4628bb154",
            };

            var expectedHashesAsVectors = expectedHashes.Select(hash => new Vector<byte>(HexadecimalStringToByteArray(hash))).ToArray();

            var processor = new StringsProcessor(Encoding.ASCII.GetBytes(SourcePhrase), MaxWordsInPhrase, ReadInput());

            Console.WriteLine($"Initialization complete; time from start: {stopwatch.Elapsed}");

#if DEBUG
            // it makes the program slow (as all anagrams are generated twice), but this code is only run in a debug mode
            var totalAnagramsCount = processor.GeneratePhrases().Count();
            Console.WriteLine($"Total anagrams count: {totalAnagramsCount}; time from start: {stopwatch.Elapsed}");
#endif

            processor.GeneratePhrases()
                .Select(phraseBytes => new { phraseBytes, hashVector = ComputeHashVector(phraseBytes) })
                .Where(tuple => Array.IndexOf(expectedHashesAsVectors, tuple.hashVector) >= 0)
                .Select(tuple => new { phrase = Encoding.ASCII.GetString(tuple.phraseBytes), hash = VectorToHexadecimalString(tuple.hashVector) })
                .ForAll(phraseInfo => Console.WriteLine($"Found phrase for {phraseInfo.hash}: {phraseInfo.phrase}; time from start is {stopwatch.Elapsed}"));

            stopwatch.Stop();
            Console.WriteLine($"Done; time from start: {stopwatch.Elapsed}");
        }

        // Code taken from http://stackoverflow.com/a/321404/831314
        private static byte[] HexadecimalStringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        // Bouncy Castle is used instead of standard .NET methods for performance reasons
        private static Vector<byte> ComputeHashVector(byte[] input)
        {
            var digest = new MD5Digest();
            digest.BlockUpdate(input, 0, input.Length);
            byte[] hash = new byte[16];
            digest.DoFinal(hash, 0);
            return new Vector<byte>(hash);
        }

        private static string VectorToHexadecimalString(Vector<byte> hash)
        {
            return string.Concat(Enumerable.Range(0, 16).Select(i => hash[i].ToString("x2")));
        }

        private static IEnumerable<byte[]> ReadInput()
        {
            string line;
            while ((line = Console.ReadLine()) != null)
            {
                yield return Encoding.ASCII.GetBytes(line);
            }
        }

#if SINGLE_THREADED
        private static void ForAll<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var entry in source)
            {
                action(entry);
            }
        }
#endif
    }
}
