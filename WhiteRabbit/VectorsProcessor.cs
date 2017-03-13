namespace WhiteRabbit
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Numerics;

    internal sealed class VectorsProcessor
    {
        private const byte MaxComponentValue = 8;
        private const int LeastCommonMultiple = 840;

        // Ensure that permutations are precomputed prior to main run, so that processing times will be correct
        static VectorsProcessor()
        {
            PrecomputedPermutationsGenerator.HamiltonianPermutations(0);
        }

        public VectorsProcessor(Vector<byte> target, int maxVectorsCount, IEnumerable<Vector<byte>> dictionary)
        {
            if (Enumerable.Range(0, Vector<byte>.Count).Any(i => target[i] > MaxComponentValue))
            {
                throw new ArgumentException($"Every value should be at most {MaxComponentValue} (at most {MaxComponentValue} same characters allowed in the source string)", nameof(target));
            }

            this.Target = target;

            this.MaxVectorsCount = maxVectorsCount;
            this.Dictionary = ImmutableArray.Create(FilterVectors(dictionary, target).ToArray());
        }

        private Vector<byte> Target { get; }

        private int MaxVectorsCount { get; }

        private ImmutableArray<VectorInfo> Dictionary { get; }

        // Produces all sequences of vectors with the target sum
        public ParallelQuery<Vector<byte>[]> GenerateSequences()
        {
            return GenerateUnorderedSequences(this.Target, GetVectorNorm(this.Target, this.Target), this.MaxVectorsCount, this.Dictionary, 0)
                .AsParallel()
                .Select(Enumerable.ToArray)
                .SelectMany(GeneratePermutations);
        }

        // We want words with more letters (and among these, words with more "rare" letters) to appear first, to reduce the searching time somewhat.
        // Applying such a sort, we reduce the total number of triplets to check for anagrams from ~62M to ~29M.
        // Total number of quadruplets is reduced from 1468M to mere 311M.
        // And total number of quintuplets becomes reasonable 1412M.
        // Also, it produces the intended results faster (as these are more likely to contain longer words - e.g. "poultry outwits ants" is more likely than "p o u l t r y o u t w i t s a n t s").
        // This method basically gives us the 1-norm of the vector in the space rescaled so that the target is [1, 1, ..., 1].
        private static int GetVectorNorm(Vector<byte> vector, Vector<byte> target)
        {
            var norm = 0;
            for (var i = 0; target[i] != 0; i++)
            {
                norm += (LeastCommonMultiple * vector[i]) / target[i];
            }

            return norm;
        }

        private static VectorInfo[] FilterVectors(IEnumerable<Vector<byte>> vectors, Vector<byte> target)
        {
            return vectors
                .Where(vector => Vector.GreaterThanOrEqualAll(target, vector))
                .Select(vector => new VectorInfo(vector, GetVectorNorm(vector, target)))
                .OrderByDescending(vectorInfo => vectorInfo.Norm)
                .ToArray();
        }

        // This method takes most of the time, so everything related to it must be optimized.
        // In every sequence, next vector always goes after the previous one from dictionary.
        // E.g. if dictionary is [x, y, z], then only [x, y] sequence could be generated, and [y, x] will never be generated.
        // That way, the complexity of search goes down by a factor of MaxVectorsCount! (as if [x, y] does not add up to a required target, there is no point in checking [y, x])
        private static IEnumerable<ImmutableStack<Vector<byte>>> GenerateUnorderedSequences(Vector<byte> remainder, int remainderNorm, int allowedRemainingWords, ImmutableArray<VectorInfo> dictionary, int currentDictionaryPosition)
        {
            if (allowedRemainingWords > 1)
            {
                var newAllowedRemainingWords = allowedRemainingWords - 1;

                // E.g. if remainder norm is 7, 8 or 9, and allowedRemainingWords is 3,
                // we need the largest remaining word to have a norm of at least 3
                var requiredRemainderPerWord = (remainderNorm + allowedRemainingWords - 1) / allowedRemainingWords;

                for (var i = FindFirstWithNormLessOrEqual(remainderNorm, dictionary, currentDictionaryPosition); i < dictionary.Length; i++)
                {
                    var currentVectorInfo = dictionary[i];
                    if (currentVectorInfo.Vector == remainder)
                    {
                        yield return ImmutableStack.Create(currentVectorInfo.Vector);
                    }
                    else if (currentVectorInfo.Norm < requiredRemainderPerWord)
                    {
                        break;
                    }
                    else if (Vector.LessThanOrEqualAll(currentVectorInfo.Vector, remainder))
                    {
                        var newRemainder = remainder - currentVectorInfo.Vector;
                        var newRemainderNorm = remainderNorm - currentVectorInfo.Norm;
                        foreach (var result in GenerateUnorderedSequences(newRemainder, newRemainderNorm, newAllowedRemainingWords, dictionary, i))
                        {
                            yield return result.Push(currentVectorInfo.Vector);
                        }
                    }
                }
            }
            else
            {
                for (var i = FindFirstWithNormLessOrEqual(remainderNorm, dictionary, currentDictionaryPosition); i < dictionary.Length; i++)
                {
                    var currentVectorInfo = dictionary[i];
                    if (currentVectorInfo.Vector == remainder)
                    {
                        yield return ImmutableStack.Create(currentVectorInfo.Vector);
                    }
                    else if (currentVectorInfo.Norm < remainderNorm)
                    {
                        break;
                    }
                }
            }
        }

        // BCL BinarySearch would find any vector with required norm, not the first one; or would find nothing if there is no such vector
        private static int FindFirstWithNormLessOrEqual(int expectedNorm, ImmutableArray<VectorInfo> dictionary, int offset)
        {
            var start = offset;
            var end = dictionary.Length - 1;

            if (dictionary[start].Norm <= expectedNorm)
            {
                return start;
            }

            if (dictionary[end].Norm > expectedNorm)
            {
                return dictionary.Length;
            }

            // Norm for start is always greater than expected norm, or start is the required position; norm for end is always less than or equal to expected norm
            // The loop always ends, because the difference always decreases; if start + 1 = end, then middle will be equal to start, and either end := middle = start or start := middle + 1 = end.
            while (start < end)
            {
                var middle = (start + end) / 2;
                var newNorm = dictionary[middle].Norm;
                if (dictionary[middle].Norm <= expectedNorm)
                {
                    end = middle;
                }
                else
                {
                    start = middle + 1;
                }
            }

            return start;
        }

        private static IEnumerable<T[]> GeneratePermutations<T>(T[] original)
        {
            var length = original.Length;
            foreach (var permutation in PrecomputedPermutationsGenerator.HamiltonianPermutations(length))
            {
                var result = new T[length];
                for (var i = 0; i < length; i++)
                {
                    result[i] = original[permutation[i]];
                }

                yield return result;
            }
        }

        private struct VectorInfo
        {
            public VectorInfo(Vector<byte> vector, int norm)
            {
                this.Vector = vector;
                this.Norm = norm;
            }

            public Vector<byte> Vector { get; }

            public int Norm { get; }
        }
    }
}
