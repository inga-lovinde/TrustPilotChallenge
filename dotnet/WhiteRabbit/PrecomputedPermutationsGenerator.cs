namespace WhiteRabbit
{
    using System.Collections.Generic;
    using System.Linq;

    internal static class PrecomputedPermutationsGenerator
    {
        private static PermutationsGenerator.Permutation[][] Permutations { get; } = new[]
        {
            PermutationsGenerator.HamiltonianPermutations(0).ToArray(),
            PermutationsGenerator.HamiltonianPermutations(1).ToArray(),
            PermutationsGenerator.HamiltonianPermutations(2).ToArray(),
            PermutationsGenerator.HamiltonianPermutations(3).ToArray(),
            PermutationsGenerator.HamiltonianPermutations(4).ToArray(),
            PermutationsGenerator.HamiltonianPermutations(5).ToArray(),
            PermutationsGenerator.HamiltonianPermutations(6).ToArray(),
            PermutationsGenerator.HamiltonianPermutations(7).ToArray(),
        };

        // for MD5 SIMD optimization and code simplicity reasons, number of permutations should divide by 4
        public static PermutationsGenerator.Permutation[] HamiltonianPermutations(int n)
        {
            return Permutations[n];
        }

        private static PermutationsGenerator.Permutation[] GeneratePermutations(int n)
        {
            var result = PermutationsGenerator.HamiltonianPermutations(n).ToList();
            if (result.Count == 0)
            {
                return result.ToArray();
            }

            var extra = (4 - (result.Count % 4)) % 4;
            return result.Concat(Enumerable.Repeat(result[0], extra)).ToArray();
        }
    }
}
