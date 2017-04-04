namespace WhiteRabbit
{
    using System.Collections.Generic;
    using System.Linq;

    internal static class PrecomputedPermutationsGenerator
    {
        private static PermutationsGenerator.Permutation[][] Permutations { get; } = new[]
        {
            GeneratePermutations(0),
            GeneratePermutations(1),
            GeneratePermutations(2),
            GeneratePermutations(3),
            GeneratePermutations(4),
            GeneratePermutations(5),
            GeneratePermutations(6),
            GeneratePermutations(7),
        };

        public static PermutationsGenerator.Permutation[] HamiltonianPermutations(int n)
        {
            return Permutations[n];
        }

        private static PermutationsGenerator.Permutation[] GeneratePermutations(int n)
        {
            var result = PermutationsGenerator.HamiltonianPermutations(n).ToArray();
            if (result.Length % Constants.PhrasesPerSet == 0)
            {
                return result;
            }

            return result.Concat(Enumerable.Repeat(result[0], Constants.PhrasesPerSet - (result.Length % Constants.PhrasesPerSet))).ToArray();
        }
    }
}
