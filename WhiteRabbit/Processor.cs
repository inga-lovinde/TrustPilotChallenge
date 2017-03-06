namespace WhiteRabbit
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    internal class Processor
    {
        private const int DifferentChars = 12;
        private const int ArraySize = DifferentChars * sizeof(int);

        public Processor(string sourceString, int maxWordsCount)
        {
            var rawNumberOfOccurrences = sourceString.Where(ch => ch != ' ').GroupBy(ch => ch).ToDictionary(group => group.Key, group => group.Count());
            this.IntToChar = rawNumberOfOccurrences.Select(kvp => kvp.Key).OrderBy(ch => ch).ToArray();
            if (this.IntToChar.Length != DifferentChars)
            {
                throw new ArgumentException("Unsupported phrase", nameof(sourceString));
            }
            this.CharToInt = Enumerable.Range(0, DifferentChars).ToDictionary(i => this.IntToChar[i], i => i);
            this.NumberOfOccurrences = Enumerable.Range(0, DifferentChars).Select(i => this.IntToChar[i]).Select(ch => rawNumberOfOccurrences.ContainsKey(ch) ? rawNumberOfOccurrences[ch] : 0).ToArray();
            this.MaxWordsCount = maxWordsCount;
        }

        private Dictionary<char, int> CharToInt { get; }

        private char[] IntToChar { get; }

        private int[] NumberOfOccurrences { get; }

        private int TotalCharsNumber { get; }

        private int MaxWordsCount { get; }

        private long Iterations { get; set; } = 0;

        public IEnumerable<string> GeneratePhrases(IEnumerable<string> words)
        {
            var filtered = FilterWords(words);
            var dictionary = ImmutableStack.Create(filtered.Reverse().ToArray());
            var anagrams = GenerateOrderedPhrases(this.NumberOfOccurrences, ImmutableStack.Create<int[]>(), dictionary)
                .Select(list => list.Select(word => new string(word.Select(i => this.IntToChar[i]).ToArray())).ToArray());
            var anagramsWithPermutations = anagrams.SelectMany(GeneratePermutations).Select(list => string.Join(" ", list));
            return anagramsWithPermutations;
        }

        private IEnumerable<int[]> FilterWords(IEnumerable<string> words)
        {
            return words
                .Where(word => word.All(this.CharToInt.ContainsKey))
                .OrderBy(word => word)
                .Distinct()
                .Select(word => word.Select(ch => this.CharToInt[ch]).ToArray())
                .Where(word => word.GroupBy(ch => ch).All(group => group.Count() <= this.NumberOfOccurrences[group.Key]));
        }

        private int[] GetStatus(int[] originalState, int[] newWord, out int status)
        {
            var tmpArray = new int[DifferentChars];
            Buffer.BlockCopy(originalState, 0, tmpArray, 0, ArraySize);

            foreach (var ch in newWord)
            {
                --tmpArray[ch];
            }

            // Negative if at least one element is negative; zero if all elements are zero; positive if all elements are non-negative and at least one element is positive
            status = tmpArray[0] | tmpArray[1] | tmpArray[2] | tmpArray[3] | tmpArray[4] | tmpArray[5] | tmpArray[6] | tmpArray[7] | tmpArray[8] | tmpArray[9] | tmpArray[10] | tmpArray[11];
            return tmpArray;
        }

        // This method takes most of the time, so everything related to it must be optimized
        private IEnumerable<int[][]> GenerateOrderedPhrases(int[] currentState, ImmutableStack<int[]> phraseStack, ImmutableStack<int[]> dictionaryStack)
        {
            var remainder = dictionaryStack;
            var count = phraseStack.Count() + 1;
            while (!remainder.IsEmpty)
            {
                int[] currentWord;
                var nextRemainder = remainder.Pop(out currentWord);

                this.Iterations++;
                if (this.Iterations % 1000000 == 0)
                {
                    Console.WriteLine($"Iteration #{this.Iterations}: {string.Join(" ", phraseStack.Push(currentWord).Reverse().Select(word => new string(word.Select(ch => this.IntToChar[ch]).ToArray())))}");
                }

                int status;
                var state = GetStatus(currentState, currentWord, out status);
                if (status > 0 && count < this.MaxWordsCount)
                {
                    foreach (var result in GenerateOrderedPhrases(state, phraseStack.Push(currentWord), remainder))
                    {
                        yield return result;
                    }
                }
                else if (status == 0)
                {
                    yield return phraseStack.Push(currentWord).Reverse().ToArray();
                }

                remainder = nextRemainder;
            }
        }

        private IEnumerable<string[]> GeneratePermutations(string[] original)
        {
            foreach (var permutation in PermutationsGenerator.HamiltonianPermutations(original.Length))
            {
                yield return permutation.Select(i => original[i]).ToArray();
            }
        }
    }
}
