namespace WhiteRabbit
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Numerics;
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

        public unsafe void ProcessPermutations(PhraseSet initialPhraseSet, Word[] allWords, int[] wordIndexes, ulong[] permutations, uint[] expectedHashesVector, Action<byte[], uint> action)
        {
            fixed (uint* bufferPointer = this.Buffer, initialBufferPointer = initialPhraseSet.Buffer)
            {
                fixed (ulong* permutationsPointer = permutations)
                {
                    fixed (int* wordIndexesPointer = wordIndexes)
                    {
                        fixed (Word* allWordsPointer = allWords)
                        {
                            fixed (uint* expectedHashesPointer = expectedHashesVector)
                            {
                                for (var i = 0; i < permutations.Length; i += Constants.PhrasesPerSet)
                                {
                                    MD5Unmanaged.FillPhraseSet(
                                        (ulong*)initialBufferPointer,
                                        (ulong*)bufferPointer,
                                        (ulong*)allWordsPointer,
                                        wordIndexesPointer,
                                        permutationsPointer + i,
                                        wordIndexes.Length);

                                    MD5Unmanaged.ComputeMD5(bufferPointer, expectedHashesPointer);

                                    if (bufferPointer[Constants.PhrasesPerSet / 2] != 0xFFFFFFFF)
                                    {
                                        for (var j = 0; j < Constants.PhrasesPerSet; j++)
                                        {
                                            // 16 matches are packed in 8 32-bit numbers: [0,1], [8,9], [2,3], [10,11], [4, 5], [12, 13], [6, 7], [14, 15]
                                            var position = ((j / 2) % 4) * 2 + (j / 8);
                                            var match = (bufferPointer[position] >> (4 * (j % 2))) & 0xF0F0F0F;
                                            if (match != 0)
                                            {
                                                var bufferInfo = ((ulong)bufferPointer[Constants.PhrasesPerSet] << 32) | bufferPointer[j];
                                                MD5Unmanaged.FillPhraseSet(
                                                    (ulong*)initialBufferPointer,
                                                    (ulong*)bufferPointer,
                                                    (ulong*)allWordsPointer,
                                                    wordIndexesPointer,
                                                    permutationsPointer + i,
                                                    wordIndexes.Length);
                                                action(this.GetBytes(j), match);
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
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
