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
        /// <summary>
        /// Main entry point
        /// </summary>
        public static void Main()
        {
            var expectedHashes = new[]
            {
                "e4820b45d2277f3844eac66c903e84be",
                "23170acc097c24edb98fc5488ab033fe",
                "665e5bcb0c20062fe8abaaf4628bb154",
            };

            var expectedHashesAsVectors = new HashSet<Vector<byte>>(expectedHashes.Select(hash => new Vector<byte>(StringToByteArray(hash))));

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            using (var hasher = MD5.Create())
            {

                var processor = new StringsProcessor("poultry outwits ants", 4, ReadInput());

                var startBlock = DataflowBlockHelpers.Id<Vector<byte>[]>();

                var task = startBlock
                    .Pipe(processor.CreateUnorderedSequencesToPhrasesTransform())
                    .Pipe(phrase =>
                    {
                        //Console.WriteLine("Found phrase: " + phrase);

                        var hash = new Vector<byte>(hasher.ComputeHash(Encoding.ASCII.GetBytes(phrase)));
                        return new PhraseWithHash(phrase, hash);
                    })
                    .PipeMany(phraseWithHash =>
                    {
                        //Console.WriteLine($"Found phrase with hash: " + phraseWithHash.Phrase);

                        if (!expectedHashesAsVectors.Contains(phraseWithHash.Hash))
                        {
                            return Enumerable.Empty<PhraseWithHash>();
                        }

                        return new PhraseWithHash[]
                        {
                            phraseWithHash,
                        };
                    })
                    .LinkForever(phraseWithHash =>
                    {
                        Console.WriteLine($"Found phrase for hash {phraseWithHash.Hash}: {phraseWithHash.Phrase} (spent {stopwatch.Elapsed})");
                    });

                Console.WriteLine($"Initialization complete: time spent: {stopwatch.Elapsed}");
                processor.PostUnorderedSequences(startBlock);

                task.Wait();
                Console.WriteLine($"Total time spent: {stopwatch.Elapsed}");
            }
        }

        // Code taken from http://stackoverflow.com/a/321404/831314
        private static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        private static IEnumerable<Tuple<string, Vector<byte>>> AddHashes(IEnumerable<string> input)
        {
            using (MD5 hasher = MD5.Create())
            {
                foreach (var line in input)
                {
                    var data = hasher.ComputeHash(Encoding.ASCII.GetBytes(line));
                    yield return Tuple.Create(line, new Vector<byte>(data));
                }
            }
        }

        private static IEnumerable<string> ReadInput()
        {
            string line;
            while ((line = Console.ReadLine()) != null)
            {
                yield return line;
            }
        }

        private class PhraseWithHash
        {
            public PhraseWithHash(string phrase, Vector<byte> hash)
            {
                this.Phrase = phrase;
                this.Hash = hash;
            }

            public string Phrase { get; }

            public Vector<byte> Hash { get; }
        }
    }
}
