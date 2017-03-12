namespace WhiteRabbit
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics;
    using System.Linq;
    using System.Numerics;

    internal class VectorsProcessor
    {
        // Ensure that permutations are precomputed prior to main run, so that processing times will be correct
        static VectorsProcessor()
        {
            PrecomputedPermutationsGenerator.HamiltonianPermutations(0);
        }

        public VectorsProcessor(Vector<byte> target, int maxVectorsCount, IEnumerable<Vector<byte>> dictionary, Func<Vector<byte>, string> vectorToString)
        {
#if SUPPORT_LARGE_STRINGS
            if (Enumerable.Range(0, Vector<byte>.Count).Any(i => target[i] > 8))
            {
                throw new ArgumentException("Every value should be at most 8 (at most 8 same characters allowed in the source string)", nameof(target));
            }
#else
            if (Enumerable.Range(0, Vector<byte>.Count).Any(i => target[i] > 4))
            {
                throw new ArgumentException("Every value should be at most 4 (at most 4 same characters allowed in the source string)", nameof(target));
            }
#endif

            this.Target = target;

#if !SUPPORT_LARGE_STRINGS
            this.TargetComplement = new Vector<byte>(Enumerable.Range(0, Vector<byte>.Count).Select(i => (byte)(this.Target[i] == 0 ? 0 : (byte)(12 / this.Target[i]))).ToArray());
#endif

            this.TargetNorm = Vector.Dot(target, Vector<byte>.One);
            this.MaxVectorsCount = maxVectorsCount;
            this.VectorToString = vectorToString;
            this.Dictionary = ImmutableStack.Create(FilterVectors(dictionary, target, this.TargetComplement).ToArray());
        }

        private Vector<byte> Target { get; }

        private Vector<byte> TargetComplement { get; }

        private byte TargetNorm { get; }

        private int MaxVectorsCount { get; }

        private ImmutableStack<Vector<byte>> Dictionary { get; }

        private Func<Vector<byte>, string> VectorToString { get; }

        private long Iterations { get; set; } = 0;

        // Produces all sequences of vectors with the target sum
        public ParallelQuery<Vector<byte>[]> GenerateSequences()
        {
            var unorderedSequences = this.GenerateUnorderedSequences(this.Target, ImmutableStack.Create<Vector<byte>>(), this.Dictionary)
                .AsParallel();
            var allSequences = unorderedSequences.SelectMany(this.GeneratePermutations);

            return allSequences;
        }

        // We want words with more letters (and among these, words with more "rare" letters) to appear first, to reduce the searching time somewhat.
        // Applying such a sort, we reduce the total number of triplets to check for anagrams from ~62M to ~29M.
        // Total number of quadruplets is reduced from 1468M to mere 311M.
        // And total number of quintuplets becomes reasonable 1412M.
        // Also, it produces the intended results faster (as these are more likely to contain longer words - e.g. "poultry outwits ants" is more likely than "p o u l t r y o u t w i t s a n t s").
        // This method basically gives us the 1-norm of the vector in the space rescaled so that the target is [1, 1, ..., 1].
        private static int GetVectorWeight(Vector<byte> vector, Vector<byte> target, Vector<byte> targetComplement)
        {
#if SUPPORT_LARGE_STRINGS
            var weight = 0;
            for (var i = 0; target[i] != 0; i++)
            {
                weight += (840 * vector[i]) / target[i]; // 840 = LCM(1, 2, .., 8), so that the result will be a whole number (unless Target[i] > 8)
            }

            return weight;
#else
            return Vector.Dot(vector, targetComplement);
#endif
        }

        private static IEnumerable<Vector<byte>> FilterVectors(IEnumerable<Vector<byte>> vectors, Vector<byte> target, Vector<byte> targetComplement)
        {
            return vectors
                .Where(vector => Vector.GreaterThanOrEqualAll(target, vector))
                .OrderBy(vector => GetVectorWeight(vector, target, targetComplement));
        }

        [Conditional("DEBUG")]
        private void DebugState(ImmutableStack<Vector<byte>> partialSumStack, Vector<byte> currentVector)
        {
            this.Iterations++;
            if (this.Iterations % 1000000 == 0)
            {
                Console.WriteLine($"Iteration #{this.Iterations}: {string.Join(" ", partialSumStack.Push(currentVector).Reverse().Select(vector => this.VectorToString(vector)))}");
            }
        }

        // This method takes most of the time, so everything related to it must be optimized.
        // In every sequence, next vector always goes after the previous one from dictionary.
        // E.g. if dictionary is [x, y, z], then only [x, y] sequence could be generated, and [y, x] will never be generated.
        // That way, the complexity of search goes down by a factor of MaxVectorsCount! (as if [x, y] does not add up to a required target, there is no point in checking [y, x])
        private IEnumerable<Vector<byte>[]> GenerateUnorderedSequences(Vector<byte> remainder, ImmutableStack<Vector<byte>> partialSumStack, ImmutableStack<Vector<byte>> dictionaryStack)
        {
            var allowedRemainingWords = this.MaxVectorsCount - partialSumStack.Count();
            if (allowedRemainingWords > 1)
            {

#if !SUPPORT_LARGE_STRINGS
                // e.g. if remainder norm is 7, 8 or 9, and allowedRemainingWords is 3,
                // we need the largest remaining word to have a norm of at least 3
                var remainderNorm = Vector.Dot(remainder, this.TargetComplement);
                var requiredRemainder = (remainderNorm + allowedRemainingWords - 1) / allowedRemainingWords;
#endif

                var dictionaryTail = dictionaryStack;
                while (!dictionaryTail.IsEmpty)
                {
                    Vector<byte> currentVector;
                    var nextDictionaryTail = dictionaryTail.Pop(out currentVector);

                    this.DebugState(partialSumStack, currentVector);

                    if (currentVector == remainder)
                    {
                        yield return partialSumStack.Push(currentVector).Reverse().ToArray();
                    }
#if !SUPPORT_LARGE_STRINGS
                    else if (Vector.Dot(currentVector, this.TargetComplement) < requiredRemainder)
                    {
                        break;
                    }
#endif
                    else if (Vector.LessThanOrEqualAll(currentVector, remainder))
                    {
                        var newRemainder = remainder - currentVector;
                        foreach (var result in this.GenerateUnorderedSequences(newRemainder, partialSumStack.Push(currentVector), dictionaryTail))
                        {
                            yield return result;
                        }
                    }

                    dictionaryTail = nextDictionaryTail;
                }
            }
            else
            {
                var dictionaryTail = dictionaryStack;
                while (!dictionaryTail.IsEmpty)
                {
                    Vector<byte> currentVector;
                    dictionaryTail = dictionaryTail.Pop(out currentVector);

                    this.DebugState(partialSumStack, currentVector);

                    var newRemainder = remainder - currentVector;
                    if (newRemainder == Vector<byte>.Zero)
                    {
                        yield return partialSumStack.Push(currentVector).Reverse().ToArray();
                    }
                }
            }
        }

        private IEnumerable<T[]> GeneratePermutations<T>(T[] original)
        {
            foreach (var permutation in PrecomputedPermutationsGenerator.HamiltonianPermutations(original.Length))
            {
                yield return permutation.Select(i => original[i]).ToArray();
            }
        }
    }
}
