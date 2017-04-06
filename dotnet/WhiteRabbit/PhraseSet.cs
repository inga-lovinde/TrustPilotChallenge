namespace WhiteRabbit
{
    // Anagram representation optimized for MD5
    internal unsafe struct PhraseSet
    {
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
                    byte[] currentWord = words[permutation[0]];
                    var j = 0;
                    var wordIndex = 0;
                    var currentPointer = (byte*)startPointer;
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
