using System.Numerics;

namespace WhiteRabbit
{
    // Anagram representation optimized for MD5
    internal unsafe struct PhraseSet
    {
        private const int WordIndexMask = unchecked((int)(((uint)-1) << 16));

        private const int WordIndexIncrement = ((int)1) << 16;

        private const int CharIndexMask = ((int)1 << 16) - 1;

        public fixed uint Buffer[8 * Constants.PhrasesPerSet];

        public PhraseSet(byte[][] words, int[][] permutations, int offset, int numberOfCharacters)
        {
            fixed (uint* bufferPointer = this.Buffer)
            {
                var length = numberOfCharacters + words.Length - 1;

                for (var i = 0; i < Constants.PhrasesPerSet; i++)
                {
                    var permutation = permutations[offset + i];
                    var startPointer = bufferPointer + i * 8;
                    var currentPointer = (byte*)startPointer;
                    byte* lastPointer = currentPointer + length;

                    int currentState = 0; // wordIndex << 16 + charIndex
                    byte[] currentWord = words[permutation[0]];

                    for (; currentPointer < lastPointer; currentPointer++)
                    {
                        currentWord = words[permutation[currentState >> 16]];
                        *currentPointer = currentWord[(currentState & CharIndexMask)];

                        currentState++;
                        var nextStateNextWord = new Vector<int>((currentState + WordIndexIncrement) & WordIndexMask);
                        currentState = Vector.ConditionalSelect(Vector.GreaterThanOrEqual(new Vector<int>(currentState & CharIndexMask), new Vector<int>(currentWord.Length)), nextStateNextWord, new Vector<int>(currentState))[0];
                    }
                    *currentPointer = 128;

                    startPointer[7] = (uint)(length << 3);
                }
            }
        }

        public byte[] GetBytes(int number)
        {
            System.Diagnostics.Debug.Assert(number < Constants.PhrasesPerSet);

            fixed(uint* bufferPointer = this.Buffer)
            {
                var phrasePointer = bufferPointer + 8 * number;
                var length = phrasePointer[7] >> 3;
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
