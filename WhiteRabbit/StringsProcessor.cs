namespace WhiteRabbit
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Numerics;

    internal class StringsProcessor
    {
        public StringsProcessor(byte[] sourceString, int maxWordsCount, IEnumerable<byte[]> words)
        {
            var filteredSource = sourceString.Where(ch => ch != 32).ToArray();
            this.NumberOfCharacters = filteredSource.Length;
            this.VectorsConverter = new VectorsConverter(filteredSource);

            // Dictionary of vectors to array of words represented by this vector
            this.VectorsToWords = words
                .Distinct(new ByteArrayEqualityComparer())
                .Select(word => new { word, vector = this.VectorsConverter.GetVector(word) })
                .Where(tuple => tuple.vector != null)
                .Select(tuple => new { tuple.word, vector = tuple.vector.Value })
                .GroupBy(tuple => tuple.vector)
                .ToDictionary(group => group.Key, group => group.Select(tuple => tuple.word).ToArray());

            this.VectorsProcessor = new VectorsProcessor(
                this.VectorsConverter.GetVector(filteredSource).Value,
                maxWordsCount,
                this.VectorsToWords.Keys,
                this.VectorsConverter.GetString);
        }

        private VectorsConverter VectorsConverter { get; }

        private Dictionary<Vector<byte>, byte[][]> VectorsToWords { get; }

        private VectorsProcessor VectorsProcessor { get; }

        private int NumberOfCharacters { get; }

        public ParallelQuery<byte[]> GeneratePhrases()
        {
            // task of finding anagrams could be reduced to the task of finding sequences of dictionary vectors with the target sum
            var sums = this.VectorsProcessor.GenerateSequences();

            // converting sequences of vectors to the sequences of words...
            var anagramsWords = sums
                .Select(sum => ImmutableStack.Create(sum.Select(vector => this.VectorsToWords[vector]).ToArray()))
                .SelectMany(this.Flatten)
                .Select(stack => stack.ToArray());

            return anagramsWords.Select(WordsToPhrase);
        }

        // Converts e.g. pair of variants [[a, b, c], [d, e]] into all possible pairs: [[a, d], [a, e], [b, d], [b, e], [c, d], [c, e]]
        private IEnumerable<ImmutableStack<T>> Flatten<T>(ImmutableStack<T[]> phrase)
        {
            if (phrase.IsEmpty)
            {
                return new[] { ImmutableStack.Create<T>() };
            }

            T[] wordVariants;
            var newStack = phrase.Pop(out wordVariants);
            return this.Flatten(newStack).SelectMany(remainder => wordVariants.Select(word => remainder.Push(word)));
        }

        private byte[] WordsToPhrase(byte[][] words)
        {
            var result = new byte[this.NumberOfCharacters + words.Length - 1];

            Buffer.BlockCopy(words[0], 0, result, 0, words[0].Length);
            var position = words[0].Length;
            for (var i = 1; i < words.Length; i++)
            {
                result[position] = 32;
                position++;

                Buffer.BlockCopy(words[i], 0, result, position, words[i].Length);
                position += words[i].Length;
            }

            return result;
        }
    }
}
