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

        public VectorsProcessor(Vector<byte> target, int maxVectorsCount, Vector<byte>[] dictionary)
        {
            if (Enumerable.Range(0, Vector<byte>.Count).Any(i => target[i] > MaxComponentValue))
            {
                throw new ArgumentException($"Every value should be at most {MaxComponentValue} (at most {MaxComponentValue} same characters allowed in the source string)", nameof(target));
            }

            this.Target = target;

            this.MaxVectorsCount = maxVectorsCount;
            this.Dictionary = ImmutableArray.Create(FilterVectors(dictionary, target).ToArray());

            var normsIndex = new int[GetVectorNorm(target, target) + 1];
            var offset = 0;
            for (var i = normsIndex.Length - 1; i >= 0; i--)
            {
                while (offset < this.Dictionary.Length && this.Dictionary[offset].Norm > i)
                {
                    offset++;
                }

                normsIndex[i] = offset;
            }

            this.NormsIndex = ImmutableArray.Create(normsIndex);
        }

        private Vector<byte> Target { get; }

        private int MaxVectorsCount { get; }

        private ImmutableArray<VectorInfo> Dictionary { get; }

        // Stores index of the first vector from Dictionary with norm less than or equal to offset
        private ImmutableArray<int> NormsIndex { get; }

        // Produces all sets of vectors with the target sum
        public IEnumerable<int[]> GenerateSequences()
        {
            return this.GenerateUnorderedSequences(this.Target, GetVectorNorm(this.Target, this.Target), this.MaxVectorsCount, 0)
                .Select(Enumerable.ToArray);
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

        private static VectorInfo[] FilterVectors(Vector<byte>[] vectors, Vector<byte> target)
        {
            return Enumerable.Range(0, vectors.Length)
                .Where(i => Vector.GreaterThanOrEqualAll(target, vectors[i]))
                .Select(i => new VectorInfo(vectors[i], GetVectorNorm(vectors[i], target), i))
                .OrderByDescending(vectorInfo => vectorInfo.Norm)
                .ToArray();
        }

        // This method takes most of the time, so everything related to it must be optimized.
        // In every sequence, next vector always goes after the previous one from dictionary.
        // E.g. if dictionary is [x, y, z], then only [x, y] sequence could be generated, and [y, x] will never be generated.
        // That way, the complexity of search goes down by a factor of MaxVectorsCount! (as if [x, y] does not add up to a required target, there is no point in checking [y, x])
        private IEnumerable<ImmutableStack<int>> GenerateUnorderedSequences(Vector<byte> remainder, int remainderNorm, int allowedRemainingWords, int currentDictionaryPosition)
        {
            if (allowedRemainingWords > 1)
            {
                var newAllowedRemainingWords = allowedRemainingWords - 1;

                // E.g. if remainder norm is 7, 8 or 9, and allowedRemainingWords is 3,
                // we need the largest remaining word to have a norm of at least 3
                var requiredRemainderPerWord = (remainderNorm + allowedRemainingWords - 1) / allowedRemainingWords;

                for (var i = Math.Max(this.NormsIndex[remainderNorm], currentDictionaryPosition); i < this.Dictionary.Length; i++)
                {
                    var currentVectorInfo = this.Dictionary[i];
                    if (currentVectorInfo.Vector == remainder)
                    {
                        yield return ImmutableStack.Create(currentVectorInfo.Index);
                    }
                    else if (currentVectorInfo.Norm < requiredRemainderPerWord)
                    {
                        break;
                    }
                    else if (Vector.LessThanOrEqualAll(currentVectorInfo.Vector, remainder))
                    {
                        var newRemainder = remainder - currentVectorInfo.Vector;
                        var newRemainderNorm = remainderNorm - currentVectorInfo.Norm;
                        foreach (var result in this.GenerateUnorderedSequences(newRemainder, newRemainderNorm, newAllowedRemainingWords, i))
                        {
                            yield return result.Push(currentVectorInfo.Index);
                        }
                    }
                }
            }
            else
            {
                for (var i = Math.Max(this.NormsIndex[remainderNorm], currentDictionaryPosition); i < this.Dictionary.Length; i++)
                {
                    var currentVectorInfo = this.Dictionary[i];
                    if (currentVectorInfo.Vector == remainder)
                    {
                        yield return ImmutableStack.Create(currentVectorInfo.Index);
                    }
                    else if (currentVectorInfo.Norm < remainderNorm)
                    {
                        break;
                    }
                }
            }
        }

        private struct VectorInfo
        {
            public VectorInfo(Vector<byte> vector, int norm, int index)
            {
                this.Vector = vector;
                this.Norm = norm;
                this.Index = index;
            }

            public Vector<byte> Vector { get; }

            public int Norm { get; }

            public int Index { get; }
        }
    }
}
