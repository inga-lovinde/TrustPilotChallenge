namespace WhiteRabbit
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Numerics;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Security.Cryptography;
    using System.Text;

    /// <summary>
    /// Main class
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Main entry point
        /// </summary>
        public static void Main()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var processor = new StringsProcessor("poultry outwits ants", 4);
            var expectedHashes = new[]
            {
                "e4820b45d2277f3844eac66c903e84be",
                "23170acc097c24edb98fc5488ab033fe",
                "665e5bcb0c20062fe8abaaf4628bb154",
            };

            var expectedHashesAsVectors = new HashSet<Vector<byte>>(expectedHashes.Select(hash => new Vector<byte>(StringToByteArray(hash))));

            var phrases = processor.GeneratePhrases(ReadInput());
            using (var hasher = MD5.Create())
            {
                var phrasesWithHashes = phrases
                    .Select(phrase => new { phrase, hash = ComputeHash(hasher, phrase) })
                    .SubscribeOn(NewThreadScheduler.Default);

                var filteredPhrases = phrasesWithHashes.Where(tuple => expectedHashesAsVectors.Contains(tuple.hash));

                foreach (var result in filteredPhrases.ToEnumerable())
                {
                    Console.WriteLine($"Found phrase with hash {HashToString(result.hash)}: {result.phrase} (spent {stopwatch.Elapsed})");
                }
            }

            stopwatch.Stop();
            Console.WriteLine($"Total time spent: {stopwatch.Elapsed}");
        }

        // Code taken from http://stackoverflow.com/a/321404/831314
        private static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        private static Vector<byte> ComputeHash(HashAlgorithm hasher, string phrase)
        {
            return new Vector<byte>(hasher.ComputeHash(Encoding.ASCII.GetBytes(phrase)));
        }

        private static string HashToString(Vector<byte> hash)
        {
            return string.Concat(Enumerable.Range(0, 16).Select(i => hash[i].ToString("x2")));
        }

        private static IEnumerable<string> ReadInput()
        {
            string line;
            while ((line = Console.ReadLine()) != null)
            {
                yield return line;
            }
        }
    }
}
