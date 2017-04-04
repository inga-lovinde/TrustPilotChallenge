namespace WhiteRabbit
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.Linq;
    using System.Numerics;
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

            var sourcePhrase = ConfigurationManager.AppSettings["SourcePhrase"];
            var sourceChars = ToOrderedChars(sourcePhrase);

            var maxWordsInPhrase = int.Parse(ConfigurationManager.AppSettings["MaxWordsInPhrase"]);

            if (sourceChars.Length + maxWordsInPhrase > 27)
            {
                Console.WriteLine("Only anagrams of up to 27 characters are allowed");
                return;
            }

            if (!BitConverter.IsLittleEndian)
            {
                Console.WriteLine("Only little-endian systems are supported due to MD5Digest optimizations");
                return;
            }

            if (IntPtr.Size != 8)
            {
                Console.WriteLine("Only 64-bit systems are supported due to MD5Digest optimizations");
            }

            var expectedHashesAsVectors = ConfigurationManager.AppSettings["ExpectedHashes"]
                .Split(',')
                .Select(hash => new Vector<uint>(HexadecimalStringToUnsignedIntArray(hash)))
                .ToArray();

#if DEBUG
            var anagramsBag = new ConcurrentBag<string>();
#endif

            var processor = new StringsProcessor(
                Encoding.ASCII.GetBytes(sourcePhrase),
                maxWordsInPhrase,
                ReadInput());

            Console.WriteLine($"Initialization complete; time from start: {stopwatch.Elapsed}");

#if DEBUG
            var fastPhrasesCount = processor.GetPhrasesCount();
            Console.WriteLine($"Number of phrases: {fastPhrasesCount}; time from start: {stopwatch.Elapsed}");
#endif

            stopwatch.Restart();

            processor.GeneratePhrases()
                .ForAll(phraseBytes =>
                {
                    Debug.Assert(
                        sourceChars == ToOrderedChars(ToString(phraseBytes)),
                        $"StringsProcessor produced incorrect anagram: {ToString(phraseBytes)}");

                    var hashVector = MD5Digest.Compute(phraseBytes);
                    if (Array.IndexOf(expectedHashesAsVectors, hashVector) >= 0)
                    {
                        var phrase = ToString(phraseBytes);
                        var hash = VectorToHexadecimalString(hashVector);
                        Console.WriteLine($"Found phrase for {hash}: {phrase}; time from start is {stopwatch.Elapsed}");
                    }

#if DEBUG
                    anagramsBag.Add(ToString(phraseBytes));
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
        private static uint[] HexadecimalStringToUnsignedIntArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 8 == 0)
                             .Select(x => ChangeEndianness(hex.Substring(x, 8)))
                             .Select(hexLe => Convert.ToUInt32(hexLe, 16))
                             .ToArray();
        }

        private static string VectorToHexadecimalString(Vector<uint> hash)
        {
            var components = Enumerable.Range(0, 4)
                .Select(i => hash[i].ToString("x8"))
                .Select(ChangeEndianness);

            return string.Concat(components);
        }

        private static string ChangeEndianness(string hex)
        {
            return hex.Substring(6, 2) + hex.Substring(4, 2) + hex.Substring(2, 2) + hex.Substring(0, 2);
        }

        private static string ToString(Phrase phrase)
        {
            return Encoding.ASCII.GetString(phrase.GetBytes());
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
