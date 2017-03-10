namespace WhiteRabbit
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Numerics;
    using System.Security.Cryptography;
    using System.Text;

    /// <summary>
    /// Main class
    /// </summary>
    public static class Program
    {
        const string SourcePhrase = "poultry outwits ants";

        const int MaxWordsInPhrase = 4;

        /// <summary>
        /// Main entry point
        /// </summary>
        public static void Main()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var processor = new StringsProcessor(Encoding.ASCII.GetBytes(SourcePhrase), MaxWordsInPhrase);
            var expectedHashes = new[]
            {
                "e4820b45d2277f3844eac66c903e84be",
                "23170acc097c24edb98fc5488ab033fe",
                "665e5bcb0c20062fe8abaaf4628bb154",
            };

            var expectedHashesAsVectors = new HashSet<Vector<byte>>(expectedHashes.Select(hash => new Vector<byte>(StringToByteArray(hash))));

            foreach (var result in AddHashes(processor.GeneratePhrases(ReadInput())))
            {
                if (expectedHashesAsVectors.Contains(result.Item2))
                {
                    Console.WriteLine($"Found phrase: {Encoding.ASCII.GetString(result.Item1)} (spent {stopwatch.Elapsed})");
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

        private static IEnumerable<Tuple<byte[], Vector<byte>>> AddHashes(IEnumerable<byte[]> input)
        {
            using (MD5 hasher = MD5.Create())
            {
                foreach (var line in input)
                {
                    var data = hasher.ComputeHash(line);
                    yield return Tuple.Create(line, new Vector<byte>(data));
                }
            }
        }

        private static IEnumerable<byte[]> ReadInput()
        {
            string line;
            while ((line = Console.ReadLine()) != null)
            {
                yield return Encoding.ASCII.GetBytes(line);
            }

            //System.Threading.Thread.Sleep(10000);
        }
    }
}
