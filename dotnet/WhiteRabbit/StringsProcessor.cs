namespace WhiteRabbit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
    using System.Threading.Tasks;

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

            var allWordsAndVectors = words
                .Where(word => word != null && word.Length > 0)
                .Select(word => new { word, vector = this.VectorsConverter.GetVector(word) })
                .Where(tuple => tuple.vector != null)
                .Select(tuple => tuple.word)
                .Distinct(new ByteArrayEqualityComparer())
                .Select(word => word)
                .ToArray();

            // Dictionary of vectors to array of words represented by this vector
            var vectorsToWords = allWordsAndVectors
                .Select((word, index) => new { word, index, vector = this.VectorsConverter.GetVector(word).Value })
                .GroupBy(tuple => tuple.vector)
                .Select(group => new { vector = group.Key, words = group.Select(tuple => tuple.index).ToArray() })
                .ToList();

            this.WordsDictionary = vectorsToWords.Select(tuple => tuple.words).ToArray();

            this.AllWords = allWordsAndVectors.Select(word => new Word(word)).ToArray();

            this.VectorsProcessor = new VectorsProcessor(
                this.VectorsConverter.GetVector(filteredSource).Value,
                maxWordsCount,
                vectorsToWords.Select(tuple => tuple.vector).ToArray());
        }

        private VectorsConverter VectorsConverter { get; }

        private Word[] AllWords { get; }

        /// <summary>
        /// WordsDictionary[vectorIndex] = [word1index, word2index, ...]
        /// </summary>
        private int[][] WordsDictionary { get; }

        private VectorsProcessor VectorsProcessor { get; }

        private int NumberOfCharacters { get; }

        public void CheckPhrases(Vector<uint> expectedHashes, Action<byte[], uint> action)
        {
            // task of finding anagrams could be reduced to the task of finding sequences of dictionary vectors with the target sum
            var sums = this.VectorsProcessor.GenerateSequences();

            // converting sequences of vectors to the sequences of words...
            Parallel.ForEach(sums, new ParallelOptions { MaxDegreeOfParallelism = Constants.NumberOfThreads }, sum => ProcessSum(sum, expectedHashes, action));
        }

        public long GetPhrasesCount()
        {
            var sums = this.VectorsProcessor.GenerateSequences();
            return (from sum in sums
                    let filter = ComputeFilter(sum)
                    let wordsVariantsNumber = this.ConvertVectorsToWordsNumber(sum)
                    let permutationsNumber = PrecomputedPermutationsGenerator.GetPermutationsNumber(sum.Length, filter)
                    let total = wordsVariantsNumber * permutationsNumber
                    select total)
                    .Sum();
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

        private int[][] ConvertVectorsToWordIndexes(int[] vectors)
        {
            var length = vectors.Length;
            var words = new int[length][];
            for (var i = 0; i < length; i++)
            {
                words[i] = this.WordsDictionary[vectors[i]];
            }

            return words;
        }

        private long ConvertVectorsToWordsNumber(int[] vectors)
        {
            long result = 1;
            for (var i = 0; i < vectors.Length; i++)
            {
                result *= this.WordsDictionary[vectors[i]].Length;
            }

            return result;
        }

        private void ProcessSum(int[] sum, Vector<uint> expectedHashes, Action<byte[], uint> action)
        {
            var initialPhraseSet = new PhraseSet();
            initialPhraseSet.Init();
            initialPhraseSet.FillLength(this.NumberOfCharacters, sum.Length);
            var phraseSet = new PhraseSet();
            phraseSet.Init();
            var permutationsFilter = ComputeFilter(sum);
            var wordsVariants = this.ConvertVectorsToWordIndexes(sum);
            foreach (var wordsArray in Flattener.Flatten(wordsVariants))
            {
                phraseSet.ProcessPermutations(
                    initialPhraseSet,
                    this.AllWords,
                    wordsArray,
                    PrecomputedPermutationsGenerator.HamiltonianPermutations(wordsArray.Length, permutationsFilter),
                    expectedHashes,
                    action);
            }
        }
    }
}
