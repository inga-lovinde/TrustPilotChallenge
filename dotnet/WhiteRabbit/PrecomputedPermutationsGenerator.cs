namespace WhiteRabbit
{
    using System.Collections.Generic;
    using System.Linq;

    internal static class PrecomputedPermutationsGenerator
    {
        private static PermutationsGenerator.Permutation[][] Permutations { get; } = Enumerable.Range(0, 8).Select(GeneratePermutations).ToArray();

        private static long[] PermutationsNumbers { get; } = GeneratePermutationsNumbers().Take(19).ToArray();

        public static PermutationsGenerator.Permutation[] HamiltonianPermutations(int n) => Permutations[n];

        public static long GetPermutationsNumber(int n) => PermutationsNumbers[n];

        private static PermutationsGenerator.Permutation[] GeneratePermutations(int n)
        {
            var result = PermutationsGenerator.HamiltonianPermutations(n).ToArray();
            if (result.Length % Constants.PhrasesPerSet == 0)
            {
                return result;
            }

            return result.Concat(Enumerable.Repeat(result[0], Constants.PhrasesPerSet - (result.Length % Constants.PhrasesPerSet))).ToArray();
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
