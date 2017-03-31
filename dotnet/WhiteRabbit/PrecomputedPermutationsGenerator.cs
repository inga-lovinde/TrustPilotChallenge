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

        public static IEnumerable<PermutationsGenerator.Permutation> HamiltonianPermutations(int n)
        {
            if (n > 9)
            {
                return PermutationsGenerator.HamiltonianPermutations(n);
            }

            return Permutations[n];
        }
    }
}
