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
                fixed (ulong* permutationsPointer = permutations)
                {
                    fixed (int* wordIndexesPointer = wordIndexes)
                    {
                        fixed (Word* allWordsPointer = allWords)
                        {
                            WhiteRabbitUnmanagedBridge.MD5Unmanaged.FillPhraseSet(bufferPointer, (long*)allWordsPointer, wordIndexesPointer, permutationsPointer, permutationOffset, numberOfCharacters, wordIndexes.Length);
                        }
                    }
                }
            }
        }

        private static unsafe void FillPhraseSet(long* bufferPointer, long* allWordsPointer, int* wordIndexes, ulong* permutationsPointer, int permutationOffset, int numberOfCharacters, int numberOfWords)
        {
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
