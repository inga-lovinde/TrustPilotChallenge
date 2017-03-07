﻿namespace WhiteRabbit
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    internal class StringsProcessor
    {
        public StringsProcessor(string sourceString, int maxWordsCount)
        {
            var filteredSource = new string(sourceString.Where(ch => ch != ' ').ToArray());
            this.VectorsConverter = new VectorsConverter(filteredSource);
            this.VectorsProcessor = new VectorsProcessor(
                this.VectorsConverter.GetVector(filteredSource).Value,
                maxWordsCount,
                this.VectorsConverter.GetString);
        }

        private VectorsConverter VectorsConverter { get; }

        private VectorsProcessor VectorsProcessor { get; }

        public IEnumerable<string> GeneratePhrases(IEnumerable<string> words)
        {
            // Dictionary of vectors to array of words represented by this vector
            var formattedWords = words
                .Distinct()
                .Select(word => new { word, vector = this.VectorsConverter.GetVector(word) })
                .Where(tuple => tuple.vector != null)
                .Select(tuple => new { tuple.word, vector = tuple.vector.Value })
                .GroupBy(tuple => tuple.vector)
                .ToDictionary(group => group.Key, group => group.Select(tuple => tuple.word).ToArray());

            // task of finding anagrams could be reduced to the task of finding sequences of dictionary vectors with the target sum
            var sums = this.VectorsProcessor.GenerateSequences(formattedWords.Keys);

            // converting sequences of vectors to the sequences of words...
            var anagramsWords = sums
                .Select(sum => ImmutableStack.Create(sum.Select(vector => formattedWords[vector]).ToArray()))
                .SelectMany(Flatten)
                .Select(stack => stack.ToArray());

            return anagramsWords.Select(list => string.Join(" ", list));
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
            return Flatten(newStack).SelectMany(remainder => wordVariants.Select(word => remainder.Push(word)));
        }
    }
}
