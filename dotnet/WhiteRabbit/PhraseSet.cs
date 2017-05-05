namespace WhiteRabbit
{
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    // Anagram representation optimized for MD5
    internal struct PhraseSet
    {
        public uint[] Buffer;

        public unsafe PhraseSet(Word[] allWords, int[] wordIndexes, ulong[] permutations, int permutationOffset, int numberOfCharacters)
        {
            Debug.Assert(numberOfCharacters + wordIndexes.Length - 1 < 27);

            this.Buffer = new uint[8 * Constants.PhrasesPerSet];

            fixed (uint* bufferPointer = this.Buffer)
            {
                fixed (ulong* permutationsPointer = permutations)
                {
                    fixed (int* wordIndexesPointer = wordIndexes)
                    {
                        fixed (Word* allWordsPointer = allWords)
                        {
                            WhiteRabbitUnmanagedBridge.MD5Unmanaged.FillPhraseSet((long*)bufferPointer, (long*)allWordsPointer, wordIndexesPointer, permutationsPointer, permutationOffset, numberOfCharacters, wordIndexes.Length);
                        }
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetMD5(int number)
        {
            return this.Buffer[number * 8 + 7];
        }

        public unsafe byte[] GetBytes(int number)
        {
            Debug.Assert(number < Constants.PhrasesPerSet);

            fixed(uint* bufferPointer = this.Buffer)
            {
                var phrasePointer = bufferPointer + 8 * number;
                var length = 0;
                for (var i = 27; i >= 0; i--)
                {
                    if (((byte*)phrasePointer)[i] == 128)
                    {
                        length = i;
                        break;
                    }
                }

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
