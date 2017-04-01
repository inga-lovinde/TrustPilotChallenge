namespace WhiteRabbit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal sealed class StringsProcessor
    {
        private const byte SPACE = 32;

        // Ensure that permutations are precomputed prior to main run, so that processing times will be correct
        static StringsProcessor()
        {
            PrecomputedPermutationsGenerator.HamiltonianPermutations(0);
        }

        public StringsProcessor(byte[] sourceString, int maxWordsCount, IEnumerable<byte[]> words)
        {
            var filteredSource = sourceString.Where(ch => ch != SPACE).ToArray();
            this.NumberOfCharacters = filteredSource.Length;
            this.VectorsConverter = new VectorsConverter(filteredSource);

            // Dictionary of vectors to array of words represented by this vector
            var vectorsToWords = words
                .Where(word => word != null && word.Length > 0)
                .Select(word => new { word = word.Concat(new byte[] { SPACE }).ToArray(), vector = this.VectorsConverter.GetVector(word) })
                .Where(tuple => tuple.vector != null)
                .Select(tuple => new { tuple.word, vector = tuple.vector.Value })
                .GroupBy(tuple => tuple.vector)
                .Select(group => new { vector = group.Key, words = group.Select(tuple => tuple.word).Distinct(new ByteArrayEqualityComparer()).ToArray() })
                .ToList();

            this.WordsDictionary = vectorsToWords.Select(tuple => tuple.words).ToArray();

            this.VectorsProcessor = new VectorsProcessor(
                this.VectorsConverter.GetVector(filteredSource).Value,
                maxWordsCount,
                vectorsToWords.Select(tuple => tuple.vector).ToArray());
        }

        private VectorsConverter VectorsConverter { get; }

        /// <summary>
        /// WordsDictionary[vectorIndex] = [word1, word2, ...]
        /// </summary>
        private byte[][][] WordsDictionary { get; }

        private VectorsProcessor VectorsProcessor { get; }

        private int NumberOfCharacters { get; }

#if SINGLE_THREADED
        public IEnumerable<byte[]> GeneratePhrases()
#else
        public ParallelQuery<Phrase> GeneratePhrases()
#endif
        {
            // task of finding anagrams could be reduced to the task of finding sequences of dictionary vectors with the target sum
            var sums = this.VectorsProcessor.GenerateSequences();

            // converting sequences of vectors to the sequences of words...
            return sums
                .Select(this.ConvertVectorsToWords)
                .SelectMany(Flattener.Flatten)
                .SelectMany(GeneratePermutationsChunked)
                .Select(this.ConvertWordsToPhrase);
        }

        private static IEnumerable<T[][]> GeneratePermutationsChunked<T>(T[] original)
        {
            var length = original.Length;
            var permutations = PrecomputedPermutationsGenerator.HamiltonianPermutations(length);
            for (var i = 0; i < permutations.Length; i += 4)
            {
                yield return new[]
                {
                    GetPermutedArray(original, permutations[i]),
                    GetPermutedArray(original, permutations[i + 1]),
                    GetPermutedArray(original, permutations[i + 2]),
                    GetPermutedArray(original, permutations[i + 3]),
                };
            }
        }

        private static T[] GetPermutedArray<T>(T[] original, PermutationsGenerator.Permutation permutation)
        {
            var length = original.Length;
            var result = new T[length];
            for (var i = 0; i < length; i++)
            {
                result[i] = original[permutation[i]];
            }
            return result;
        }

        private byte[][][] ConvertVectorsToWords(int[] vectors)
        {
            var length = vectors.Length;
            var words = new byte[length][][];
            for (var i = 0; i < length; i++)
            {
                words[i] = this.WordsDictionary[vectors[i]];
            }

            return words;
        }

        private unsafe PhrasesChunk ConvertWordsToPhrase(byte[][][] words)
        {
            return new PhrasesChunk(words[0], words[1], words[2], words[3], this.NumberOfCharacters);
        }
    }
}
