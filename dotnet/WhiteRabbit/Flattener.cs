namespace WhiteRabbit
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    /// <summary>
    /// Converts e.g. pair of variants [[a, b, c], [d, e]] into all possible pairs: [[a, d], [a, e], [b, d], [b, e], [c, d], [c, e]]
    /// </summary>
    internal static class Flattener
    {
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

        private static IEnumerable<T[]> Flatten9<T>(T[][] phrase)
        {
            foreach (var item0 in phrase[0])
            foreach (var item1 in phrase[1])
            foreach (var item2 in phrase[2])
            foreach (var item3 in phrase[3])
            foreach (var item4 in phrase[4])
            foreach (var item5 in phrase[5])
            foreach (var item6 in phrase[6])
            foreach (var item7 in phrase[7])
            foreach (var item8 in phrase[8])
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
                    item8,
                };
        }

        private static IEnumerable<T[]> Flatten10<T>(T[][] phrase)
        {
            foreach (var item0 in phrase[0])
            foreach (var item1 in phrase[1])
            foreach (var item2 in phrase[2])
            foreach (var item3 in phrase[3])
            foreach (var item4 in phrase[4])
            foreach (var item5 in phrase[5])
            foreach (var item6 in phrase[6])
            foreach (var item7 in phrase[7])
            foreach (var item8 in phrase[8])
            foreach (var item9 in phrase[9])
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
                    item8,
                    item9,
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
                case 9:
                    return Flatten9(wordVariants);
                case 10:
                    return Flatten10(wordVariants);
                default:
                    throw new ArgumentOutOfRangeException(nameof(wordVariants));
            }
        }
    }
}
