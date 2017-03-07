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
        public VectorsProcessor(Vector<byte> target, int maxVectorsCount, Func<Vector<byte>, string> vectorToString)
        {
            this.Target = target;
            this.MaxVectorsCount = maxVectorsCount;
            this.VectorToString = vectorToString;
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

        private Func<Vector<byte>, string> VectorToString { get; }

        private long Iterations { get; set; } = 0;

        public IEnumerable<Vector<byte>[]> GenerateSums(IEnumerable<Vector<byte>> vectors)
        {
            var filteredVectors = FilterVectors(vectors);
            var dictionary = ImmutableStack.Create(filteredVectors.ToArray());
            var orderedSums = GenerateOrderedSums(this.Target, ImmutableStack.Create<Vector<byte>>(), dictionary);
            var allSums = orderedSums.SelectMany(GeneratePermutations);

            return allSums;
        }

        private IEnumerable<Vector<byte>> FilterVectors(IEnumerable<Vector<byte>> vectors)
        {
            return vectors
                .Where(vector => ((this.Target - vector) & Negative) == Vector<byte>.Zero);
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

        // This method takes most of the time, so everything related to it must be optimized
        private IEnumerable<Vector<byte>[]> GenerateOrderedSums(Vector<byte> remainder, ImmutableStack<Vector<byte>> partialSumStack, ImmutableStack<Vector<byte>> dictionaryStack)
        {
            var count = partialSumStack.Count() + 1;
            if (count < this.MaxVectorsCount)
            {
                var dictionaryTail = dictionaryStack;
                while (!dictionaryTail.IsEmpty)
                {
                    Vector<byte> currentVector;
                    var nextDictionaryTail = dictionaryTail.Pop(out currentVector);

                    DebugState(partialSumStack, currentVector);

                    var newRemainder = remainder - currentVector;
                    if (newRemainder == Vector<byte>.Zero)
                    {
                        yield return partialSumStack.Push(currentVector).Reverse().ToArray();
                    }
                    else if ((newRemainder & Negative) == Vector<byte>.Zero)
                    {
                        foreach (var result in GenerateOrderedSums(newRemainder, partialSumStack.Push(currentVector), dictionaryTail))
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

                    DebugState(partialSumStack, currentVector);

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
            foreach (var permutation in PermutationsGenerator.HamiltonianPermutations(original.Length))
            {
                yield return permutation.Select(i => original[i]).ToArray();
            }
        }
    }
}
