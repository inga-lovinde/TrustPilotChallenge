﻿namespace WhiteRabbit
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.IO;
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
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var sourcePhrase = ConfigurationManager.AppSettings["SourcePhrase"];

            var maxWordsInPhrase = int.Parse(ConfigurationManager.AppSettings["MaxWordsInPhrase"]);

            if (sourcePhrase.Where(ch => ch != ' ').Count() + maxWordsInPhrase > 28)
            {
                Console.WriteLine("Only anagrams of up to 27 characters (including whitespace) are allowed");
                return;
            }

            if (maxWordsInPhrase > Constants.MaxNumberOfWords)
            {
                Console.WriteLine($"Only anagrams of up to {Constants.MaxNumberOfWords} words are allowed");
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

            var expectedHashesFirstComponentsArray = new uint[8];
            {
                int i = 0;
                foreach (var expectedHash in ConfigurationManager.AppSettings["ExpectedHashes"].Split(','))
                {
                    expectedHashesFirstComponentsArray[i] = HexadecimalStringToUnsignedIntArray(expectedHash)[0];
                    expectedHashesFirstComponentsArray[i + 1] = HexadecimalStringToUnsignedIntArray(expectedHash)[0];
                    i += 2;
                }
            }

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

            processor.CheckPhrases(expectedHashesFirstComponentsArray, (phraseBytes, hashFirstComponent) =>
            {
                var phrase = Encoding.ASCII.GetString(phraseBytes);
                var hash = ComputeFullMD5(phraseBytes);
                Console.WriteLine($"Found phrase for {hash} ({hashFirstComponent:x8}): {phrase}; time from start is {stopwatch.Elapsed}");
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

        // We can afford to spend some time here; this code will only run for matched phrases (and for one in several billion non-matched)
        private static string ComputeFullMD5(byte[] phraseBytes)
        {
            using (var hashAlgorithm = new MD5CryptoServiceProvider())
            {
                var resultBytes = hashAlgorithm.ComputeHash(phraseBytes);
                return string.Concat(resultBytes.Select(b => b.ToString("x2")));
            }
        }

        private static string ChangeEndianness(string hex)
        {
            return hex.Substring(6, 2) + hex.Substring(4, 2) + hex.Substring(2, 2) + hex.Substring(0, 2);
        }

        private static IEnumerable<byte[]> ReadInput()
        {
            string line;
            while ((line = Console.ReadLine()) != null)
            {
                yield return Encoding.ASCII.GetBytes(line);
            }
        }
    }
}
