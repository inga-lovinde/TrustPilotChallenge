namespace WhiteRabbit
{
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using WhiteRabbitUnmanagedBridge;

    // Anagram representation optimized for MD5
    internal struct PhraseSet
    {
        private uint[] Buffer;

        public void Init()
        {
            this.Buffer = new uint[8 * Constants.PhrasesPerSet];
        }

        public unsafe void FillLength(int numberOfCharacters, int numberOfWords)
        {
            fixed (uint* bufferPointer = this.Buffer)
            {
                var length = (uint)(numberOfCharacters + numberOfWords - 1);
                var lengthInBits = (uint)(length << 3);
                for (var i = 0; i < Constants.PhrasesPerSet; i++)
                {
                    bufferPointer[7 + i * 8] = lengthInBits;
                    ((byte*)bufferPointer)[length + i * 32] = 128 ^ ' ';
                }
            }
        }

        public unsafe void FillPhraseSet(PhraseSet initial, Word[] allWords, int[] wordIndexes, ulong[] permutations, int permutationOffset)
        {
            fixed (uint* bufferPointer = this.Buffer, initialBufferPointer = initial.Buffer)
            {
                fixed (ulong* permutationsPointer = permutations)
                {
                    fixed (int* wordIndexesPointer = wordIndexes)
                    {
                        fixed (Word* allWordsPointer = allWords)
                        {
                            MD5Unmanaged.FillPhraseSet(
                                (ulong*)initialBufferPointer,
                                (ulong*)bufferPointer,
                                (ulong*)allWordsPointer,
                                wordIndexesPointer,
                                permutationsPointer + permutationOffset,
                                wordIndexes.Length);
                        }
                    }
                }
            }
        }

        public unsafe void ComputeMD5()
        {
            fixed (uint* inputBuffer = this.Buffer)
            {
                MD5Unmanaged.ComputeMD5(inputBuffer);
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

            fixed (uint* bufferPointer = this.Buffer)
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

        public unsafe string DebugBytes(int number)
        {
            Debug.Assert(number < Constants.PhrasesPerSet);

            fixed (uint* bufferPointer = this.Buffer)
            {
                var bytes = (byte*)bufferPointer;
                return string.Concat(Enumerable.Range(32 * number, 32).Select(i => bytes[i].ToString("X2")));
            }
        }
    }
}
