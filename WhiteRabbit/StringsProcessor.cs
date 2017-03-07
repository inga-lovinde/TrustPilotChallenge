namespace WhiteRabbit
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
            var formattedWords = words
                .Distinct()
                .Select(word => new { word, vector = this.VectorsConverter.GetVector(word) })
                .Where(tuple => tuple.vector != null)
                .Select(tuple => new { tuple.word, vector = tuple.vector.Value })
                .GroupBy(tuple => tuple.vector)
                .ToDictionary(group => group.Key, group => group.Select(tuple => tuple.word).ToArray());


            var sums = this.VectorsProcessor.GenerateSums(formattedWords.Keys);

            var anagramsWords = sums
                .Select(sum => ImmutableStack.Create(sum.Select(vector => formattedWords[vector]).ToArray()))
                .SelectMany(Flatten)
                .Select(stack => stack.ToArray());

            return anagramsWords.Select(list => string.Join(" ", list));
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
