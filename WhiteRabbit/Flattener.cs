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
            for (var i0 = 0; i0 < phrase[0].Length; i0++)
            {
                for (var i1 = 0; i1 < phrase[1].Length; i1++)
                {
                    for (var i2 = 0; i2 < phrase[2].Length; i2++)
                    {
                        yield return new T[]
                        {
                            phrase[0][i0],
                            phrase[1][i1],
                            phrase[2][i2],
                        };
                    }
                }
            }
        }

        // Fast hard-coded implementation for 4 words
        private static IEnumerable<T[]> Flatten4<T>(T[][] phrase)
        {
            for (var i0 = 0; i0 < phrase[0].Length; i0++)
            {
                for (var i1 = 0; i1 < phrase[1].Length; i1++)
                {
                    for (var i2 = 0; i2 < phrase[2].Length; i2++)
                    {
                        for (var i3 = 0; i3 < phrase[3].Length; i3++)
                        {
                            yield return new T[]
                            {
                                phrase[0][i0],
                                phrase[1][i1],
                                phrase[2][i2],
                                phrase[3][i3],
                            };
                        }
                    }
                }
            }
        }

        // Fast hard-coded implementation for 5 words
        private static IEnumerable<T[]> Flatten5<T>(T[][] phrase)
        {
            for (var i0 = 0; i0 < phrase[0].Length; i0++)
            {
                for (var i1 = 0; i1 < phrase[1].Length; i1++)
                {
                    for (var i2 = 0; i2 < phrase[2].Length; i2++)
                    {
                        for (var i3 = 0; i3 < phrase[3].Length; i3++)
                        {
                            for (var i4 = 0; i4 < phrase[4].Length; i4++)
                            {
                                yield return new T[]
                                {
                                    phrase[0][i0],
                                    phrase[1][i1],
                                    phrase[2][i2],
                                    phrase[3][i3],
                                    phrase[4][i4],
                                };
                            }
                        }
                    }
                }
            }
        }

        // Fast hard-coded implementation for 6 words
        private static IEnumerable<T[]> Flatten6<T>(T[][] phrase)
        {
            for (var i0 = 0; i0 < phrase[0].Length; i0++)
            {
                for (var i1 = 0; i1 < phrase[1].Length; i1++)
                {
                    for (var i2 = 0; i2 < phrase[2].Length; i2++)
                    {
                        for (var i3 = 0; i3 < phrase[3].Length; i3++)
                        {
                            for (var i4 = 0; i4 < phrase[4].Length; i4++)
                            {
                                for (var i5 = 0; i5 < phrase[5].Length; i5++)
                                {
                                    yield return new T[]
                                    {
                                        phrase[0][i0],
                                        phrase[1][i1],
                                        phrase[2][i2],
                                        phrase[3][i3],
                                        phrase[4][i4],
                                    };
                                }
                            }
                        }
                    }
                }
            }
        }

        // Fast hard-coded implementation for 7 words
        private static IEnumerable<T[]> Flatten7<T>(T[][] phrase)
        {
            for (var i0 = 0; i0 < phrase[0].Length; i0++)
            {
                for (var i1 = 0; i1 < phrase[1].Length; i1++)
                {
                    for (var i2 = 0; i2 < phrase[2].Length; i2++)
                    {
                        for (var i3 = 0; i3 < phrase[3].Length; i3++)
                        {
                            for (var i4 = 0; i4 < phrase[4].Length; i4++)
                            {
                                for (var i5 = 0; i5 < phrase[5].Length; i5++)
                                {
                                    for (var i6 = 0; i6 < phrase[6].Length; i6++)
                                    {
                                        yield return new T[]
                                        {
                                            phrase[0][i0],
                                            phrase[1][i1],
                                            phrase[2][i2],
                                            phrase[3][i3],
                                            phrase[4][i4],
                                        };
                                    }
                                }
                            }
                        }
                    }
                }
            }
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
                default:
                    return FlattenAny(ImmutableStack.Create(wordVariants)).Select(words => words.ToArray());
            }
        }
    }
}
