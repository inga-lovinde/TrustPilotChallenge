namespace WhiteRabbit
{
    using System.Diagnostics;

    // Anagram representation optimized for MD5
    internal struct PhraseSet
    {
        public long[] Buffer;

        public unsafe PhraseSet(Word[] allWords, int[] wordIndexes, ulong[] permutations, int permutationOffset, int numberOfCharacters)
        {
            Debug.Assert(numberOfCharacters + wordIndexes.Length - 1 < 27);

            this.Buffer = new long[4 * Constants.PhrasesPerSet];

            fixed (long* bufferPointer = this.Buffer)
            {
                long* longBuffer = (long*)bufferPointer;
                int numberOfWords = wordIndexes.Length;

                fixed (ulong* permutationsPointer = permutations)
                {
                    var currentPermutationPointer = permutationsPointer + permutationOffset;
                    for (var i = 0; i < Constants.PhrasesPerSet; i++, currentPermutationPointer++)
                    {
                        var permutation = *currentPermutationPointer;
                        if (permutation == 0)
                        {
                            continue;
                        }

                        var cumulativeWordOffsetX4 = 0;
                        for (var j = 0; j < numberOfWords; j++)
                        {
                            var currentWord = allWords[wordIndexes[permutation & 15]];
                            permutation = permutation >> 4;
                            longBuffer[0] |= currentWord.Buffers[cumulativeWordOffsetX4 + 0];
                            longBuffer[1] |= currentWord.Buffers[cumulativeWordOffsetX4 + 1];
                            longBuffer[2] ^= currentWord.Buffers[cumulativeWordOffsetX4 + 2];
                            longBuffer[3] ^= currentWord.Buffers[cumulativeWordOffsetX4 + 3];
                            cumulativeWordOffsetX4 += currentWord.LengthX4;
                        }

                        longBuffer += 4;
                    }
                }

                var length = numberOfCharacters + numberOfWords - 1;
                byte* byteBuffer = ((byte*)bufferPointer) + length;
                for (var i = 0; i < Constants.PhrasesPerSet; i++)
                {
                    *byteBuffer = 128;
                    byteBuffer += 32;
                }

                var lengthInBits = (uint)(length << 3);
                uint* uintBuffer = ((uint*)bufferPointer) + 7;
                for (var i = 0; i < Constants.PhrasesPerSet; i++)
                {
                    *uintBuffer = lengthInBits;
                    uintBuffer += 8;
                }
            }
        }

        public unsafe byte[] GetBytes(int number)
        {
            Debug.Assert(number < Constants.PhrasesPerSet);

            fixed(long* bufferPointer = this.Buffer)
            {
                var phrasePointer = bufferPointer + 4 * number;
                var length = ((uint*)phrasePointer)[7] >> 3;
                var result = new byte[length];
                for (var i = 0; i < length; i++)
                {
                    result[i] = ((byte*)phrasePointer)[i];
                }

                return result;
            }
        }
    }
}
