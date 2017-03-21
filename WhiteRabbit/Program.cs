namespace WhiteRabbit
{
    using System;
    using System.Collections.Concurrent;
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

        private const int MaxWordsInPhrase = 5;

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

            var anagramsBag = new ConcurrentBag<string>();
            var sourceChars = ToOrderedChars(SourcePhrase);

            var processor = new StringsProcessor(Encoding.ASCII.GetBytes(SourcePhrase), MaxWordsInPhrase, ReadInput());

            Console.WriteLine($"Initialization complete; time from start: {stopwatch.Elapsed}");

            processor.GeneratePhrases()
                .ForAll(phraseBytes =>
                {
                    Debug.Assert(
                        sourceChars == ToOrderedChars(Encoding.ASCII.GetString(phraseBytes)),
                        $"StringsProcessor produced incorrect anagram: {Encoding.ASCII.GetString(phraseBytes)}");

                    var hashVector = ComputeHashVector(phraseBytes);
                    if (Array.IndexOf(expectedHashesAsVectors, hashVector) >= 0)
                    {
                        var phrase = Encoding.ASCII.GetString(phraseBytes);
                        var hash = VectorToHexadecimalString(hashVector);
                        Console.WriteLine($"Found phrase for {hash}: {phrase}; time from start is {stopwatch.Elapsed}");
                    }

#if DEBUG
                    anagramsBag.Add(Encoding.ASCII.GetString(phraseBytes));
#endif
                });

            Console.WriteLine($"Done; time from start: {stopwatch.Elapsed}");

#if DEBUG
            var anagramsArray = anagramsBag.ToArray();
            var anagramsSet = new HashSet<string>(anagramsArray);
            Array.Sort(anagramsArray);

            Console.WriteLine("All anagrams:");
            for (var i = 0; i < anagramsArray.Length; i++)
            {
                Console.WriteLine(anagramsArray[i]);
            }

            // Duplicate anagrams are expected, as e.g. "norway spoils tut tut" will be taken twice:
            // as "norway1 spoils2 tut3 tut4" and "norway1 spoils2 tut4 tut3"
            // (in addition to e.g. "norway1 tut3 spoils2 tut4")
            Console.WriteLine($"Total anagrams count: {anagramsArray.Length}; unique anagrams: {anagramsSet.Count}; time from start: {stopwatch.Elapsed}");
#endif
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

        private static string ToOrderedChars(string source)
        {
            return new string(source.Where(ch => ch != ' ').OrderBy(ch => ch).ToArray());
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
