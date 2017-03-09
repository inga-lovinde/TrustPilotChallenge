namespace WhiteRabbit
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Numerics;
    using System.Threading.Tasks.Dataflow;
    internal class StringsProcessor
    {
        public StringsProcessor(string sourceString, int maxWordsCount, IEnumerable<string> words)
        {
            var filteredSource = new string(sourceString.Where(ch => ch != ' ').ToArray());
            this.VectorsConverter = new VectorsConverter(filteredSource);

            // Dictionary of vectors to array of words represented by this vector
            this.VectorsToWords = words
                .Distinct()
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

        private VectorsProcessor VectorsProcessor { get; }

        private VectorsConverter VectorsConverter { get; }

        private Dictionary<Vector<byte>, string[]> VectorsToWords { get; }

        public void PostUnorderedSequences(ITargetBlock<Vector<byte>[]> target)
        {
            this.VectorsProcessor.GenerateUnorderedSequences().WriteToTargetBlock(target);
        }

        public IPropagatorBlock<Vector<byte>[], string> CreateUnorderedSequencesToPhrasesTransform()
        {
            return DataflowBlockHelpers.Id<Vector<byte>[]>()
                .PipeMany(this.VectorsProcessor.UnorderedSequenceToOrderedSequences)
                .Pipe(this.OrderedSequenceToWordVariants)
                .PipeMany(this.WordVariantsToFlatWords)
                .Pipe(this.FlatWordsToPhrase);
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

        private ImmutableStack<string[]> OrderedSequenceToWordVariants(Vector<byte>[] sum)
        {
            return ImmutableStack.CreateRange(sum.Select(vector => this.VectorsToWords[vector]));
        }

        private IEnumerable<ImmutableStack<string>> WordVariantsToFlatWords(ImmutableStack<string[]> wordVariants)
        {
            return this.Flatten(wordVariants);
        }

        private string FlatWordsToPhrase(ImmutableStack<string> words)
        {
            return string.Join(" ", words);
        }
    }
}
