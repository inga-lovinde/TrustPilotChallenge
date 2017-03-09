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
        public VectorsProcessor(Vector<byte> target, int maxVectorsCount, IEnumerable<Vector<byte>> vectors, Func<Vector<byte>, string> vectorToString)
        {
            this.Target = target;
            this.MaxVectorsCount = maxVectorsCount;
            this.VectorToString = vectorToString;

            var filteredVectors = FilterVectors(vectors, target);
            this.Vectors = ImmutableStack.Create(filteredVectors.ToArray());
        }

        /// <summary>
        /// Negative sign bit.
        /// (byte)b &amp; (byte)128 equals zero for non-negative (0..127) bytes and equals (byte)128 for negative (128..255) bytes.
        /// Similarly, vector &amp; Negative equals zero if all bytes are non-negative, and does not equal zero if some bytes are negative.
        /// Use <code>(vector &amp; Negative) == Vector&lt;byte&gt;.Zero</code> to determine if all components are non-negative.
        /// </summary>
        private static Vector<byte> Negative { get; } = new Vector<byte>(Enumerable.Repeat((byte)128, 16).ToArray());

        private Vector<byte> Target { get; }

        private int MaxVectorsCount { get; }

        private ImmutableStack<Vector<byte>> Vectors { get; }

        private Func<Vector<byte>, string> VectorToString { get; }

        private long Iterations { get; set; } = 0;

        // Produces all sequences of vectors with the target sum
        public IEnumerable<Vector<byte>[]> GenerateSequences()
        {
            var unorderedSequences = this.GenerateUnorderedSequences(this.Target, ImmutableStack.Create<Vector<byte>>(), this.Vectors);
            var allSequences = unorderedSequences.SelectMany(this.GeneratePermutations);

            return allSequences;
        }

        // We want words with more letters (and among these, words with more "rare" letters) to appear first, to reduce the searching time somewhat.
        // Applying such a sort, we reduce the total number of triplets to check for anagrams from ~62M to ~29M.
        // Total number of quadruplets is reduced from 1468M to mere 311M.
        // Also, it produces the intended results faster (as these are more likely to contain longer words - e.g. "poultry outwits ants" is more likely than "p o u l t r y o u t w i t s a n t s").
        // This method basically gives us the 1-norm of the vector in the space rescaled so that the target is [1, 1, ..., 1].
        private static int GetVectorWeight(Vector<byte> vector, Vector<byte> target)
        {
            var weight = 0;
            for (var i = 0; target[i] != 0; i++)
            {
                weight += (720 * vector[i]) / target[i]; // 720 = 6!, so that the result will be a whole number (unless Target[i] > 6)
            }

            return weight;
        }

        private static IEnumerable<Vector<byte>> FilterVectors(IEnumerable<Vector<byte>> vectors, Vector<byte> target)
        {
            return vectors
                .Where(vector => ((target - vector) & Negative) == Vector<byte>.Zero)
                .OrderBy(vector => GetVectorWeight(vector, target));
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
            var count = partialSumStack.Count() + 1;
            if (count < this.MaxVectorsCount)
            {
                var dictionaryTail = dictionaryStack;
                while (!dictionaryTail.IsEmpty)
                {
                    Vector<byte> currentVector;
                    var nextDictionaryTail = dictionaryTail.Pop(out currentVector);

                    this.DebugState(partialSumStack, currentVector);

                    var newRemainder = remainder - currentVector;
                    if (newRemainder == Vector<byte>.Zero)
                    {
                        yield return partialSumStack.Push(currentVector).Reverse().ToArray();
                    }
                    else if ((newRemainder & Negative) == Vector<byte>.Zero)
                    {
                        foreach (var result in this.GenerateUnorderedSequences(newRemainder, partialSumStack.Push(currentVector), dictionaryTail))
                        {
                            yield return result;
                        }
                    }

                    dictionaryTail = nextDictionaryTail;
                }
            }
            else if (count == this.MaxVectorsCount)
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
