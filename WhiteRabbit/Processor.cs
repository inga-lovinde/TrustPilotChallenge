namespace WhiteRabbit
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Numerics;

    internal class Processor
    {
        private const int DifferentChars = 12;
        private const int ArraySize = DifferentChars * sizeof(int);

        public Processor(string sourceString, int maxWordsCount)
        {
            var filteredSource = new string(sourceString.Where(ch => ch != ' ').ToArray());
            this.VectorsConverter = new VectorsConverter(filteredSource);
            this.Target = this.VectorsConverter.GetVector(filteredSource).Value;
            this.MaxWordsCount = maxWordsCount;
        }

        private static Vector<byte> Negative { get; } = new Vector<byte>(Enumerable.Repeat((byte)128, 16).ToArray());

        private VectorsConverter VectorsConverter { get; }

        private Vector<byte> Target { get; }

        private int MaxWordsCount { get; }

        private long Iterations { get; set; } = 0;

        public IEnumerable<string> GeneratePhrases(IEnumerable<string> words)
        {
            var formattedWordsList = FormatWords(words);
            var formattedWords = formattedWordsList.ToDictionary(tuple => tuple.Item1, tuple => tuple.Item2);

            var dictionary = ImmutableStack.Create(formattedWordsList.Select(tuple => tuple.Item1).ToArray());
            var anagrams = GenerateOrderedPhrases(this.Target, ImmutableStack.Create<Vector<byte>>(), dictionary);
            var anagramsWithPermutations = anagrams.SelectMany(GeneratePermutations);
            var anagramsWords = anagramsWithPermutations
                .Select(list => ImmutableStack.Create(list.Select(wordArray => formattedWords[wordArray]).ToArray()))
                .SelectMany(Flatten)
                .Select(stack => stack.ToArray());

            return anagramsWords.Select(list => string.Join(" ", list));
        }

        private List<Tuple<Vector<byte>, string[]>> FormatWords(IEnumerable<string> words)
        {
            return words
                .Distinct()
                .Select(word => new { word, vector = this.VectorsConverter.GetVector(word) })
                .Where(tuple => tuple.vector != null)
                .Select(tuple => new { tuple.word, vector = tuple.vector.Value })
                .Where(tuple => ((this.Target - tuple.vector) & Negative) == Vector<byte>.Zero)
                .GroupBy(tuple => tuple.vector)
                .Select(group => Tuple.Create(group.Key, group.Select(tuple => tuple.word).ToArray()))
                .OrderByDescending(tuple => this.VectorsConverter.GetString(tuple.Item1)) //so that letters that are more rare will come first
                .ToList();
        }

        // This method takes most of the time, so everything related to it must be optimized
        private IEnumerable<Vector<byte>[]> GenerateOrderedPhrases(Vector<byte> currentState, ImmutableStack<Vector<byte>> phraseStack, ImmutableStack<Vector<byte>> dictionaryStack)
        {
            var count = phraseStack.Count() + 1;
            if (count < this.MaxWordsCount)
            {
                var remainder = dictionaryStack;
                while (!remainder.IsEmpty)
                {
                    Vector<byte> currentWord;
                    var nextRemainder = remainder.Pop(out currentWord);

                    this.Iterations++;
                    if (this.Iterations % 1000000 == 0)
                    {
                        Console.WriteLine($"Iteration #{this.Iterations}: {string.Join(" ", phraseStack.Push(currentWord).Reverse().Select(word => this.VectorsConverter.GetString(word)))}");
                    }

                    var newState = currentState - currentWord;
                    if (newState == Vector<byte>.Zero)
                    {
                        yield return phraseStack.Push(currentWord).Reverse().ToArray();
                    }
                    else if ((newState & Negative) == Vector<byte>.Zero)
                    {
                        foreach (var result in GenerateOrderedPhrases(newState, phraseStack.Push(currentWord), remainder))
                        {
                            yield return result;
                        }
                    }

                    remainder = nextRemainder;
                }
            }
            else if (count == this.MaxWordsCount)
            {
                var remainder = dictionaryStack;
                while (!remainder.IsEmpty)
                {
                    Vector<byte> currentWord;
                    var nextRemainder = remainder.Pop(out currentWord);

                    this.Iterations++;
                    if (this.Iterations % 1000000 == 0)
                    {
                        Console.WriteLine($"Iteration #{this.Iterations}: {string.Join(" ", phraseStack.Push(currentWord).Reverse().Select(word => this.VectorsConverter.GetString(word)))}");
                    }

                    var newState = currentState - currentWord;
                    if (newState == Vector<byte>.Zero)
                    {
                        yield return phraseStack.Push(currentWord).Reverse().ToArray();
                    }

                    remainder = nextRemainder;
                }
            }
        }

        private IEnumerable<T[]> GeneratePermutations<T>(T[] original)
        {
            foreach (var permutation in PermutationsGenerator.HamiltonianPermutations(original.Length))
            {
                yield return permutation.Select(i => original[i]).ToArray();
            }
        }

        private IEnumerable<ImmutableStack<string>> Flatten(ImmutableStack<string[]> phrase)
        {
            if (phrase.IsEmpty)
            {
                return new[] { ImmutableStack.Create<string>() };
            }

            string[] wordVariants;
            var newStack = phrase.Pop(out wordVariants);
            return Flatten(newStack).SelectMany(remainder => wordVariants.Select(word => remainder.Push(word)));
        }
    }
}
