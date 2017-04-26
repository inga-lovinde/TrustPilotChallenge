namespace WhiteRabbit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal static class PrecomputedPermutationsGenerator
    {
        private static ulong[][][] Permutations { get; } = Enumerable.Range(0, 9).Select(GeneratePermutations).ToArray();

        private static long[] PermutationsNumbers { get; } = GeneratePermutationsNumbers().Take(19).ToArray();

        public static ulong[] HamiltonianPermutations(int n, uint filter) => Permutations[n][filter];

        public static long GetPermutationsNumber(int n) => PermutationsNumbers[n];

        private static ulong[][] GeneratePermutations(int n)
        {
            if (n == 0)
            {
                return new ulong[0][];
            }

            var allPermutations = PermutationsGenerator.HamiltonianPermutations(n)
                .Select(FormatPermutation)
                .ToArray();

            var statesCount = (uint)1 << (n - 1);
            var result = new ulong[statesCount][];
            for (uint i = 0; i < statesCount; i++)
            {
                result[i] = PadToWholeChunks(FilterPermutations(allPermutations, i).ToArray(), Constants.PhrasesPerSet);
            }

            return result;
        }

        private static IEnumerable<ulong> FilterPermutations(IEnumerable<ulong> permutations, uint state)
        {
            for (int position = 0; position < 16; position++)
            {
                if (((state >> position) & 1) != 0)
                {
                    var innerPosition = (uint)position;
                    permutations = permutations.Where(permutation => IsOrderPreserved(permutation, innerPosition));
                }
            }

            return permutations;
        }

        public static bool IsOrderPreserved(ulong permutation, uint position)
        {
            var currentPermutation = permutation;

            while (currentPermutation != 0)
            {
                if ((currentPermutation & 15) == position)
                {
                    return true;
                }

                if ((currentPermutation & 15) == (position + 1))
                {
                    return false;
                }

                currentPermutation = currentPermutation >> 4;
            }

            throw new ApplicationException("Malformed permutation " + permutation + " for position " + position);
        }

        private static T[] PadToWholeChunks<T>(T[] original, int chunkSize)
        {
            if (original.Length % chunkSize == 0)
            {
                return original;
            }

            return original.Concat(Enumerable.Repeat(default(T), chunkSize - (original.Length % chunkSize))).ToArray();
        }

        private static ulong FormatPermutation(PermutationsGenerator.Permutation permutation)
        {
            System.Diagnostics.Debug.Assert(permutation.PermutationData.Length <= 16);

            ulong result = 0;
            for (var i = 0; i < permutation.PermutationData.Length; i++)
            {
                result |= (ulong)(permutation.PermutationData[i]) << (4 * i);
            }

            return result;
        }

        private static IEnumerable<long> GeneratePermutationsNumbers()
        {
            long result = 1;
            yield return result;

            var i = 1;
            while (true)
            {
                result *= i;
                yield return result;
                i++;
            }
        }
    }
}
