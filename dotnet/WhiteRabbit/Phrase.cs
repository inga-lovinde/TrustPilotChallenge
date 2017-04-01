﻿namespace WhiteRabbit
{
    // Anagram representation optimized for MD5
    internal unsafe struct Phrase
    {
        public fixed uint Buffer[8];

        public Phrase(byte[][] words, PermutationsGenerator.Permutation permutation, int numberOfCharacters)
        {
            fixed (uint* bufferPointer = this.Buffer)
            {
                var length = numberOfCharacters + words.Length - 1;

                byte[] currentWord = words[permutation[0]];
                var j = 0;
                var wordIndex = 0;
                var currentPointer = (byte*)bufferPointer;
                byte* lastPointer = currentPointer + length;
                for (; currentPointer < lastPointer; currentPointer++)
                {
                    if (j >= currentWord.Length)
                    {
                        j = 0;
                        wordIndex++;
                        currentWord = words[permutation[wordIndex]];
                    }

                    *currentPointer = currentWord[j];
                    j++;
                }
                *currentPointer = 128;

                bufferPointer[7] = (uint)(length << 3);
            }
        }

        public byte[] GetBytes()
        {
            fixed(uint* bufferPointer = this.Buffer)
            {
                var length = bufferPointer[7] >> 3;
                var result = new byte[length];
                for (var i = 0; i < length; i++)
                {
                    result[i] = ((byte*)bufferPointer)[i];
                }

                return result;
            }
        }
    }
}
