namespace WhiteRabbit
{
    using System.Collections.Generic;
    using System.Linq;

    internal static class PrecomputedPermutationsGenerator
    {
        private static long[][] Permutations { get; } = Enumerable.Range(0, 9).Select(GeneratePermutations).ToArray();

        private static long[] PermutationsNumbers { get; } = GeneratePermutationsNumbers().Take(19).ToArray();

        public static long[] HamiltonianPermutations(int n) => Permutations[n];

        public static long GetPermutationsNumber(int n) => PermutationsNumbers[n];

        private static long[] GeneratePermutations(int n)
        {
            var result = PermutationsGenerator.HamiltonianPermutations(n)
                .Select(FormatPermutation)
                .ToArray();

            return PadToWholeChunks(result, Constants.PhrasesPerSet);
        }

        private static T[] PadToWholeChunks<T>(T[] original, int chunkSize)
        {
            if (original.Length % chunkSize == 0)
            {
                return original;
            }

            return original.Concat(Enumerable.Repeat(default(T), chunkSize - (original.Length % chunkSize))).ToArray();
        }

        private static long FormatPermutation(PermutationsGenerator.Permutation permutation)
        {
            System.Diagnostics.Debug.Assert(permutation.PermutationData.Length <= 16);

            long result = 0;
            for (var i = 0; i < permutation.PermutationData.Length; i++)
            {
                result |= (long)(permutation.PermutationData[i]) << (4 * i);
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
