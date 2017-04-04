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
                    var hashVectors = MD5Digest.Compute(phraseBytes);
                    for (var i = 0; i < Constants.PhrasesPerSet; i++)
                    {
                        Debug.Assert(
                            sourceChars == ToOrderedChars(ToString(phraseBytes, i)),
                            $"StringsProcessor produced incorrect anagram: {ToString(phraseBytes, i)}");

                        if (Array.IndexOf(expectedHashesAsVectors, hashVectors[i]) >= 0)
                        {
                            var phrase = ToString(phraseBytes, i);
                            var hash = VectorToHexadecimalString(hashVectors[i]);
                            Console.WriteLine($"Found phrase for {hash}: {phrase}; time from start is {stopwatch.Elapsed}");
                        }
                    }
                });

            Console.WriteLine($"Done; time from start: {stopwatch.Elapsed}");

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

        private static string ToString(PhraseSet phrase, int offset)
        {
            return Encoding.ASCII.GetString(phrase.GetBytes(offset));
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
