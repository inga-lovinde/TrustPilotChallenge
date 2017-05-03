namespace WhiteRabbit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal static class PrecomputedPermutationsGenerator
    {
        static PrecomputedPermutationsGenerator()
        {
            Permutations = new ulong[Constants.MaxNumberOfWords + 1][][];
            PermutationsNumbers = new long[Constants.MaxNumberOfWords + 1][];
            for (var i = 0; i <= Constants.MaxNumberOfWords; i++)
            {
                var permutationsInfo = GeneratePermutations(i);
                Permutations[i] = permutationsInfo.Item1;
                PermutationsNumbers[i] = permutationsInfo.Item2;
            }
        }

        private static ulong[][][] Permutations { get; }

        private static long[][] PermutationsNumbers { get; }

        public static ulong[] HamiltonianPermutations(int n, uint filter) => Permutations[n][filter];

        public static long GetPermutationsNumber(int n, uint filter) => PermutationsNumbers[n][filter];

        private static Tuple<ulong[][], long[]> GeneratePermutations(int n)
        {
            if (n == 0)
            {
                return Tuple.Create(new ulong[0][], new long[0]);
            }

            var allPermutations = PermutationsGenerator.HamiltonianPermutations(n)
                .Select(FormatPermutation)
                .ToArray();

            var statesCount = (uint)1 << (n - 1);
            var resultUnpadded = new PermutationInfo[statesCount][];

            resultUnpadded[0] = allPermutations;
            for (uint i = 1; i < statesCount; i++)
            {
                var mask = i;
                mask |= mask >> 1;
                mask |= mask >> 2;
                mask |= mask >> 4;
                mask |= mask >> 8;
                mask |= mask >> 16;
                mask = mask >> 1;
                var existing = i & mask;
                var seniorBit = i ^ existing;
                var position = 0;
                while (seniorBit != 0)
                {
                    seniorBit = seniorBit >> 1;
                    position++;
                }

                resultUnpadded[i] = resultUnpadded[existing]
                    .Where(info => ((info.PermutationInverse >> (4 * (position - 1))) % 16 < (info.PermutationInverse >> (4 * position)) % 16))
                    .ToArray();
            }

            var result = new ulong[statesCount][];
            var numbers = new long[statesCount];
            for (uint i = 0; i < statesCount; i++)
            {
                result[i] = PadToWholeChunks(resultUnpadded[i], Constants.PhrasesPerSet);
                numbers[i] = resultUnpadded[i].LongLength;
            }

            return Tuple.Create(result, numbers);
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

        private static ulong[] PadToWholeChunks(PermutationInfo[] original, int chunkSize)
        {
            ulong[] result;
            if (original.Length % chunkSize == 0)
            {
                result = new ulong[original.Length];
            }
            else
            {
                result = new ulong[original.Length + chunkSize - (original.Length % chunkSize)];
            }

            for (var i = 0; i < original.Length; i++)
            {
                result[i] = original[i].Permutation;
            }

            return result;
        }

        private static PermutationInfo FormatPermutation(PermutationsGenerator.Permutation permutation)
        {
            System.Diagnostics.Debug.Assert(permutation.PermutationData.Length <= 16);

            ulong result = 0;
            ulong resultInverse = 0;
            for (var i = 0; i < permutation.PermutationData.Length; i++)
            {
                var source = i;
                var target = permutation.PermutationData[i];
                result |= (ulong)(target) << (4 * source);
                resultInverse |= (ulong)(source) << (4 * target);
            }

            return new PermutationInfo { Permutation = result, PermutationInverse = resultInverse };
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

        private struct PermutationInfo
        {
            public ulong Permutation;
            public ulong PermutationInverse;
        }
    }
}
