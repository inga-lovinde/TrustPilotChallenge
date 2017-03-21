namespace WhiteRabbit
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    internal sealed class StringsProcessor
    {
        public StringsProcessor(byte[] sourceString, int maxWordsCount, IEnumerable<byte[]> words)
        {
            var filteredSource = sourceString.Where(ch => ch != 32).ToArray();
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
        public ParallelQuery<byte[]> GeneratePhrases()
#endif
        {
            // task of finding anagrams could be reduced to the task of finding sequences of dictionary vectors with the target sum
            var sums = this.VectorsProcessor.GenerateSequences();

            // converting sequences of vectors to the sequences of words...
            return sums
                .Select(this.ConvertVectorsToWords)
                .SelectMany(FlattenWords)
                .Select(this.ConvertWordsToPhrase);
        }

        // Converts e.g. pair of variants [[a, b, c], [d, e]] into all possible pairs: [[a, d], [a, e], [b, d], [b, e], [c, d], [c, e]]
        private static IEnumerable<ImmutableStack<T>> Flatten<T>(ImmutableStack<T[]> phrase)
        {
            if (phrase.IsEmpty)
            {
                return new[] { ImmutableStack.Create<T>() };
            }

            T[] wordVariants;
            var newStack = phrase.Pop(out wordVariants);
            return Flatten(newStack).SelectMany(remainder => wordVariants.Select(word => remainder.Push(word)));
        }

        private static IEnumerable<Tuple<int, ImmutableStack<byte[]>>> FlattenWords(Tuple<int, ImmutableStack<byte[][]>> wordVariants)
        {
            var item1 = wordVariants.Item1;
            return Flatten(wordVariants.Item2).Select(words => Tuple.Create(item1, words));
        }

        private Tuple<int, ImmutableStack<byte[][]>> ConvertVectorsToWords(int[] vectors)
        {
            var length = vectors.Length;
            var words = new byte[length][][];
            for (var i = 0; i < length; i++)
            {
                words[i] = this.WordsDictionary[vectors[i]];
            }

            return Tuple.Create(length, ImmutableStack.Create(words));
        }

        private byte[] ConvertWordsToPhrase(Tuple<int, ImmutableStack<byte[]>> words)
        {
            var wordCount = words.Item1;
            var result = new byte[this.NumberOfCharacters + wordCount - 1];

            byte[] currentWord;
            var currentStack = words.Item2.Pop(out currentWord);
            Buffer.BlockCopy(currentWord, 0, result, 0, currentWord.Length);
            var position = currentWord.Length;
            while (!currentStack.IsEmpty)
            {
                result[position] = 32;
                position++;

                currentStack = currentStack.Pop(out currentWord);
                Buffer.BlockCopy(currentWord, 0, result, position, currentWord.Length);
                position += currentWord.Length;
            }

            return result;
        }
    }
}
