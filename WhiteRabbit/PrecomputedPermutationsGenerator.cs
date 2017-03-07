namespace WhiteRabbit
{
    using System.Collections.Generic;
    using System.Linq;

    internal static class PrecomputedPermutationsGenerator
    {
        private static PermutationsGenerator.Permutation[] Permutations1 { get; } = PermutationsGenerator.HamiltonianPermutations(1).ToArray();

        private static PermutationsGenerator.Permutation[] Permutations2 { get; } = PermutationsGenerator.HamiltonianPermutations(2).ToArray();

        private static PermutationsGenerator.Permutation[] Permutations3 { get; } = PermutationsGenerator.HamiltonianPermutations(3).ToArray();

        private static PermutationsGenerator.Permutation[] Permutations4 { get; } = PermutationsGenerator.HamiltonianPermutations(4).ToArray();

        private static PermutationsGenerator.Permutation[] Permutations5 { get; } = PermutationsGenerator.HamiltonianPermutations(5).ToArray();

        public static IEnumerable<PermutationsGenerator.Permutation> HamiltonianPermutations(int n)
        {
            switch (n)
            {
                case 1:
                    return Permutations1;
                case 2:
                    return Permutations2;
                case 3:
                    return Permutations3;
                case 4:
                    return Permutations4;
                case 5:
                    return Permutations5;
                default:
                    return PermutationsGenerator.HamiltonianPermutations(n);
            }
        }
    }
}
