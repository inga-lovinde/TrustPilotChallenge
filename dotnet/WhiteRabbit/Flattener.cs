namespace WhiteRabbit
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    /// <summary>
    /// Converts e.g. pair of variants [[a, b, c], [d, e]] into all possible pairs: [[a, d], [a, e], [b, d], [b, e], [c, d], [c, e]]
    /// </summary>
    internal static class Flattener
    {
        // Slow universal implementation
        private static IEnumerable<ImmutableStack<T>> FlattenAny<T>(ImmutableStack<T[]> phrase)
        {
            if (phrase.IsEmpty)
            {
                return new[] { ImmutableStack.Create<T>() };
            }

            T[] wordVariants;
            var newStack = phrase.Pop(out wordVariants);
            return FlattenAny(newStack).SelectMany(remainder => wordVariants.Select(word => remainder.Push(word)));
        }

        // Fast hard-coded implementation for 3 words
        private static IEnumerable<T[]> Flatten3<T>(T[][] phrase)
        {
            foreach (var item0 in phrase[0])
            foreach (var item1 in phrase[1])
            foreach (var item2 in phrase[2])
                yield return new T[]
                {
                    item0,
                    item1,
                    item2,
                };
        }

        // Fast hard-coded implementation for 4 words
        private static IEnumerable<T[]> Flatten4<T>(T[][] phrase)
        {
            foreach (var item0 in phrase[0])
            foreach (var item1 in phrase[1])
            foreach (var item2 in phrase[2])
            foreach (var item3 in phrase[3])
                yield return new T[]
                {
                    item0,
                    item1,
                    item2,
                    item3,
                };
        }

        // Fast hard-coded implementation for 5 words
        private static IEnumerable<T[]> Flatten5<T>(T[][] phrase)
        {
            foreach (var item0 in phrase[0])
            foreach (var item1 in phrase[1])
            foreach (var item2 in phrase[2])
            foreach (var item3 in phrase[3])
            foreach (var item4 in phrase[4])
                yield return new T[]
                {
                    item0,
                    item1,
                    item2,
                    item3,
                    item4,
                };
        }

        // Fast hard-coded implementation for 6 words
        private static IEnumerable<T[]> Flatten6<T>(T[][] phrase)
        {
            foreach (var item0 in phrase[0])
            foreach (var item1 in phrase[1])
            foreach (var item2 in phrase[2])
            foreach (var item3 in phrase[3])
            foreach (var item4 in phrase[4])
            foreach (var item5 in phrase[5])
                yield return new T[]
                {
                    item0,
                    item1,
                    item2,
                    item3,
                    item4,
                    item5,
                };
        }

        // Fast hard-coded implementation for 7 words
        private static IEnumerable<T[]> Flatten7<T>(T[][] phrase)
        {
            foreach (var item0 in phrase[0])
            foreach (var item1 in phrase[1])
            foreach (var item2 in phrase[2])
            foreach (var item3 in phrase[3])
            foreach (var item4 in phrase[4])
            foreach (var item5 in phrase[5])
            foreach (var item6 in phrase[6])
                yield return new T[]
                {
                    item0,
                    item1,
                    item2,
                    item3,
                    item4,
                    item5,
                    item6,
                };
        }

        // Fast hard-coded implementation for 7 words
        private static IEnumerable<T[]> Flatten8<T>(T[][] phrase)
        {
            foreach (var item0 in phrase[0])
            foreach (var item1 in phrase[1])
            foreach (var item2 in phrase[2])
            foreach (var item3 in phrase[3])
            foreach (var item4 in phrase[4])
            foreach (var item5 in phrase[5])
            foreach (var item6 in phrase[6])
            foreach (var item7 in phrase[7])
                yield return new T[]
                {
                    item0,
                    item1,
                    item2,
                    item3,
                    item4,
                    item5,
                    item6,
                    item7,
                };
        }

        public static IEnumerable<T[]> Flatten<T>(T[][] wordVariants)
        {
            switch (wordVariants.Length)
            {
                case 3:
                    return Flatten3(wordVariants);
                case 4:
                    return Flatten4(wordVariants);
                case 5:
                    return Flatten5(wordVariants);
                case 6:
                    return Flatten6(wordVariants);
                case 7:
                    return Flatten7(wordVariants);
                case 8:
                    return Flatten8(wordVariants);
                default:
                    return FlattenAny(ImmutableStack.Create(wordVariants)).Select(words => words.ToArray());
            }
        }
    }
}
