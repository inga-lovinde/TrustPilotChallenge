namespace WhiteRabbit
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Code taken from https://ericlippert.com/2013/04/22/producing-permutations-part-three/
    /// </summary>
    internal sealed class PermutationsGenerator
    {
        public static IEnumerable<Permutation> HamiltonianPermutations(int n)
        {
            if (n < 0)
            {
                throw new ArgumentOutOfRangeException("n");
            }

            return Permutation.HamiltonianPermutationsIterator(n);
        }

        public struct Permutation : IEnumerable<int>
        {
            private Permutation(int[] permutation)
            {
                this.PermutationData = permutation;
            }

            private Permutation(IEnumerable<int> permutation)
                : this(permutation.ToArray())
            {
            }

            public static Permutation Empty { get; } = new Permutation(new int[] { });

            public int[] PermutationData { get; }

            public static IEnumerable<Permutation> HamiltonianPermutationsIterator(int n)
            {
                if (n == 0)
                {
                    yield return Empty;
                    yield break;
                }

                bool forwards = false;
                foreach (Permutation permutation in HamiltonianPermutationsIterator(n - 1))
                {
                    for (int index = 0; index < n; index += 1)
                    {
                        yield return new Permutation(Extensions.InsertAt(permutation, forwards ? index : n - index - 1, n - 1));
                    }

                    forwards = !forwards;
                }
            }

            public IEnumerator<int> GetEnumerator()
            {
                foreach (int item in this.PermutationData)
                {
                    yield return item;
                }
            }

            IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
        }

        public static class Extensions
        {
            public static IEnumerable<T> InsertAt<T>(IEnumerable<T> items, int position, T newItem)
            {
                if (items == null)
                {
                    throw new ArgumentNullException("items");
                }

                if (position < 0)
                {
                    throw new ArgumentOutOfRangeException("position");
                }

                return InsertAtIterator<T>(items, position, newItem);
            }

            private static IEnumerable<T> InsertAtIterator<T>(IEnumerable<T> items, int position, T newItem)
            {
                int index = 0;
                bool yieldedNew = false;
                foreach (T item in items)
                {
                    if (index == position)
                    {
                        yield return newItem;
                        yieldedNew = true;
                    }

                    yield return item;
                    index += 1;
                }

                if (index == position)
                {
                    yield return newItem;
                    yieldedNew = true;
                }

                if (!yieldedNew)
                {
                    throw new ArgumentOutOfRangeException("position");
                }
            }
        }
    }
}
