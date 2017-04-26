﻿namespace WhiteRabbit
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
                fixed (ulong* permutationsPointer = permutations)
                {
                    fixed (int* wordIndexesPointer = wordIndexes)
                    {
                        fixed (Word* allWordsPointer = allWords)
                        {
                            FillPhraseSet(bufferPointer, (long*)allWordsPointer, wordIndexesPointer, permutationsPointer, permutationOffset, numberOfCharacters, wordIndexes.Length);
                        }
                    }
                }
            }
        }

        private static unsafe void FillPhraseSet(long* bufferPointer, long* allWordsPointer, int* wordIndexes, ulong* permutationsPointer, int permutationOffset, int numberOfCharacters, int numberOfWords)
        {
            long* longBuffer = (long*)bufferPointer;

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
                    var currentWord = allWordsPointer + wordIndexes[permutation & 15] * 128;
                    permutation = permutation >> 4;
                    longBuffer[0] |= currentWord[cumulativeWordOffsetX4 + 0];
                    longBuffer[1] |= currentWord[cumulativeWordOffsetX4 + 1];
                    longBuffer[2] ^= currentWord[cumulativeWordOffsetX4 + 2];
                    longBuffer[3] ^= currentWord[cumulativeWordOffsetX4 + 3];
                    cumulativeWordOffsetX4 += unchecked((int)currentWord[127]);
                }

                longBuffer += 4;
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
