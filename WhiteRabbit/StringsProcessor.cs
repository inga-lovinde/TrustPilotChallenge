namespace WhiteRabbit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal sealed class StringsProcessor
    {
        private const byte SPACE = 32;

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
                .Select(this.ConvertWordsToPhrase);
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

        private unsafe Phrase ConvertWordsToPhrase(byte[][] words)
        {
            return new Phrase(words, this.NumberOfCharacters);
        }
    }
}
