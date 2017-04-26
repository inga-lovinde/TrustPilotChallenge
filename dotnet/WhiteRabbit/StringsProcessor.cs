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
            PrecomputedPermutationsGenerator.HamiltonianPermutations(1, 0);
        }

        public StringsProcessor(byte[] sourceString, int maxWordsCount, IEnumerable<byte[]> words)
        {
            var filteredSource = sourceString.Where(ch => ch != SPACE).ToArray();
            this.NumberOfCharacters = filteredSource.Length;
            this.VectorsConverter = new VectorsConverter(filteredSource);

            // Dictionary of vectors to array of words represented by this vector
            var vectorsToWords = words
                .Where(word => word != null && word.Length > 0)
                .Select(word => new { word, vector = this.VectorsConverter.GetVector(word) })
                .Where(tuple => tuple.vector != null)
                .Select(tuple => new { tuple.word, vector = tuple.vector.Value })
                .GroupBy(tuple => tuple.vector)
                .Select(group => new { vector = group.Key, words = group.Select(tuple => tuple.word).Distinct(new ByteArrayEqualityComparer()).Select(word => new Word(word)).ToArray() })
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
        private Word[][] WordsDictionary { get; }

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
            return from sum in sums
                   let filter = ComputeFilter(sum)
                   let wordsVariants = this.ConvertVectorsToWords(sum)
                   from wordsArray in Flattener.Flatten(wordsVariants)
                   from phraseSet in this.ConvertWordsToPhrases(wordsArray, filter)
                   select phraseSet;
        }

        public long GetPhrasesCount()
        {
            return this.VectorsProcessor.GenerateSequences()
                .Select(this.ConvertVectorsToWordsNumber)
                .Sum(tuple => tuple.Item2 * PrecomputedPermutationsGenerator.GetPermutationsNumber(tuple.Item1));
        }

        private static uint ComputeFilter(int[] vectors)
        {
            uint result = 0;
            for (var i = 1; i < vectors.Length; i++)
            {
                if (vectors[i] == vectors[i - 1])
                {
                    result |= (uint)1 << (i - 1);
                }
            }

            return result;
        }

        private Word[][] ConvertVectorsToWords(int[] vectors)
        {
            var length = vectors.Length;
            var words = new Word[length][];
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

        private IEnumerable<PhraseSet> ConvertWordsToPhrases(Word[] words, uint filter)
        {
            var permutations = PrecomputedPermutationsGenerator.HamiltonianPermutations(words.Length, filter);
            var permutationsLength = permutations.Length;
            for (var i = 0; i < permutationsLength; i += Constants.PhrasesPerSet)
            {
                yield return new PhraseSet(words, permutations, i, this.NumberOfCharacters);
            }
        }
    }
}
