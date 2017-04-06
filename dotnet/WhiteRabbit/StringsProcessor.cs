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
        public IEnumerable<PhraseSet> GeneratePhrases()
#else
        public ParallelQuery<PhraseSet> GeneratePhrases()
#endif
        {
            // task of finding anagrams could be reduced to the task of finding sequences of dictionary vectors with the target sum
            var sums = this.VectorsProcessor.GenerateSequences();

            // converting sequences of vectors to the sequences of words...
            return sums
                .Select(this.ConvertVectorsToWords)
                .SelectMany(Flattener.Flatten)
                .SelectMany(this.ConvertWordsToPhrases);
        }

        public long GetPhrasesCount()
        {
            return this.VectorsProcessor.GenerateSequences()
                .Select(this.ConvertVectorsToWordsNumber)
                .Sum(tuple => tuple.Item2 * PrecomputedPermutationsGenerator.GetPermutationsNumber(tuple.Item1));
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

        private Tuple<int, long> ConvertVectorsToWordsNumber(int[] vectors)
        {
            long result = 1;
            for (var i = 0; i < vectors.Length; i++)
            {
                result *= this.WordsDictionary[vectors[i]].Length;
            }

            return Tuple.Create(vectors.Length, result);
        }

        private IEnumerable<PhraseSet> ConvertWordsToPhrases(byte[][] words)
        {
            var permutations = PrecomputedPermutationsGenerator.HamiltonianPermutations(words.Length);
            var permutationsLength = permutations.Length;
            for (var i = 0; i < permutationsLength; i += Constants.PhrasesPerSet)
            {
                yield return new PhraseSet(words, permutations, i, this.NumberOfCharacters);
            }
        }
    }
}
